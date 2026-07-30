namespace _Game.Infrastructure.Persistence
{
    public interface IPersistentDataHandler
    {
        void Load(IPersistentData data);
        void Save(IPersistentData data);
        void Flush();
    }
}
