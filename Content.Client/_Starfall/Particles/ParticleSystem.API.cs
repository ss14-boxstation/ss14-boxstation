using System.Numerics;
using Content.Shared._Starfall.Particles;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// API for <see cref="ParticleSystem"/>.
/// Use these methods to create and remove particle effects from other systems.
/// </summary>
public sealed partial class ParticleSystem
{
    /// <summary>
    /// Spawns a particle effect at the map position of <paramref name="entity"/>,
    /// optionally attaching it so it follows the entity.
    /// </summary>
    /// <param name="effectId">The prototype ID of the effect to spawn.</param>
    /// <param name="entity">The entity to spawn particles on.</param>
    /// <param name="colorOverride">Optional color tint.</param>
    /// <param name="attach">When <c>true</c> (default), the emitter follows <paramref name="entity"/>.</param>
    /// <param name="overrides">Optional runtime overrides applied before the first burst (if any).</param>
    /// <param name="initialVelocity">
    /// Seeds the emitter's velocity before the first tick.
    /// Required for <see cref="ParticleEffectPrototype.InheritVelocity"/> to work on burst effects,
    /// since burst particles are emitted before any tick can compute velocity automatically.
    /// </param>
    /// <returns>The <see cref="ActiveEmitter"/> handle, or <c>null</c> if the effect could not be spawned.</returns>
    public ActiveEmitter? CreateParticle(
        ProtoId<ParticleEffectPrototype> effectId,
        EntityUid entity,
        Color? colorOverride = null,
        bool attach = true,
        ParticleRuntimeOverrides? overrides = null,
        Vector2? initialVelocity = null)
    {
        var coords = _transform.GetMapCoordinates(entity);
        return SpawnEffect(effectId, coords, attach ? entity : null, colorOverride, overrides, initialVelocity);
    }

    /// <summary>
    /// Spawns a particle effect at the given map coordinates.
    /// </summary>
    /// <param name="effectId">The prototype ID of the effect to spawn.</param>
    /// <param name="coords">World position to spawn at.</param>
    /// <param name="colorOverride">Optional color tint.</param>
    /// <param name="overrides">Optional runtime overrides applied before the first burst (if any).</param>
    /// <param name="initialVelocity">
    /// Seeds the emitter's velocity before the first tick.
    /// Required for <see cref="ParticleEffectPrototype.InheritVelocity"/> to work on burst effects,
    /// since burst particles are emitted before any tick can compute velocity automatically.
    /// </param>
    /// <returns>The <see cref="ActiveEmitter"/> handle, or <c>null</c> if the effect could not be spawned.</returns>
    public ActiveEmitter? CreateParticle(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        Color? colorOverride = null,
        ParticleRuntimeOverrides? overrides = null,
        Vector2? initialVelocity = null)
    {
        return SpawnEffect(effectId, coords, null, colorOverride, overrides, initialVelocity);
    }

    /// <summary>
    /// Stops and removes a particle emitter by its <see cref="ActiveEmitter"/> reference. Nullable.
    /// </summary>
    public void RemoveParticle(ActiveEmitter? emitter)
    {
        if (emitter != null)
            StopEffect(emitter);
    }

    /// <summary>
    /// Stops and removes a particle emitter by its numeric handle.
    /// </summary>
    public void RemoveParticle(uint handle)
    {
        StopEffect(handle);
    }

    /// <summary>Stops a running emitter, preventing new particles from being emitted. Existing particles live out their lifetime.</summary>
    public void StopEffect(uint handle)
    {
        if (handle == 0)
            return;
        if (_emittersByHandle.TryGetValue(handle, out var emitter))
            emitter.Exhausted = true;
    }

    /// <summary>Stops a running emitter by direct reference. Existing particles live out their lifetime.</summary>
    public static void StopEffect(ActiveEmitter emitter)
    {
        emitter.Exhausted = true;
    }

    /// <summary>Updates the intensity multiplier on a running emitter by handle.</summary>
    public void UpdateIntensity(uint handle, float intensity)
    {
        if (handle == 0)
            return;
        if (_emittersByHandle.TryGetValue(handle, out var emitter))
            emitter.Intensity = Math.Max(intensity, 0f);
    }

    /// <summary>Updates the intensity multiplier on a running emitter by direct reference.</summary>
    public static void UpdateIntensity(ActiveEmitter emitter, float intensity)
    {
        emitter.Intensity = Math.Max(intensity, 0f);
    }

    public IReadOnlyList<ActiveEmitter> GetEmitters()
    {
        return _emitters;
    }

    /// <summary>
    /// Immediately destroys all active emitters and kills every live particle.
    /// This is the nuclear option, use it when something has gone very wrong.
    /// </summary>
    /// <returns>Number of emitters that were cleared.</returns>
    public int KillAll()
    {
        var count = _emitters.Count;
        _emitters.Clear();
        _emittersByHandle.Clear();
        _liveParticleCount = 0;
        return count;
    }

    /// <summary>Spawns a particle effect at a given map coordinate.</summary>
    public ActiveEmitter? SpawnEffect(ProtoId<ParticleEffectPrototype> effectId, MapCoordinates coords, EntityUid? attachedEntity = null, Color? colorOverride = null, ParticleRuntimeOverrides? overrides = null, Vector2? initialVelocity = null)
    {
        return SpawnEffect(effectId,
            coords,
            depth: 0,
            attachedEntity: attachedEntity,
            colorOverride: colorOverride,
            overrides: overrides,
            initialVelocity: initialVelocity);
    }

    private ActiveEmitter? SpawnEffect(ProtoId<ParticleEffectPrototype> effectId, MapCoordinates coords, int depth, EntityUid? attachedEntity = null, Color? colorOverride = null, ParticleRuntimeOverrides? overrides = null, Vector2? initialVelocity = null)
    {
        if (coords.MapId == MapId.Nullspace || !_mapSystem.MapExists(coords.MapId))
            return null;

        if (depth > MaxSubEmitterDepth)
        {
            Log.Warning($"ParticleSystem: subemitter depth exceeded MaxSubEmitterDepth ({MaxSubEmitterDepth}). Dropping '{effectId}'. DO NOT RECUSIVELY STACK SUBEMITTERS.");
            return null;
        }

        if (!_protoManager.TryIndex(effectId, out var proto))
            return null;

        if (!ValidatePrototypeForSpawn(proto))
            return null;

        // Skip quality check if this is a gameplay-critical particle
        if (_quality == 0 && !proto.IgnoreQualitySettings)
            return null;

        // Even IgnoreQualitySettings effects are capped at 8 simultaneous emitters when quality is Off.
        if (_quality == 0 && proto.IgnoreQualitySettings)
        {
            var ignoreQualityEmitterCount = 0;
            foreach (var e in _emitters)
            {
                if (e.Proto.IgnoreQualitySettings)
                    ignoreQualityEmitterCount++;
            }
            if (ignoreQualityEmitterCount >= 8)
                return null;
        }

        var emitter = CreateEmitter(proto, coords, attachedEntity);
        emitter.ColorOverride = colorOverride;
        emitter.SubEmitterDepth = depth;

        if (overrides != null)
            ApplyOverrides(emitter, overrides);

        // Pre-seed velocity so burst emitters can use InheritVelocity correctly.
        if (initialVelocity.HasValue)
        {
            emitter.EmitterVelocity = initialVelocity.Value;
            emitter.PreviousPosition = coords.Position;
            emitter.VelocityInitialized = true;
        }

        // Add before BurstEmit so the live count is tracked correctly when EmitParticle runs.
        _emitters.Add(emitter);
        _emittersByHandle.Add(emitter.Handle, emitter);

        if (proto.Burst)
            BurstEmit(emitter);

        return emitter;
    }

    /// <summary>
    /// Patches runtime overrides on a live emitter by handle.
    /// Only non-null fields are applied, null fields are left unchanged.
    /// </summary>
    public void UpdateRuntime(uint handle, ParticleRuntimeOverrides overrides)
    {
        if (handle == 0)
            return;
        if (_emittersByHandle.TryGetValue(handle, out var emitter))
            ApplyOverrides(emitter, overrides);
    }

    /// <summary>
    /// Patches runtime overrides on a live emitter by direct reference.
    /// Use this when you already have the <see cref="ActiveEmitter"/> from <see cref="SpawnEffect"/>.
    /// </summary>
    public static void UpdateRuntime(ActiveEmitter emitter, ParticleRuntimeOverrides overrides)
    {
        ApplyOverrides(emitter, overrides);
    }

    private static void ApplyOverrides(ActiveEmitter emitter, ParticleRuntimeOverrides src)
    {
        emitter.Overrides ??= new ParticleRuntimeOverrides();
        var dst = emitter.Overrides;

        // ᓚᘏᗢ <( here lies the "UNHOLY IF STATEMENT BLOCK AND DESPAIR" you were not missed.
        // Anyway we override everything in this big block
        dst.StartColor = src.StartColor ?? dst.StartColor;
        dst.EndColor = src.EndColor ?? dst.EndColor;
        dst.ColorOverride = src.ColorOverride ?? dst.ColorOverride;
        dst.Shader = src.Shader ?? dst.Shader;
        dst.RenderLayer = src.RenderLayer ?? dst.RenderLayer;
        dst.ParticleSize = src.ParticleSize ?? dst.ParticleSize;
        dst.SizeVariance = src.SizeVariance ?? dst.SizeVariance;
        dst.StretchFactor = src.StretchFactor ?? dst.StretchFactor;
        dst.Lifetime = src.Lifetime ?? dst.Lifetime;
        dst.LifetimeVariance = src.LifetimeVariance ?? dst.LifetimeVariance;
        dst.Speed = src.Speed ?? dst.Speed;
        dst.SpeedVariance = src.SpeedVariance ?? dst.SpeedVariance;
        dst.ConstantForce = src.ConstantForce ?? dst.ConstantForce;
        dst.Gravity = src.Gravity ?? dst.Gravity;
        dst.Drag = src.Drag ?? dst.Drag;
        dst.TerminalSpeed = src.TerminalSpeed ?? dst.TerminalSpeed;
        dst.NoiseStrength = src.NoiseStrength ?? dst.NoiseStrength;
        dst.NoiseFrequency = src.NoiseFrequency ?? dst.NoiseFrequency;
        dst.InheritVelocity = src.InheritVelocity ?? dst.InheritVelocity;
        dst.StartRotation = src.StartRotation ?? dst.StartRotation;
        dst.StartRotationVariance = src.StartRotationVariance ?? dst.StartRotationVariance;
        dst.RotationSpeed = src.RotationSpeed ?? dst.RotationSpeed;
        dst.RotationSpeedVariance = src.RotationSpeedVariance ?? dst.RotationSpeedVariance;
        dst.EmissionRate = src.EmissionRate ?? dst.EmissionRate;
        dst.MaxCount = src.MaxCount ?? dst.MaxCount;
        dst.Duration = src.Duration ?? dst.Duration;
        dst.SpreadAngle = src.SpreadAngle ?? dst.SpreadAngle;
        dst.SpawnOffset = src.SpawnOffset ?? dst.SpawnOffset;

        if (src.EmitAngle is { } emitAngle)
        {
            dst.EmitAngle = emitAngle;
            if (emitter.TargetEntity == null && emitter.TargetPosition == null)
                emitter.EffectiveEmitAngle = (float)emitAngle.Theta;
        }
    }

    /// <summary>
    /// Spawns a particle effect whose emission direction tracks a target entity each tick.
    /// When the entity is deleted the emitter retains its last angle.
    /// </summary>
    public ActiveEmitter? SpawnEffectAimAt(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        EntityUid targetEntity,
        EntityUid? attachedEntity = null)
    {
        var emitter = SpawnEffect(effectId, coords, attachedEntity);
        if (emitter != null)
            emitter.TargetEntity = targetEntity;
        return emitter;
    }

    /// <summary>
    /// Spawns a particle effect whose emission direction points at a fixed world position.
    /// </summary>
    public ActiveEmitter? SpawnEffectAimAt(
        ProtoId<ParticleEffectPrototype> effectId,
        MapCoordinates coords,
        Vector2 targetWorldPosition,
        EntityUid? attachedEntity = null)
    {
        var emitter = SpawnEffect(effectId, coords, attachedEntity);
        if (emitter != null)
            emitter.TargetPosition = targetWorldPosition;
        return emitter;
    }
}
