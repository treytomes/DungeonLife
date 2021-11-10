using System;

namespace DungeonLife
{
    class CellSelectedEventArgs : EventArgs
    {
        public CellSelectedEventArgs(int worldX, int worldY, int screenX, int screenY)
        {
            WorldX = worldX;
            WorldY = worldY;
            ScreenX = screenX;
            ScreenY = screenY;

        }

        public int WorldX { get; }
        public int WorldY { get; }
        public int ScreenX { get; }
        public int ScreenY { get; }
    }
}
