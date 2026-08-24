using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace LyricsProviders.Tests;

[TestClass]
public class MultiTrackInfoProviderTests
{
    private static readonly TrackInfo TrackInfo = new() { Artist = "Artist", Title = "Title" };

    [TestMethod]
    public async Task SyncedLaterBeatsUnsyncedEarlier()
    {
        var unsynced = new FakeProvider("Unsynced", new UnsyncedLyric("plain"));
        var synced = new FakeProvider("Synced", new SyncedLyric("[00:01.00]line", SyncedLyricType.Lrc));
        var unused = new FakeProvider("Unused", new UnsyncedLyric("later"));

        var multi = new MultiTrackInfoProvider([unsynced, synced, unused]);
        var track = await multi.FindTrackAsync(TrackInfo);

        Assert.IsInstanceOfType(track.Lyrics, typeof(SyncedLyric));
        Assert.AreEqual("Synced", multi.CurrentProvider.DisplayName);
        Assert.IsTrue(unsynced.WasCalled);
        Assert.IsTrue(synced.WasCalled);
        Assert.IsFalse(unused.WasCalled);
    }

    [TestMethod]
    public async Task FirstSyncedWinsWithoutCallingLaterProviders()
    {
        var synced = new FakeProvider("Synced", new SyncedLyric("[00:01.00]line", SyncedLyricType.Lrc));
        var later = new FakeProvider("Later", new SyncedLyric("[00:02.00]other", SyncedLyricType.Lrc));

        var multi = new MultiTrackInfoProvider([synced, later]);
        var track = await multi.FindTrackAsync(TrackInfo);

        Assert.IsInstanceOfType(track.Lyrics, typeof(SyncedLyric));
        Assert.AreEqual("[00:01.00]line", track.Lyrics.Text);
        Assert.AreEqual("Synced", multi.CurrentProvider.DisplayName);
        Assert.IsTrue(synced.WasCalled);
        Assert.IsFalse(later.WasCalled);
    }

    [TestMethod]
    public async Task OnlyUnsyncedReturnsFirstUnsynced()
    {
        var first = new FakeProvider("First", new UnsyncedLyric("first"));
        var second = new FakeProvider("Second", new UnsyncedLyric("second"));

        var multi = new MultiTrackInfoProvider([first, second]);
        var track = await multi.FindTrackAsync(TrackInfo);

        Assert.IsInstanceOfType(track.Lyrics, typeof(UnsyncedLyric));
        Assert.AreEqual("first", track.Lyrics.Text);
        Assert.AreEqual("First", multi.CurrentProvider.DisplayName);
        Assert.IsTrue(first.WasCalled);
        Assert.IsTrue(second.WasCalled);
    }

    [TestMethod]
    public async Task AllNoneReturnsNoneLyric()
    {
        var first = new FakeProvider("First", new NoneLyric("missing"));
        var second = new FakeProvider("Second", new NoneLyric("also missing"));

        var multi = new MultiTrackInfoProvider([first, second]);
        var track = await multi.FindTrackAsync(TrackInfo);

        Assert.IsInstanceOfType(track.Lyrics, typeof(NoneLyric));
        Assert.AreEqual("Lyrics not found", ((NoneLyric)track.Lyrics).Error);
        Assert.IsNull(multi.CurrentProvider);
        Assert.IsTrue(first.WasCalled);
        Assert.IsTrue(second.WasCalled);
    }

    [TestMethod]
    public async Task NoneThenUnsyncedThenSyncedPrefersSynced()
    {
        var none = new FakeProvider("None", new NoneLyric("missing"));
        var unsynced = new FakeProvider("Unsynced", new UnsyncedLyric("plain"));
        var synced = new FakeProvider("Synced", new SyncedLyric("[00:01.00]line", SyncedLyricType.Lrc));

        var multi = new MultiTrackInfoProvider([none, unsynced, synced]);
        var track = await multi.FindTrackAsync(TrackInfo);

        Assert.IsInstanceOfType(track.Lyrics, typeof(SyncedLyric));
        Assert.AreEqual("Synced", multi.CurrentProvider.DisplayName);
    }

    private sealed class FakeProvider : ITrackInfoProvider
    {
        private readonly ILyric _lyrics;

        public string DisplayName { get; }
        public bool WasCalled { get; private set; }

        public FakeProvider(string displayName, ILyric lyrics)
        {
            DisplayName = displayName;
            _lyrics = lyrics;
        }

        public Task<Track> FindTrackAsync(TrackInfo trackInfo)
        {
            WasCalled = true;
            return Task.FromResult(new Track(trackInfo) { Lyrics = _lyrics });
        }
    }
}
