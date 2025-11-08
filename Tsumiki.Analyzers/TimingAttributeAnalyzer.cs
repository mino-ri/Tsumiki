using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Tsumiki.Analyzers;

/// <summary>
/// Timing 属性の制限をチェックする Roslyn Analyzer です。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TimingAttributeAnalyzer : DiagnosticAnalyzer
{
    private const string InitTimingAttributeName = "Tsumiki.Metadata.InitTimingAttribute";
    private const string EventTimingAttributeName = "Tsumiki.Metadata.EventTimingAttribute";
    private const string AudioTimingAttributeName = "Tsumiki.Metadata.AudioTimingAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.EventTimingCallsInitTiming,
        DiagnosticDescriptors.EventTimingModifiesInitTimingField,
        DiagnosticDescriptors.AudioTimingCallsInitTiming,
        DiagnosticDescriptors.AudioTimingModifiesInitTimingField,
        DiagnosticDescriptors.AudioTimingAllocatesHeap,
        DiagnosticDescriptors.EventTimingAllocatesHeap,
        DiagnosticDescriptors.AudioTimingCallsEventTiming,
        DiagnosticDescriptors.AudioTimingModifiesEventTimingField);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // メソッド呼び出しをチェック
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);

        // オブジェクト生成（ヒープメモリ確保）をチェック
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);

        // 代入式（フィールド変更）をチェック
        context.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);
    }

    /// <summary>
    /// メソッド呼び出しを分析します。
    /// </summary>
    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // 呼び出し元のメソッドを取得
        var callerMethod = GetContainingMethod(invocation);
        if (callerMethod == null)
            return;

        var callerSymbol = context.SemanticModel.GetDeclaredSymbol(callerMethod);
        if (callerSymbol == null)
            return;

        // 呼び出し元の Timing 属性を取得
        var callerTiming = GetTimingAttribute(callerSymbol);
        if (callerTiming == TimingType.None)
            return;

        // 呼び出し先のメソッドを取得
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol calleeSymbol)
            return;

        // 呼び出し先の Timing 属性を取得
        var calleeTiming = GetTimingAttribute(calleeSymbol);
        if (calleeTiming == TimingType.None)
            return;

        // ルール違反をチェック
        CheckMethodCallViolation(context, invocation, callerSymbol, callerTiming, calleeSymbol, calleeTiming);
    }

    /// <summary>
    /// オブジェクト生成（ヒープメモリ確保）を分析します。
    /// </summary>
    private void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;

        // 生成しているメソッドを取得
        var method = GetContainingMethod(creation);
        if (method == null)
            return;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (methodSymbol == null)
            return;

        // メソッドの Timing 属性を取得
        var timing = GetTimingAttribute(methodSymbol);

        // 生成している型を取得
        var typeInfo = context.SemanticModel.GetTypeInfo(creation);
        if (typeInfo.Type == null)
            return;

        // クラス型の場合のみチェック（構造体は値型なのでヒープに確保されない）
        if (typeInfo.Type.TypeKind is not TypeKind.Class and not TypeKind.Interface and not TypeKind.Delegate)
            return;

        // AudioTiming メソッド内でのヒープメモリ確保はエラー
        if (timing == TimingType.Audio)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.AudioTimingAllocatesHeap,
                creation.GetLocation(),
                methodSymbol.Name,
                $"new {typeInfo.Type.Name}()");
            context.ReportDiagnostic(diagnostic);
        }
        // EventTiming メソッド内でのヒープメモリ確保は警告
        else if (timing == TimingType.Event)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EventTimingAllocatesHeap,
                creation.GetLocation(),
                methodSymbol.Name,
                $"new {typeInfo.Type.Name}()");
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// 代入式（フィールド変更）を分析します。
    /// </summary>
    private void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        // 代入元のメソッドを取得
        var method = GetContainingMethod(assignment);
        if (method == null)
            return;

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(method);
        if (methodSymbol == null)
            return;

        // メソッドの Timing 属性を取得
        var methodTiming = GetTimingAttribute(methodSymbol);
        if (methodTiming is TimingType.None or TimingType.Init)
            return;

        // 代入先を取得
        var leftSymbol = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
        if (leftSymbol == null)
            return;

        // フィールドまたはプロパティの場合のみチェック
        if (leftSymbol.Kind is not SymbolKind.Field and not SymbolKind.Property)
            return;

        // フィールド/プロパティが属する型を取得
        var containingType = leftSymbol.ContainingType;
        if (containingType == null)
            return;

        // 型の Timing 属性を取得
        var typeTiming = GetTimingAttribute(containingType);
        if (typeTiming == TimingType.None)
            return;

        // ルール違反をチェック
        CheckFieldModificationViolation(context, assignment, methodSymbol, methodTiming, containingType, typeTiming);
    }

    /// <summary>
    /// メソッド呼び出しのルール違反をチェックします。
    /// </summary>
    private void CheckMethodCallViolation(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol caller,
        TimingType callerTiming,
        IMethodSymbol callee,
        TimingType calleeTiming)
    {
        // EventTiming → InitTiming 呼び出しは禁止（エラー）
        if (callerTiming == TimingType.Event && calleeTiming == TimingType.Init)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EventTimingCallsInitTiming,
                invocation.GetLocation(),
                caller.Name,
                callee.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // AudioTiming → InitTiming 呼び出しは禁止（エラー）
        if (callerTiming == TimingType.Audio && calleeTiming == TimingType.Init)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.AudioTimingCallsInitTiming,
                invocation.GetLocation(),
                caller.Name,
                callee.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // AudioTiming → EventTiming 呼び出しは基本的に避けるべき（警告）
        // ただし、`// EVENT CALL` コメントがある場合は許可
        if (callerTiming == TimingType.Audio && calleeTiming == TimingType.Event)
        {
            if (!HasEventCallComment(invocation))
            {
                var diagnostic = Diagnostic.Create(
                    DiagnosticDescriptors.AudioTimingCallsEventTiming,
                    invocation.GetLocation(),
                    caller.Name,
                    callee.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// フィールド変更のルール違反をチェックします。
    /// </summary>
    private void CheckFieldModificationViolation(
        SyntaxNodeAnalysisContext context,
        AssignmentExpressionSyntax assignment,
        IMethodSymbol method,
        TimingType methodTiming,
        INamedTypeSymbol fieldType,
        TimingType fieldTypeTiming)
    {
        // EventTiming → InitTiming 構造体フィールド変更は禁止（エラー）
        if (methodTiming == TimingType.Event && fieldTypeTiming == TimingType.Init)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.EventTimingModifiesInitTimingField,
                assignment.GetLocation(),
                method.Name,
                fieldType.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // AudioTiming → InitTiming 構造体フィールド変更は禁止（エラー）
        if (methodTiming == TimingType.Audio && fieldTypeTiming == TimingType.Init)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.AudioTimingModifiesInitTimingField,
                assignment.GetLocation(),
                method.Name,
                fieldType.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // AudioTiming → EventTiming 構造体フィールド変更は基本的に避けるべき（警告）
        if (methodTiming == TimingType.Audio && fieldTypeTiming == TimingType.Event)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.AudioTimingModifiesEventTimingField,
                assignment.GetLocation(),
                method.Name,
                fieldType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    /// <summary>
    /// ノードを含むメソッドを取得します。
    /// </summary>
    private static MethodDeclarationSyntax? GetContainingMethod(SyntaxNode node)
    {
        return node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
    }

    /// <summary>
    /// シンボルの Timing 属性を取得します。
    /// </summary>
    private static TimingType GetTimingAttribute(ISymbol symbol)
    {
        var attributes = symbol.GetAttributes();

        foreach (var attr in attributes)
        {
            var attrName = attr.AttributeClass?.ToDisplayString();
            if (attrName == InitTimingAttributeName)
                return TimingType.Init;
            if (attrName == EventTimingAttributeName)
                return TimingType.Event;
            if (attrName == AudioTimingAttributeName)
                return TimingType.Audio;
        }

        return TimingType.None;
    }

    /// <summary>
    /// ノードの前に `// EVENT CALL` コメントがあるかチェックします。
    /// </summary>
    private static bool HasEventCallComment(SyntaxNode node)
    {
        // InvocationExpression 自体のトリビアをチェック
        if (CheckTriviaForEventCall(node.GetLeadingTrivia()))
            return true;

        // 親ノード（通常は ExpressionStatement）のトリビアもチェック
        return node.Parent != null && CheckTriviaForEventCall(node.Parent.GetLeadingTrivia());
    }

    /// <summary>
    /// トリビアリスト内に `// EVENT CALL` コメントがあるかチェックします。
    /// </summary>
    private static bool CheckTriviaForEventCall(SyntaxTriviaList triviaList)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                var comment = trivia.ToString();
                if (comment.Contains("EVENT CALL"))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Timing 属性の種類を表します。
    /// </summary>
    private enum TimingType
    {
        None,
        Init,
        Event,
        Audio
    }
}
