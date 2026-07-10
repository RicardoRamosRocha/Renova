namespace Renova.Web.ViewModels;

public sealed class ProfileAvatarViewModel
{
    public string Name { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public string Size { get; set; } = "sm";
}
