namespace Tsumiki.Core

open System

[<Flags>]
type VstParameterFlags =
    /// フラグなし
    | NoFlags = 0
    /// パラメータにオートメーションを設定できます。
    | CanAutomate = 1
    /// パラメータは読み取り専用です。
    | IsReadOnly = 2
    /// パラメータは範囲外の値が指定されるとラップされます。
    | IsWrapAround = 4
    /// パラメータはリストとして表示される必要があります。
    | IsList = 8
    /// パラメータは表示されず、プラグイン外部から変更できないことを表します。(CanAutomate が設定されず、ReadOnly が設定された状態です)
    | IsHidden = 16


[<Sealed; AttributeUsage(AttributeTargets.Class)>]
type VstModelAttribute(modelName: string, definitionType: Type) =
    inherit Attribute()
    member _.DifinitionType = definitionType
    member _.ModelName = modelName


[<Sealed; AttributeUsage(AttributeTargets.Property)>]
type VstUnitAttribute(id: int, parameterIdOffset: int) =
    inherit Attribute()
    member _.Id = id
    member _.ParameterIdOffset = parameterIdOffset


[<Sealed; AttributeUsage(AttributeTargets.Property)>]
type VstParameterAttribute(id: int, defaultNormalizedValue: float) =
    inherit Attribute()
    member val Units: string | null = null with get, set
    member val ShortTitle: string | null = null with get, set
    member val StepCount: int = 0 with get, set
    member val Flags: VstParameterFlags = VstParameterFlags.CanAutomate with get, set

    member _.DefaultNormalizedValue = defaultNormalizedValue
    member _.Id = id


[<Sealed; AttributeUsage(AttributeTargets.Property)>]
type VstRangeParameterAttribute(id: int, minValue: float, maxValue: float, defaultPlainValue: float) =
    inherit Attribute()
    member val Units: string | null = null with get, set
    member val ShortTitle: string | null = null with get, set
    member val StepCount: int = 0 with get, set
    member val Flags: VstParameterFlags = VstParameterFlags.CanAutomate with get, set

    member _.Id = id
    member _.MinValue = minValue
    member _.MaxValue = maxValue
    member _.DefaultPlainValue = defaultPlainValue


[<Sealed; AttributeUsage(AttributeTargets.Property)>]
type VstBoolParameterAttribute(id: int, defaultValue: bool) =
    inherit Attribute()
    member val Units: string | null = null with get, set
    member val ShortTitle: string | null = null with get, set
    member val Flags: VstParameterFlags = VstParameterFlags.CanAutomate with get, set

    member _.DefaultValue = defaultValue
    member _.Id = id
