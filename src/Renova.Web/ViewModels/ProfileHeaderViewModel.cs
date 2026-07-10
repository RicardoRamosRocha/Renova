namespace Renova.Web.ViewModels;

public sealed class ProfileHeaderViewModel
{
    public string Name { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string? Status { get; set; }

    public string? SupportingText { get; set; }

    public string? PhotoUrl { get; set; }

    public string PhotoInputName { get; set; } = "Photo";

    public string RemovePhotoInputName { get; set; } = "RemovePhoto";

    public bool ShowPhotoActions { get; set; } = true;
}
