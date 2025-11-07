namespace Tsumiki.Core;

public interface ITsumikiModel
{
    [VstRangeParameter(2, 0.0, 1.0, 1.0)]
    float Master { get; }

    [VstRangeParameter(3, 0, 24, 12, StepCount = 24)]
    int Bend { get; }

    [VstRangeParameter(4, -1, 100, 0, StepCount = 100, Flags = VstParameterFlags.IsWrapAround)]
    int Glide { get; }

    [VstRangeParameter(5, -6, 6, 0, StepCount = 12)]
    int Octave { get; }

    [VstRangeParameter(6, 1, 9, 1, StepCount = 8, Flags = VstParameterFlags.IsWrapAround)]
    int Mux { get; }

    [VstBoolParameter(7, false)]
    bool MuxHarmonic { get; }

    [VstRangeParameter(8, 0, 100, 10, StepCount = 100, Units = "cent")]
    int MuxDetune { get; }

    [VstRangeParameter(9, 0.0, 1.0, 1.0)]
    float MuxStereo { get; }

    [VstUnit(1, 10)]
    ICarrierUnit A1 { get; }

    [VstUnit(2, 30)]
    IModulatorUnit A2 { get; }

    [VstUnit(3, 50)]
    ICarrierUnit B1 { get; }

    [VstUnit(4, 70)]
    IModulatorUnit B2 { get; }

    [VstUnit(5, 90)]
    IFilterUnit Filter { get; }

    [VstUnit(6, 100)]
    IModulationUnit Modulation { get; }

    [VstUnit(7, 200)]
    ITuningUnit Tuning { get; }
}

public interface ICarrierUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 1.0)]
    float Level { get; }

    [VstRangeParameter(1, 0.01, 16.0, 1.0)]
    double Pitch { get; }

    [VstBoolParameter(2, false)]
    bool Sync { get; }

    [VstRangeParameter(3, 0.0, 1.0, 0.0)]
    float Phase { get; }

    [VstRangeParameter(4, -1.0, 1.0, 0.0)]
    float ShapeX { get; }

    [VstRangeParameter(5, -1.0, 1.0, 0.0)]
    float ShapeY { get; }

    [VstRangeParameter(6, -1.0, 1.0, 0.0)]
    float Pan { get; }

    [VstRangeParameter(15, 0, 80, 40, StepCount = 80)]
    int Attack { get; }

    [VstRangeParameter(16, 0, 80, 40, StepCount = 80)]
    int Decay { get; }

    [VstRangeParameter(17, 0.0, 1.0, 1.0)]
    float Sustain { get; }

    [VstRangeParameter(18, 0, 80, 40, StepCount = 80)]
    int Release { get; }
}

public interface IModulatorUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 1.0)]
    float Level { get; }

    [VstRangeParameter(1, 0.01, 16.0, 1.0)]
    double Pitch { get; }

    [VstBoolParameter(2, false)]
    bool Sync { get; }

    [VstRangeParameter(3, 0.0, 1.0, 0.0)]
    float Phase { get; }

    [VstRangeParameter(4, 0.0, 1.0, 1.0)]
    float Feedback { get; }

    [VstRangeParameter(15, 0, 80, 40, StepCount = 80)]
    int Attack { get; }

    [VstRangeParameter(16, 0, 80, 40, StepCount = 80)]
    int Decay { get; }

    [VstRangeParameter(17, 0.0, 1.0, 1.0)]
    float Sustain { get; }

    [VstRangeParameter(18, 0, 80, 40, StepCount = 80)]
    int Release { get; }
}

public interface IFilterUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 0.0)]
    float Mix { get; }

    [VstRangeParameter(1, -1.0, 1.0, 0.0)]
    float MorphFactor { get; }

    [VstRangeParameter(2, -64, 64, 0, StepCount = 128)]
    int Cutoff { get; }

    [VstRangeParameter(3, 0.0, 0.98, 0.49)]
    float Resonance { get; }
}

public interface IDelayUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 0.0)]
    float Mix { get; }

    [VstRangeParameter(1, 2, 500, 250, StepCount = 498, Units = "ms")]
    int Delay { get; }

    [VstRangeParameter(2, 0.0, 1.0, 1.0)]
    float Feedback { get; }

    [VstBoolParameter(3, false)]
    bool Cross { get; }

    [VstRangeParameter(4, 20, 120, 50, StepCount = 100)]
    int LowCut { get; }

    [VstRangeParameter(5, 20, 120, 90, StepCount = 100)]
    int HighCut { get; }
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
    [VstRangeParameter(0, -16.0, 16.0, 0.0)]
    double APitch { get; }

    [VstRangeParameter(1, 0.0, 1.0, 1.0)]
    float ALevel { get; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    float APan { get; }

    [VstRangeParameter(3, -16.0, 16.0, 0.0)]
    double BPitch { get; }

    [VstRangeParameter(4, 0.0, 1.0, 1.0)]
    float BLevel { get; }

    [VstRangeParameter(5, -1.0, 1.0, 0.0)]
    float BPan { get; }

    [VstRangeParameter(6, -64, 64, 0, StepCount = 128)]
    int FilterCutoff { get; }

    [VstRangeParameter(7, 0.0, 0.98, 0.49)]
    float FilterResonance { get; }

    [VstRangeParameter(8, 0.0, 1.0, 1.0)]
    float LfoLevel { get; }

    [VstRangeParameter(9, 0.01, 500.0, 4.0, Units = "Hz")]
    double LfoSpeed { get; }
}

public interface IEnvelopeUnit
{
    [VstRangeParameter(0, 0, 80, 40, StepCount = 80)]
    int Attack { get; }

    [VstRangeParameter(1, 0, 80, 40, StepCount = 80)]
    int Decay { get; }

    [VstRangeParameter(2, 0.0, 1.0, 1.0)]
    float Sustain { get; }

    [VstRangeParameter(3, 0, 80, 40, StepCount = 80)]
    int Release { get; }
}

public interface ILfoUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 1.0)]
    float Level { get; }

    [VstRangeParameter(1, 0.01, 500.0, 4.0, Units = "Hz")]
    double Speed { get; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    float ShapeX { get; }

    [VstRangeParameter(3, -1.0, 1.0, 0.0)]
    float ShapeY { get; }
}

public interface ITuningUnit
{
    [VstRangeParameter(6, 0, 127, 0, StepCount = 127)]
    int Root { get; }

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
    [VstRangeParameter(0, 1, 999, 0, StepCount = 998)]
    int GeneratorN { get; }

    [VstRangeParameter(1, 0, 999, 0, StepCount = 999)]
    int GeneratorD { get; }

    [VstRangeParameter(2, 1, 999, 0, StepCount = 998)]
    int GeneratorPn { get; }

    [VstRangeParameter(3, 0, 999, 0, StepCount = 999)]
    int GeneratorPd { get; }

    [VstRangeParameter(4, 1, 999, 0, StepCount = 998)]
    int PeriodN { get; }

    [VstRangeParameter(5, 0, 999, 0, StepCount = 999)]
    int PeriodD { get; }

    [VstRangeParameter(6, 1, 999, 0, StepCount = 998)]
    int PeriodPn { get; }

    [VstRangeParameter(7, 0, 999, 0, StepCount = 999)]
    int PeriodPd { get; }

    [VstRangeParameter(8, -64, 64, 0, StepCount = 128)]
    int Offset { get; }

    [VstRangeParameter(9, 1, 127, 0, StepCount = 126)]
    int KeyPeriod { get; }
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
    [VstRangeParameter(0, 1, 999, 0, StepCount = 998)]
    int GeneratorN { get; }

    [VstRangeParameter(1, 2, 999, 0, StepCount = 999)]
    int GeneratorD { get; }

    [VstRangeParameter(2, 12, 999, 0, StepCount = 998)]
    int GeneratorPn { get; }

    [VstRangeParameter(3, 0, 999, 0, StepCount = 999)]
    int GeneratorPd { get; }
}

public interface IChannelTuningUnit
{
    [VstStringListParameter(0, typeof(TuningSetType))]
    TuningSetType Set { get; }

    [VstRangeParameter(1, 1, 999, 0, StepCount = 998)]
    int RatioN { get; }

    [VstRangeParameter(2, 2, 999, 0, StepCount = 999)]
    int RatioD { get; }

    [VstRangeParameter(3, 12, 999, 0, StepCount = 998)]
    int RatioPn { get; }

    [VstRangeParameter(4, 0, 999, 0, StepCount = 999)]
    int RatioPd { get; }

    [VstRangeParameter(5, -64, 64, 0, StepCount = 128)]
    int KeyOffset { get; }
}

public enum TuningSetType
{
    None = 0,
    A = 1,
    B = 2,
    C = 3,
}
