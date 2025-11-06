namespace Tsumiki.Core;

public interface ITsumikiModel
{
    [VstParameter(2, 0.75f, Units = "dB")]
    float Gain { get; }
}
