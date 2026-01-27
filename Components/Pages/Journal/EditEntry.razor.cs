using Microsoft.AspNetCore.Components;
using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;
using Microsoft.JSInterop;

namespace JournalApp.Components.Pages.Journal
{
    public partial class EditEntry : ComponentBase
    {
        [Parameter] public int Id { get; set; }

        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected JournalEntry? Model { get; set; }
        protected HashSet<string> SecondaryMoodSet { get; set; } = new();
        
        protected bool IsLoading { get; set; } = true;
        protected bool IsSaving { get; set; } = false;
        protected string Message { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                if (Id > 0)
                {
                    Model = await Repo.GetByIdAsync(Id);
                    if (Model != null)
                    {
                        // Hydrate secondary moods
                        SecondaryMoodSet.Clear();
                        if (!string.IsNullOrWhiteSpace(Model.SecondaryMoods))
                        {
                            foreach (var m in Model.SecondaryMoods.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                SecondaryMoodSet.Add(m.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading entry: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [Inject] public IJSRuntime JS { get; set; } = default!;

        protected async Task InsertMarkdownAsync(string prefix, string suffix = "")
        {
            await JS.InvokeVoidAsync("window.editorHelper.wrapSelection", "markdown-editor", prefix, suffix);
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

        protected async Task UpdateAsync()
        {
            if (Model == null) return;

            Message = string.Empty;
            IsSaving = true;

            if (string.IsNullOrWhiteSpace(Model.Title))
            {
                Message = "Title is required.";
                IsSaving = false;
                return;
            }

            if (SecondaryMoodSet.Count > 2)
            {
                Message = "Too many moods selected.";
                IsSaving = false;
                return;
            }

            Model.SecondaryMoods = string.Join(", ", SecondaryMoodSet);
            Model.UpdatedAt = DateTime.Now;

            try
            {
                await Repo.SaveAsync(Model);
                Message = "Entry updated successfully!";
                
                // Optional: Navigate back after delay? 
                // For now, let user see message.
            }
            catch (Exception ex)
            {
                Message = "Error updating entry.";
                Console.WriteLine(ex);
            }
            IsSaving = false;
        }
    }
}
