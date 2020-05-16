namespace PlayerWatching
{
    public interface IPlayerWatcher : IPlayer
    {
        string Name { get; }
        bool UpdateMediaInfo();
    }
}
