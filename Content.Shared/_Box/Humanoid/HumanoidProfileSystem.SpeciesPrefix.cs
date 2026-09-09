// Ported from _Misfit, renamespaced to _Box
// TODO: make this take a loc string from the component
using Content.Shared._Box.Traits;

namespace Content.Shared.Humanoid;

public sealed partial class HumanoidProfileSystem
{
    public string GetSyntheticRepresentation(EntityUid uid, string speciesText)
    {
        return HasComp<SpeciesPrefixComponent>(uid)
            ? Loc.GetString("synthetic-component-examine", ("species", speciesText))
            : speciesText;
    }
}
