using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DungeonLife
{
    public abstract class EntityBehavior
    {
        protected static IRandom _random = new ThreadSafeRandom();
        protected readonly Entity _entity;

        public EntityBehavior(Entity entity)
        {
            _entity = entity;
        }

        /// <returns>Did the behavior do anything?</returns>
        public abstract bool Update(IWorldState world);

        public override string ToString()
        {
            return GetType().GetCustomAttribute<DisplayNameAttribute>().DisplayName;
        }

        protected IEnumerable<WorldCell> IterateOver(int x, int y, WorldCell[,] cells, int radius)
        {
            var xMin = Math.Max(0, x - radius);
            var xMax = Math.Min(cells.GetLength(0) - 1, x + radius);
            var yMin = Math.Max(0, y - radius);
            var yMax = Math.Min(cells.GetLength(1) - 1, y + radius);

            for (var xx = xMin; xx <= xMax; xx++)
            {
                for (var yy = yMin; yy <= yMax; yy++)
                {
                    yield return cells[xx, yy];
                }
            }
        }
    }

    /// <summary>
    /// The entity will wander aimlessly.
    /// </summary>
    [DisplayName("Wandering")]
    public class EntityWanderBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        private float _changeDirectionChance = (float)_random.NextDouble();

        public EntityWanderBehavior(Entity entity)
            : base(entity)
        {
            _entity.MovingDirection = Direction.Random;
        }

        public override bool Update(IWorldState world)
        {
            if (_random.NextDouble() > _changeDirectionChance)
            {
                var newDirection = Direction.Random;
                _entity.MovingDirection.DeltaX = (_entity.MovingDirection.DeltaX * MAINTENANCE_BIAS + newDirection.DeltaX) / (MAINTENANCE_BIAS + 1);
                _entity.MovingDirection.DeltaY = (_entity.MovingDirection.DeltaY * MAINTENANCE_BIAS + newDirection.DeltaY) / (MAINTENANCE_BIAS + 1);
                _entity.MovingDirection.Normalize();
                return true;
            }
            return false;
        }
    }

    [DisplayName("Separating")]
    public class EntitySeparationBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        private float _separationAmount;
        private float _separationAmountSquared;

        public EntitySeparationBehavior(Entity entity, float separationAmount = 2)
            : base(entity)
        {
            _separationAmount = separationAmount;
            _separationAmountSquared = _separationAmount * _separationAmount;
        }

        public override bool Update(IWorldState world)
        {
            var entities = world.GetEntitiesInArea(_entity.X, _entity.Y, _entity.RangeOfSight, _entity.GetType());
            var count = entities.Count();
            if (count <= 1)
            {
                return false;
            }

            foreach (var entity in entities)
            {
                if (entity == _entity)
                {
                    continue;
                }

                float dx = _entity.X - entity.X;
                float dy = _entity.Y - entity.Y;
                float distance = dx * dx + dy * dy;
                if (distance <= _separationAmountSquared)
                {
                    // It's too close!

                    // The normalized distance is the new movement direction.
                    distance = (float)Math.Sqrt(distance);
                    dx = dx / distance;
                    dy = dy / distance;

                    if ((dx == 0) && (dy == 0))
                    {
                        return false;
                    }

                    _entity.MovingDirection.DeltaX = (_entity.MovingDirection.DeltaX * MAINTENANCE_BIAS + dx) / (MAINTENANCE_BIAS + 1);
                    _entity.MovingDirection.DeltaY = (_entity.MovingDirection.DeltaY * MAINTENANCE_BIAS + dy) / (MAINTENANCE_BIAS + 1);
                    _entity.MovingDirection.Normalize();
                    return true;
                }
            }
            return false;
        }
    }

    [DisplayName("Aligning")]
    public class EntityAlignmentBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        public EntityAlignmentBehavior(Entity entity)
            : base(entity)
        {
        }

        public override bool Update(IWorldState world)
        {
            var entities = world.GetEntitiesInArea(_entity.X, _entity.Y, _entity.RangeOfSight, _entity.GetType());
            var count = entities.Count();
            if (count <= 1)
            {
                // Can't flock with yourself.
                return false;
            }

            float dx = 0;
            float dy = 0;
            foreach (var entity in entities)
            {
                dx += entity.MovingDirection.DeltaX;
                dy += entity.MovingDirection.DeltaY;
            }
            dx /= count;
            dy /= count;

            if ((dx == 0) && (dy == 0))
            {
                return false;
            }

            _entity.MovingDirection.DeltaX = (_entity.MovingDirection.DeltaX * MAINTENANCE_BIAS + dx) / (MAINTENANCE_BIAS + 1);
            _entity.MovingDirection.DeltaY = (_entity.MovingDirection.DeltaY * MAINTENANCE_BIAS + dy) / (MAINTENANCE_BIAS + 1);
            _entity.MovingDirection.Normalize();
            return true;
        }
    }

    [DisplayName("Cohesing")]
    public class EntityCohesionBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        public EntityCohesionBehavior(Entity entity)
            : base(entity)
        {
        }

        public override bool Update(IWorldState world)
        {
            var entities = world.GetEntitiesInArea(_entity.X, _entity.Y, _entity.RangeOfSight, _entity.GetType());
            var count = entities.Count();
            if (count <= 1)
            {
                // Can't flock with yourself.
                return false;
            }

            float cx = 0;
            float cy = 0;
            foreach (var entity in entities)
            {
                cx += entity.X;
                cy += entity.Y;
                count++;
            }
            cx /= count;
            cy /= count;

            if ((cx == _entity.X) && (cy == _entity.Y))
            {
                return false;
            }

            var dx = _entity.X - cx;
            var dy = _entity.Y - cy;

            if ((dx == 0) && (dy == 0))
            {
                return false;
            }

            var d = new Direction(dx, dy);
            d.Normalize();

            _entity.MovingDirection.DeltaX = (_entity.MovingDirection.DeltaX * MAINTENANCE_BIAS + dx) / (MAINTENANCE_BIAS + 1);
            _entity.MovingDirection.DeltaY = (_entity.MovingDirection.DeltaY * MAINTENANCE_BIAS + dy) / (MAINTENANCE_BIAS + 1);
            _entity.MovingDirection.Normalize();
            return true;
        }
    }

    /// <summary>
    /// Seek out water to satisfy thirst.
    /// </summary>
    [DisplayName("Thirsty")]
    public class EntityThirstBehavior : EntityBehavior
    {
        /// <summary>
        /// Water is perfectly thirst-quenching.
        /// </summary>
        private const float WATER_QUENCH_FACTOR = 1.0f;

        private float _drinkSpeed;

        public EntityThirstBehavior(Entity entity, float drinkSpeed = 0.1f)
            : base(entity)
        {
            _drinkSpeed = drinkSpeed;
        }

        private bool Drink(WorldCell cell)
        {
            if (cell is WaterSourceCell)
            {
                _entity.Thirst -= _drinkSpeed * WATER_QUENCH_FACTOR;
                if (_entity.Thirst < 0)
                {
                    _entity.Thirst = 0;
                }
                return true;
            }
            return false;
        }

        public override bool Update(IWorldState world)
        {
            if (_entity.Thirst == 0)
            {
                return false;
            }

            var isThirsty = _random.NextDouble() < _entity.Thirst;
            if (!isThirsty)
            {
                return false;
            }

            var currentCell = world.Cells[(int)_entity.X, (int)_entity.Y] as WaterSourceCell;
            if (currentCell != null)
            {
                if (Drink(currentCell))
                {
                    return true;
                }
            }

            // Seek out water.
            var cells = IterateOver((int)_entity.X, (int)_entity.Y, world.Cells, _entity.RangeOfSight);

            var closestWaterDistance = float.MaxValue;
            var closestWaterX = float.MaxValue;
            var closestWaterY = float.MaxValue;

            var closestHumidDistance = float.MaxValue;
            var closestHumidX = float.MaxValue;
            var closestHumidY = float.MaxValue;
            var closestHumidity = float.MinValue;
            float dx;
            float dy;

            foreach (var cell in cells)
            {
                if (cell is WaterSourceCell)
                {
                    dx = _entity.X - cell.X;
                    dy = _entity.Y - cell.Y;
                    var dist = dx * dx + dy * dy;
                    if (dist < closestWaterDistance)
                    {
                        closestWaterDistance = dist;
                        closestWaterX = cell.X;
                        closestWaterY = cell.Y;

                        if (closestWaterDistance <= 1)
                        {
                            break;
                        }
                    }
                }
                else if (closestWaterDistance == float.MaxValue)
                {
                    // No water found yet, so look for the most humid cell.

                    dx = _entity.X - cell.X;
                    dy = _entity.Y - cell.Y;
                    var dist = dx * dx + dy * dy;

                    // Is the cell either more humid than the current target, or just as humid and closer?
                    if ((cell.Humidity > closestHumidity) || ((cell.Humidity == closestHumidDistance) && (dist < closestHumidDistance)))
                    {
                        closestHumidDistance = dist;
                        closestHumidX = cell.X;
                        closestHumidY = cell.Y;
                        closestHumidity = cell.Humidity;
                    }
                }
            }

            dx = 0.0f;
            dy = 0.0f;
            if (closestWaterDistance != float.MaxValue)
            {
                if (closestWaterDistance <= 1)
                {
                    // You're next to the water, so take a drink.
                    if (Drink(world.Cells[(int)closestWaterX, (int)closestWaterY]))
                    {
                        return true;
                    }
                }

                dx = closestWaterX - _entity.X;
                dy = closestWaterY - _entity.Y;
            }
            else if (closestHumidDistance != float.MaxValue)
            {
                dx = closestHumidX - _entity.X;
                dy = closestHumidY - _entity.Y;
            }

            if ((dx != 0.0) || (dy != 0.0f))
            {
                _entity.MovingDirection.DeltaX = dx;
                _entity.MovingDirection.DeltaY = dy;
                _entity.MovingDirection.Normalize();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// Seek out algae to satisfy hunger.
    /// </summary>
    [DisplayName("Hungry")]
    public class EntityHungerBehavior : EntityBehavior
    {
        /// <summary>
        /// Reducing the food value of algae should force for travelling in search of food.
        /// </summary>
        private const float ALGAE_FOOD_FACTOR = 0.2f;

        private float _eatSpeed;

        public EntityHungerBehavior(Entity entity, float eatSpeed = 0.1f)
            : base(entity)
        {
            _eatSpeed = eatSpeed;
        }

        private bool Eat(WorldCell cell)
        {
            if (cell is FloorWorldCell)
            {
                var floor = cell as FloorWorldCell;
                if (floor.AlgaeLevel > 0)
                {
                    // Consume the local flora.

                    if (floor.AlgaeLevel > _eatSpeed)
                    {
                        floor.AlgaeLevel -= _eatSpeed / ALGAE_FOOD_FACTOR;
                        _entity.Hunger -= _eatSpeed * ALGAE_FOOD_FACTOR;
                    }
                    else
                    {
                        _entity.Hunger -= floor.AlgaeLevel * ALGAE_FOOD_FACTOR;
                        floor.AlgaeLevel = 0;
                    }

                    if (_entity.Hunger < 0)
                    {
                        _entity.Hunger = 0;
                    }

                    return true;
                }
            }
            return false;
        }

        public override bool Update(IWorldState world)
        {
            if (_entity.Hunger == 0)
            {
                return false;
            }

            var isHungry = _random.NextDouble() < _entity.Hunger;
            if (!isHungry)
            {
                return false;
            }

            // Seek out food.

            var currentCell = world.Cells[(int)_entity.X, (int)_entity.Y];
            if (Eat(currentCell))
            {
                return true;
            }

            // Seek out water.
            var cells = IterateOver((int)_entity.X, (int)_entity.Y, world.Cells, _entity.RangeOfSight);

            var closestFoodDistance = float.MaxValue;
            var closestFoodX = float.MaxValue;
            var closestFoodY = float.MaxValue;
            var closestFoodLevel = float.MinValue;
            float dx;
            float dy;

            foreach (var cell in cells)
            {
                var floor = cell as FloorWorldCell;
                if ((floor != null) && (floor.AlgaeLevel > 0))
                {
                    // Grab either the closest floor cell or the one with the highest food level.
                    dx = _entity.X - cell.X;
                    dy = _entity.Y - cell.Y;
                    var dist = dx * dx + dy * dy;

                    if ((dist < closestFoodDistance) || ((dist == closestFoodDistance) && (floor.AlgaeLevel > closestFoodLevel)))
                    {
                        closestFoodDistance = dist;
                        closestFoodX = cell.X;
                        closestFoodY = cell.Y;
                        closestFoodLevel = floor.AlgaeLevel;
                    }
                }
            }

            dx = 0.0f;
            dy = 0.0f;
            if (closestFoodDistance != float.MaxValue)
            {
                if (closestFoodDistance <= 1)
                {
                    if (Eat(world.Cells[(int)closestFoodX, (int)closestFoodY]))
                    {
                        return true;
                    }
                }
                dx = closestFoodX - _entity.X;
                dy = closestFoodY - _entity.Y;
            }

            if ((dx != 0.0) || (dy != 0.0f))
            {
                _entity.MovingDirection.DeltaX = dx;
                _entity.MovingDirection.DeltaY = dy;
                _entity.MovingDirection.Normalize();
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class BehaviorFactory
    {
        public static EntityBehavior Wander(Entity entity)
        {
            return new EntityWanderBehavior(entity);
        }

        public static EntityBehavior Separation(Entity entity)
        {
            return new EntitySeparationBehavior(entity);
        }

        public static EntityBehavior Alignment(Entity entity)
        {
            return new EntityAlignmentBehavior(entity);
        }

        public static EntityBehavior Cohesion(Entity entity)
        {
            return new EntityCohesionBehavior(entity);
        }

        public static EntityBehavior Thirst(Entity entity)
        {
            return new EntityThirstBehavior(entity);
        }

        public static EntityBehavior Hunger(Entity entity)
        {
            return new EntityHungerBehavior(entity);
        }
    }
}
