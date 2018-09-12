namespace Assets.GameLogic.Updater
{
    public interface IUpdater<T>
        where T : IUpdatable
    {
        void Register(T updatable);
        void Deregister(T updatable);
    }
}