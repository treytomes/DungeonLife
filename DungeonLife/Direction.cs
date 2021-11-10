using System;

namespace DungeonLife
{
    public class Direction
    {
        private static IRandom _random = ThreadSafeRandom.Instance;

        public Direction(float dx, float dy)
        {
            DeltaX = dx;
            DeltaY = dy;
        }

        public float DeltaX { get; set; }
        public float DeltaY { get; set; }

        public static Direction North { get; } = new Direction(0, -1);
        public static Direction South { get; } = new Direction(0, 1);
        public static Direction East { get; } = new Direction(1, 0);
        public static Direction West { get; } = new Direction(-1, 0);

        public static Direction Random
        {
            get
            {
                var dx = (float)_random.NextDouble() * 2.0f - 1.0f;
                var dy = (float)_random.NextDouble() * 2.0f - 1.0f;
                return new Direction(dx, dy);

                //switch (_random.Next(0, 4))
                //{
                //    case 0:
                //        return North;
                //    case 1:
                //        return South;
                //    case 2:
                //        return East;
                //    case 3:
                //        return West;
                //    default:
                //        return null;
                //}
            }
        }

        public void Normalize()
        {
            var length = (float)Math.Sqrt((DeltaX * DeltaX) + (DeltaY * DeltaY));
            if (length != 0)
            {
                DeltaX /= length;
                DeltaY /= length;
            }
        }
    }
}
