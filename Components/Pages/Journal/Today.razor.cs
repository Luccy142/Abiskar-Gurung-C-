using Microsoft.AspNetCore.Components;
using JournalApp.Models.Entities;
using JournalApp.Services.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Today : ComponentBase
    {
        [Inject] public IJournalEntryService JournalService { get; set; } = default!;
        protected HashSet<string> SecondaryMoodSet { get; set; } = new HashSet<string>();

        protected JournalEntry Model { get; set; } = new JournalEntry();
        protected bool HasEntry { get; set; }
        protected string Message { get; set; } = string.Empty;
        protected bool IsSaving { get; set; } = false;

        protected bool CanSave =>
            !string.IsNullOrWhiteSpace(Model.Title) &&
            !string.IsNullOrWhiteSpace(Model.Content);

        protected void InsertMarkdown(string snippet)
        {
            Model.Content ??= string.Empty;

            if (!string.IsNullOrWhiteSpace(Model.Content))
                Model.Content += "\n";

            Model.Content += snippet;
        }

        protected override void OnInitialized()
        {
            // Always start fresh for "Today's Journal" as requested.
            // User can write multiple entries per day.
            Model = new JournalEntry 
            { 
                EntryDate = DateTime.Today 
            };
            SecondaryMoodSet.Clear();
            HasEntry = false;
        }
        protected void OnSecondaryMoodChanged(string mood, bool isChecked)
        {
            if (isChecked)
            {
                if (SecondaryMoodSet.Count >= 2)
                {
                    Message = "Only 2 secondary moods allowed.";
                    return;
                }

                SecondaryMoodSet.Add(mood);
            }
            else
            {
                SecondaryMoodSet.Remove(mood);
            }
        }



        protected async Task SaveAsync()
        {
            Message = string.Empty;
            IsSaving = true;

            if (string.IsNullOrWhiteSpace(Model.Title))
            {
                Message = "Title is required.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Model.Content))
            {
                Message = "Content cannot be empty.";
                return;
            }

            if (SecondaryMoodSet.Count > 2)
            {
                Message = "You can select up to 2 secondary moods only.";
                return;
            }

            Model.SecondaryMoods = string.Join(", ", SecondaryMoodSet);


            try
            {
                var saved = await JournalService.CreateOrUpdateTodayAsync(Model);
                // Reset for next entry as requested
                Model = new JournalEntry { EntryDate = DateTime.Today };
                HasEntry = false;
                SecondaryMoodSet.Clear();
                
                Message = "Saved! You can now write another entry.";
            }
            catch (Exception ex)
            {
                Message = "Something went wrong while saving. Please try again.";
                Console.WriteLine(ex);
            }
            IsSaving = false;
        }

        // Delete functionality moved to Journal Entries list


    }
}
