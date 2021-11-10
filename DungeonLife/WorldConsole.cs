using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DungeonLife
{
    internal class WorldConsole : ControlsConsole, IWorldState
    {
        #region Constants

        public const int WORLD_WIDTH = 256;
        public const int WORLD_HEIGHT = 256;
        private const int MS_PER_FRAME = 100;

        #endregion

        #region Events

        public event EventHandler<CellSelectedEventArgs> CellSelected;

        #endregion

        #region Fields

        private static IRandom _random = ThreadSafeRandom.Instance;
        private CellSurface _sharedSurface;
        private bool _isDrawing = false;
        private bool _isMoving = false;
        private Point _moveStartViewerPoint;
        private Point _moveStartMousePoint;
        private TimeSpan _totalElapsedTime = TimeSpan.Zero;
        private TimeSpan _lastUpdateTime = TimeSpan.Zero;

        #endregion

        #region Constructors

        public WorldConsole(int width, int height)
            : base(width, height)
        {
            Cells = new WorldCell[WORLD_WIDTH, WORLD_HEIGHT];
            BuildWorld();

            Entities = new List<Entity>();
            PlaceEntities();

            // Create the shared surface
            _sharedSurface = new CellSurface(WORLD_WIDTH, WORLD_HEIGHT);

            // Create viewer controls and attach them to the surface
            var viewer = new SurfaceViewer(width, height);
            viewer.ScrollBarMode = SurfaceViewer.ScrollBarModes.AsNeeded;
            viewer.SetSurface(_sharedSurface);
            viewer.Position = (0, 0);
            viewer.MouseMove += Viewer_MouseMove;
            Controls.Add(viewer);
        }

        #endregion

        #region Properties

        public TimeSpan WorldTime { get; set; }

        public WorldCell[,] Cells { get; private set; }

        public List<Entity> Entities { get; }
        public bool IsRunning { get; set; } = true;
        public bool IsStepping { get; set; } = false;

        #endregion

        #region Methods

        public Entity GetEntityAt(float x, float y)
        {
            foreach (var entity in Entities)
            {
                var dx = entity.Position.X - x;
                var dy = entity.Position.Y - y;
                if (dx * dx + dy * dy <= 2)
                {
                    return entity;
                }
            }
            return null;
        }

        public IEnumerable<Entity> GetEntitiesInArea(float x, float y, float radius, Type entityType = null)
        {
            foreach (var entity in Entities)
            {
                var dx = entity.Position.X - x;
                var dy = entity.Position.Y - y;
                if ((dx * dx + dy * dy) <= radius * radius)
                {
                    if ((entityType == null) || (entityType == entity.GetType()))
                    {
                        yield return entity;
                    }
                }
            }
        }

        public bool IsMovementBlocked(float x, float y)
        {
            if ((x < 0) || (x >= Cells.GetLength(0)) || (y < 0) || (y >= Cells.GetLength(1)))
            {
                return true;
            }
            return Cells[(int)x, (int)y]?.BlocksMovement ?? false;
        }

        public void SetAlgaeValue(int x, int y, float value)
        {
            var cell = Cells[x, y] as FloorWorldCell;
            if (cell == null)
            {
                return;
            }
            cell.AlgaeLevel = value;
        }

        private void BuildWorld()
        {
            // Fill the area with floors.
            for (var x = 1; x < WORLD_WIDTH - 1; x++)
            {
                for (var y = 1; y < WORLD_HEIGHT - 1; y++)
                {
                    Cells[x, y] = new FloorWorldCell(x, y);
                }
            }

            // Make a small pond.
            DrawPond(Cells, 32, 32, 12);
            DrawPond(Cells, 200, 96, 24);
            DrawPond(Cells, 0, 255, 32);

            var numPillars = _random.Next(5, 10);
            for (var n = 0; n < numPillars; n++)
            {
                var radius = _random.Next(4, 12);
                var x = _random.Next(0, Cells.GetLength(0));
                var y = _random.Next(0, Cells.GetLength(1));
                DrawPillar(Cells, x, y, radius);
            }

            // Build the borders.
            for (var x = 0; x < WORLD_WIDTH; x++)
            {
                Cells[x, 0] = new BorderWorldCell(x, 0);
                Cells[x, WORLD_HEIGHT - 1] = new BorderWorldCell(x, WORLD_HEIGHT - 1);
            }
            for (var y = 0; y < WORLD_HEIGHT; y++)
            {
                Cells[0, y] = new BorderWorldCell(0, y);
                Cells[WORLD_WIDTH - 1, y] = new BorderWorldCell(WORLD_WIDTH - 1, y);
            }
        }

        private void DrawPond(WorldCell[,] cells, int pondX, int pondY, int radius)
        {
            var radius2 = radius * radius;

            for (var x = -radius; x <= radius; x++)
            {
                var dx = pondX + x;
                if ((dx < 0) || (dx >= WORLD_WIDTH))
                {
                    continue;
                }

                for (var y = -radius; y <= radius; y++)
                {
                    var dy = pondY + y;
                    if ((dy < 0) || (dy >= WORLD_HEIGHT))
                    {
                        continue;
                    }

                    if (x * x + y * y <= radius2)
                    {
                        cells[dx, dy] = new WaterSourceCell(dx, dy);
                    }
                }
            }
        }

        private void DrawPillar(WorldCell[,] cells, int pillarX, int pillarY, int radius)
        {
            var radius2 = radius * radius;

            for (var x = -radius; x <= radius; x++)
            {
                var dx = pillarX + x;
                if ((dx < 0) || (dx >= WORLD_WIDTH))
                {
                    continue;
                }

                for (var y = -radius; y <= radius; y++)
                {
                    var dy = pillarY + y;
                    if ((dy < 0) || (dy >= WORLD_HEIGHT))
                    {
                        continue;
                    }

                    if (x * x + y * y <= radius2)
                    {
                        cells[dx, dy] = new WallWorldCell(dx, dy);
                    }
                }
            }
        }

        private void PlaceEntities()
        {
            var numEntities = _random.Next(50, 100);
            for (var n = 0; n < numEntities; n++)
            {
                var isPlaced = false;
                while (!isPlaced)
                {
                    var x = _random.Next(0, 256);
                    var y = _random.Next(0, 256);
                    var cell = Cells[x, y];
                    if (!(cell is FloorWorldCell))
                    {
                        continue;
                    }

                    var oink = new OinkEntity(x, y);
                    Entities.Add(oink);
                    isPlaced = true;
                }
            }
        }

        public override void Render(TimeSpan delta)
        {
            if (!IsRunning && !IsStepping)
            {
                delta = TimeSpan.Zero;
            }

            _totalElapsedTime += delta;
            var worldDelta = TimeSpan.FromMinutes(1);
            var isUpdating = IsRunning || IsStepping;
            if (IsStepping || _totalElapsedTime.Subtract(_lastUpdateTime).Milliseconds >= MS_PER_FRAME)
            {
                WorldTime += worldDelta;
                isUpdating = true;
                _lastUpdateTime = _totalElapsedTime;
                IsStepping = false;
            }

            Parallel.ForEach(Cells.Cast<WorldCell>(), cell =>
            {
                if (isUpdating)
                {
                    cell.Update(this);
                }
                cell.Draw(delta, _sharedSurface);
            });

            Parallel.ForEach(Entities, e =>
            {
                if (isUpdating)
                {
                    e.Update(this, worldDelta);
                }
                e.Render(_sharedSurface);
            });

            base.Render(delta);
        }

        #endregion

        #region Event Handlers

        private bool _isRightButtonDown = false;

        private void Viewer_MouseMove(object sender, ControlBase.ControlMouseState e)
        {
            var viewer = (SurfaceViewer)sender;
            if (e.OriginalMouseState.Mouse.LeftButtonDown)
            {
                // Left mouse button selects a cell.

                if (!_isDrawing)
                {
                    _isDrawing = true;
                }

                if (viewer.IsMouseButtonStateClean && viewer.SurfaceControl.MouseArea.Contains(e.MousePosition))
                {
                    CellSelected?.Invoke(this, new CellSelectedEventArgs(e.MousePosition.X + viewer.ChildSurface.ViewPosition.X, e.MousePosition.Y + viewer.ChildSurface.ViewPosition.Y, e.MousePosition.X, e.MousePosition.Y));
                    //SetCellValue(e.MousePosition.X + viewer.ChildSurface.ViewPosition.X, e.MousePosition.Y + viewer.ChildSurface.ViewPosition.Y, 1.0f);
                }
            }
            else if (e.OriginalMouseState.Mouse.MiddleButtonDown)
            {
                // Middle mouse button controls the viewport position.

                if (!_isMoving)
                {
                    _isMoving = true;
                    _moveStartMousePoint = e.MousePosition;
                    _moveStartViewerPoint = (viewer.HorizontalScroller.Value, viewer.VerticalScroller.Value);
                }

                if (viewer.IsMouseButtonStateClean && viewer.SurfaceControl.MouseArea.Contains(e.MousePosition))
                {
                    //SetCellValue(e.MousePosition.X + viewer.ChildSurface.ViewPosition.X, e.MousePosition.Y + viewer.ChildSurface.ViewPosition.Y, 1.0f);
                    var delta = _moveStartMousePoint - e.MousePosition;
                    viewer.HorizontalScroller.Value = _moveStartViewerPoint.X + delta.X;
                    viewer.VerticalScroller.Value = _moveStartViewerPoint.Y + delta.Y;
                }
            }
            else if (e.OriginalMouseState.Mouse.RightButtonDown)
            {
                // Right mouse button edits the algae level.
                if (!_isRightButtonDown)
                {
                    Point cellPoint = (e.MousePosition.X + viewer.ChildSurface.ViewPosition.X, e.MousePosition.Y + viewer.ChildSurface.ViewPosition.Y);
                    var cell = Cells[cellPoint.X, cellPoint.Y] as FloorWorldCell;
                    if (cell != null)
                    {
                        var algae = cell.AlgaeLevel;
                        if (algae > 0)
                        {
                            algae = 0.0f;
                        }
                        else
                        {
                            algae = 1.0f;
                        }
                        cell.AlgaeLevel = algae;
                    }
                    _isRightButtonDown = true;
                }
            }
            else
            {
                _isDrawing = false;
                _isMoving = false;
                _isRightButtonDown = false;
            }
        }

        #endregion
    }
}
