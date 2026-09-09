using Content.Shared._Box.Metabolism; // Box Change: Metabolism info
using Content.Shared.Metabolism; // Box Change: Metabolism info
using Robust.Shared.Prototypes; // Box Change: Metabolism info
using Robust.Shared.Serialization;

namespace Content.Shared.MedicalScanner;

/// <summary>
/// On interacting with an entity retrieves the entity UID for use with getting the current damage of the mob.
/// </summary>
[Serializable, NetSerializable]
public sealed class HealthAnalyzerScannedUserMessage : BoundUserInterfaceMessage
{
    public HealthAnalyzerUiState State;

    public HealthAnalyzerScannedUserMessage(HealthAnalyzerUiState state)
    {
        State = state;
    }
}

/// <summary>
/// Contains the current state of a health analyzer control. Used for the health analyzer and cryo pod.
/// </summary>
[Serializable, NetSerializable]
public struct HealthAnalyzerUiState
{
    public readonly NetEntity? TargetEntity;
    public float Temperature;
    public float BloodLevel;
    public bool? ScanMode;
    public bool? Bleeding;
    public bool? Unrevivable;
    // Start Box Change: Metabolism info
    public HashSet<ProtoId<MetabolismCategoryPrototype>> MetabolismCategories = [];
    public Dictionary<ProtoId<MetabolismStagePrototype>, HashSet<ProtoId<MetabolizerTypePrototype>>> MetabolismTypes = [];
    // End Box Change

    public HealthAnalyzerUiState() {}

    public HealthAnalyzerUiState(NetEntity? targetEntity, float temperature, float bloodLevel, bool? scanMode, bool? bleeding, bool? unrevivable, HashSet<ProtoId<MetabolismCategoryPrototype>> metabolismCategories, Dictionary<ProtoId<MetabolismStagePrototype>, HashSet<ProtoId<MetabolizerTypePrototype>>> metabolismTypes) // Box Change: Metabolism info
    {
        TargetEntity = targetEntity;
        Temperature = temperature;
        BloodLevel = bloodLevel;
        ScanMode = scanMode;
        Bleeding = bleeding;
        Unrevivable = unrevivable;
        // Start Box Change: Metabolism info
        MetabolismCategories = metabolismCategories;
        MetabolismTypes = metabolismTypes;
        // End Box Change
    }
}
