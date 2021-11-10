using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DungeonLife
{
    public abstract class WorldCell
    {
        #region Constants

        protected const float IDEAL_TEMPERATURE = 21;
        protected const int TEMPERATURE_SEARCH_RADIUS = 1;
        protected const int HUMIDITY_SEARCH_RADIUS = 2;
        protected const bool SHOW_TEMPERATURE = true;
        protected const bool SHOW_HUMIDITY = true;
        protected const float HUMIDITY_DELTA = 0.1f;

        #endregion

        #region Fields

        protected static IRandom _random = ThreadSafeRandom.Instance;
        private Color _backgroundColor = Color.Black;

        #endregion

        #region Constructors

        public WorldCell(int x, int y)
        {
            Position = new Vector2(x, y);
        }

        #endregion

        #region Properties

        public Vector2 Position { get; }
        public float MovementSpeedMultiplier { get; set; } = 1.0f;

        public bool BlocksMovement
        {
            get
            {
                return MovementSpeedMultiplier == 0;
            }
        }

        public float Temperature { get; set; } = IDEAL_TEMPERATURE;
        public float Humidity { get; set; } = 0;

        public virtual Color BackgroundColor
        {
            get
            {
                var color = _backgroundColor;
                if (SHOW_TEMPERATURE)
                {
                    var tempColor = Color.Lerp(Color.Blue, Color.Red, (Temperature - LifeAppSettings.MinimumTemperature) / LifeAppSettings.TemperatureRange);
                    color = Color.Lerp(color, tempColor, 0.5f);
                }
                if (SHOW_HUMIDITY)
                {
                    color = Color.Lerp(color, Color.AnsiWhite, Humidity);
                }

                return color;
            }
            set => _backgroundColor = value;
        }

        public virtual Color ForegroundColor { get; set; } = Color.White;
        public virtual int Glyph { get; set; } = '.';

        #endregion

        #region Methods

        public virtual void Update(IWorldState state)
        {
            Humidity = GetRegionHumidity(Position, state.Cells) * 0.999f;
            Humidity = MathHelpers.Clamp(Humidity, 0.0f, 1.0f);
        }

        public virtual void Draw(TimeSpan delta, CellSurface surface)
        {
            surface.SetGlyph((int)Position.X, (int)Position.Y, Glyph, ForegroundColor, BackgroundColor);
        }

        protected IEnumerable<WorldCell> IterateOver(Vector2 position, WorldCell[,] cells, int radius)
        {
            var xMin = Math.Max(0, Position.X - radius);
            var xMax = Math.Min(cells.GetLength(0) - 1, Position.X + radius);
            var yMin = Math.Max(0, Position.Y - radius);
            var yMax = Math.Min(cells.GetLength(1) - 1, Position.Y + radius);

            for (var xx = xMin; xx <= xMax; xx++)
            {
                for (var yy = yMin; yy <= yMax; yy++)
                {
                    yield return cells[(int)xx, (int)yy];
                }
            }
        }

        protected float SumOver(Vector2 position, WorldCell[,] cells, int radius, Func<WorldCell, float> property)
        {
            return IterateOver(position, cells, radius).Sum(property);
        }

        protected float AverageOver(Vector2 position, WorldCell[,] cells, int radius, Func<WorldCell, float> property)
        {
            var numCells = (radius * 2 + 1) * (radius * 2 + 1);
            return SumOver(position, cells, radius, property) / numCells;
        }

        /// <summary>
        /// Get the average temperature for the region.
        /// </summary>
        protected float GetRegionTemperature(Vector2 position, WorldCell[,] cells, int radius = TEMPERATURE_SEARCH_RADIUS)
        {
            return AverageOver(position, cells, radius, c => c.Temperature);
        }

        /// <summary>
        /// Get the average humidity for the region.
        /// </summary>
        protected float GetRegionHumidity(Vector2 position, WorldCell[,] cells, int radius = HUMIDITY_SEARCH_RADIUS)
        {
            return AverageOver(position, cells, radius, c => c.Humidity);
        }

        #endregion
    }
}
