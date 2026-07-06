---
name: build-lyrixound
description: Build, run, debug, test, and package the Lyrixound WPF app and its sibling libraries. Use when the user asks to build, compile, run, debug, test, or package the solution, troubleshoots build/configuration errors, mentions ReleasePortable, MSIX, .wapproj, or asks where settings or logs live at runtime.
---

# Build / Run / Package Lyrixound

The solution targets .NET 10 on Windows and depends on Windows 10 SDK 10.0.17763 (for SMTC and WASAPI loopback). It will not build on Linux/macOS and won't fully run inside a sandboxed Windows container.

## Solution layout

Top-level file: `LyricsFinder.sln`. Projects of interest:

- `Lyrixound` - WinExe WPF app, the thing to run.
- `LyricsFinder.Core` - shared models, `netstandard2.0`.
- `Providers/LyricsProviders` + `.Demo` + `.Tests`.
- `Watchers/SmtcWatcher` + `Watchers/PlayerWatching` + `Watchers/Win10Watcher`.
- `ShazamIO` - audio recognition client.
- `LyrixoundPackaging` - MSIX `.wapproj`.

## Build configurations

| Configuration | Notes |
|---|---|
| `Debug` | Local dev, full PDBs, settings under `%LocalAppData%/Lyrixound/settings/`. |
| `Release` | Optimized installed build. Same settings location as Debug. |
| `ReleasePortable` | Defines `PORTABLE`. `App.xaml.cs` switches `_dataFolder` to `AppDomain.CurrentDomain.BaseDirectory`, so settings/log files live next to the exe - useful for USB/portable builds. |

When asked for "portable mode" always use `ReleasePortable`.

## Common commands

Run from the repo root in PowerShell.

**Restore + build everything:**
```powershell
dotnet build LyricsFinder.sln -c Debug
```

**Run the app (Debug):**
```powershell
dotnet run --project Lyrixound -c Debug
```

**Provider smoke test (no UI):**
```powershell
dotnet run --project Providers/LyricsProviders.Demo -c Debug
```

**SMTC console test:**
```powershell
dotnet run --project Watchers/SmtcWatcher.ConsoleTest -c Debug
```

**Tests:**
```powershell
dotnet test Providers/LyricsProviders.Tests
dotnet test Watchers/PlayerWatching.Tests
```

The lyrics-provider tests hit live APIs (LrcLib, LyricsOvh, Musixmatch, Google). Expect occasional flakes if a service is rate-limiting; that's not a regression unless multiple sources fail together.

**Portable Release build:**
```powershell
dotnet publish Lyrixound -c ReleasePortable -r win-x64 --self-contained false -o publish/portable
```

**MSIX package (requires Visual Studio / MSBuild with WAP workload):**
```powershell
msbuild LyrixoundPackaging\LyrixoundPackaging.wapproj /p:Configuration=Release /p:Platform=x64
```
`dotnet build` does not understand `.wapproj`; always shell out to `msbuild` for packaging.

## Runtime data locations

| Mode | Settings | Logs |
|---|---|---|
| Debug / Release | `%LocalAppData%\Lyrixound\settings\*.json` | per `Lyrixound/NLog.config` |
| ReleasePortable | `<exeDir>\settings\*.json` | next to the exe |

Settings files (one per registered `JsonSettings`): `app.json`, `directories_provider.json`, `lyrics.json`, `window.json`, `google_provider.json`. If a settings file is corrupt, `LoadSettings<T>` moves it to `<name>.json.bak` and recreates defaults - don't manually nuke files unless `.bak` exists.

## Troubleshooting

- **`TargetFrameworkVersion` errors on first build**: install the .NET 10 SDK and Windows 10 SDK 10.0.17763. Visual Studio 2022/2026 with the .NET desktop + UWP/Windows App workloads is the supported IDE.
- **`COMReference` failure in `LyricsProviders`**: `MSHTML.dll` is referenced via tlbimp; re-run `dotnet build` after closing any process that locked `obj/Debug/net10.0/Interop.MSHTML.dll`. Don't add a NuGet alternative - the Google provider relies on this.
- **No audio captured in `AudioRecognitionService`**: ensure something is actually playing on the default playback device. WASAPI loopback records what's mixed to the default endpoint; muted or silent apps produce empty buffers.
- **Rating reminder pops every launch in dev**: `Settings.LaunchCount % 3 == 0`. Set `DontShowRatingReminder = true` in `%LocalAppData%/Lyrixound/settings/app.json` while developing.
- **`Lyrixound.csproj.Backup.tmp` left over**: safe to delete; created by VS when migrating the project file.

## Don't

- Don't bump `TargetFramework` away from the pinned Windows 10 SDK (`net10.0-windows10.0.17763.0`) for `Lyrixound` / `SmtcWatcher` - SMTC APIs require that exact platform version.
- Don't change `LyricsFinder.Core` TFM from `netstandard2.0`; it's intentionally consumed by both legacy and net10 projects.
- Don't add per-project `nuget.config` or `Directory.Build.props` - the repo deliberately has none.
