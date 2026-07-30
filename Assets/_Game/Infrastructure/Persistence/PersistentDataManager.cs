using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace _Game.Infrastructure.Persistence
{
    public sealed class PersistentDataManager : IInitializable, ITickable, IDisposable
    {
        private readonly IReadOnlyList<IPersistentData> _allData;
        private readonly IPersistentDataHandler _handler;

        public PersistentDataManager(
            IEnumerable<IPersistentData> allData,
            IPersistentDataHandler handler)
        {
            _allData = new List<IPersistentData>(allData);
            _handler = handler;
        }

        public void Initialize()
        {
            foreach (var data in _allData)
            {
                _handler.Load(data);
            }
        }

        public void Tick()
        {
            SaveDirtyData();
        }

        public void Dispose()
        {
            SaveDirtyData();
        }

        private void SaveDirtyData()
        {
            var savedAnything = false;

            foreach (var data in _allData)
            {
                if (!data.IsDirty)
                {
                    continue;
                }

                var savedRevision = data.Revision;
                _handler.Save(data);
                data.MarkSaved(savedRevision);
                savedAnything = true;
            }

            if (savedAnything)
            {
                _handler.Flush();
            }
        }
    }
}
