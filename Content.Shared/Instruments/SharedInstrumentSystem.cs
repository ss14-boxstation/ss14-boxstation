namespace Content.Shared.Instruments;

public abstract class SharedInstrumentSystem : EntitySystem
{
    public abstract bool ResolveInstrument(EntityUid uid, ref SharedInstrumentComponent? component);

    public virtual void SetupRenderer(EntityUid uid, bool fromStateChange, SharedInstrumentComponent? instrument = null)
    {
    }

    public virtual void EndRenderer(EntityUid uid, bool fromStateChange, SharedInstrumentComponent? instrument = null)
    {
    }

    public void SetInstrumentProgram(EntityUid uid, SharedInstrumentComponent component, byte program, byte bank)
    {
        component.InstrumentBank = bank;
        component.InstrumentProgram = program;
        Dirty(uid, component);
    }
    // Start Box Change: Harpy DAW implant
    public void SetProgramChange(EntityUid uid, SharedInstrumentComponent component, bool state)
    {
        component.AllowPercussion = state;
        component.AllowProgramChange = state;
    }
    public void ToggleProgramChange(EntityUid uid, SharedInstrumentComponent component, out bool state)
    {
        component.AllowPercussion = !component.AllowPercussion;
        component.AllowProgramChange = !component.AllowProgramChange;
        state = component.AllowProgramChange;
    }
    // End Box Change
}
