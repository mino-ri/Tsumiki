using System;
using System.Runtime.CompilerServices;
using NPlug;
using Tsumiki.View;

namespace Tsumiki;

public class ViewParameter(AudioParameter parameter, TsumikiController? controller) : IViewParameter<double>
{
    public double Value { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double DefaultValue => parameter.DefaultNormalizedValue;
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;

    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class FloatViewParameter(AudioParameter parameter, TsumikiController? controller) : IViewParameter<float>
{
    public float Value { get => (float)parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public float DefaultValue => (float)parameter.DefaultNormalizedValue;
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;
    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class DoubleRangeViewParameter(AudioRangeParameter parameter, TsumikiController? controller) : IRangeViewParameter<double>
{
    public double Value { get => parameter.Value; set => parameter.Value = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public double DefaultValue => parameter.ToPlain(parameter.DefaultNormalizedValue);
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public double MinValue => parameter.MinValue;
    public double MaxValue => parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class Int32RangeViewParameter(AudioRangeParameter parameter, TsumikiController? controller) : IRangeViewParameter<int>
{
    public int Value { get => (int)parameter.Value; set => parameter.Value = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public int DefaultValue => (int)parameter.ToPlain(parameter.Value);
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public int MinValue => (int)parameter.MinValue;
    public int MaxValue => (int)parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class FloatRangeViewParameter(AudioRangeParameter parameter, TsumikiController? controller) : IRangeViewParameter<float>
{
    public float Value { get => (float)parameter.Value; set => parameter.Value = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public float DefaultValue => (float)parameter.ToPlain(parameter.Value);
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public float MinValue => (float)parameter.MinValue;
    public float MaxValue => (float)parameter.MaxValue;
    public int StepCount => parameter.StepCount;
    public int Id => parameter.Id.Value;
    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class BoolViewParameter(AudioBoolParameter parameter, TsumikiController? controller) : IViewParameter<bool>
{
    public bool Value { get => parameter.Value; set => parameter.Value = value; }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public bool DefaultValue => parameter.DefaultNormalizedValue > 0.5;
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;
    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();
}

public class EnumViewParameter<T>(AudioStringListParameter parameter, TsumikiController? controller) : IRangeViewParameter<T>
    where T : Enum
{
    public T Value { get => AsT(parameter.SelectedItem); set => parameter.SelectedItem = Unsafe.As<T, int>(ref value); }
    public double NormalizedValue { get => parameter.NormalizedValue; set => parameter.NormalizedValue = value; }
    public T DefaultValue { get; } = AsT((int)parameter.ToPlain(parameter.DefaultNormalizedValue));
    public double DefaultNormalizedValue => parameter.DefaultNormalizedValue;
    public int Id => parameter.Id.Value;
    public T MinValue { get; } = AsT(0);
    public T MaxValue { get; } = AsT(parameter.Items.Length - 1);
    public int StepCount => parameter.StepCount;

    public void BeginEdit() => controller?.BeginEditParameter(parameter);
    public void EndEdit() => controller?.EndEditParameter();

    private static T AsT(int value) => Unsafe.As<int, T>(ref value);
}
