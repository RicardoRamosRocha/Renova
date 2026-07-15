namespace Renova.Web.ViewModels;

public sealed class PremiumEmptyStateViewModel
{
    public string Icon { get; set; } = "ph ph-folder-open";

    public string Title { get; set; } = "Nenhum registro encontrado";

    public string Description { get; set; } = "Ajuste os filtros ou crie um novo registro para continuar.";

    public string? ActionText { get; set; }

    public string? ActionIcon { get; set; } = "ph ph-plus";

    public string? ActionUrl { get; set; }
}
