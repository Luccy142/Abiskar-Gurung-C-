using Microsoft.AspNetCore.Components;
using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Entries : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;

        protected List<JournalEntry> AllEntries { get; set; } = new();
        protected List<JournalEntry> PageEntries { get; set; } = new();
        protected string SearchText { get; set; } = string.Empty;

        protected int PageSize { get; set; } = 10;
        protected int CurrentPage { get; set; } = 1;

        protected int TotalPages { get; set; } = 1;
        protected DateTime? FromDate { get; set; }
        protected DateTime? ToDate { get; set; }
        protected JournalApp.Models.Enums.MoodType? MoodFilter { get; set; }
        protected string TagFilter { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            AllEntries = await Repo.GetAllAsync();
            ApplyPagination();
        }

        protected void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                ApplyPagination();
            }
        }

        protected void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                ApplyPagination();
            }
        }
        protected void OnSearchChanged(ChangeEventArgs e)
        {
            SearchText = e.Value?.ToString() ?? string.Empty;
            CurrentPage = 1;
            ApplyPagination();
        }
        protected void ApplyFilters()
        {
            CurrentPage = 1;
            ApplyPagination();
        }
        protected void OnTagFilterChanged(ChangeEventArgs e)
        {
            TagFilter = e.Value?.ToString() ?? string.Empty;
            CurrentPage = 1;
            ApplyPagination();
        }


        private void ApplyPagination()
        {
            IEnumerable<JournalEntry> filtered = AllEntries;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim().ToLower();

                filtered = filtered.Where(e =>
                    (e.Title ?? string.Empty).ToLower().Contains(q) ||
                    (e.Content ?? string.Empty).ToLower().Contains(q));
            }

            if (FromDate.HasValue)
            {
                filtered = filtered.Where(e => e.EntryDate.Date >= FromDate.Value.Date);
            }

            if (ToDate.HasValue)
            {
                filtered = filtered.Where(e => e.EntryDate.Date <= ToDate.Value.Date);
            }
            if (MoodFilter.HasValue)
            {
                filtered = filtered.Where(e => e.PrimaryMood == MoodFilter.Value);
            }
            if (!string.IsNullOrWhiteSpace(TagFilter))
            {
                var t = TagFilter.Trim().ToLower();
                filtered = filtered.Where(e => (e.Tags ?? string.Empty).ToLower().Contains(t));
            }

            // Execute query after ALL filters are applied
            var filteredList = filtered.OrderByDescending(e => e.EntryDate).ToList();

            TotalPages = filteredList.Count == 0 ? 1 : (int)Math.Ceiling(filteredList.Count / (double)PageSize);

            if (filteredList.Count == 0)
            {
                PageEntries = new List<JournalEntry>();
                return;
            }

            if (CurrentPage > TotalPages) CurrentPage = 1;

            PageEntries = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private string GetMoodEmoji(JournalApp.Models.Enums.MoodType mood)
        {
            return mood switch
            {
                JournalApp.Models.Enums.MoodType.Happy => "😃",
                JournalApp.Models.Enums.MoodType.Excited => "🤩",
                JournalApp.Models.Enums.MoodType.Relaxed => "😌",
                JournalApp.Models.Enums.MoodType.Grateful => "🥰",
                JournalApp.Models.Enums.MoodType.Confident => "😎",
                JournalApp.Models.Enums.MoodType.Calm => "🌿",
                JournalApp.Models.Enums.MoodType.Thoughtful => "🤔",
                JournalApp.Models.Enums.MoodType.Curious => "🧐",
                JournalApp.Models.Enums.MoodType.Nostalgic => "📻",
                JournalApp.Models.Enums.MoodType.Bored => "😑",
                JournalApp.Models.Enums.MoodType.Sad => "😢",
                JournalApp.Models.Enums.MoodType.Angry => "😠",
                JournalApp.Models.Enums.MoodType.Stressed => "😫",
                JournalApp.Models.Enums.MoodType.Lonely => "🥀",
                JournalApp.Models.Enums.MoodType.Anxious => "😰",
                _ => "🙂"
            };
        }


    
        [Inject] public Microsoft.JSInterop.IJSRuntime JS { get; set; } = default!;

        protected async Task ConfirmDelete(JournalEntry entry)
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", new object[] { $"Are you sure you want to delete '{entry.Title}'?" });
            if (confirmed)
            {
                await Repo.DeleteAsync(entry);
                // Refresh list
                AllEntries = await Repo.GetAllAsync();
                ApplyPagination();
            }
        }
    }
}
