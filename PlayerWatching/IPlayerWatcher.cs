namespace PlayerWatching
{
    public interface IPlayerWatcher : IPlayer
    {
        string DisplayName { get; }
        void Initialize();
        bool UpdateMediaInfo();
    }
}
