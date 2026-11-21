
namespace Document.Api.Models;

public class DocumentHistory
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int PerformedById { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
