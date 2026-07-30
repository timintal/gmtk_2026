using _Game.Infrastructure.Persistence;
using NUnit.Framework;

namespace Code.Infrastructure.Persistence.Tests
{
    public sealed class PersistentDataTests
    {
        [Test]
        public void GeneratedPropertyMarksDataDirtyOnlyWhenValueChanges()
        {
            var data = new SettingsData();

            Assert.That(data.IsDirty, Is.False);

            data.MusicVolume = 0.2f;
            Assert.That(data.IsDirty, Is.False);

            data.MusicVolume = 0.5f;
            Assert.That(data.IsDirty, Is.True);
            Assert.That(data.Revision, Is.EqualTo(1));

            data.MarkSaved(data.Revision);
            Assert.That(data.IsDirty, Is.False);

            data.MusicVolume = 0.5f;
            Assert.That(data.IsDirty, Is.False);

            data.MusicVolume = 0.75f;
            Assert.That(data.IsDirty, Is.True);
            Assert.That(data.Revision, Is.EqualTo(2));
        }

        [Test]
        public void ManagerSavesDirtyDataAndFlushesOnce()
        {
            var data = new SettingsData();
            var handler = new RecordingHandler();
            var manager = new PersistentDataManager(
                new IPersistentData[] { data },
                handler);

            manager.Initialize();
            data.SfxVolume = 0.25f;
            manager.Tick();

            Assert.That(handler.LoadCount, Is.EqualTo(1));
            Assert.That(handler.SaveCount, Is.EqualTo(1));
            Assert.That(handler.FlushCount, Is.EqualTo(1));
            Assert.That(data.IsDirty, Is.False);

            manager.Tick();

            Assert.That(handler.SaveCount, Is.EqualTo(1));
            Assert.That(handler.FlushCount, Is.EqualTo(1));
        }

        private sealed class RecordingHandler : IPersistentDataHandler
        {
            public int LoadCount { get; private set; }
            public int SaveCount { get; private set; }
            public int FlushCount { get; private set; }

            public void Load(IPersistentData data)
            {
                LoadCount++;
                data.MarkLoaded();
            }

            public void Save(IPersistentData data)
            {
                SaveCount++;
            }

            public void Flush()
            {
                FlushCount++;
            }
        }
    }
}
