using System;
using System.Collections.Generic;

namespace _Game.Infrastructure.Persistence
{
    [Serializable]
    public abstract class PersistentDataBase : IPersistentData
    {
        [NonSerialized] private int _revision;
        [NonSerialized] private int _savedRevision;

        public abstract string Key { get; }
        public int Revision => _revision;
        public bool IsDirty => _revision != _savedRevision;

        protected bool SetPersistentField<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            MarkDirty();
            return true;
        }

        protected void MarkDirty()
        {
            unchecked
            {
                _revision++;
            }
        }

        public void MarkLoaded()
        {
            _revision = 0;
            _savedRevision = 0;
        }

        public void MarkSaved(int revision)
        {
            _savedRevision = revision;
        }
    }
}
