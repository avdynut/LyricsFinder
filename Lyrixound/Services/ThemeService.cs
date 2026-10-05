using Lyrixound.Configuration;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Media;

namespace Lyrixound.Services;

public class ThemeService
{
    private static readonly Color LightPrimary = Color.FromRgb(0x67, 0x3A, 0xB7);
    private static readonly Color DarkPrimary = Color.FromRgb(0xB3, 0x88, 0xFF);

    private readonly Settings _settings;
    private readonly ILicenseService _licenseService;
    private bool? _appliedDark;

    public ThemeService(Settings settings, ILicenseService licenseService)
    {
        _settings = settings;
        _licenseService = licenseService;
        _licenseService.EntitlementChanged += (_, _) => Apply();
        Apply();
    }

    public event EventHandler ThemeChanged;

    public bool IsDarkThemeActive => _licenseService.IsPro && _settings.IsDarkTheme;

    public void SetDarkTheme(bool isDark)
    {
        _settings.IsDarkTheme = isDark;
        Apply();
    }

    public void ToggleDarkTheme() => SetDarkTheme(!IsDarkThemeActive);

    public void Apply()
    {
        var app = Application.Current;
        if (app == null)
        {
            return;
        }

        if (app.Dispatcher.CheckAccess())
        {
            ApplyCore();
        }
        else
        {
            app.Dispatcher.Invoke(ApplyCore);
        }
    }

    private void ApplyCore()
    {
        var dark = IsDarkThemeActive;
        if (_appliedDark == dark)
        {
            return;
        }

        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();
        theme.SetBaseTheme(dark ? BaseTheme.Dark : BaseTheme.Light);
        theme.SetPrimaryColor(dark ? DarkPrimary : LightPrimary);
        paletteHelper.SetTheme(theme);
        _appliedDark = dark;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
