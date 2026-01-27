using Microsoft.AspNetCore.Components;
using JournalApp.Services.Interfaces;

namespace JournalApp.Components.Pages.Analytics
{
    public partial class Dashboard : ComponentBase
    {
        [Inject] public IStreakService StreakService { get; set; } = default!;
        [Inject] public IAnalyticsService Analytics { get; set; } = default!;

        protected int CurrentStreak { get; set; }
        protected int LongestStreak { get; set; }

        protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-30);
        protected DateTime ToDate { get; set; } = DateTime.Today;

        protected List<DateTime> MissedDays { get; set; } = new();
        protected Dictionary<string, int> MoodDist { get; set; } = new();
        protected string? MostFrequentMood { get; set; }
        protected Dictionary<string, int> TopTags { get; set; } = new();
        protected List<(DateTime date, int wordCount)> WordTrend { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        protected async Task LoadAsync()
        {
            CurrentStreak = await StreakService.GetCurrentStreakAsync();
            LongestStreak = await StreakService.GetLongestStreakAsync();
            MissedDays = await StreakService.GetMissedDaysAsync(FromDate, ToDate);
            MoodDist = await Analytics.GetMoodCategoryDistributionAsync(FromDate, ToDate);
            MostFrequentMood = await Analytics.GetMostFrequentMoodAsync(FromDate, ToDate);
            TopTags = await Analytics.GetMostUsedTagsAsync(FromDate, ToDate, 10);
            WordTrend = await Analytics.GetWordCountTrendAsync(FromDate, ToDate);


        }
    }
}
