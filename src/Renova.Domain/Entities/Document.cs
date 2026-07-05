namespace Renova.Domain.Entities;

public class Document : BaseTenantEntity
{
    public Guid PersonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public string? UploadedBy { get; set; }

    public Person Person { get; set; } = null!;
}
