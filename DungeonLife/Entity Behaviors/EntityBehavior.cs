using System.ComponentModel;
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
}
