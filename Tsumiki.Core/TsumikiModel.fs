namespace Tsumiki.Core


type ITsumikiOsc1ShapeUnit =
    [<VstRangeParameter(1, 0.0, 128.0, 64.0, StepCount = 129)>]
    abstract member Level: int


type ITsumikiOsc1Unit =
    [<VstRangeParameter(1, 0.0, 128.0, 64.0, StepCount = 129)>]
    abstract member Level: int
    [<VstUnit(2, 20)>]
    abstract member Shape: ITsumikiOsc1ShapeUnit


type ITsumikiModel =
    [<VstParameter(1, 0.75, Units = "dB", StepCount = 1)>]
    abstract member Gain: double

    [<VstBoolParameter(2, true, Flags = VstParameterFlags.NoFlags)>]
    abstract member Sync: bool

    [<VstUnit(1, 10)>]
    abstract member Osc1: ITsumikiOsc1Unit
