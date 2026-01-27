using JournalApp.Services.Interfaces;
using Microsoft.Maui;
using Microsoft.Maui.Storage;

namespace JournalApp.Services.Implementations
{
    public class ThemeService : IThemeService
    {
        private const string ThemeKey = "app_theme";

        public event Action<string>? ThemeChanged;

        public string GetTheme()
        {
            var theme = Preferences.Get(ThemeKey, "light");
            return theme == "dark" ? "dark" : "light";
        }

        public void SetTheme(string theme)
        {
            var safe = theme == "dark" ? "dark" : "light";
            Preferences.Set(ThemeKey, safe);
            ApplyAppTheme(safe);
            ThemeChanged?.Invoke(safe);
        }

        public void ApplyStoredTheme()
        {
            var saved = GetTheme();
            ApplyAppTheme(saved);
        }

        private static void ApplyAppTheme(string theme)
        {
            var target = theme == "dark" ? AppTheme.Dark : AppTheme.Light;
            if (Application.Current is not null)
            {
                Application.Current.UserAppTheme = target;
            }
        }
    }
}
