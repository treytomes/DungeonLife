using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace DungeonLife
{
    class WorldConsole : ControlsConsole, IWorldState
    {
        #region Constants

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

        public WorldConsole(int width, int height, WorldGenerator generator = null)
            : base(width, height)
        {
            Cells = new WorldCellCollection(LifeAppSettings.WorldWidth, LifeAppSettings.WorldHeight);
            Entities = new EntityCollection();

            (generator ?? new SpaciousCavernGenerator()).Generate(this);

            // Create the shared surface
            _sharedSurface = new CellSurface(LifeAppSettings.WorldWidth, LifeAppSettings.WorldHeight);

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
        public WorldCellCollection Cells { get; private set; }
        public EntityCollection Entities { get; }
        public bool IsRunning { get; set; } = true;
        public bool IsStepping { get; set; } = false;

        #endregion

        #region Methods

        public bool IsMovementBlocked(Vector2 position)
        {
            return Cells.IsMovementBlocked(position) || Entities.IsMovementBlocked(position);
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

            Parallel.ForEach(Cells, cell =>
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
