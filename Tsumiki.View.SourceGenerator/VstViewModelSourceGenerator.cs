using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsumiki.View.SourceGenerator;

[Generator]
public class VstViewModelSourceGenerator : IIncrementalGenerator
{
    private static readonly Dictionary<string, string> ParameterAttributeNameToAudioName = new()
    {
        ["VstParameterAttribute"] = "AudioParameter",
        ["VstRangeParameterAttribute"] = "AudioRangeParameter",
        ["VstBoolParameterAttribute"] = "AudioBoolParameter",
        ["VstStringListParameterAttribute"] = "AudioStringListParameter",
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(classDeclarations, static (spc, source) => Execute(source!, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is InterfaceDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private class ModelInfo(INamedTypeSymbol generateType, INamedTypeSymbol definitionType)
    {
        public INamedTypeSymbol GenerateType { get; } = generateType;
        public INamedTypeSymbol DefinitionType { get; } = definitionType;
    }

    private static ModelInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not INamedTypeSymbol generateSymbol)
            return null;

        var attribute = generateSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Tsumiki.Metadata.VstModelAttribute");
        if (attribute is null)
            return null;

        if (attribute.ConstructorArguments[0].Value is not string)
            return null;

        if (attribute.ConstructorArguments[1].Value is not INamedTypeSymbol definitionType)
            return null;

        return new ModelInfo(generateSymbol, definitionType);
    }

    private static void Execute(ModelInfo modelInfo, SourceProductionContext context)
    {
        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine($"namespace {modelInfo.GenerateType.ContainingNamespace.ToDisplayString()};");

        foreach (var interfaceType in GetUnitTypesRecursion(modelInfo.DefinitionType))
        {
            GenerateViewModelInterface(sourceBuilder, interfaceType);
        }

        var sourceText = SourceText.From(sourceBuilder.ToString(), Encoding.UTF8);
        context.AddSource($"{modelInfo.GenerateType.Name}.g.cs", sourceText);
    }

    private static IPropertySymbol[] GetUnitProperties(INamedTypeSymbol typeSymbol)
    {
        return [.. typeSymbol.AllInterfaces.SelectMany(s => s.GetMembers()).Concat(typeSymbol.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "VstUnitAttribute"))];
    }

    private static INamedTypeSymbol[] GetUnitTypesRecursion(INamedTypeSymbol typeSymbol)
    {
        var hashSet = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var result = new List<INamedTypeSymbol>([typeSymbol]);
        hashSet.Add(typeSymbol);

        for (var i = 0; i < result.Count; i++)
        {
            var targetProperties = typeSymbol.AllInterfaces.SelectMany(s => s.GetMembers()).Concat(typeSymbol.GetMembers())
                .OfType<IPropertySymbol>()
                .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "VstUnitAttribute"));

            foreach (var unitProperty in targetProperties)
            {
                if (unitProperty.Type is INamedTypeSymbol namedTypeSymbol && hashSet.Add(namedTypeSymbol))
                {
                    result.Add(namedTypeSymbol);
                }
            }
        }

        return [.. result];
    }

    private static IPropertySymbol[] GetParameterProperties(INamedTypeSymbol typeSymbol)
    {
        return [.. typeSymbol.AllInterfaces.SelectMany(s => s.GetMembers()).Concat(typeSymbol.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name)))];
    }

    private static void GenerateViewModelInterface(StringBuilder sourceBuilder, INamedTypeSymbol interfaceType)
    {
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"public partial interface {GetViewInterfaceName(interfaceType)}");
        sourceBuilder.AppendLine("{");

        foreach (var property in GetParameterProperties(interfaceType))
        {
            GeneratePropertyImplementation(sourceBuilder, property);
        }

        sourceBuilder.AppendLine();
        foreach (var unitProperty in GetUnitProperties(interfaceType))
        {
            sourceBuilder.AppendLine($"    public {GetViewInterfaceName(unitProperty.Type)} {unitProperty.Name} {{ get; }}");
        }

        sourceBuilder.AppendLine("}");
    }

    private static string GetViewInterfaceName(ITypeSymbol interfaceType)
    {
        return interfaceType.Name.Replace("Model", "ViewModel").Replace("Unit", "ViewModel");
    }

    private static void GeneratePropertyImplementation(StringBuilder sourceBuilder, IPropertySymbol property)
    {
        var attribute = property.GetAttributes().First(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name));
        var returnType = property.Type.ToDisplayString();
        var interfaceType = attribute.AttributeClass?.Name switch
        {
            "VstRangeParameterAttribute" => $"IRangeViewParameter<{returnType}>",
            _ => $"IViewParameter<{returnType}>",
        };

        sourceBuilder.AppendLine($"    public {interfaceType} {property.Name} {{ get; }}");
    }
}
