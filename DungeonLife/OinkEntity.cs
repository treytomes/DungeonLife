using SadRogue.Primitives;
using System;
using System.ComponentModel;

namespace DungeonLife
{
    [DisplayName("Oink")]
    class OinkEntity : Entity
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
