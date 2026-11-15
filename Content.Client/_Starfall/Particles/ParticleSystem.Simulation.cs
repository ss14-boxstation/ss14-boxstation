using System.Numerics;
using Content.Shared._Starfall.Particles;
using Robust.Shared.Map;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// Simulates particle motion and handles particle lifecycle management.
/// </summary>
public sealed partial class ParticleSystem
{
    #region =^..^= Particle Simulation =^..^=
    private static Vector2 RotateVector(Vector2 vector, float radians)
    {
        var cos = MathF.Cos(radians);
        var sin = MathF.Sin(radians);
        return new Vector2(
            vector.X * cos - vector.Y * sin,
            vector.X * sin + vector.Y * cos);
    }
    private static void SimulateParticle(
        ParticleData p,
        float dt,
        float dragMul,
        Vector2 constForce,
        float termSpeed,
        float termSpeedSq,
        float gravity,
        float noiseStr,
        float noiseFreq,
        ParticleCurveCache curves)
    {
        var ageRatio = p.AgeRatio;

        // Drag: dragMul is MathF.Exp(-drag * dt) precomputed per tick
        if (dragMul < 1f)
            p.Velocity *= dragMul;

        // ConstantForce
        if (constForce != Vector2.Zero)
            p.Velocity += constForce * dt;

        // ForceOverLifetime
        if (curves.Force is { } forceCurve)
            p.Velocity += ParticleCurveCache.Sample(forceCurve, p.CurveIndex, p.CurveLerp) * dt;

        // SpeedOverLifetime: rescale velocity magnitude to the curve-defined speed
        if (curves.Speed is { } speedCurve)
        {
            var curveSpeed = ParticleCurveCache.Sample(speedCurve, p.CurveIndex, p.CurveLerp) * p.SpawnSpeed;
            var currentSpeed = p.Velocity.Length();
            if (currentSpeed > 0f)
                p.Velocity = p.Velocity / currentSpeed * curveSpeed;
        }

        // Terminal speed cap: termSpeedSq is termSpeed*termSpeed precomputed per tick
        if (termSpeedSq < float.MaxValue)
        {
            var speedSq = p.Velocity.LengthSquared();
            if (speedSq > termSpeedSq)
                p.Velocity *= termSpeed / MathF.Sqrt(speedSq);
        }

        // Advance position
        p.LocalOffset += p.Velocity * dt;

        // VelocityOverLifetime: positional nudge (does not modify velocity)
        if (curves.Velocity is { } velocityCurve)
            p.LocalOffset += ParticleCurveCache.Sample(velocityCurve, p.CurveIndex, p.CurveLerp) * dt;

        // Gravity
        if (gravity != 0f)
            p.LocalOffset.Y += -gravity * dt * ageRatio;

        // Noise
        if (noiseStr > 0f)
        {
            var ageSec = p.Age;
            var nx = ValueNoise(p.NoiseOffset.X + ageSec * noiseFreq, p.NoiseOffset.Y);
            var ny = ValueNoise(p.NoiseOffset.X, p.NoiseOffset.Y + ageSec * noiseFreq);
            p.LocalOffset += new Vector2(nx, ny) * noiseStr * dt;
        }

        // SPIN!!!!
        if (p.RotationSpeed != 0f)
            p.Rotation += p.RotationSpeed * dt;
    }

    /// <summary>Converts a particle's screen-space LocalOffset to a world position.</summary>
    private Vector2 ComputeParticleWorldPos(ParticleData p, ActiveEmitter emitter, float eyeAngle)
    {
        var worldOffset = RotateVector(p.LocalOffset, -eyeAngle);
        switch (emitter.Proto.ResolvedSimulationSpace)
        {
            case ParticleSimulationSpace.Map:
                return p.SpawnMapPosition + worldOffset;
            case ParticleSimulationSpace.Grid:
                var particleCoords = new EntityCoordinates(
                    p.SpawnCoordinates.EntityId,
                    p.SpawnCoordinates.Position + worldOffset);
                return Deleted(particleCoords.EntityId)
                    ? emitter.MapCoords.Position
                    : _transform.ToMapCoordinates(particleCoords).Position;
            default:
                return emitter.MapCoords.Position + worldOffset;
        }
    }

    /// <summary>
    /// Ages particles on off-screen emitters without running full simulation.
    /// Kills expired particles and decrements the live count.
    /// </summary>
    private void AgeOffScreenParticles(ActiveEmitter emitter, float dt)
    {
        emitter.Age += TimeSpan.FromSeconds(dt);
        var duration = emitter.Overrides?.Duration ?? emitter.Proto.Duration;
        if (!emitter.Exhausted && duration > TimeSpan.Zero && emitter.Age >= duration)
            emitter.Exhausted = true;

        for (var i = emitter.LiveParticles.Count - 1; i >= 0; i--)
        {
            var p = emitter.LiveParticles[i];
            p.Age += dt;
            p.UpdateCurveSample();
            if (p.Age >= p.Lifetime)
                KillParticle(emitter, i);
        }
    }

    /// <summary>
    /// Removes one live particle and returns it to the emitter-local pool.
    /// All particle death paths go through here so the global budget cannot quietly drift. =^..^=
    /// </summary>
    private void KillParticle(ActiveEmitter emitter, int index)
    {
        var particle = emitter.LiveParticles[index];
        var last = emitter.LiveParticles.Count - 1;
        if (index != last)
            emitter.LiveParticles[index] = emitter.LiveParticles[last];
        emitter.LiveParticles.RemoveAt(last);
        emitter.FreePool.Enqueue(particle);
        _liveParticleCount--;
    }
    private Vector2 SampleEmissionShape(EmissionShapeData shape)
    {
        switch (shape.Type)
        {
            case EmissionShapeType.Point:
                return Vector2.Zero;
            case EmissionShapeType.CircleEdge:
            {
                var a = _random.NextFloat(0f, MathF.PI * 2f);
                return new Vector2(MathF.Cos(a), MathF.Sin(a)) * shape.Radius;
            }
            case EmissionShapeType.CircleFill:
            {
                var a = _random.NextFloat(0f, MathF.PI * 2f);
                var r = shape.Radius * MathF.Sqrt(_random.NextFloat(0f, 1f));
                return new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
            }
            case EmissionShapeType.Box:
            {
                return new Vector2(_random.NextFloat(-shape.BoxExtents.X, shape.BoxExtents.X),
                                   _random.NextFloat(-shape.BoxExtents.Y, shape.BoxExtents.Y));
            }
            default:
                return Vector2.Zero;
        }
    }

    #endregion
}
