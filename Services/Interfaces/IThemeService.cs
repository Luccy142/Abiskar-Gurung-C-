using System;

namespace JournalApp.Services.Interfaces
{
    public interface IThemeService
    {
        event Action<string>? ThemeChanged;

        string GetTheme();          // "light" or "dark"
        void SetTheme(string theme);
        void ApplyStoredTheme();
    }
}
