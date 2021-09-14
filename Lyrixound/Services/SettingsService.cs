using LyricsProviders.DirectoriesProvider;
using Lyrixound.Configuration;
using nucs.JsonSettings;
using nucs.JsonSettings.Autosave;
using System;
using System.IO;

namespace Lyrixound.Services;

public static class SettingsService
{
    public static string DataFolder { get; }
    public static Settings Settings { get; }
    public static DirectoriesProviderSettings DirectoriesSettings { get; }

    static SettingsService()
    {
#if PORTABLE
        DataFolder = AppDomain.CurrentDomain.BaseDirectory;
#else
        DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), App.AppName);
        Directory.CreateDirectory(DataFolder);
#endif

        Settings = LoadSettings<Settings>("app.json");
        DirectoriesSettings = LoadSettings<DirectoriesProviderSettings>("directories_provider.json");

        if (DirectoriesSettings.LyricsDirectories.Count == 0)
        {
            var lyricsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "lyrics");
            DirectoriesSettings.LyricsDirectories.Add(lyricsFolder);
            DirectoriesSettings.Save();
        }
    }

    public static T LoadSettings<T>(string filename) where T : JsonSettings
    {
        var filePath = Path.Combine(DataFolder, "settings", filename);
        return JsonSettings.Load<T>(filePath).EnableAutosave();
    }
}

