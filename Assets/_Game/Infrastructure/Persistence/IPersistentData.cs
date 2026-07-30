namespace _Game.Infrastructure.Persistence
{
    public interface IPersistentData
    {
        string Key { get; }
        int Revision { get; }
        bool IsDirty { get; }

        void MarkLoaded();
        void MarkSaved(int revision);
    }
}
