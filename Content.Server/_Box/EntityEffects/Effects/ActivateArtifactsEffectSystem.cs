using Content.Server.Xenoarchaeology.XenoArtifacts; // Box Change - imp - Duo XenoArch
using Content.Shared.EntityEffects;

namespace Content.Server._Box.EntityEffects.Effects;

/// <summary>
/// Force activate the current Natural Artifact node, like with Artifexium
/// </summary
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class ActivateArtifactEntityEffectSystem : EntityEffectSystem<ArtifactComponent, Shared._Impstation.EntityEffects.Effects.ActivateArtifact>
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;
    protected override void Effect(Entity<ArtifactComponent> entity, ref EntityEffectEvent<Shared._Impstation.EntityEffects.Effects.ActivateArtifact> args)
    {
        _artifact.TryActivateArtifact(entity, logMissing: false);
    }
}
