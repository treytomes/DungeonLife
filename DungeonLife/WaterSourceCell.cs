using SadConsole;
using SadRogue.Primitives;
using System;

namespace DungeonLife
{
    /// <summary>
    /// Water defies temperature changes, and will spread humidity as it warms up.
    /// </summary>
    public class WaterSourceCell : WorldCell
    {
        private const int GLYPH_WATER1 = '~';
        private const int GLYPH_WATER2 = 247;
        private const int ANIMATE_MS = 500;

        private TimeSpan _totalTime = TimeSpan.Zero;

        public WaterSourceCell(int x, int y)
            : base(x, y)
        {
            ForegroundColor = Color.AnsiWhite;
            BackgroundColor = Color.AnsiBlue;
            Glyph = GLYPH_WATER1;
            MovementSpeedMultiplier = 0.25f;
        }

        public override void Draw(TimeSpan delta, CellSurface surface)
        {
            _totalTime += delta;
            if (_totalTime.TotalMilliseconds > ANIMATE_MS)
            {
                Glyph = (Glyph == GLYPH_WATER1) ? GLYPH_WATER2 : GLYPH_WATER1;
                _totalTime = TimeSpan.Zero;
            }
            base.Draw(delta, surface);
        }

        public override void Update(IWorldState state)
        {
            base.Update(state);

            var tempDistance = Temperature - IDEAL_TEMPERATURE;
            // Water will evaporate or condense depending on how hot or cold it is.
            if ((tempDistance > 0) && (Humidity < 1))
            {
                Humidity += HUMIDITY_DELTA * tempDistance;
            }
            else if ((tempDistance < 0) && (Humidity > 0))
            {
                Humidity -= HUMIDITY_DELTA;
            }

            var newTemp = GetRegionTemperature(X, Y, state.Cells);

            // Water act as a temperature stabilizer; much more so than floors.
            Temperature = (2 * Temperature + newTemp) / 3.0f;
            if (Temperature > IDEAL_TEMPERATURE)
            {
                Temperature -= 0.001f;
            }
            else if (Temperature < IDEAL_TEMPERATURE)
            {
                Temperature += 0.001f;
            }
        }
    }
}
