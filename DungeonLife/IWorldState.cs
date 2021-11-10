using System;
using System.Collections.Generic;

namespace DungeonLife
{
    public interface IWorldState
    {
        TimeSpan WorldTime { get; set; }
        WorldCell[,] Cells { get; }

        Entity GetEntityAt(float x, float y);
        IEnumerable<Entity> GetEntitiesInArea(float x, float y, float radius, Type entityType = null);
        bool IsMovementBlocked(float x, float y);
    }
}
