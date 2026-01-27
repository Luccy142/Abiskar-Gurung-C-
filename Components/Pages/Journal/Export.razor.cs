using Microsoft.AspNetCore.Components;
using JournalApp.Repositories.Interfaces;
using JournalApp.Services.Interfaces;
using JournalApp.Models.Entities;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Export : ComponentBase
    {
        [Inject] public IJournalEntryRepository Repo { get; set; } = default!;
        [Inject] public IPdfExportService PdfExport { get; set; } = default!;

        protected DateTime FromDate { get; set; } = DateTime.Today.AddDays(-7);
        protected DateTime ToDate { get; set; } = DateTime.Today;

        protected string FileName { get; set; } = "journal_export.pdf";
        protected string Message { get; set; } = string.Empty;
        protected string? ExportedPath { get; set; }

        protected async Task ExportAsync()
        {
            Message = string.Empty;
            ExportedPath = null;

            try
            {
                var from = FromDate.Date;
                var to = ToDate.Date;
                if (to < from) (from, to) = (to, from);

                var all = await Repo.GetAllAsync();
                var selected = all
                    .Where(e => e.EntryDate.Date >= from && e.EntryDate.Date <= to)
                    .OrderBy(e => e.EntryDate)
                    .ToList();

                if (selected.Count == 0)
                {
                    Message = "No entries found in this date range.";
                    return;
                }

                var path = await PdfExport.ExportAsync(selected, FileName);
                ExportedPath = path;
                Message = "PDF exported successfully.";
            }
            catch (Exception ex)
            {
                Message = "Export failed. Please try again.";
                Console.WriteLine(ex);
            }
        }

    }
}
