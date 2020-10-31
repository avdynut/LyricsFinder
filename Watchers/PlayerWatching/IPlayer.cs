using LyricsFinder.Core;
using System;

namespace PlayerWatching
{
    public interface IPlayer : IDisposable
    {
        Track Track { get; }
        PlayerState PlayerState { get; }
    }
}
