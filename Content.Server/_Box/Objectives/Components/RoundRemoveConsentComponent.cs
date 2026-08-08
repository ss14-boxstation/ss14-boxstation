//Modified from https://github.com/DeltaV-Station/Delta-v/blob/847f534ff6ae66de6d3fa1c51ba145379ae2a0e1/Content.Server/_DV/Objectives/Components/TargetObjectiveImmuneComponent.cs
namespace Content.Server._Box.Objectives.Components;

/// <summary>
/// Use this to mark a player as immune to any target objectives, useful for ghost roles or events.
/// </summary>
[RegisterComponent]
public sealed partial class RoundRemoveConsentComponent : Component;
