using System.Numerics;
using Content.Shared._Starfall.Particles;
using Robust.Shared.Map;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// Responsible for managing active particle emitters, including their creation, ticking, and particle emission.
/// </summary>
public sealed partial class ParticleSystem
{
    #region =^..^= Emitter Internals =^..^=

    /// <summary>
    /// Creates a new ActiveEmitter from a prototype and initial state.
    /// </summary>
    private ActiveEmitter CreateEmitter(ParticleEffectPrototype proto, MapCoordinates coords, EntityUid? attached)
    {
        var emitter = new ActiveEmitter
        {
            Proto = proto,
            MapCoords = coords,
            Coordinates = _transform.ToCoordinates(coords),
            AttachedEntity = attached,
            Handle = _nextHandle++,
            SpawnOffset = proto.SpawnOffset,
            Curves = GetCurveCache(proto),
            ResolvedPrototypeMaxCount = proto.MaxCount ?? CalculateMaxCount(proto),
        };
        ResolveFrames(emitter);

        emitter.EffectiveEmitAngle = (float)emitter.Proto.EmitAngle.Theta;
        ApplyEmitterRotation(emitter, (float)_eye.CurrentEye.Rotation);

        foreach (var _ in proto.Bursts)
            emitter.FiredBursts.Add(false);

        return emitter;
    }

    private void TickEmitter(ActiveEmitter emitter, float dt, float eyeAngle)
    {
        var proto = emitter.Proto;

        // Update attached entity position and track emitter velocity
        var newPos = emitter.MapCoords.Position;
        if (emitter.AttachedEntity is { } attachedEnt)
        {
            if (Deleted(attachedEnt))
            {
                emitter.Exhausted = true;
                emitter.AttachedEntity = null;
            }
            else
            {
                var attachedCoords = _transform.GetMapCoordinates(attachedEnt);
                if (attachedCoords.MapId == MapId.Nullspace)
                {
                    emitter.Exhausted = true;
                    emitter.AttachedEntity = null;
                    return;
                }

                newPos = attachedCoords.Position;
                emitter.MapCoords = attachedCoords; // update both position AND MapId
                emitter.Coordinates = _transform.ToCoordinates(attachedCoords);
            }
        }

        if (!emitter.VelocityInitialized)
        {
            emitter.PreviousPosition = newPos;
            emitter.VelocityInitialized = true;
        }

        if (dt > 0f)
            emitter.EmitterVelocity = (newPos - emitter.PreviousPosition) / dt;

        emitter.PreviousPosition = newPos;

        // Aim-at: recompute emit angle toward target each tick
        Vector2? targetWorldPos = null;
        if (emitter.TargetEntity is { } targetEnt)
        {
            if (!Deleted(targetEnt))
                targetWorldPos = _transform.GetMapCoordinates(targetEnt).Position;
            else
                emitter.TargetEntity = null; // entity GONE, fall to TargetPosition
        }
        if (targetWorldPos == null && emitter.TargetPosition.HasValue)
            targetWorldPos = emitter.TargetPosition.Value;

        if (targetWorldPos.HasValue)
        {
            var worldDir = targetWorldPos.Value - emitter.MapCoords.Position;
            if (worldDir.LengthSquared() > 0.0001f)
            {
                // Convert world direction to screen-space direction to angle (0 = screen-up)
                var screenDirection = RotateVector(worldDir, eyeAngle);
                emitter.EffectiveEmitAngle = MathF.Atan2(screenDirection.X, screenDirection.Y);
            }
        }
        else
        {
            // No target, keep in sync with the overridden emit angle if set, otherwise prototype default
            var baseAngle = emitter.Overrides?.EmitAngle ?? emitter.Proto.EmitAngle;
            emitter.EffectiveEmitAngle = (float)baseAngle.Theta;

            ApplyEmitterRotation(emitter, eyeAngle);
        }

        // Resolve overridable scalars once per tick
        // ᓚᘏᗢ <( look how pretty the formatting is :)
        var ovr          = emitter.Overrides;
        var drag         = ovr?.Drag          ?? proto.Drag;
        var constForce   = ovr?.ConstantForce ?? proto.ConstantForce;
        var termSpeed    = ovr?.TerminalSpeed ?? proto.TerminalSpeed;
        var gravity      = ovr?.Gravity       ?? proto.Gravity;
        var noiseStr     = ovr?.NoiseStrength ?? proto.NoiseStrength;
        var noiseFreq    = ovr?.NoiseFrequency ?? proto.NoiseFrequency;
        var duration     = (float)(ovr?.Duration      ?? proto.Duration).TotalSeconds;
        var emissionRate = ovr?.EmissionRate  ?? proto.EmissionRate;
        var maxCount     = ResolveMaxCount(emitter);

        if (proto.RotateWithEmitter && constForce != Vector2.Zero &&
            emitter.AttachedEntity is { } forceEnt && !Deleted(forceEnt))
        {
            // Directional effects define force in emitter-local space. Convert it to the
            // screen-space simulation coordinates; the draw path later applies the grid transform.
            var localForce = Transform(forceEnt).LocalRotation.RotateVec(constForce);
            constForce = RotateVector(localForce, eyeAngle);
        }

        // Precompute per-tick constants for SimulateParticle to avoid recomputing per particle.
        var dragMul     = drag > 0f ? MathF.Exp(-drag * dt) : 1f;
        var termSpeedSq = termSpeed > 0f ? termSpeed * termSpeed : float.MaxValue;

        // Advance age and check duration
        emitter.Age += TimeSpan.FromSeconds(dt);
        if (!emitter.Exhausted && duration > 0f && emitter.Age.TotalSeconds >= duration)
            emitter.Exhausted = true;

        // RSI animation
        if (emitter.Delays.Length > 0 && emitter.Frames.Length > 0)
        {
            emitter.AnimTimer += dt;
            while (emitter.AnimTimer >= emitter.Delays[emitter.AnimFrame])
            {
                var delay = emitter.Delays[emitter.AnimFrame];
                if (delay <= 0f)
                    break;
                emitter.AnimTimer -= delay;
                emitter.AnimFrame = (emitter.AnimFrame + 1) % emitter.Frames.Length;
            }
        }

        // Simulate live particles. Reverse iteration lets KillParticle remove in-place.
        for (var i = emitter.LiveParticles.Count - 1; i >= 0; i--)
        {
            var p = emitter.LiveParticles[i];
            p.Age += dt;
            p.UpdateCurveSample();

            if (p.Age >= p.Lifetime)
            {
                if (proto.SubEmitterOnDeath.HasValue)
                {
                    var worldPos = ComputeParticleWorldPos(p, emitter, eyeAngle);
                    _pendingSubEmitters.Add((proto.SubEmitterOnDeath.Value,
                        new MapCoordinates(worldPos, emitter.MapCoords.MapId),
                        emitter.SubEmitterDepth + 1));
                }

                KillParticle(emitter, i);
                continue;
            }

            SimulateParticle(p, dt, dragMul, constForce, termSpeed, termSpeedSq, gravity, noiseStr, noiseFreq, emitter.Curves);
        }

        // Timed bursts
        if (!emitter.Exhausted)
        {
            for (var b = 0; b < proto.Bursts.Count; b++)
            {
                if (emitter.FiredBursts[b])
                    continue;
                var burst = proto.Bursts[b];
                if (emitter.Age < burst.Time)
                    continue;

                var qualityMult = GetQualityMultiplier(proto);
                var scaledMax = GetScaledMaxCount(emitter, maxCount, qualityMult);
                var toEmit = (int)Math.Ceiling(burst.Count * qualityMult * emitter.Intensity);
                for (var j = 0; j < toEmit && _liveParticleCount < _globalBudget && emitter.LiveCount < scaledMax; j++)
                    EmitParticle(emitter, eyeAngle);
                emitter.FiredBursts[b] = true;
            }
        }

        // Continuous emission
        if (!emitter.Exhausted && !proto.Burst)
        {
            var qualityMult = GetQualityMultiplier(proto);
            var scaledMax = GetScaledMaxCount(emitter, maxCount, qualityMult);
            var available = _globalBudget - _liveParticleCount;
            var canEmit = Math.Min(scaledMax - emitter.LiveCount, available);
            if (canEmit > 0)
            {
                // EmissionOverTime rate multiplier
                var emissionMult = 1f;
                if (emitter.Curves.Emission is { } emissionCurve)
                {
                    var t = duration > 0f
                        ? Math.Clamp((float)(emitter.Age.TotalSeconds / duration), 0f, 1f)
                        : Math.Clamp((float)emitter.Age.TotalSeconds, 0f, 1f);
                    emissionMult = ParticleCurveCache.Sample(emissionCurve, t);
                }

                emitter.EmitAccum += emissionRate * emissionMult * dt * emitter.Intensity;
                var toEmit = (int)emitter.EmitAccum;
                emitter.EmitAccum -= toEmit;
                toEmit = Math.Min(toEmit, canEmit);

                for (var i = 0; i < toEmit; i++)
                    EmitParticle(emitter, eyeAngle);
            }
        }

        if (proto.Burst && !emitter.Exhausted)
            emitter.Exhausted = true;
    }

    private void BurstEmit(ActiveEmitter emitter)
    {
        var proto = emitter.Proto;
        var eyeAngle = (float)_eye.CurrentEye.Rotation;
        var qualityMultiplier = GetQualityMultiplier(proto);
        var requestedMax = ResolveMaxCount(emitter);
        var count = GetScaledMaxCount(emitter, requestedMax, qualityMultiplier);
        for (var i = 0; i < count && _liveParticleCount < _globalBudget; i++)
            EmitParticle(emitter, eyeAngle);
    }

    private void EmitParticle(ActiveEmitter emitter, float eyeAngle)
    {
        var proto = emitter.Proto;

        ParticleData p;
        if (emitter.FreePool.TryDequeue(out var pooled))
        {
            p = pooled;
            p.Reset();
        }
        else
        {
            p = new ParticleData();
        }

        // Resolve spawn time overridable fields
        _liveParticleCount++;
        var ovr = emitter.Overrides;
        var lifetime        = (float)(ovr?.Lifetime         ?? proto.Lifetime).TotalSeconds;
        var lifetimeVar     = (float)(ovr?.LifetimeVariance  ?? proto.LifetimeVariance).TotalSeconds;
        var spreadAngle     = (float)(ovr?.SpreadAngle?.Theta     ?? proto.SpreadAngle.Theta);
        var speed0          = ovr?.Speed             ?? proto.Speed;
        var speedVar        = ovr?.SpeedVariance     ?? proto.SpeedVariance;
        var sizeVar         = ovr?.SizeVariance      ?? proto.SizeVariance;
        var inheritVel      = ovr?.InheritVelocity   ?? proto.InheritVelocity;
        var startRot        = (float)(ovr?.StartRotation?.Theta         ?? proto.StartRotation.Theta);
        var startRotVar     = (float)(ovr?.StartRotationVariance?.Theta ?? proto.StartRotationVariance.Theta);
        var rotSpeed        = (float)(ovr?.RotationSpeed?.Theta         ?? proto.RotationSpeed.Theta);
        var rotSpeedVar     = (float)(ovr?.RotationSpeedVariance?.Theta ?? proto.RotationSpeedVariance.Theta);

        p.Lifetime = MathF.Max(lifetime + _random.NextFloat(-lifetimeVar, lifetimeVar), 0.05f);

        var spreadHalf = spreadAngle * 0.5f;
        var angle = emitter.EffectiveEmitAngle + _random.NextFloat(-spreadHalf, spreadHalf);

        var speed = speed0 + _random.NextFloat(-speedVar, speedVar);
        speed = Math.Max(speed, 0f);

        p.Velocity = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * speed;
        p.LocalOffset = SampleEmissionShape(proto.Shape);

        // Local effects carry their spawn offset in LocalOffset. Map/grid effects bake it into
        // their captured spawn origin below so it is not applied twice.
        var spawnOffset = emitter.Overrides?.SpawnOffset ?? emitter.SpawnOffset;
        var simulationSpace = proto.ResolvedSimulationSpace;
        if (simulationSpace == ParticleSimulationSpace.Local && spawnOffset != default)
        {
            p.LocalOffset += RotateVector(spawnOffset, eyeAngle);
        }

        // InheritVelocity: convert emitter world velocity to screen space then add
        if (inheritVel != 0f && emitter.EmitterVelocity != Vector2.Zero)
        {
            var wv = emitter.EmitterVelocity * inheritVel;
            p.Velocity += RotateVector(wv, eyeAngle);
        }

        if (simulationSpace != ParticleSimulationSpace.Local)
        {
            if (proto.RotateWithEmitter && emitter.AttachedEntity is { } offsetEnt && !Deleted(offsetEnt))
            {
                // SpawnOffset is local to the attached entity for directional effects. Because
                // these coordinates are relative to the grid, shuttle rotation moves the nozzle
                // position automatically on every subsequent draw.
                var localOffset = Transform(offsetEnt).LocalRotation.RotateVec(spawnOffset);
                p.SpawnCoordinates = new EntityCoordinates(
                    emitter.Coordinates.EntityId,
                    emitter.Coordinates.Position + localOffset);
            }
            else
            {
                var spawnCoords = new MapCoordinates(
                    emitter.MapCoords.Position + spawnOffset,
                    emitter.MapCoords.MapId);
                p.SpawnCoordinates = _transform.ToCoordinates(spawnCoords);
            }

            p.SpawnMapPosition = _transform.ToMapCoordinates(p.SpawnCoordinates).Position;
        }

        p.SpawnSpeed = speed;
        p.SpawnIntensity = emitter.Intensity;

        // SizeVariance
        if (sizeVar > 0f)
            p.SizeMultiplier = 1f + _random.NextFloat(-sizeVar, sizeVar);
        else
            p.SizeMultiplier = 1f;

        p.Rotation = startRot + _random.NextFloat(-startRotVar, startRotVar);
        p.RotationSpeed = rotSpeed + _random.NextFloat(-rotSpeedVar, rotSpeedVar);

        // Unique noise offset so each particle gets different turbulence
        p.NoiseOffset = new Vector2(_random.NextFloat(-100f, 100f), _random.NextFloat(-100f, 100f));

        emitter.LiveParticles.Add(p);

        // Sub-emitter on spawn
        if (proto.SubEmitterOnSpawn.HasValue)
        {
            var worldPos = ComputeParticleWorldPos(p, emitter, eyeAngle);
            _pendingSubEmitters.Add((proto.SubEmitterOnSpawn.Value,
                new MapCoordinates(worldPos, emitter.MapCoords.MapId),
                emitter.SubEmitterDepth + 1));
        }
    }

    #endregion
    private void RemoveEmitterAt(int index)
    {
        var emitter = _emitters[index];
        _emittersByHandle.Remove(emitter.Handle);
        _emitters.RemoveAt(index);
    }
    private float GetQualityMultiplier(ParticleEffectPrototype prototype)
    {
        if (prototype.IgnoreQualitySettings)
            return 1f;

        return QualityMultipliers[Math.Clamp(_quality, 0, QualityMultipliers.Length - 1)];
    }

    private int GetScaledMaxCount(ActiveEmitter emitter, int requestedMax, float qualityMultiplier)
    {
        var effectiveMax = emitter.Proto.IgnoreQualitySettings && _quality < 3
            ? Math.Min(requestedMax, IgnoreQualityMaxParticles)
            : requestedMax;
        return (int)Math.Ceiling(
            Math.Min(effectiveMax, HardMaxParticles) * qualityMultiplier * Math.Max(emitter.Intensity, 0f));
    }

    private static int ResolveMaxCount(ActiveEmitter emitter)
    {
        if (emitter.Overrides?.MaxCount is { } runtimeMax)
            return runtimeMax;
        return emitter.ResolvedPrototypeMaxCount;
    }

    private static int CalculateMaxCount(ParticleEffectPrototype prototype)
    {
        if (prototype.Burst)
            return DefaultBurstMaxCount;

        var lifetime = Math.Max(
            (prototype.Lifetime + prototype.LifetimeVariance.Duration()).TotalSeconds,
            0.05);

        var peakEmissionMultiplier = prototype.EmissionOverTime.Count == 0 ? 1f : 0f;
        foreach (var key in prototype.EmissionOverTime)
            peakEmissionMultiplier = Math.Max(peakEmissionMultiplier, key.Value);

        var continuousCount = (int)Math.Ceiling(
            Math.Max(prototype.EmissionRate, 0f) * peakEmissionMultiplier * lifetime);

        // Timed bursts can overlap both continuous particles and one another. Adding them all is
        // intentionally conservative; the global budget and hard ceiling still get the final say.
        long burstCount = 0;
        foreach (var burst in prototype.Bursts)
            burstCount += Math.Max(burst.Count, 0);
        
        return (int)Math.Clamp(continuousCount + burstCount, 1L, AutomaticMaxCount);
    }

    private void ApplyEmitterRotation(ActiveEmitter emitter, float eyeAngle)
    {
        if (!emitter.Proto.RotateWithEmitter ||
            emitter.AttachedEntity is not { } rotatingEnt ||
            Deleted(rotatingEnt))
            return;

        // Stay in grid-local space here; rendering applies the grid's live world rotation.
        var localDirection = Transform(rotatingEnt).LocalRotation.RotateVec(Vector2.UnitY);
        var screenDirection = RotateVector(localDirection, eyeAngle);
        emitter.EffectiveEmitAngle += MathF.Atan2(screenDirection.X, screenDirection.Y);
    }
}
