using Tsumiki.Core;

namespace Tsumiki.Test.Core;

public class MockTsumikiModel : ITsumikiModel
{
    public float Master { get; set; } = 0.5f;
    public float PitchBend { get; set; } = 0.0f;
    public float Wheel { get; set; }
    public float AfterTouch { get; set; }

    public IInputUnit Input { get; } = new MockInputUnit();
    public ICarrierUnit A1 { get; } = new MockCarrierUnit();
    public IModulatorUnit A2 { get; } = new MockModulatorUnit();
    public ICarrierUnit B1 { get; } = new MockCarrierUnit();
    public IModulatorUnit B2 { get; } = new MockModulatorUnit();
    public IFilterUnit Filter { get; } = new MockFilterUnit();
    public IDelayUnit Delay { get; } = new MockDelayUnit();
    public IModulationUnit Modulation { get; } = new MockModulationUnit();
}

public class MockInputUnit : IInputUnit
{
    public int Bend { get; set; } = 12;
    public int Glide { get; set; }
    public int Octave { get; set; }
    public int Stack { get; set; } = 1;
    public StackMode StackMode { get; set; }
    public int StackDetune { get; set; } = 10;
    public float StackStereo { get; set; }
}

public class MockCarrierUnit : ICarrierUnit
{
    public float Pan { get; set; }
    public float Level { get; set; } = 0.5f;
    public double Pitch { get; set; } = 1.0;
    public bool Sync { get; set; }
    public float ShapeX { get; set; }
    public float ShapeY { get; set; }
    public int Attack { get; set; } = 40;
    public int Decay { get; set; } = 40;
    public float Sustain { get; set; } = 1.0f;
    public int Release { get; set; } = 40;
}

public class MockModulatorUnit : IModulatorUnit
{
    public float Level { get; set; } = 0.5f;
    public double Pitch { get; set; } = 1.0;
    public bool Sync { get; set; }
    public float ShapeX { get; set; }
    public float ShapeY { get; set; }
    public int Attack { get; set; } = 40;
    public int Decay { get; set; } = 40;
    public float Sustain { get; set; } = 1.0f;
    public int Release { get; set; } = 40;
}

public class MockFilterUnit : IFilterUnit
{
    public float Mix { get; set; }
    public int Cutoff { get; set; }
    public float Resonance { get; set; } = 0.49f;
}

public class MockDelayUnit : IDelayUnit
{
    public float Mix { get; set; }
    public int Delay { get; set; } = 250;
    public float Feedback { get; set; } = 0.5f;
    public bool Cross { get; set; }
    public int LowCut { get; set; } = 50;
    public int HighCut { get; set; } = 90;
}

public class MockModulationUnit : IModulationUnit
{
    public ILfoUnit Lfo { get; } = new MockLfoUnit();
    public IModulationEnvelopeUnit Envelope { get; } = new MockModulationEnvelopeUnit();
    public IModulationSourceUnit LfoSpeed { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit LfoLevel { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit APitch { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit APan { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit A1Level { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit A2Level { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit BPitch { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit BPan { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit B1Level { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit B2Level { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit FilterCutoff { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit FilterResonance { get; } = new MockModulationSourceUnit();
    public IModulationSourceUnit FilterMix { get; } = new MockModulationSourceUnit();
}

public class MockLfoUnit : ILfoUnit
{
    public float Level { get; set; } = 1.0f;
    public double Speed { get; set; } = 4.0;
    public float ShapeX { get; set; }
    public float ShapeY { get; set; }
}

public class MockModulationEnvelopeUnit : IModulationEnvelopeUnit
{
    public float Level { get; set; } = 0.5f;
    public int Attack { get; set; } = 40;
    public int Decay { get; set; } = 40;
    public float Sustain { get; set; } = 1.0f;
    public int Release { get; set; } = 40;
}

public class MockModulationSourceUnit : IModulationSourceUnit
{
    public double Lfo { get; set; }
    public double Env { get; set; }
    public double Wheel { get; set; }
    public double Velocity { get; set; }
    public double Pressure { get; set; }
}
