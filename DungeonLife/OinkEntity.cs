using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DungeonLife
{
    public class Entity
    {
        #region Constants
        
        // A high thirst multiplier makes the entity favor water like a duck.

        public float HungerMultiplier { get; protected set; } = 1.5f;
        public float ThirstMultiplier { get; protected set; } = 3.0f;

        #endregion

        #region Constructors

        public Entity(int x, int y)
        {
            Position = new Vector2(x, y);
            MaturityAge = TimeSpan.Zero;
            Age = TimeSpan.Zero;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Behaviors to be executed in order.  The first behavior that does something
        /// will be the active behavior for this round.
        /// </summary>
        public List<EntityBehavior> Behaviors { get; } = new List<EntityBehavior>();

        public EntityBehavior ActiveBehavior { get; private set; }

        public Vector2 MovingDirection { get; set; }

        public Vector2 Position { get; set; }
        public TimeSpan MaturityAge { get; protected set; }
        public TimeSpan Age { get; private set; }

        /// <summary>
        /// 0% = no hunger.  100% = starvation.
        /// </summary>
        public float Hunger { get; set; }

        /// <summary>
        /// 0% = no thirst.  100% = dying of thirst.
        /// </summary>
        public float Thirst { get; set; }

        /// <summary>
        /// Rate of energy consumption.  Indirectly increases hunger and thirst.
        /// </summary>
        public float Metabolism { get; protected set; }

        public int RangeOfSight { get; protected set; }

        public int BabyGlyph { get; protected set; }
        public int AdultGlyph { get; protected set; }
        public Color ForegroundColor { get; protected set; }

        public bool IsAdult
        {
            get
            {
                return Age >= MaturityAge;
            }
        }

        public bool IsChild
        {
            get
            {
                return !IsAdult;
            }
        }

        public int Glyph
        {
            get
            {
                return (Age > MaturityAge) ? AdultGlyph : BabyGlyph;
            }
        }

        #endregion

        #region Methods

        public virtual void Update(IWorldState world, TimeSpan worldDelta)
        {
            foreach (var state in Behaviors)
            {
                if (state.Update(world))
                {
                    // Stop at the first behavior that actually does something.
                    ActiveBehavior = state;
                    break;
                }
            }

            Age += worldDelta;
            Hunger += Metabolism * HungerMultiplier;
            Hunger = MathHelpers.Clamp(Hunger, 0.0f, 1.0f);
            Thirst += Metabolism * ThirstMultiplier;
            Thirst = MathHelpers.Clamp(Thirst, 0.0f, 1.0f);

            var cell = world.Cells[(int)Position.X, (int)Position.Y];
            var newPosition = Position + MovingDirection * cell.MovementSpeedMultiplier;
            if (world.IsMovementBlocked(newPosition.X, newPosition.Y))
            {
                MovingDirection = -MovingDirection;
            }
            else
            {
                Position = newPosition;

                // TODO: If the cell is wet, increase the wetness of this entity.
                // TODO: Water should slow movement speed.
            }
        }

        public void Render(CellSurface surface)
        {
            surface.SetGlyph((int)Position.X, (int)Position.Y, Glyph);
            surface.SetForeground((int)Position.X, (int)Position.Y, ForegroundColor);
        }

        #endregion
    }

    [DisplayName("Oink")]
    public class OinkEntity : Entity
    {
        private const int GLYPH_BABY = 148;
        private const int GLYPH_ADULT = 153;

        public OinkEntity(int x, int y)
            : base(x, y)
        {
            MaturityAge = TimeSpan.FromDays(3);
            BabyGlyph = GLYPH_BABY;
            AdultGlyph = GLYPH_ADULT;
            ForegroundColor = Color.Pink;
            Metabolism = 0.0001f;
            RangeOfSight = 6;

            // Oinks are hungry creatures.
            HungerMultiplier = 5.0f;
            ThirstMultiplier = 1.0f;

            // TODO: Oxygen
            // TODO: Reproduction
            Behaviors.Add(BehaviorFactory.Thirst(this));
            Behaviors.Add(BehaviorFactory.Hunger(this));
            // TODO: Temperature
            // TODO: Wetness; much much to the oinks like to be wet?
            Behaviors.Add(BehaviorFactory.Separation(this));
            Behaviors.Add(BehaviorFactory.Alignment(this));
            Behaviors.Add(BehaviorFactory.Wander(this));
            Behaviors.Add(BehaviorFactory.Cohesion(this));
        }
    }
}
