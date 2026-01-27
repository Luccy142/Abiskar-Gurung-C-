using JournalApp.Models.Entities;
using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;

namespace JournalApp.Services.Implementations
{
    public class JournalEntryService : IJournalEntryService
    {
        private readonly IJournalEntryRepository _repo;

        public JournalEntryService(IJournalEntryRepository repo)
        {
            _repo = repo;
        }

        public async Task<JournalEntry?> GetTodayEntryAsync()
        {
            return await _repo.GetByDateAsync(DateTime.Today);
        }

        public async Task<JournalEntry> CreateOrUpdateTodayAsync(JournalEntry input)
        {
            var today = DateTime.Today;

            // If entry handles its own identity (ID > 0), check DB for that specific entry
            if (input.Id > 0)
            {
                var existingById = await _repo.GetByIdAsync(input.Id);
                if (existingById != null)
                {
                    existingById.Title = input.Title?.Trim() ?? string.Empty;
                    existingById.Content = input.Content ?? string.Empty;
                    existingById.PrimaryMood = input.PrimaryMood;
                    existingById.SecondaryMoods = input.SecondaryMoods;
                    existingById.Category = input.Category;
                    existingById.Tags = input.Tags;
                    existingById.UpdatedAt = DateTime.Now;

                    await _repo.SaveAsync(existingById);
                    return existingById;
                }
            }
            
            // Otherwise, check if there's already an entry for today solely to support "Resume Draft" behavior
            // BUT if the input is explicitly intended as "New" (via UI reset), we might want to bypass this.
            // However, to satisfy "Save & Reset" -> "Enter Another", "Reset" clears ID.
            // So if ID is 0, we should probably Create New. 
            // The catch: OnInitialized loads "GetByDate". If we have 5, it loads one.
            // For now, let's treat ID=0 as "Create New". *Unless* we want to enforce singleton.
            // Given the user request, we explicitly WANT multiple.
            
            var newEntry = new JournalEntry
            {
                EntryDate = today,
                Title = input.Title?.Trim() ?? string.Empty,
                Content = input.Content ?? string.Empty,
                PrimaryMood = input.PrimaryMood,
                SecondaryMoods = input.SecondaryMoods,
                Category = input.Category,
                Tags = input.Tags,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _repo.SaveAsync(newEntry);
            return newEntry;
        }

        public async Task<bool> DeleteTodayAsync()
        {
            var existing = await _repo.GetByDateAsync(DateTime.Today);
            if (existing == null) return false;

            await _repo.DeleteAsync(existing);
            return true;
        }
    }
}
