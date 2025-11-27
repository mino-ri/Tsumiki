using System;
using System.Runtime.CompilerServices;
using NPlug;
using Tsumiki.View;

namespace Tsumiki;

public class ViewParameter(AudioParameter parameter) : IViewParameter<double>
{
    public double Value { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double DefaultValue => parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;
}

public class FloatViewParameter(AudioParameter parameter) : IViewParameter<float>
{
    public float Value { get => (float)parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public float DefaultValue => (float)parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;
}

public class DoubleRangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<double>
{
    public double Value { get => parameter.Value; set => parameter.Value = value; }
    public double DefaultValue => parameter.ToPlain(parameter.Value);
    public double MinValue => parameter.MinValue;
    public double MaxValue => parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
}

public class Int32RangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<int>
{
    public int Value { get => (int)parameter.Value; set => parameter.Value = value; }
    public int DefaultValue => (int)parameter.ToPlain(parameter.Value);
    public int MinValue => (int)parameter.MinValue;
    public int MaxValue => (int)parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
}

public class FloatRangeViewParameter(AudioRangeParameter parameter) : IRangeViewParameter<float>
{
    public float Value { get => (float)parameter.Value; set => parameter.Value = value; }
    public float DefaultValue => (float)parameter.ToPlain(parameter.Value);
    public float MinValue => (float)parameter.MinValue;
    public float MaxValue => (float)parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
}

public class BoolViewParameter(AudioBoolParameter parameter) : IViewParameter<bool>
{
    public bool Value { get => parameter.Value; set => parameter.Value = value; }
    public bool DefaultValue => parameter.DefaultNormalizedValue > 0.5;
    public int Id => parameter.Id.Value;
}

public class EnumViewParameter<T>(AudioStringListParameter parameter) : IViewParameter<T>
    where T : Enum
{
    public T Value { get => AsT(parameter.SelectedItem); set => parameter.SelectedItem = Unsafe.As<T, int>(ref value); }
    public T DefaultValue { get; } = AsT((int)parameter.ToPlain(parameter.DefaultNormalizedValue));
    public int Id => parameter.Id.Value;

    private static T AsT(int value) => Unsafe.As<int, T>(ref value);
}
