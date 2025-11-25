using System;
using System.Runtime.CompilerServices;
using NPlug;
using Tsumiki.View;

namespace Tsumiki;

public class ViewParameter(AudioParameter parameter) : IViewParameter<double>
{
    public double Value { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double DefaultValue { get; } = parameter.DefaultNormalizedValue;
}

public class FloatViewParameter(AudioParameter parameter) : IViewParameter<float>
{
    public float Value { get => (float)parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public float DefaultValue { get; } = (float)parameter.DefaultNormalizedValue;
}

public class DoubleRangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<double>
{
    public double Value { get => parameter.Value; set => parameter.Value = value; }
    public double DefaultValue { get; } = parameter.ToPlain(parameter.Value);
    public double MinValue { get; } = parameter.MinValue;
    public double MaxValue { get; } = parameter.MaxValue;
    public int StepCount { get; } = parameter.StepCount;
}

public class Int32RangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<int>
{
    public int Value { get => (int)parameter.Value; set => parameter.Value = value; }
    public int DefaultValue { get; } = (int)parameter.ToPlain(parameter.Value);
    public int MinValue { get; } = (int)parameter.MinValue;
    public int MaxValue { get; } = (int)parameter.MaxValue;
    public int StepCount { get; } = parameter.StepCount;
}

public class FloatRangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<float>
{
    public float Value { get => (float)parameter.Value; set => parameter.Value = value; }
    public float DefaultValue { get; } = (float)parameter.ToPlain(parameter.Value);
    public float MinValue { get; } = (float)parameter.MinValue;
    public float MaxValue { get; } = (float)parameter.MaxValue;
    public int StepCount { get; } = parameter.StepCount;
}

public class BoolViewParameter(AudioBoolParameter parameter) : IViewParameter<bool>
{
    public bool Value { get => parameter.Value; set => parameter.Value = value; }
    public bool DefaultValue { get; } = parameter.DefaultNormalizedValue > 0.5;
}

public class EnumViewParameter<T>(AudioStringListParameter parameter) : IViewParameter<T>
    where T : Enum
{
    public T Value { get => AsT(parameter.SelectedItem); set => parameter.SelectedItem = Unsafe.As<T, int>(ref value); }
    public T DefaultValue { get; } = AsT((int)parameter.ToPlain(parameter.DefaultNormalizedValue));

    private static T AsT(int value) => Unsafe.As<int, T>(ref value);
}
