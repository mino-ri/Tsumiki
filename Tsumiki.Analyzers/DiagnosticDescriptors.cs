using Microsoft.CodeAnalysis;

namespace Tsumiki.Analyzers;

/// <summary>
/// Timing 属性制限に関する診断記述子を定義します。
/// </summary>
internal static class DiagnosticDescriptors
{
    private const string Category = "Tsumiki.Timing";

    // ========================================
    // エラーレベル（「絶対に」違反）
    // ========================================

    /// <summary>
    /// TSK001: EventTiming メソッドから InitTiming メソッドを呼び出してはいけません。
    /// </summary>
    public static readonly DiagnosticDescriptor EventTimingCallsInitTiming = new(
        id: "TSK001",
        title: "EventTiming メソッドから InitTiming メソッドの呼び出しは禁止されています",
        messageFormat: "EventTiming メソッド '{0}' 内で InitTiming メソッド '{1}' を呼び出すことは禁止されています",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TSK002: EventTiming メソッドから InitTiming 構造体のフィールドを変更してはいけません。
    /// </summary>
    public static readonly DiagnosticDescriptor EventTimingModifiesInitTimingField = new(
        id: "TSK002",
        title: "EventTiming メソッドから InitTiming 構造体のフィールド変更は禁止されています",
        messageFormat: "EventTiming メソッド '{0}' 内で InitTiming 構造体 '{1}' のフィールドを変更することは禁止されています",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TSK003: AudioTiming メソッドから InitTiming メソッドを呼び出してはいけません。
    /// </summary>
    public static readonly DiagnosticDescriptor AudioTimingCallsInitTiming = new(
        id: "TSK003",
        title: "AudioTiming メソッドから InitTiming メソッドの呼び出しは禁止されています",
        messageFormat: "AudioTiming メソッド '{0}' 内で InitTiming メソッド '{1}' を呼び出すことは禁止されています",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TSK004: AudioTiming メソッドから InitTiming 構造体のフィールドを変更してはいけません。
    /// </summary>
    public static readonly DiagnosticDescriptor AudioTimingModifiesInitTimingField = new(
        id: "TSK004",
        title: "AudioTiming メソッドから InitTiming 構造体のフィールド変更は禁止されています",
        messageFormat: "AudioTiming メソッド '{0}' 内で InitTiming 構造体 '{1}' のフィールドを変更することは禁止されています",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// TSK005: AudioTiming メソッド内でヒープメモリを確保してはいけません。
    /// </summary>
    public static readonly DiagnosticDescriptor AudioTimingAllocatesHeap = new(
        id: "TSK005",
        title: "AudioTiming メソッド内でのヒープメモリ確保は禁止されています",
        messageFormat: "AudioTiming メソッド '{0}' 内でヒープメモリを確保することは禁止されています: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ========================================
    // 警告レベル（「基本的に」違反）
    // ========================================

    /// <summary>
    /// TSK101: EventTiming メソッド内でヒープメモリの確保は避けるべきです。
    /// </summary>
    public static readonly DiagnosticDescriptor EventTimingAllocatesHeap = new(
        id: "TSK101",
        title: "EventTiming メソッド内でのヒープメモリ確保は避けてください",
        messageFormat: "EventTiming メソッド '{0}' 内でヒープメモリを確保しています: {1}",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// TSK102: AudioTiming メソッドから EventTiming メソッドの呼び出しは基本的に避けるべきです。
    /// </summary>
    public static readonly DiagnosticDescriptor AudioTimingCallsEventTiming = new(
        id: "TSK102",
        title: "AudioTiming メソッドから EventTiming メソッドの呼び出しは基本的に避けてください",
        messageFormat: "AudioTiming メソッド '{0}' 内で EventTiming メソッド '{1}' を呼び出しています。",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "AudioTiming 属性が付与されたメソッドから EventTiming 属性が付与されたメソッドを呼び出すことは基本的に避けるべきです。条件分岐などで呼び出し回数が十分に少ない場合のみ、'// EVENT CALL' コメントを付けて呼び出すことができます.");

    /// <summary>
    /// TSK103: AudioTiming メソッドから EventTiming 構造体のフィールド変更は基本的に避けるべきです。
    /// </summary>
    public static readonly DiagnosticDescriptor AudioTimingModifiesEventTimingField = new(
        id: "TSK103",
        title: "AudioTiming メソッドから EventTiming 構造体のフィールド変更は基本的に避けてください",
        messageFormat: "AudioTiming メソッド '{0}' 内で EventTiming 構造体 '{1}' のフィールドを変更しています",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}
