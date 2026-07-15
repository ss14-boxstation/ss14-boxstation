using Content.Shared._Box.Acolyte.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.NPC.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Box.Acolyte.Components;

/// <summary>
/// Signifies that an entity is the acolyte chosen by a game rule
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AcolyteComponent : Component
{
    #region Actions

    [DataField]
    public EntProtoId<EntityTargetActionComponent> SoulHarvestAction = "ActionAcolyteSoulHarvest";

    [DataField, AutoNetworkedField]
    public EntityUid? SoulHarvestActionEntity;

    #endregion

    /// <summary>
    /// The popup that will happen when an Acolyte starts to harvest a soul.
    /// </summary>
    [DataField]
    public LocId SoulHarvestStartPopupText = "soul-harvest-popup-start";

    /// <summary>
    /// The popup that will happen when an Acolyte finishes harvesting a soul.
    /// </summary>
    [DataField]
    public LocId SoulHarvestEndPopupText = "soul-harvest-popup-end";
}

public sealed partial class AcolyteSoulHarvestActionEvent : EntityTargetActionEvent;
