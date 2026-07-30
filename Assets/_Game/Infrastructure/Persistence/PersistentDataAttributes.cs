using System;

namespace _Game.Infrastructure.Persistence
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class PersistentDataAttribute : Attribute
    {
        public PersistentDataAttribute(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class PersistentFieldAttribute : Attribute
    {
        public PersistentFieldAttribute()
        {
        }

        public PersistentFieldAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; }
    }
}
