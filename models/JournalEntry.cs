namespace JournalApp.Models;

public class JournalEntry
{
    public int Id { get; set; }
    public DateTime EntryDate { get; set; }   // one per day
    public string Title { get; set; }
    public string Content { get; set; }

    public string PrimaryMood { get; set; }
    public string? SecondaryMood1 { get; set; }
    public string? SecondaryMood2 { get; set; }

    public string Tags { get; set; } // comma separated

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
