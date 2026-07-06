---
name: add-lyrics-provider
description: Add a new lyrics source to the Lyrixound multi-provider fallback chain. Use when the user asks to add, integrate, or wire up a new lyrics provider (e.g. Genius, AZLyrics, Apple Music, a custom API), to expose a new lyrics backend in the app, or mentions ITrackInfoProvider.
---

# Add a Lyrics Provider

This skill walks through every file that must change to plug a new source into the `MultiTrackInfoProvider` fallback chain. Follow it end-to-end; skipping any step leaves the provider unreachable from the UI.

## Mental model

A provider is a small unit that takes a `TrackInfo` and returns a `Track` whose `Lyrics` is either a real `SyncedLyric`/`UnsyncedLyric` or a `NoneLyric(reason)`. The host (`App.xaml.cs`) registers it, lists it in default settings, and the user can re-order/disable it from the Settings window. The first provider in the user's order that returns non-empty lyrics wins.

## Checklist

```
- [ ] 1. Create the provider folder and class (Providers/LyricsProviders/<Name>/)
- [ ] 2. Add a settings class if the provider has user-configurable fields
- [ ] 3. Implement ITrackInfoProvider.FindTrackAsync
- [ ] 4. Register the provider type and settings in App.xaml.cs > RegisterTypes
- [ ] 5. Add the provider to _defaultProviders in Settings.cs
- [ ] 6. Add a unit test in Providers/LyricsProviders.Tests
- [ ] 7. Add an icon in Lyrixound/Icons (used by NameToIconUriConverter)
```

## Step 1: Folder and class

Create `Providers/LyricsProviders/<Name>/<Name>TrackInfoProvider.cs`. Mirror the layout of `LrcLib/` or `LyricsOvh/`. The class must look like:

```csharp
using LyricsFinder.Core;
using LyricsFinder.Core.LyricTypes;
using NLog;

namespace LyricsProviders.<Name>;

public class <Name>TrackInfoProvider : ITrackInfoProvider
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public const string Name = "<Name>";
    public string DisplayName => Name;

    public async Task<Track> FindTrackAsync(TrackInfo trackInfo)
    {
        var track = new Track(trackInfo);
        try
        {
            // ... call the API or scrape the source ...
            // track.Lyrics = new SyncedLyric(lrcText, SyncedLyricType.Lrc) { Source = new Uri(url) };
            // or:
            // track.Lyrics = new UnsyncedLyric(plainText) { Source = new Uri(url) };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching lyrics from <Name>");
            track.Lyrics = new NoneLyric(ex.Message);
        }

        if (track.Lyrics == null)
            track.Lyrics = new NoneLyric("No lyrics found");

        return track;
    }
}
```

Rules:
- Never throw out of `FindTrackAsync` - always return a `Track` with `NoneLyric(reason)` on failure.
- Prefer synced lyrics (`SyncedLyric` with LRC text). If the API returns synced + plain, return synced.
- Always set `Source` on the lyric to the page URL so the "open lyrics" button works.
- If the source can be detected as LRC by `LrcParser.IsLrcFormat(text)`, prefer `SyncedLyric`.

## Step 2: Settings (optional)

If the provider needs configuration (API key, locale, base URL), add `Providers/LyricsProviders/<Name>/<Name>ProviderSettings.cs`:

```csharp
using nucs.JsonSettings;

namespace LyricsProviders.<Name>;

public class <Name>ProviderSettings : JsonSettings
{
    public override string FileName { get; set; }
    public virtual string ApiKey { get; set; } = "";
}
```

All settable properties MUST be `virtual` (autosave proxy requirement).

## Step 3: HTTP client

If the provider hits an HTTP endpoint, follow the existing patterns:

- A static `<Name>API.cs` helper next to the provider (see `LrcLibAPI.cs`, `LyricsOvhAPI.cs`, `MusixmatchAPI.cs`) that exposes `static async Task<JsonDocument> Xxx(...)`.
- Use `System.Text.Json` to parse responses (Newtonsoft is reserved for `JsonSettings`).
- Reuse a single static `HttpClient` per API class; don't `new HttpClient()` per call.

## Step 4: Register in App.xaml.cs

In `Lyrixound/App.xaml.cs > RegisterTypes`, add a registration line in the existing block:

```csharp
containerRegistry
    .RegisterInstance(settings)
    .RegisterInstance(directoriesSettings)
    .RegisterInstance(LoadSettings<LyricsSettings>("lyrics.json"))
    .RegisterInstance(LoadSettings<WindowSettings>("window.json"))
    .RegisterInstance(LoadSettings<GoogleProviderSettings>("google_provider.json"))
    // 👇 add settings registration if you created a settings class
    .RegisterInstance(LoadSettings<<Name>ProviderSettings>("<name>_provider.json"))
    .Register<ITrackInfoProvider, DirectoriesTrackInfoProvider>(DirectoriesTrackInfoProvider.Name)
    .Register<ITrackInfoProvider, LrcLibTrackInfoProvider>(LrcLibTrackInfoProvider.Name)
    .Register<ITrackInfoProvider, MusixmatchTrackInfoProvider>(MusixmatchTrackInfoProvider.Name)
    .Register<ITrackInfoProvider, LyricsOvhTrackInfoProvider>(LyricsOvhTrackInfoProvider.Name)
    .Register<ITrackInfoProvider, GoogleTrackInfoProvider>(GoogleTrackInfoProvider.Name)
    // 👇 add the provider
    .Register<ITrackInfoProvider, <Name>TrackInfoProvider>(<Name>TrackInfoProvider.Name);
```

The key `<Name>TrackInfoProvider.Name` is what `Settings.LyricsProviders` looks up by string; keep it consistent everywhere.

## Step 5: Default ordering

In `Lyrixound/Configuration/Settings.cs`, add the provider to `_defaultProviders` at the position you want it to appear by default:

```csharp
private readonly List<Element> _defaultProviders =
[
    new Element(DirectoriesTrackInfoProvider.Name, isEnabled: true),
    new Element(LrcLibTrackInfoProvider.Name, isEnabled: true),
    new Element(MusixmatchTrackInfoProvider.Name, isEnabled: true),
    new Element(LyricsOvhTrackInfoProvider.Name, isEnabled: true),
    new Element(<Name>TrackInfoProvider.Name, isEnabled: true),
];
```

Existing users' `app.json` will be merged via `OnAfterLoad` (`Union` by `Element.Name`) so they get the new entry appended at the end.

## Step 6: Test

Add `Providers/LyricsProviders.Tests/<Name>TrackInfoProviderTests.cs`. Use the test tracks from `TestTrack.cs` and mirror the structure of `LrcLibTrackInfoProviderTests.cs`. Tests hit live APIs - that's the project convention; do not introduce mocking infrastructure just for one provider.

## Step 7: Icon

Drop a 16x16-ish PNG named exactly `<Name>.png` into `Lyrixound/Icons/`. The file is referenced at runtime by `NameToIconUriConverter` using `DisplayName`. `Icons/*.png` is already set to `CopyToOutputDirectory=Always` so no csproj change is needed.

## Verification

After implementing, sanity-check by running the demo CLI:

```powershell
dotnet run --project Providers/LyricsProviders.Demo
```

Then build and launch `Lyrixound` and confirm the provider appears in Settings → Providers and that searches route through it when it's first in the order.
