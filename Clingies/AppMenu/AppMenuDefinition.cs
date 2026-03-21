using System.Collections.Generic;

namespace Clingies.AppMenu;

internal sealed record AppMenuDefinition(
    string Id,
    string? Label = null,
    string? Tooltip = null,
    string? IconId = null,
    bool Enabled = true,
    bool Separator = false,
    IReadOnlyList<AppMenuDefinition>? Children = null);
