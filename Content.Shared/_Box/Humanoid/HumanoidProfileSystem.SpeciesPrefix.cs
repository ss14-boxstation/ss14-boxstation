// Ported from _Misfit, renamespaced to _Box
// TODO: make this take a loc string from the component
using Content.Shared._Box.Traits;

namespace Content.Shared.Humanoid;

public sealed partial class HumanoidProfileSystem
{
    public string GetPrefixRepresentation(EntityUid uid, string speciesText)
    {
        return TryComp<SpeciesPrefixComponent>(uid, out var prefix)
            ? Loc.GetString(prefix.ExaminePrefix, ("species", speciesText))
            : speciesText;
    }
}
