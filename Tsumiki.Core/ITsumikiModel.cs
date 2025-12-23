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

    [VstUnit(8, 100)]
    IModulationUnit Modulation { get; }

    [VstStringListParameter(999, typeof(SaveMode))]
    SaveMode SaveMode { get; set; }

    [VstUnit(9, 1000)]
    ITuningUnit Tuning { get; }
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

public interface IOperatorUnit : IEnvelopeUnit
{
    [VstRangeParameter(4, 0.0, 1.0, 0.5)]
    float Level { get; set; }

    [VstRangeParameter(5, 0.0, 16.0, 1.0)]
    double Pitch { get; set; }

    [VstBoolParameter(6, false)]
    bool Sync { get; set; }

    [VstRangeParameter(7, -1.0, 1.0, 0.0)]
    float ShapeX { get; set; }

    [VstRangeParameter(8, -1.0, 1.0, 0.0)]
    float ShapeY { get; set; }
}

public interface ICarrierUnit : IOperatorUnit
{
    [VstRangeParameter(9, -1.0, 1.0, 0.0)]
    float Pan { get; set; }
}

public interface IModulatorUnit : IOperatorUnit;

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

    [VstRangeParameter(1, 2, 514, 250, StepCount = 512, Units = "ms")]
    int Delay { get; set; }

    [VstRangeParameter(2, 0.0, 1.0, 0.5)]
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
    ILfoUnit Lfo { get; }

    [VstUnit(12, 5)]
    IModulationEnvelopeUnit Envelope { get; }

    [VstUnit(14, 15)]
    IModulationSourceUnit LfoSpeed { get; }

    [VstUnit(15, 20)]
    IModulationSourceUnit LfoLevel { get; }

    [VstUnit(16, 25)]
    IModulationSourceUnit APitch { get; }

    [VstUnit(17, 30)]
    IModulationSourceUnit APan { get; }

    [VstUnit(18, 35)]
    IModulationSourceUnit A1Level { get; }

    [VstUnit(19, 40)]
    IModulationSourceUnit A2Level { get; }

    [VstUnit(20, 45)]
    IModulationSourceUnit BPitch { get; }

    [VstUnit(21, 50)]
    IModulationSourceUnit BPan { get; }

    [VstUnit(22, 55)]
    IModulationSourceUnit B1Level { get; }

    [VstUnit(23, 60)]
    IModulationSourceUnit B2Level { get; }

    [VstUnit(24, 65)]
    IModulationSourceUnit FilterCutoff { get; }

    [VstUnit(25, 70)]
    IModulationSourceUnit FilterResonance { get; }

    [VstUnit(26, 75)]
    IModulationSourceUnit FilterMix { get; }
}

public interface IModulationSourceUnit
{
    [VstRangeParameter(0, -1.0, 1.0, 0.0)]
    double Lfo { get; set; }

    [VstRangeParameter(1, -1.0, 1.0, 0.0)]
    double Env { get; set; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    double Wheel { get; set; }

    [VstRangeParameter(3, -1.0, 1.0, 0.0)]
    double Velocity { get; set; }

    [VstRangeParameter(4, -1.0, 1.0, 0.0)]
    double Pressure { get; set; }
}

public interface IEnvelopeUnit
{
    [VstRangeParameter(0, 0, 80, 40, StepCount = 80)]
    int Attack { get; set; }

    [VstRangeParameter(1, 0, 80, 40, StepCount = 80)]
    int Decay { get; set; }

    [VstRangeParameter(2, 0.0, 1.0, 0.5)]
    float Sustain { get; set; }

    [VstRangeParameter(3, 0, 80, 40, StepCount = 80)]
    int Release { get; set; }
}

public interface IModulationEnvelopeUnit : IEnvelopeUnit
{
    [VstRangeParameter(4, 0.0, 1.0, 1.0)]
    float Level { get; set; }
}

public interface ILfoUnit
{
    [VstRangeParameter(0, 0.0, 1.0, 1.0)]
    float Level { get; set; }

    [VstRangeParameter(1, 0.0, 80.0, 4.0, Units = "Hz")]
    double Speed { get; set; }

    [VstRangeParameter(2, -1.0, 1.0, 0.0)]
    float ShapeX { get; set; }

    [VstRangeParameter(3, -1.0, 1.0, 0.0)]
    float ShapeY { get; set; }
}

public enum SaveMode
{
    Full,
    TimbreOnly,
    TuningOnly,
}

public interface ITuningUnit
{
    [VstRangeParameter(0, 0, 128, 60, StepCount = 128)]
    int Root { get; set; }

    [VstRangeParameter(1, 1, 127, 12, StepCount = 126)]
    int KeyPeriod { get; set; }

    [VstUnit(31, 50)]
    IChannelTuningUnit Channel1 { get; }

    [VstUnit(32, 200)]
    IChannelTuningUnit Channel2 { get; }

    [VstUnit(33, 350)]
    IChannelTuningUnit Channel3 { get; }

    [VstUnit(34, 500)]
    IChannelTuningUnit Channel4 { get; }

    [VstUnit(35, 650)]
    IChannelTuningUnit Channel5 { get; }

    [VstUnit(36, 800)]
    IChannelTuningUnit Channel6 { get; }

    [VstUnit(37, 950)]
    IChannelTuningUnit Channel7 { get; }

    [VstUnit(38, 1100)]
    IChannelTuningUnit Channel8 { get; }

    [VstUnit(39, 1250)]
    IChannelTuningUnit Channel9 { get; }

    [VstUnit(40, 1400)]
    IChannelTuningUnit Channel10 { get; }

    [VstUnit(41, 1550)]
    IChannelTuningUnit Channel11 { get; }

    [VstUnit(42, 1700)]
    IChannelTuningUnit Channel12 { get; }

    [VstUnit(43, 1850)]
    IChannelTuningUnit Channel13 { get; }

    [VstUnit(44, 2000)]
    IChannelTuningUnit Channel14 { get; }

    [VstUnit(45, 2150)]
    IChannelTuningUnit Channel15 { get; }

    [VstUnit(46, 2300)]
    IChannelTuningUnit Channel16 { get; }
}

public interface IChannelTuningUnit
{
    [VstRangeParameter(0, 0, 127, 0, StepCount = 127)]
    int Offset { get; set; }

    [VstRangeParameter(1, 1, 999, 1, StepCount = 998)]
    int RatioN { get; set; }

    [VstRangeParameter(2, 1, 999, 1, StepCount = 998)]
    int RatioD { get; set; }

    [VstRangeParameter(3, 0, 999, 1, StepCount = 999)]
    int RatioPn { get; set; }

    [VstRangeParameter(4, 1, 999, 1, StepCount = 998)]
    int RatioPd { get; set; }

    [VstRangeParameter(5, 1, 999, 2, StepCount = 998)]
    int GeneratorN { get; set; }

    [VstRangeParameter(6, 1, 999, 1, StepCount = 998)]
    int GeneratorD { get; set; }

    [VstRangeParameter(7, 0, 999, 1, StepCount = 999)]
    int GeneratorPn { get; set; }

    [VstRangeParameter(8, 1, 999, 12, StepCount = 998)]
    int GeneratorPd { get; set; }

    [VstRangeParameter(9, 1, 999, 2, StepCount = 998)]
    int PeriodN { get; set; }

    [VstRangeParameter(10, 1, 999, 1, StepCount = 998)]
    int PeriodD { get; set; }

    [VstRangeParameter(11, 0, 999, 1, StepCount = 999)]
    int PeriodPn { get; set; }

    [VstRangeParameter(12, 1, 999, 1, StepCount = 998)]
    int PeriodPd { get; set; }

    [VstBoolParameter(13, false)]
    bool IsCustomPitch { get; set; }

    [VstRangeParameter(20, 0.0, 128.0, 0.0, Flags = VstParameterFlags.IsHidden)] double Pitch000 { get; set; }
    [VstRangeParameter(21, 0.0, 128.0, 1.0, Flags = VstParameterFlags.IsHidden)] double Pitch001 { get; set; }
    [VstRangeParameter(22, 0.0, 128.0, 2.0, Flags = VstParameterFlags.IsHidden)] double Pitch002 { get; set; }
    [VstRangeParameter(23, 0.0, 128.0, 3.0, Flags = VstParameterFlags.IsHidden)] double Pitch003 { get; set; }
    [VstRangeParameter(24, 0.0, 128.0, 4.0, Flags = VstParameterFlags.IsHidden)] double Pitch004 { get; set; }
    [VstRangeParameter(25, 0.0, 128.0, 5.0, Flags = VstParameterFlags.IsHidden)] double Pitch005 { get; set; }
    [VstRangeParameter(26, 0.0, 128.0, 6.0, Flags = VstParameterFlags.IsHidden)] double Pitch006 { get; set; }
    [VstRangeParameter(27, 0.0, 128.0, 7.0, Flags = VstParameterFlags.IsHidden)] double Pitch007 { get; set; }
    [VstRangeParameter(28, 0.0, 128.0, 8.0, Flags = VstParameterFlags.IsHidden)] double Pitch008 { get; set; }
    [VstRangeParameter(29, 0.0, 128.0, 9.0, Flags = VstParameterFlags.IsHidden)] double Pitch009 { get; set; }
    [VstRangeParameter(30, 0.0, 128.0, 10.0, Flags = VstParameterFlags.IsHidden)] double Pitch010 { get; set; }
    [VstRangeParameter(31, 0.0, 128.0, 11.0, Flags = VstParameterFlags.IsHidden)] double Pitch011 { get; set; }
    [VstRangeParameter(32, 0.0, 128.0, 12.0, Flags = VstParameterFlags.IsHidden)] double Pitch012 { get; set; }
    [VstRangeParameter(33, 0.0, 128.0, 13.0, Flags = VstParameterFlags.IsHidden)] double Pitch013 { get; set; }
    [VstRangeParameter(34, 0.0, 128.0, 14.0, Flags = VstParameterFlags.IsHidden)] double Pitch014 { get; set; }
    [VstRangeParameter(35, 0.0, 128.0, 15.0, Flags = VstParameterFlags.IsHidden)] double Pitch015 { get; set; }
    [VstRangeParameter(36, 0.0, 128.0, 16.0, Flags = VstParameterFlags.IsHidden)] double Pitch016 { get; set; }
    [VstRangeParameter(37, 0.0, 128.0, 17.0, Flags = VstParameterFlags.IsHidden)] double Pitch017 { get; set; }
    [VstRangeParameter(38, 0.0, 128.0, 18.0, Flags = VstParameterFlags.IsHidden)] double Pitch018 { get; set; }
    [VstRangeParameter(39, 0.0, 128.0, 19.0, Flags = VstParameterFlags.IsHidden)] double Pitch019 { get; set; }
    [VstRangeParameter(40, 0.0, 128.0, 20.0, Flags = VstParameterFlags.IsHidden)] double Pitch020 { get; set; }
    [VstRangeParameter(41, 0.0, 128.0, 21.0, Flags = VstParameterFlags.IsHidden)] double Pitch021 { get; set; }
    [VstRangeParameter(42, 0.0, 128.0, 22.0, Flags = VstParameterFlags.IsHidden)] double Pitch022 { get; set; }
    [VstRangeParameter(43, 0.0, 128.0, 23.0, Flags = VstParameterFlags.IsHidden)] double Pitch023 { get; set; }
    [VstRangeParameter(44, 0.0, 128.0, 24.0, Flags = VstParameterFlags.IsHidden)] double Pitch024 { get; set; }
    [VstRangeParameter(45, 0.0, 128.0, 25.0, Flags = VstParameterFlags.IsHidden)] double Pitch025 { get; set; }
    [VstRangeParameter(46, 0.0, 128.0, 26.0, Flags = VstParameterFlags.IsHidden)] double Pitch026 { get; set; }
    [VstRangeParameter(47, 0.0, 128.0, 27.0, Flags = VstParameterFlags.IsHidden)] double Pitch027 { get; set; }
    [VstRangeParameter(48, 0.0, 128.0, 28.0, Flags = VstParameterFlags.IsHidden)] double Pitch028 { get; set; }
    [VstRangeParameter(49, 0.0, 128.0, 29.0, Flags = VstParameterFlags.IsHidden)] double Pitch029 { get; set; }
    [VstRangeParameter(50, 0.0, 128.0, 30.0, Flags = VstParameterFlags.IsHidden)] double Pitch030 { get; set; }
    [VstRangeParameter(51, 0.0, 128.0, 31.0, Flags = VstParameterFlags.IsHidden)] double Pitch031 { get; set; }
    [VstRangeParameter(52, 0.0, 128.0, 32.0, Flags = VstParameterFlags.IsHidden)] double Pitch032 { get; set; }
    [VstRangeParameter(53, 0.0, 128.0, 33.0, Flags = VstParameterFlags.IsHidden)] double Pitch033 { get; set; }
    [VstRangeParameter(54, 0.0, 128.0, 34.0, Flags = VstParameterFlags.IsHidden)] double Pitch034 { get; set; }
    [VstRangeParameter(55, 0.0, 128.0, 35.0, Flags = VstParameterFlags.IsHidden)] double Pitch035 { get; set; }
    [VstRangeParameter(56, 0.0, 128.0, 36.0, Flags = VstParameterFlags.IsHidden)] double Pitch036 { get; set; }
    [VstRangeParameter(57, 0.0, 128.0, 37.0, Flags = VstParameterFlags.IsHidden)] double Pitch037 { get; set; }
    [VstRangeParameter(58, 0.0, 128.0, 38.0, Flags = VstParameterFlags.IsHidden)] double Pitch038 { get; set; }
    [VstRangeParameter(59, 0.0, 128.0, 39.0, Flags = VstParameterFlags.IsHidden)] double Pitch039 { get; set; }
    [VstRangeParameter(60, 0.0, 128.0, 40.0, Flags = VstParameterFlags.IsHidden)] double Pitch040 { get; set; }
    [VstRangeParameter(61, 0.0, 128.0, 41.0, Flags = VstParameterFlags.IsHidden)] double Pitch041 { get; set; }
    [VstRangeParameter(62, 0.0, 128.0, 42.0, Flags = VstParameterFlags.IsHidden)] double Pitch042 { get; set; }
    [VstRangeParameter(63, 0.0, 128.0, 43.0, Flags = VstParameterFlags.IsHidden)] double Pitch043 { get; set; }
    [VstRangeParameter(64, 0.0, 128.0, 44.0, Flags = VstParameterFlags.IsHidden)] double Pitch044 { get; set; }
    [VstRangeParameter(65, 0.0, 128.0, 45.0, Flags = VstParameterFlags.IsHidden)] double Pitch045 { get; set; }
    [VstRangeParameter(66, 0.0, 128.0, 46.0, Flags = VstParameterFlags.IsHidden)] double Pitch046 { get; set; }
    [VstRangeParameter(67, 0.0, 128.0, 47.0, Flags = VstParameterFlags.IsHidden)] double Pitch047 { get; set; }
    [VstRangeParameter(68, 0.0, 128.0, 48.0, Flags = VstParameterFlags.IsHidden)] double Pitch048 { get; set; }
    [VstRangeParameter(69, 0.0, 128.0, 49.0, Flags = VstParameterFlags.IsHidden)] double Pitch049 { get; set; }
    [VstRangeParameter(70, 0.0, 128.0, 50.0, Flags = VstParameterFlags.IsHidden)] double Pitch050 { get; set; }
    [VstRangeParameter(71, 0.0, 128.0, 51.0, Flags = VstParameterFlags.IsHidden)] double Pitch051 { get; set; }
    [VstRangeParameter(72, 0.0, 128.0, 52.0, Flags = VstParameterFlags.IsHidden)] double Pitch052 { get; set; }
    [VstRangeParameter(73, 0.0, 128.0, 53.0, Flags = VstParameterFlags.IsHidden)] double Pitch053 { get; set; }
    [VstRangeParameter(74, 0.0, 128.0, 54.0, Flags = VstParameterFlags.IsHidden)] double Pitch054 { get; set; }
    [VstRangeParameter(75, 0.0, 128.0, 55.0, Flags = VstParameterFlags.IsHidden)] double Pitch055 { get; set; }
    [VstRangeParameter(76, 0.0, 128.0, 56.0, Flags = VstParameterFlags.IsHidden)] double Pitch056 { get; set; }
    [VstRangeParameter(77, 0.0, 128.0, 57.0, Flags = VstParameterFlags.IsHidden)] double Pitch057 { get; set; }
    [VstRangeParameter(78, 0.0, 128.0, 58.0, Flags = VstParameterFlags.IsHidden)] double Pitch058 { get; set; }
    [VstRangeParameter(79, 0.0, 128.0, 59.0, Flags = VstParameterFlags.IsHidden)] double Pitch059 { get; set; }
    [VstRangeParameter(80, 0.0, 128.0, 60.0, Flags = VstParameterFlags.IsHidden)] double Pitch060 { get; set; }
    [VstRangeParameter(81, 0.0, 128.0, 61.0, Flags = VstParameterFlags.IsHidden)] double Pitch061 { get; set; }
    [VstRangeParameter(82, 0.0, 128.0, 62.0, Flags = VstParameterFlags.IsHidden)] double Pitch062 { get; set; }
    [VstRangeParameter(83, 0.0, 128.0, 63.0, Flags = VstParameterFlags.IsHidden)] double Pitch063 { get; set; }
    [VstRangeParameter(84, 0.0, 128.0, 64.0, Flags = VstParameterFlags.IsHidden)] double Pitch064 { get; set; }
    [VstRangeParameter(85, 0.0, 128.0, 65.0, Flags = VstParameterFlags.IsHidden)] double Pitch065 { get; set; }
    [VstRangeParameter(86, 0.0, 128.0, 66.0, Flags = VstParameterFlags.IsHidden)] double Pitch066 { get; set; }
    [VstRangeParameter(87, 0.0, 128.0, 67.0, Flags = VstParameterFlags.IsHidden)] double Pitch067 { get; set; }
    [VstRangeParameter(88, 0.0, 128.0, 68.0, Flags = VstParameterFlags.IsHidden)] double Pitch068 { get; set; }
    [VstRangeParameter(89, 0.0, 128.0, 69.0, Flags = VstParameterFlags.IsHidden)] double Pitch069 { get; set; }
    [VstRangeParameter(90, 0.0, 128.0, 70.0, Flags = VstParameterFlags.IsHidden)] double Pitch070 { get; set; }
    [VstRangeParameter(91, 0.0, 128.0, 71.0, Flags = VstParameterFlags.IsHidden)] double Pitch071 { get; set; }
    [VstRangeParameter(92, 0.0, 128.0, 72.0, Flags = VstParameterFlags.IsHidden)] double Pitch072 { get; set; }
    [VstRangeParameter(93, 0.0, 128.0, 73.0, Flags = VstParameterFlags.IsHidden)] double Pitch073 { get; set; }
    [VstRangeParameter(94, 0.0, 128.0, 74.0, Flags = VstParameterFlags.IsHidden)] double Pitch074 { get; set; }
    [VstRangeParameter(95, 0.0, 128.0, 75.0, Flags = VstParameterFlags.IsHidden)] double Pitch075 { get; set; }
    [VstRangeParameter(96, 0.0, 128.0, 76.0, Flags = VstParameterFlags.IsHidden)] double Pitch076 { get; set; }
    [VstRangeParameter(97, 0.0, 128.0, 77.0, Flags = VstParameterFlags.IsHidden)] double Pitch077 { get; set; }
    [VstRangeParameter(98, 0.0, 128.0, 78.0, Flags = VstParameterFlags.IsHidden)] double Pitch078 { get; set; }
    [VstRangeParameter(99, 0.0, 128.0, 79.0, Flags = VstParameterFlags.IsHidden)] double Pitch079 { get; set; }
    [VstRangeParameter(100, 0.0, 128.0, 80.0, Flags = VstParameterFlags.IsHidden)] double Pitch080 { get; set; }
    [VstRangeParameter(101, 0.0, 128.0, 81.0, Flags = VstParameterFlags.IsHidden)] double Pitch081 { get; set; }
    [VstRangeParameter(102, 0.0, 128.0, 82.0, Flags = VstParameterFlags.IsHidden)] double Pitch082 { get; set; }
    [VstRangeParameter(103, 0.0, 128.0, 83.0, Flags = VstParameterFlags.IsHidden)] double Pitch083 { get; set; }
    [VstRangeParameter(104, 0.0, 128.0, 84.0, Flags = VstParameterFlags.IsHidden)] double Pitch084 { get; set; }
    [VstRangeParameter(105, 0.0, 128.0, 85.0, Flags = VstParameterFlags.IsHidden)] double Pitch085 { get; set; }
    [VstRangeParameter(106, 0.0, 128.0, 86.0, Flags = VstParameterFlags.IsHidden)] double Pitch086 { get; set; }
    [VstRangeParameter(107, 0.0, 128.0, 87.0, Flags = VstParameterFlags.IsHidden)] double Pitch087 { get; set; }
    [VstRangeParameter(108, 0.0, 128.0, 88.0, Flags = VstParameterFlags.IsHidden)] double Pitch088 { get; set; }
    [VstRangeParameter(109, 0.0, 128.0, 89.0, Flags = VstParameterFlags.IsHidden)] double Pitch089 { get; set; }
    [VstRangeParameter(110, 0.0, 128.0, 90.0, Flags = VstParameterFlags.IsHidden)] double Pitch090 { get; set; }
    [VstRangeParameter(111, 0.0, 128.0, 91.0, Flags = VstParameterFlags.IsHidden)] double Pitch091 { get; set; }
    [VstRangeParameter(112, 0.0, 128.0, 92.0, Flags = VstParameterFlags.IsHidden)] double Pitch092 { get; set; }
    [VstRangeParameter(113, 0.0, 128.0, 93.0, Flags = VstParameterFlags.IsHidden)] double Pitch093 { get; set; }
    [VstRangeParameter(114, 0.0, 128.0, 94.0, Flags = VstParameterFlags.IsHidden)] double Pitch094 { get; set; }
    [VstRangeParameter(115, 0.0, 128.0, 95.0, Flags = VstParameterFlags.IsHidden)] double Pitch095 { get; set; }
    [VstRangeParameter(116, 0.0, 128.0, 96.0, Flags = VstParameterFlags.IsHidden)] double Pitch096 { get; set; }
    [VstRangeParameter(117, 0.0, 128.0, 97.0, Flags = VstParameterFlags.IsHidden)] double Pitch097 { get; set; }
    [VstRangeParameter(118, 0.0, 128.0, 98.0, Flags = VstParameterFlags.IsHidden)] double Pitch098 { get; set; }
    [VstRangeParameter(119, 0.0, 128.0, 99.0, Flags = VstParameterFlags.IsHidden)] double Pitch099 { get; set; }
    [VstRangeParameter(120, 0.0, 128.0, 100.0, Flags = VstParameterFlags.IsHidden)] double Pitch100 { get; set; }
    [VstRangeParameter(121, 0.0, 128.0, 101.0, Flags = VstParameterFlags.IsHidden)] double Pitch101 { get; set; }
    [VstRangeParameter(122, 0.0, 128.0, 102.0, Flags = VstParameterFlags.IsHidden)] double Pitch102 { get; set; }
    [VstRangeParameter(123, 0.0, 128.0, 103.0, Flags = VstParameterFlags.IsHidden)] double Pitch103 { get; set; }
    [VstRangeParameter(124, 0.0, 128.0, 104.0, Flags = VstParameterFlags.IsHidden)] double Pitch104 { get; set; }
    [VstRangeParameter(125, 0.0, 128.0, 105.0, Flags = VstParameterFlags.IsHidden)] double Pitch105 { get; set; }
    [VstRangeParameter(126, 0.0, 128.0, 106.0, Flags = VstParameterFlags.IsHidden)] double Pitch106 { get; set; }
    [VstRangeParameter(127, 0.0, 128.0, 107.0, Flags = VstParameterFlags.IsHidden)] double Pitch107 { get; set; }
    [VstRangeParameter(128, 0.0, 128.0, 108.0, Flags = VstParameterFlags.IsHidden)] double Pitch108 { get; set; }
    [VstRangeParameter(129, 0.0, 128.0, 109.0, Flags = VstParameterFlags.IsHidden)] double Pitch109 { get; set; }
    [VstRangeParameter(130, 0.0, 128.0, 110.0, Flags = VstParameterFlags.IsHidden)] double Pitch110 { get; set; }
    [VstRangeParameter(131, 0.0, 128.0, 111.0, Flags = VstParameterFlags.IsHidden)] double Pitch111 { get; set; }
    [VstRangeParameter(132, 0.0, 128.0, 112.0, Flags = VstParameterFlags.IsHidden)] double Pitch112 { get; set; }
    [VstRangeParameter(133, 0.0, 128.0, 113.0, Flags = VstParameterFlags.IsHidden)] double Pitch113 { get; set; }
    [VstRangeParameter(134, 0.0, 128.0, 114.0, Flags = VstParameterFlags.IsHidden)] double Pitch114 { get; set; }
    [VstRangeParameter(135, 0.0, 128.0, 115.0, Flags = VstParameterFlags.IsHidden)] double Pitch115 { get; set; }
    [VstRangeParameter(136, 0.0, 128.0, 116.0, Flags = VstParameterFlags.IsHidden)] double Pitch116 { get; set; }
    [VstRangeParameter(137, 0.0, 128.0, 117.0, Flags = VstParameterFlags.IsHidden)] double Pitch117 { get; set; }
    [VstRangeParameter(138, 0.0, 128.0, 118.0, Flags = VstParameterFlags.IsHidden)] double Pitch118 { get; set; }
    [VstRangeParameter(139, 0.0, 128.0, 119.0, Flags = VstParameterFlags.IsHidden)] double Pitch119 { get; set; }
    [VstRangeParameter(140, 0.0, 128.0, 120.0, Flags = VstParameterFlags.IsHidden)] double Pitch120 { get; set; }
    [VstRangeParameter(141, 0.0, 128.0, 121.0, Flags = VstParameterFlags.IsHidden)] double Pitch121 { get; set; }
    [VstRangeParameter(142, 0.0, 128.0, 122.0, Flags = VstParameterFlags.IsHidden)] double Pitch122 { get; set; }
    [VstRangeParameter(143, 0.0, 128.0, 123.0, Flags = VstParameterFlags.IsHidden)] double Pitch123 { get; set; }
    [VstRangeParameter(144, 0.0, 128.0, 124.0, Flags = VstParameterFlags.IsHidden)] double Pitch124 { get; set; }
    [VstRangeParameter(145, 0.0, 128.0, 125.0, Flags = VstParameterFlags.IsHidden)] double Pitch125 { get; set; }
    [VstRangeParameter(146, 0.0, 128.0, 126.0, Flags = VstParameterFlags.IsHidden)] double Pitch126 { get; set; }
    [VstRangeParameter(147, 0.0, 128.0, 127.0, Flags = VstParameterFlags.IsHidden)] double Pitch127 { get; set; }
}
