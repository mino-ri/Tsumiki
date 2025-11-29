using Tsumiki.Metadata;

namespace Tsumiki.Core;

public interface ITsumikiModel
{
    [VstRangeParameter(2, 0.0, 1.0, 0.5)]
    float Master { get; set; }

    [VstRangeParameter(3, -1.0, 1.0, 0.0)]
    float PitchBend { get; set; }

    [VstParameter(4, 0)]
    float Wheel { get; set; }

    [VstParameter(5, 0)]
    float AfterTouch { get; set; }

    [VstUnit(1, 6)]
    IInputUnit Input { get; }
    
    [VstUnit(2, 20)]
    ICarrierUnit A1 { get; }
    
    [VstUnit(3, 40)]
    IModulatorUnit A2 { get; }
    
    [VstUnit(4, 50)]
    ICarrierUnit B1 { get; }

    [VstUnit(5, 70)]
    IModulatorUnit B2 { get; }
    
    [VstUnit(6, 80)]
    IFilterUnit Filter { get; }

    [VstUnit(7, 90)]
    IDelayUnit Delay { get; }
    /*
    [VstUnit(8, 100)]
    IModulationUnit Modulation { get; }

    [VstUnit(9, 200)]
    ITuningUnit Tuning { get; }
    */
}

public enum StackMode
{
    Unison,
    Harmonic,
}

public interface IInputUnit
{
    [VstRangeParameter(0, 0, 12, 12, StepCount = 12)]
    int Bend { get; set; }

    [VstRangeParameter(1, -1, 100, 0, StepCount = 101)]
    int Glide { get; set; }

    [VstRangeParameter(2, -8, 8, 0, StepCount = 16)]
    int Octave { get; set; }

    [VstRangeParameter(3, 1, MathT.MaxStackCount, 1, StepCount = 6)]
    int Stack { get; set; }

    [VstStringListParameter(4, typeof(StackMode))]
    StackMode StackMode { get; set; }

    [VstRangeParameter(5, 0, 100, 10, StepCount = 100, Units = "cent")]
    int StackDetune { get; set; }

    [VstRangeParameter(6, -1.0, 1.0, 0.0)]
    float StackStereo { get; set; }
}

public interface ICarrierUnit : IEnvelopeUnit
{
    [VstRangeParameter(4, 0.0, 1.0, 0.5)]
    float Level { get; set; }

    [VstRangeParameter(5, 0.0, 20.0, 1.0)]
    double Pitch { get; set; }

    [VstBoolParameter(6, false)]
    bool Sync { get; set; }

    [VstRangeParameter(7, -1.0, 1.0, 0.0)]
    float ShapeX { get; set; }

    [VstRangeParameter(8, -1.0, 1.0, 0.0)]
    float ShapeY { get; set; }

    [VstRangeParameter(9, -1.0, 1.0, 0.0)]
    float Pan { get; set; }
}

public interface IModulatorUnit : IEnvelopeUnit
{
    [VstRangeParameter(4, 0.0, 1.0, 0.0)]
    float Level { get; set; }

    [VstRangeParameter(5, 0.0, 20.0, 1.0)]
    double Pitch { get; set; }

    [VstBoolParameter(6, false)]
    bool Sync { get; set; }

    [VstRangeParameter(7, -0.5, 0.5, 0.0)]
    float Phase { get; set; }

    [VstRangeParameter(8, 0.0, 1.0, 0.0)]
    float Feedback { get; set; }
}

public interface IFilterUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 0.0)]
    float Mix { get; set; }

    [VstRangeParameter(1, -64, 64, 0, StepCount = 128)]
    int Cutoff { get; set; }

    [VstRangeParameter(2, 0.0, 0.98, 0.49)]
    float Resonance { get; set; }
}

public interface IDelayUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 0.0)]
    float Mix { get; set; }

    [VstRangeParameter(1, 2, 500, 250, StepCount = 498, Units = "ms")]
    int Delay { get; set; }

    [VstRangeParameter(2, 0.0, 1.0, 1.0)]
    float Feedback { get; set; }

    [VstBoolParameter(3, false)]
    bool Cross { get; set; }

    [VstRangeParameter(4, 20, 120, 50, StepCount = 100)]
    int LowCut { get; set; }

    [VstRangeParameter(5, 20, 120, 90, StepCount = 100)]
    int HighCut { get; set; }
}

public interface IModulationUnit
{
    [VstUnit(11, 0)]
    IEnvelopeUnit Env { get; }

    [VstUnit(12, 5)]
    ILfoUnit Lfo { get; }

    [VstUnit(13, 10)]
    IModulationDestinationUnit EnvTo { get; }

    [VstUnit(14, 20)]
    IModulationDestinationUnit LfoTo { get; }

    [VstUnit(15, 30)]
    IModulationDestinationUnit WheelTo { get; }

    [VstUnit(16, 40)]
    IModulationDestinationUnit VelocityTo { get; }

    [VstUnit(17, 50)]
    IModulationDestinationUnit AfterTouchTo { get; }
}

public interface IModulationDestinationUnit
{
    [VstRangeParameter(0, -12.0, 12.0, 0.0)]
    double APitch { get; set; }

    [VstRangeParameter(1, 0.0, 1.0, 1.0)]
    float ALevel { get; set; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    float APan { get; set; }

    [VstRangeParameter(3, -12.0, 12.0, 0.0)]
    double BPitch { get; set; }

    [VstRangeParameter(4, 0.0, 1.0, 1.0)]
    float BLevel { get; set; }

    [VstRangeParameter(5, -1.0, 1.0, 0.0)]
    float BPan { get; set; }

    [VstRangeParameter(6, -64, 64, 0, StepCount = 128)]
    int FilterCutoff { get; set; }

    [VstRangeParameter(7, 0.0, 0.98, 0.49)]
    float FilterResonance { get; set; }

    [VstRangeParameter(8, 0.0, 1.0, 1.0)]
    float LfoLevel { get; set; }

    [VstRangeParameter(9, 0.01, 500.0, 4.0, Units = "Hz")]
    double LfoSpeed { get; set; }
}

public interface IEnvelopeUnit
{
    [VstRangeParameter(0, 0, 80, 40, StepCount = 80)]
    int Attack { get; set; }

    [VstRangeParameter(1, 0, 80, 40, StepCount = 80)]
    int Decay { get; set; }

    [VstRangeParameter(2, 0.0, 1.0, 1.0)]
    float Sustain { get; set; }

    [VstRangeParameter(3, 0, 80, 40, StepCount = 80)]
    int Release { get; set; }
}

public interface ILfoUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 1.0)]
    float Level { get; set; }

    [VstRangeParameter(1, 0.01, 500.0, 4.0, Units = "Hz")]
    double Speed { get; set; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    float ShapeX { get; set; }

    [VstRangeParameter(3, -1.0, 1.0, 0.0)]
    float ShapeY { get; set; }
}

public interface ITuningUnit
{
    [VstRangeParameter(6, 0, 128, 0, StepCount = 128)]
    int Root { get; set; }

    [VstUnit(20, 10)]
    ITuningSetUnit TuningA { get; }

    [VstUnit(21, 20)]
    ITuningSetUnit TuningB { get; }

    [VstUnit(22, 30)]
    IKeyTuningSetUnit TuningC { get; }

    [VstUnit(23, 80)]
    IChannelTuningUnit Channel1 { get; }

    [VstUnit(24, 86)]
    IChannelTuningUnit Channel2 { get; }

    [VstUnit(25, 92)]
    IChannelTuningUnit Channel3 { get; }

    [VstUnit(26, 98)]
    IChannelTuningUnit Channel4 { get; }

    [VstUnit(27, 104)]
    IChannelTuningUnit Channel5 { get; }

    [VstUnit(28, 110)]
    IChannelTuningUnit Channel6 { get; }

    [VstUnit(29, 116)]
    IChannelTuningUnit Channel7 { get; }

    [VstUnit(30, 122)]
    IChannelTuningUnit Channel8 { get; }

    [VstUnit(31, 128)]
    IChannelTuningUnit Channel9 { get; }

    [VstUnit(32, 134)]
    IChannelTuningUnit Channel10 { get; }

    [VstUnit(33, 140)]
    IChannelTuningUnit Channel11 { get; }

    [VstUnit(34, 146)]
    IChannelTuningUnit Channel12 { get; }

    [VstUnit(35, 152)]
    IChannelTuningUnit Channel13 { get; }

    [VstUnit(36, 158)]
    IChannelTuningUnit Channel14 { get; }

    [VstUnit(37, 164)]
    IChannelTuningUnit Channel15 { get; }

    [VstUnit(38, 170)]
    IChannelTuningUnit Channel16 { get; }
}

public interface ITuningSetUnit
{
    [VstRangeParameter(0, 0, 999, 3, StepCount = 999)]
    int GeneratorN { get; set; }

    [VstRangeParameter(1, 1, 999, 2, StepCount = 998)]
    int GeneratorD { get; set; }

    [VstRangeParameter(2, 0, 999, 1, StepCount = 998)]
    int GeneratorPn { get; set; }

    [VstRangeParameter(3, 1, 999, 1, StepCount = 998)]
    int GeneratorPd { get; set; }

    [VstRangeParameter(4, 0, 999, 2, StepCount = 999)]
    int PeriodN { get; set; }

    [VstRangeParameter(5, 1, 999, 1, StepCount = 998)]
    int PeriodD { get; set; }

    [VstRangeParameter(6, 0, 999, 1, StepCount = 999)]
    int PeriodPn { get; set; }

    [VstRangeParameter(7, 1, 999, 1, StepCount = 998)]
    int PeriodPd { get; set; }

    [VstRangeParameter(8, -64, 64, 0, StepCount = 128)]
    int Offset { get; set; }

    [VstRangeParameter(9, 1, 127, 0, StepCount = 126)]
    int KeyPeriod { get; set; }
}

public interface IKeyTuningSetUnit
{
    [VstUnit(40, 0)]
    IKeyTuningUnit Key1 { get; }
    [VstUnit(41, 4)]
    IKeyTuningUnit Key2 { get; }
    [VstUnit(42, 8)]
    IKeyTuningUnit Key3 { get; }
    [VstUnit(43, 12)]
    IKeyTuningUnit Key4 { get; }
    [VstUnit(44, 16)]
    IKeyTuningUnit Key5 { get; }
    [VstUnit(45, 20)]
    IKeyTuningUnit Key6 { get; }
    [VstUnit(46, 24)]
    IKeyTuningUnit Key7 { get; }
    [VstUnit(47, 28)]
    IKeyTuningUnit Key8 { get; }
    [VstUnit(48, 32)]
    IKeyTuningUnit Key9 { get; }
    [VstUnit(49, 36)]
    IKeyTuningUnit Key10 { get; }
    [VstUnit(50, 40)]
    IKeyTuningUnit Key11 { get; }
    [VstUnit(51, 44)]
    IKeyTuningUnit Key12 { get; }
}

public interface IKeyTuningUnit
{
    [VstRangeParameter(0, 0, 999, 2, StepCount = 999)]
    int GeneratorN { get; set; }

    [VstRangeParameter(1, 1, 999, 1, StepCount = 998)]
    int GeneratorD { get; set; }

    [VstRangeParameter(2, 0, 999, 0, StepCount = 999)]
    int GeneratorPn { get; set; }

    [VstRangeParameter(3, 1, 999, 12, StepCount = 998)]
    int GeneratorPd { get; set; }
}

public interface IChannelTuningUnit
{
    [VstStringListParameter(0, typeof(TuningSetType))]
    TuningSetType Set { get; set; }

    [VstRangeParameter(1, 0, 999, 2, StepCount = 999)]
    int RatioN { get; set; }

    [VstRangeParameter(2, 1, 999, 1, StepCount = 998)]
    int RatioD { get; set; }

    [VstRangeParameter(3, 0, 999, 0, StepCount = 999)]
    int RatioPn { get; set; }

    [VstRangeParameter(4, 1, 999, 12, StepCount = 998)]
    int RatioPd { get; set; }

    [VstRangeParameter(5, -64, 64, 0, StepCount = 128)]
    int KeyOffset { get; set; }
}

public enum TuningSetType
{
    None = 0,
    A = 1,
    B = 2,
    C = 3,
}
