using System.Numerics;
using Content.Shared._Starfall.Particles;
using Content.Shared.CCVar;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client._Starfall.Particles;

/// <summary>
/// Manages active particle emitters on the client, including their simulation and rendering via <see cref="ParticleOverlay"/>.
/// </summary>
public sealed partial class ParticleSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    private readonly List<ActiveEmitter> _emitters = new();
    private readonly Dictionary<uint, ActiveEmitter> _emittersByHandle = new();
    private readonly List<(ProtoId<ParticleEffectPrototype> Id, MapCoordinates Coords, int Depth)> _pendingSubEmitters = new();

    /// Maximum number of sub-emitter chains allowed. Prevents infinite recursive sub-emitter chains.
    public const int MaxSubEmitterDepth = 3;
    private ParticleOverlay _overlay = null!;

    // Tally of live particles across all emitters. Incremented in EmitParticle and decremented by KillParticle.
    private int _liveParticleCount;

    // Per-prototype texture/delay cache so multiple emitters sharing a prototype don't re-resolve the same RSI frames.
    private readonly Dictionary<string, (Texture[] Frames, float[] Delays)> _frameCache = new();
    private readonly Dictionary<string, ParticleCurveCache> _curveCache = new();
    // Prototypes that failed to resolve; skip re-attempting every emitter spawn.
    private readonly HashSet<string> _frameResolveFailures = [];
    private readonly HashSet<string> _invalidPrototypeWarnings = [];

    private int _quality;
    private int _globalBudget;
    private float _smoothedFrameTime = 1f / 60f;

    // Incrementing handle counter. Old values are abandoned when emitters die, handles are never reused.
    // uint gives ~4 billion spawns before wrapping, if this causes problems, you scare me.
    private uint _nextHandle = 1;

    // Emission/count multipliers per quality level: Off, Low, Medium, High.
    // So when quality is set to Low, only 25% of the particles spawn, at Medium it's 50%, and at High it's 100%.
    private static readonly float[] QualityMultipliers = [0f, 0.25f, 0.5f, 1f];

    // Default global particle budgets per quality level.
    private static readonly int[] QualityBudgets = [0, 2250, 5500, 8000];

    /// <summary>
    /// Absolute ceiling on live particles regardless of quality settings or anything else.
    /// In isolated testing, I was able to spawn ~26,000 simultaneous particles before significant frame drops.
    /// This number is NOT a target and MUST NOT be treated as one and is intentionally set well below that for several reasons:
    /// <list type="bullet">
    ///   <item><b>All particle simulation runs entirely on the CPU</b>. Every particle competes
    ///   with gameplay logic, physics, networking, and rendering on the same thread.</item>
    ///   <item>That 26k figure was measured in isolation. In a real round with entities, atmos, and players,
    ///   performance will degrade significantly sooner.</item>
    ///   <item>Emitters stack multiplicatively. Ten "small" effects at 500 particles each is already
    ///   5,000 particles before considering anything else in the scene.</item>
    /// </list>
    /// I would KILL to be able to render these on the GPU, but that is not currently an option without engine changes.
    /// <b>Do not raise this limit just because your machine can handle it.</b>
    /// This limit exists to protect performance across all hardware and real gameplay conditions.
    /// If you believe this needs to be increased, you should first justify why the effect cannot
    /// be achieved more efficiently. You do NOT need that many particles.
    /// </summary>
    private const int HardMaxParticles = 8000;

    /// <summary>
    /// Maximum particles per emitter for <see cref="ParticleEffectPrototype.IgnoreQualitySettings"/> effects
    /// when quality is below High. At High quality they respect the full <see cref="HardMaxParticles"/> ceiling.
    /// </summary>
    private const int IgnoreQualityMaxParticles = 64;

    // A Burst emitter uses MaxCount as its requested spawn count, but will not spawn more than this number of particles in a single burst.
    private const int DefaultBurstMaxCount = 50;

    /// Maximum calculated MaxCount when a prototype does not provide one explicitly.
    private const int AutomaticMaxCount = 128;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new ParticleOverlay(this, _transform);
        _overlayManager.AddOverlay(_overlay);

        _cfg.OnValueChanged(CCVars.ParticleQuality, OnQualityChanged, invokeImmediately: true);
        _cfg.OnValueChanged(CCVars.ParticleGlobalBudget, OnGlobalBudgetChanged, invokeImmediately: true);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCVars.ParticleQuality, OnQualityChanged);
        _cfg.UnsubValueChanged(CCVars.ParticleGlobalBudget, OnGlobalBudgetChanged);
        _overlayManager.RemoveOverlay(_overlay);
        _emitters.Clear();
        _emittersByHandle.Clear();
        _curveCache.Clear();
        _liveParticleCount = 0;
    }

    private void OnQualityChanged(int quality)
    {
        _quality = quality;
        if (quality >= 0 && quality < QualityBudgets.Length)
            _globalBudget = QualityBudgets[quality];
    }

    private void OnGlobalBudgetChanged(int budget)
    {
        _globalBudget = Math.Clamp(budget, 0, HardMaxParticles);
    }

    public override void FrameUpdate(float frameTime)
    {
        _smoothedFrameTime = MathHelper.Lerp(_smoothedFrameTime, frameTime, 0.05f);

        // If particles are fully disabled, drop all emitters except those flagged to ignore quality settings.
        if (_quality == 0)
        {
            for (var i = _emitters.Count - 1; i >= 0; i--)
            {
                var e = _emitters[i];
                if (e.Proto.IgnoreQualitySettings)
                    continue;
                _liveParticleCount -= e.LiveCount;
                e.LiveParticles.Clear();
                RemoveEmitterAt(i);
            }
            if (_emitters.Count == 0)
                return;
        }

        var eye = _eye.CurrentEye;
        var eyePos = eye.Position.Position;
        var eyeAngle = (float)eye.Rotation;
        var halfSize = new Vector2(eye.Zoom.X > 0 ? 20f / eye.Zoom.X : 20f, eye.Zoom.Y > 0 ? 15f / eye.Zoom.Y : 15f) * 1.5f;
        var viewBounds = new Box2(eyePos - halfSize, eyePos + halfSize);
        var currentMapId = eye.Position.MapId;

        _pendingSubEmitters.Clear();
        // Iterate emitters in reverse so we can safely remove exhausted ones by index.
        // For each emitter: skip full simulation if off-screen (only age particles), otherwise tick it.
        // Remove any emitter that is exhausted and has no live particles left.
        for (var i = _emitters.Count - 1; i >= 0; i--)
        {
            var emitter = _emitters[i];

            // Check if attached entity was deleted even when off-screen.
            if (emitter.AttachedEntity is { } attachedEnt && Deleted(attachedEnt))
            {
                emitter.Exhausted = true;
                emitter.AttachedEntity = null;
            }

            if (Deleted(emitter.Coordinates.EntityId))
            {
                _liveParticleCount -= emitter.LiveCount;
                RemoveEmitterAt(i);
                continue;
            }

            emitter.MapCoords = _transform.ToMapCoordinates(emitter.Coordinates);

            var inView = emitter.MapCoords.MapId == currentMapId
                && (viewBounds.Contains(emitter.MapCoords.Position)
                    || HasVisibleParticles(emitter, viewBounds, eyeAngle));

            if (inView)
                UpdateEmitterSimulation(emitter, frameTime, eyeAngle);
            else
                AgeOffScreenParticles(emitter, frameTime);

            if (emitter.Exhausted && emitter.LiveCount == 0)
                RemoveEmitterAt(i);
        }

        // Spawn any sub-emitters collected during this tick.
        // Use an index-based while loop instead of foreach because SpawnEffect can itself add
        // new entries to _pendingSubEmitters (sub-emitters of sub-emitters (dont do that)), which would
        // throw if we were iterating with an enumerator.
        var subIdx = 0;
        while (subIdx < _pendingSubEmitters.Count)
        {
            var (id, coords, depth) = _pendingSubEmitters[subIdx];
            subIdx++;
            SpawnEffect(id, coords, depth: depth);
        }
    }

    private void UpdateEmitterSimulation(ActiveEmitter emitter, float frameTime, float eyeAngle)
    {
        var interval = GetSimulationInterval();
        if (interval <= 0f)
        {
            emitter.SimulationAccumulator = 0f;
            TickEmitter(emitter, frameTime, eyeAngle);
            return;
        }

        emitter.SimulationAccumulator += frameTime;
        if (emitter.SimulationAccumulator < interval)
            return;

        // Preserve all elapsed time so reduced-frequency simulation changes smoothness, not speed.
        var simulationTime = Math.Min(emitter.SimulationAccumulator, 0.1f);
        emitter.SimulationAccumulator = 0f;
        TickEmitter(emitter, simulationTime, eyeAngle);
    }

    private bool HasVisibleParticles(ActiveEmitter emitter, Box2 viewBounds, float eyeAngle)
    {
        foreach (var particle in emitter.LiveParticles)
        {
            if (viewBounds.Contains(ComputeParticleWorldPos(particle, emitter, eyeAngle)))
                return true;
        }

        return false;
    }

    private float GetSimulationInterval()
    {
        var interval = _quality switch
        {
            <= 0 => 1f / 15f,
            1 => 1f / 20f,
            2 => 1f / 30f,
            _ => 0f,
        };

        // If the client is already struggling, give gameplay some room before particles ask for more.
        if (_smoothedFrameTime > 1f / 28f)
            return Math.Max(interval, 1f / 20f);
        if (_smoothedFrameTime > 1f / 40f)
            return Math.Max(interval, 1f / 30f);
        return interval;
    }
}
