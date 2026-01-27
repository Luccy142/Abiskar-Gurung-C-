using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IJournalEntryRepository _repo;

        public AnalyticsService(IJournalEntryRepository repo)
        {
            _repo = repo;
        }

        public async Task<Dictionary<string, int>> GetMoodCategoryDistributionAsync(DateTime from, DateTime to)
        {
            var (start, end) = Normalize(from, to);
            var entries = await GetRangeAsync(start, end);

            var result = new Dictionary<string, int>
            {
                ["Positive"] = 0,
                ["Neutral"] = 0,
                ["Negative"] = 0
            };

            foreach (var e in entries)
            {
                var mood = e.PrimaryMood.ToString();

                if (IsPositive(mood)) result["Positive"]++;
                else if (IsNeutral(mood)) result["Neutral"]++;
                else result["Negative"]++;
            }

            return result;
        }

        public async Task<string?> GetMostFrequentMoodAsync(DateTime from, DateTime to)
        {
            var (start, end) = Normalize(from, to);
            var entries = await GetRangeAsync(start, end);

            if (entries.Count == 0) return null;

            return entries
                .GroupBy(e => e.PrimaryMood.ToString())
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();
        }

        public async Task<Dictionary<string, int>> GetMostUsedTagsAsync(DateTime from, DateTime to, int top = 10)
        {
            var (start, end) = Normalize(from, to);
            var entries = await GetRangeAsync(start, end);

            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var e in entries)
            {
                var tags = (e.Tags ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                foreach (var t in tags)
                {
                    counts[t] = counts.TryGetValue(t, out var c) ? c + 1 : 1;
                }
            }

            return counts
                .OrderByDescending(kv => kv.Value)
                .Take(top)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public async Task<List<(DateTime date, int wordCount)>> GetWordCountTrendAsync(DateTime from, DateTime to)
        {
            var (start, end) = Normalize(from, to);
            var entries = await GetRangeAsync(start, end);

            return entries
                .OrderBy(e => e.EntryDate)
                .Select(e => (e.EntryDate.Date, CountWords(e.Content)))
                .ToList();
        }

        private async Task<List<JournalApp.Models.Entities.JournalEntry>> GetRangeAsync(DateTime start, DateTime end)
        {
            var all = await _repo.GetAllAsync();
            return all
                .Where(e => e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                .ToList();
        }

        private static (DateTime start, DateTime end) Normalize(DateTime from, DateTime to)
        {
            var start = from.Date;
            var end = to.Date;
            if (end < start) (start, end) = (end, start);
            return (start, end);
        }

        private static int CountWords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;

            return text
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Length;
        }

        // Scenario mood lists (based on your coursework sheet)
        private static bool IsPositive(string mood) =>
            mood is "Happy" or "Excited" or "Relaxed" or "Grateful" or "Confident";

        private static bool IsNeutral(string mood) =>
            mood is "Calm" or "Thoughtful" or "Curious" or "Nostalgic" or "Bored";
    }
}
