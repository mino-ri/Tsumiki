using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Tsumiki.Test")]

namespace Tsumiki.Core;

[Flags]
public enum VstParameterFlags
{
    /// <summary>
    /// フラグなし
    /// </summary>
    NoFlags = 0,
    /// <summary>
    /// パラメータにオートメーションを設定できます。
    /// </summary>
    CanAutomate = 1,
    /// <summary>
    /// パラメータは読み取り専用です。
    /// </summary>
    IsReadOnly = 2,
    /// <summary>
    /// パラメータは範囲外の値が指定されるとラップされます。
    /// </summary>
    IsWrapAround = 4,
    /// <summary>
    /// パラメータはリストとして表示される必要があります。
    /// </summary>
    IsList = 8,
    /// <summary>
    /// パラメータは表示されず、プラグイン外部から変更できないことを表します。(CanAutomate が設定されず、ReadOnly が設定された状態です)
    /// </summary>
    IsHidden = 16
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class VstModelAttribute(string modelName, Type definitionType) : Attribute()
{
    public Type DefinitionType => definitionType;
    public string ModelName => modelName;
}


[AttributeUsage(AttributeTargets.Property)]
public sealed class VstUnitAttribute(int id, int parameterIdOffset) : Attribute()
{
    public int Id => id;
    public int ParameterIdOffset => parameterIdOffset;
}


[AttributeUsage(AttributeTargets.Property)]
public sealed class VstParameterAttribute(int id, double defaultNormalizedValue) : Attribute()
{
    public string? Units { get; set; }
    public string? ShortTitle { get; set; }
    public int StepCount { get; set; }
    public VstParameterFlags Flags { get; set; } = VstParameterFlags.CanAutomate;

    public double DefaultNormalizedValue => defaultNormalizedValue;
    public int Id => id;
}


[AttributeUsage(AttributeTargets.Property)]
public sealed class VstRangeParameterAttribute(int id, double minValue, double maxValue, double defaultPlainValue) : Attribute()
{
    public string? Units { get; set; }
    public string? ShortTitle { get; set; }
    public int StepCount { get; set; }
    public VstParameterFlags Flags { get; set; } = VstParameterFlags.CanAutomate;

    public int Id => id;
    public double MinValue => minValue;
    public double MaxValue => maxValue;
    public double DefaultPlainValue => defaultPlainValue;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class VstBoolParameterAttribute(int id, bool defaultValue) : Attribute()
{

    public string? Units { get; set; }
    public string? ShortTitle { get; set; }
    public VstParameterFlags Flags { get; set; } = VstParameterFlags.CanAutomate;

    public bool DefaultValue => defaultValue;
    public int Id => id;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class VstStringListParameterAttribute(int id, Type items) : Attribute()
{
    public string? Units { get; set; }
    public string? ShortTitle { get; set; }
    public int SelectedItem { get; set; }
    public VstParameterFlags Flags { get; set; } = VstParameterFlags.CanAutomate | VstParameterFlags.IsList;

    public int Id => id;
    public Type Items => items;
}

/// <summary>
/// メソッドが初期化タイミングでのみ更新されることを表します。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor)]
internal sealed class InitTimingAttribute : Attribute { }

/// <summary>
/// 型が表すデータがイベントタイミングで更新されること、またはメソッドがイベントタイミングで呼び出されることを表します。
/// この処理内では、ヒープ上にメモリ確保することは避けるべきです。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor)]
internal sealed class EventTimingAttribute : Attribute { }

/// <summary>
/// 型が表すデータがオーディオタイミングで更新されること、またはメソッドがオーディオタイミングで呼び出されることを表します。
/// この処理内では、ヒープ上にメモリ確保することは絶対にしてはいけません。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor)]
internal sealed class AudioTimingAttribute : Attribute { }
