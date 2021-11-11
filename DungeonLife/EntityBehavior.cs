using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace DungeonLife
{
    abstract class EntityBehavior
    {
        protected static IRandom _random = ThreadSafeRandom.Instance;
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
    }

    /// <summary>
    /// The entity will wander aimlessly.
    /// </summary>
    [DisplayName("Wandering")]
    class EntityWanderBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        private float _changeDirectionChance = (float)_random.NextDouble();

        public EntityWanderBehavior(Entity entity)
            : base(entity)
        {
            _entity.MovingDirection = _random.NextDirection();
        }

        public override bool Update(IWorldState world)
        {
            if (_random.NextDouble() > _changeDirectionChance)
            {
                var newDirection = _random.NextDirection();
                _entity.MovingDirection = Vector2.Normalize((_entity.MovingDirection * MAINTENANCE_BIAS + new Vector2(newDirection.X, newDirection.Y)) / (MAINTENANCE_BIAS + 1));
                return true;
            }
            return false;
        }
    }

    [DisplayName("Separating")]
    class EntitySeparationBehavior : EntityBehavior
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
            var entities = world.Entities.InArea(_entity.Position, _entity.RangeOfSight, _entity.GetType());
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

                var delta = _entity.Position - entity.Position;
                var distance = delta.LengthSquared();
                if (distance <= _separationAmountSquared)
                {
                    // It's too close!

                    // The normalized distance is the new movement direction.
                    distance = delta.Length();
                    delta /= distance;

                    if (delta == Vector2.Zero)
                    {
                        return false;
                    }

                    delta = Vector2.Normalize(delta);
                    _entity.MovingDirection = Vector2.Normalize((_entity.MovingDirection * MAINTENANCE_BIAS + delta) / (MAINTENANCE_BIAS + 1));
                    return true;
                }
            }
            return false;
        }
    }

    [DisplayName("Aligning")]
    class EntityAlignmentBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        public EntityAlignmentBehavior(Entity entity)
            : base(entity)
        {
        }

        public override bool Update(IWorldState world)
        {
            var entities = world.Entities.InArea(_entity.Position, _entity.RangeOfSight, _entity.GetType());
            var count = entities.Count();
            if (count <= 1)
            {
                // Can't flock with yourself.
                return false;
            }

            var d = Vector2.Zero;
            foreach (var entity in entities)
            {
                d += entity.MovingDirection;
            }
            d /= count;

            if (d == Vector2.Zero)
            {
                return false;
            }

            d = Vector2.Normalize(d);
            _entity.MovingDirection = Vector2.Normalize((_entity.MovingDirection * MAINTENANCE_BIAS + d) / (MAINTENANCE_BIAS + 1));
            return true;
        }
    }

    [DisplayName("Cohesing")]
    class EntityCohesionBehavior : EntityBehavior
    {
        private const int MAINTENANCE_BIAS = 1;

        public EntityCohesionBehavior(Entity entity)
            : base(entity)
        {
        }

        public override bool Update(IWorldState world)
        {
            var entities = world.Entities.InArea(_entity.Position, _entity.RangeOfSight, _entity.GetType());
            var count = entities.Count();
            if (count <= 1)
            {
                // Can't flock with yourself.
                return false;
            }

            var center = Vector2.Zero;
            foreach (var entity in entities)
            {
                center += entity.Position;
                count++;
            }
            center /= count;

            if (center == _entity.Position)
            {
                return false;
            }

            var delta = _entity.Position - center;

            if (delta == Vector2.Zero)
            {
                return false;
            }
            delta = Vector2.Normalize(delta);

            _entity.MovingDirection = (_entity.MovingDirection * MAINTENANCE_BIAS + delta) / (MAINTENANCE_BIAS + 1);
            return true;
        }
    }

    /// <summary>
    /// Seek out water to satisfy thirst.
    /// </summary>
    [DisplayName("Thirsty")]
    class EntityThirstBehavior : EntityBehavior
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

            var currentCell = world.Cells[(int)_entity.Position.X, (int)_entity.Position.Y] as WaterSourceCell;
            if (currentCell != null)
            {
                if (Drink(currentCell))
                {
                    return true;
                }
            }

            // Seek out water.
            var cells = world.Cells.IterateOver(_entity.Position, _entity.RangeOfSight);

            var closestWaterDistance = float.MaxValue;
            var closestWaterPosition = Vector2.One * float.MaxValue;

            var closestHumidDistance = float.MaxValue;
            var closestHumidPosition = Vector2.One * float.MaxValue;
            var closestHumidity = float.MinValue;

            foreach (var cell in cells)
            {
                if (cell is WaterSourceCell)
                {
                    var dist = (_entity.Position - cell.Position).LengthSquared();
                    if (dist < closestWaterDistance)
                    {
                        closestWaterDistance = dist;
                        closestWaterPosition = cell.Position;

                        if (closestWaterDistance < 2)
                        {
                            break;
                        }
                    }
                }
                else if (closestWaterDistance == float.MaxValue)
                {
                    // No water found yet, so look for the most humid cell.
                    var dist = (_entity.Position - cell.Position).LengthSquared();

                    // Is the cell either more humid than the current target, or just as humid and closer?
                    if ((cell.Humidity > closestHumidity) || ((cell.Humidity == closestHumidDistance) && (dist < closestHumidDistance)))
                    {
                        closestHumidDistance = dist;
                        closestHumidPosition = cell.Position;
                        closestHumidity = cell.Humidity;
                    }
                }
            }

            var delta = Vector2.Zero;
            if (closestWaterDistance != float.MaxValue)
            {
                if (closestWaterDistance <= 1)
                {
                    // You're next to the water, so take a drink.
                    if (Drink(world.Cells[(int)closestWaterPosition.X, (int)closestWaterPosition.Y]))
                    {
                        return true;
                    }
                }

                delta = closestWaterPosition - _entity.Position;
            }
            else if (closestHumidDistance != float.MaxValue)
            {
                delta = closestHumidPosition - _entity.Position;
            }

            if (delta == Vector2.Zero)
            {
                return false;
            }
            else
            {
                _entity.MovingDirection = Vector2.Normalize(delta);
                return true;
            }
        }
    }
    
    /// <summary>
    /// Seek out algae to satisfy hunger.
    /// </summary>
    [DisplayName("Hungry")]
    class EntityHungerBehavior : EntityBehavior
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

            var currentCell = world.Cells[(int)_entity.Position.X, (int)_entity.Position.Y];
            if (Eat(currentCell))
            {
                return true;
            }

            var cells = world.Cells.IterateOver(_entity.Position, _entity.RangeOfSight);

            var closestFoodDistance = float.MaxValue;
            var closestFoodPosition = Vector2.One * float.MaxValue;
            var closestFoodLevel = float.MinValue;

            foreach (var cell in cells)
            {
                var floor = cell as FloorWorldCell;
                if ((floor != null) && (floor.AlgaeLevel > 0))
                {
                    // Grab either the closest floor cell or the one with the highest food level.
                    var dist = (_entity.Position - cell.Position).LengthSquared();

                    if ((dist < closestFoodDistance) || ((dist == closestFoodDistance) && (floor.AlgaeLevel > closestFoodLevel)))
                    {
                        closestFoodDistance = dist;
                        closestFoodPosition = cell.Position;
                        closestFoodLevel = floor.AlgaeLevel;
                    }
                }
            }

            var delta = Vector2.Zero;
            if (closestFoodDistance != float.MaxValue)
            {
                if (closestFoodDistance < 2)
                {
                    if (Eat(world.Cells[(int)closestFoodPosition.X, (int)closestFoodPosition.Y]))
                    {
                        return true;
                    }
                }
                delta = closestFoodPosition - _entity.Position;
            }

            if (delta == Vector2.Zero)
            {
                return false;
            }
            else
            {
                _entity.MovingDirection = Vector2.Normalize(delta);
                return true;
            }
        }
    }

    static class BehaviorFactory
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
