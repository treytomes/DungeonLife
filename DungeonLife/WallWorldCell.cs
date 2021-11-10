using SadRogue.Primitives;

namespace DungeonLife
{

    /// <summary>
    /// A wall cannot be passed.
    /// </summary>
    public class WallWorldCell : WorldCell
    {
        public WallWorldCell(int x, int y)
            : base(x, y)
        {
            ForegroundColor = Color.AnsiWhite;
            BackgroundColor = Color.AnsiBlackBright;
            Glyph = '#';
            MovementSpeedMultiplier = 0.0f;
        }
    }
}
