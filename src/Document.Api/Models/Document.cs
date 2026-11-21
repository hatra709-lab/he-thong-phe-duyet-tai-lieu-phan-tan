
namespace Document.Api.Models;

public class Document
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int CreatedById { get; set; }

    public int ApproverId { get; set; }

    public string Status { get; set; } = "Draft";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
