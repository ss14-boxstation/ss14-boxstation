using Content.Shared._Starfall.Particles;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Utility;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// Checks if the particle effect prototype is valid for spawning and resolves sprite frames for particle effects.
/// </summary>
public sealed partial class ParticleSystem
{
    private bool ValidatePrototypeForSpawn(ParticleEffectPrototype prototype)
    {
        string? error = null;

        if (prototype.MaxCount <= 0)
            error = $"maxCount must be greater than zero (got {prototype.MaxCount})";
        else if (prototype.ParticleSize < 0f)
            error = $"particleSize cannot be negative (got {prototype.ParticleSize})";
        else if (prototype.Lifetime <= TimeSpan.Zero)
            error = $"lifetime must be greater than zero (got {prototype.Lifetime})";
        else if (prototype.EmissionRate < 0f)
            error = $"emissionRate cannot be negative (got {prototype.EmissionRate})";
        else if (!IsCurveValid(prototype.ColorOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.AlphaOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.SizeOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.SpeedOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.ForceOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.VelocityOverLifetime, static key => key.Time) ||
                 !IsCurveValid(prototype.EmissionOverTime, static key => key.Time))
            error = "curve keys must be sorted and have times between 0 and 1";

        if (error == null)
            return true;

        if (_invalidPrototypeWarnings.Add(prototype.ID))
            Log.Error($"Particle effect '{prototype.ID}' is invalid: {error}.");
        return false;
    }

    private static bool IsCurveValid<T>(IReadOnlyList<T> curve, Func<T, float> getTime)
    {
        var previous = 0f;
        for (var i = 0; i < curve.Count; i++)
        {
            var time = getTime(curve[i]);
            if (time is < 0f or > 1f || i > 0 && time < previous)
                return false;
            previous = time;
        }

        return true;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<ShaderPrototype>())
            _overlay.ClearShaderCache();

        if (!args.TryGetModified<ParticleEffectPrototype>(out var modified))
            return;

        foreach (var id in modified)
        {
            _frameCache.Remove(id);
            _curveCache.Remove(id);
            _frameResolveFailures.Remove(id);
            _invalidPrototypeWarnings.Remove(id);
        }

        foreach (var emitter in _emitters)
        {
            if (!modified.Contains(emitter.Proto.ID))
                continue;

            if (!_protoManager.TryIndex<ParticleEffectPrototype>(emitter.Proto.ID, out var prototype))
            {
                // Let already-spawned particles finish, but do not keep emitting from a prototype
                // that vanished underneath us. Hot reload is spooky enough without ghost emitters. =^..^=
                emitter.Exhausted = true;
                continue;
            }

            if (!ValidatePrototypeForSpawn(prototype))
            {
                emitter.Exhausted = true;
                continue;
            }

            emitter.Proto = prototype;
            emitter.Curves = GetCurveCache(prototype);
            emitter.Frames = [];
            emitter.Delays = [];
            emitter.AnimFrame = 0;
            emitter.AnimTimer = 0f;
            ResolveFrames(emitter);

            while (emitter.FiredBursts.Count < prototype.Bursts.Count)
                emitter.FiredBursts.Add(false);
            if (emitter.FiredBursts.Count > prototype.Bursts.Count)
                emitter.FiredBursts.RemoveRange(prototype.Bursts.Count, emitter.FiredBursts.Count - prototype.Bursts.Count);
        }
    }

    /// <summary>
    /// Advances a single live particle's simulation by one step.
    /// </summary>
    private void ResolveFrames(ActiveEmitter emitter)
    {
        var protoId = emitter.Proto.ID;

        if (_frameCache.TryGetValue(protoId, out var cached))
        {
            emitter.Frames = cached.Frames;
            emitter.Delays = cached.Delays;
            return;
        }

        if (_frameResolveFailures.Contains(protoId))
            return;

        Texture[] frames;
        float[] delays = [];

        switch (emitter.Proto.Sprite)
        {
            case SpriteSpecifier.Rsi rsi:
            {
                RSI? resource;
                try
                {
                    var path = rsi.RsiPath.IsRooted
                        ? rsi.RsiPath
                        : SpriteSpecifierSerializer.TextureRoot / rsi.RsiPath;
                    resource = _resourceCache.GetResource<RSIResource>(path).RSI;
                }
                catch (Exception exception)
                {
                    MarkFrameResolutionFailed(protoId, exception.Message);
                    return;
                }

                if (!resource.TryGetState(rsi.RsiState, out var state))
                {
                    MarkFrameResolutionFailed(protoId, $"RSI state '{rsi.RsiState}' does not exist");
                    return;
                }

                frames = state.GetFrames(RsiDirection.South);
                delays = state.GetDelays();
                break;
            }
            case SpriteSpecifier.Texture tex:
            {
                try { frames = [_spriteSystem.Frame0(tex)]; }
                catch (Exception exception)
                {
                    MarkFrameResolutionFailed(protoId, exception.Message);
                    return;
                }
                break;
            }
            default:
                MarkFrameResolutionFailed(protoId, $"unsupported sprite specifier '{emitter.Proto.Sprite.GetType().Name}'");
                return;
        }

        _frameCache[protoId] = (frames, delays);
        emitter.Frames = frames;
        emitter.Delays = delays;
    }

    private void MarkFrameResolutionFailed(string prototype, string reason)
    {
        if (_frameResolveFailures.Add(prototype))
            Log.Warning($"Particle effect '{prototype}' could not resolve its sprite: {reason}");
    }
}
