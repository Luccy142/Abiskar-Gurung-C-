using Microsoft.AspNetCore.Components;
using JournalApp.Services.Interfaces;

namespace JournalApp.Components.Pages.Settings
{
    public partial class Theme : ComponentBase
    {
        [Inject] public IThemeService ThemeService { get; set; } = default!;

        protected string SelectedTheme { get; set; } = "light";
        protected string Message { get; set; } = string.Empty;

        protected override void OnInitialized()
        {
            SelectedTheme = ThemeService.GetTheme();
        }

        protected void SaveTheme()
        {
            ThemeService.SetTheme(SelectedTheme);
            Message = "Theme saved. Restart app to fully apply.";
        }
    }
}
