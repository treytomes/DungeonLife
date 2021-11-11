using System;
using System.Numerics;

namespace DungeonLife
{
    interface IWorldState
    {
        TimeSpan WorldTime { get; set; }
        WorldCellCollection Cells { get; }
        EntityCollection Entities { get; }
        bool IsMovementBlocked(Vector2 position);
    }
}
