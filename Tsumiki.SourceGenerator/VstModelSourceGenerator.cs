using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Tsumiki.SourceGenerator;

[Generator]
public class VstModelSourceGenerator : IIncrementalGenerator
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
        return node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    private class ModelInfo(string modelName, INamedTypeSymbol classSymbol, INamedTypeSymbol definitionType)
    {
        public string ModelName { get; } = modelName;
        public INamedTypeSymbol ClassSymbol { get; } = classSymbol;
        public INamedTypeSymbol DefinitionType { get; } = definitionType;
    }

    private static ModelInfo? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            return null;

        var attribute = classSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "Tsumiki.Metadata.VstModelAttribute");
        if (attribute is null)
            return null;

        if (attribute.ConstructorArguments[0].Value is not string modelName)
            return null;

        if (attribute.ConstructorArguments[1].Value is not INamedTypeSymbol definitionType)
            return null;

        return new ModelInfo(modelName, classSymbol, definitionType);
    }

    private static void Execute(ModelInfo modelInfo, SourceProductionContext context)
    {
        var classSymbol = modelInfo.ClassSymbol;
        var definitionType = modelInfo.DefinitionType;

        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine("#nullable enable");
        sourceBuilder.AppendLine("using NPlug;");
        sourceBuilder.AppendLine("using Tsumiki.View;");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine($"namespace {classSymbol.ContainingNamespace.ToDisplayString()};");
        sourceBuilder.AppendLine();

        // Generate unit classes
        foreach (var unitProperty in GetUnitProperties(definitionType))
        {
            GenerateUnitClass(sourceBuilder, unitProperty, 0);
        }

        // Generate partial class implementation
        GenerateClassImplementation(
            sourceBuilder,
            className: classSymbol.Name,
            interfaceType: definitionType,
            parameterIdOffset: 0,
            baseClassConstructor: $"base(\"{modelInfo.ModelName}\", DefaultProgramListBuilder)",
            includeByPassParameter: true);

        var sourceText = SourceText.From(sourceBuilder.ToString(), Encoding.UTF8);
        context.AddSource($"{classSymbol.Name}.g.cs", sourceText);
    }

    private static IPropertySymbol[] GetUnitProperties(INamedTypeSymbol typeSymbol)
    {
        return [.. typeSymbol.AllInterfaces.SelectMany(s => s.GetMembers()).Concat(typeSymbol.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "VstUnitAttribute"))];
    }

    private static IPropertySymbol[] GetParameterProperties(INamedTypeSymbol typeSymbol)
    {
        return [.. typeSymbol.AllInterfaces.SelectMany(s => s.GetMembers()).Concat(typeSymbol.GetMembers())
            .OfType<IPropertySymbol>()
            .Where(p => p.GetAttributes().Any(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name)))];
    }

    private static void GenerateClassImplementation(
        StringBuilder sourceBuilder,
        string className,
        INamedTypeSymbol interfaceType,
        int parameterIdOffset,
        string? baseClassConstructor,
        bool includeByPassParameter)
    {
        var unitProperties = GetUnitProperties(interfaceType);
        var parameterProperties = GetParameterProperties(interfaceType);

        // Determine base class
        var baseClass = includeByPassParameter ? "AudioProcessorModel" : "AudioUnit";
        sourceBuilder.AppendLine($"public partial class {className} : {baseClass}, {interfaceType.ToDisplayString()}");
        sourceBuilder.AppendLine("{");

        // Generate fields for parameters
        foreach (var property in parameterProperties)
        {
            var paramType = GetParameterFieldType(property);
            sourceBuilder.AppendLine($"    public {paramType} {property.Name}Parameter {{ get; }}");
        }

        // Generate fields for units
        foreach (var unitProperty in unitProperties)
        {
            var unitClassName = GetUnitClassName(unitProperty);
            sourceBuilder.AppendLine($"    public {unitClassName} {unitProperty.Name} {{ get; }}");
        }

        sourceBuilder.AppendLine();

        // Generate interface implementations for parameters
        foreach (var property in parameterProperties)
        {
            GeneratePropertyImplementation(sourceBuilder, property);
        }

        // Generate interface implementations for units
        foreach (var unitProperty in unitProperties)
        {
            sourceBuilder.AppendLine($"    {unitProperty.Type.ToDisplayString()} {interfaceType.ToDisplayString()}.{unitProperty.Name} => {unitProperty.Name};");
        }

        sourceBuilder.AppendLine();

        // Generate constructor
        var constructorBase = baseClassConstructor != null ? $" : {baseClassConstructor}" : "";
        sourceBuilder.AppendLine($"    public {className}(){constructorBase}");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine("        AddUserParameters();");

        // Initialize parameters
        foreach (var property in parameterProperties)
        {
            GenerateParameterInitialization(sourceBuilder, property, parameterIdOffset);
        }

        // Initialize units
        foreach (var unitProperty in unitProperties)
        {
            var unitClassName = GetUnitClassName(unitProperty);
            sourceBuilder.AppendLine($"        {unitProperty.Name} = AddUnit(new {unitClassName}());");
        }

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("    partial void AddUserParameters();");
        sourceBuilder.AppendLine("}");
        sourceBuilder.AppendLine();

        GenerateViewModelImplementation(sourceBuilder, className, interfaceType);
    }

    private static void GenerateViewModelImplementation(StringBuilder sourceBuilder, string className, INamedTypeSymbol interfaceType)
    {
        // Determine base class
        sourceBuilder.AppendLine($"public partial class {GetViewModelName(className)}({className} unit, TsumikiController? controller) : {GetViewInterfaceName(interfaceType)}");
        sourceBuilder.AppendLine("{");

        foreach (var property in GetParameterProperties(interfaceType))
        {
            GenerateViewModelProperty(sourceBuilder, property);
        }

        sourceBuilder.AppendLine();
        foreach (var unitProperty in GetUnitProperties(interfaceType))
        {
            var viewModelInterfaceName = GetViewInterfaceName(unitProperty.Type);
            var viewModelClassName = GetViewModelName(GetUnitClassName(unitProperty));
            sourceBuilder.AppendLine($"    public {viewModelInterfaceName} {unitProperty.Name} {{ get; }} = new {viewModelClassName}(unit.{unitProperty.Name}, controller);");
        }

        sourceBuilder.AppendLine("}");
    }

    private static void GenerateViewModelProperty(StringBuilder sourceBuilder, IPropertySymbol property)
    {
        var attribute = property.GetAttributes().First(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name));
        var returnType = property.Type.ToDisplayString();
        var interfaceType = attribute.AttributeClass?.Name switch
        {
            "VstRangeParameterAttribute" => $"IRangeViewParameter<{returnType}>",
            _ => $"IViewParameter<{returnType}>",
        };
        var initialization = (attribute.AttributeClass?.Name, returnType) switch
        {
            ("VstRangeParameterAttribute", "double") => $"new DoubleRangeViewParameter(unit.{property.Name}Parameter, controller)",
            ("VstRangeParameterAttribute", "int") => $"new Int32RangeViewParameter(unit.{property.Name}Parameter, controller)",
            ("VstRangeParameterAttribute", "float") => $"new FloatRangeViewParameter(unit.{property.Name}Parameter, controller)",
            ("VstRangeParameterAttribute", _) => $"new {property.Type.Name}RangeViewParameter(unit.{property.Name}Parameter, controller)",
            ("VstBoolParameterAttribute", _) => $"new BoolViewParameter(unit.{property.Name}Parameter, controller)",
            ("VstStringListParameterAttribute", _) => $"new EnumViewParameter<{returnType}>(unit.{property.Name}Parameter, controller)",
            (_, "float") => $"new FloatViewParameter(unit.{property.Name}Parameter, controller)",
            _ => $"new ViewParameter(unit.{property.Name}Parameter, controller)",
        };

        sourceBuilder.AppendLine($"    public {interfaceType} {property.Name} {{ get; }} = {initialization};");
    }

    private static string GetViewModelName(string className)
    {
        return className.Replace("Model", "ViewModel").Replace("Unit", "ViewModel");
    }

    private static string GetViewInterfaceName(ITypeSymbol interfaceType)
    {
        return interfaceType.Name.Replace("Model", "ViewModel").Replace("Unit", "ViewModel");
    }

    private static void GenerateUnitClass(StringBuilder sourceBuilder, IPropertySymbol unitProperty, int idOffset)
    {
        if (unitProperty.Type is not INamedTypeSymbol unitType)
            return;

        var unitClassName = GetUnitClassName(unitProperty);
        var unitAttribute = unitProperty.GetAttributes()
            .First(a => a.AttributeClass?.Name == "VstUnitAttribute");

        var unitId = (int)unitAttribute.ConstructorArguments[0].Value!;
        var parameterIdOffset = idOffset + (int)unitAttribute.ConstructorArguments[1].Value!;
        var unitName = unitProperty.Name;

        // Generate unit classes
        foreach (var innerProperty in GetUnitProperties(unitType))
        {
            GenerateUnitClass(sourceBuilder, innerProperty, parameterIdOffset);
        }

        GenerateClassImplementation(
            sourceBuilder,
            className: unitClassName,
            interfaceType: unitType,
            parameterIdOffset: parameterIdOffset,
            baseClassConstructor: $"base(\"{unitName}\", id: {unitId})",
            includeByPassParameter: false);

        sourceBuilder.AppendLine();
    }

    private static string GetUnitClassName(IPropertySymbol unitProperty)
    {
        return $"{unitProperty.Name}{unitProperty.Type.Name.Substring(1)}";
    }

    private static string GetParameterFieldType(IPropertySymbol property)
    {
        var attribute = property.GetAttributes().First(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name));
        return ParameterAttributeNameToAudioName[attribute.AttributeClass!.Name];
    }

    private static void GeneratePropertyImplementation(StringBuilder sourceBuilder, IPropertySymbol property)
    {
        var attribute = property.GetAttributes().First(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name));

        var fieldName = $"{property.Name}Parameter";
        var returnType = property.Type.ToDisplayString();
        var valueGetter = attribute.AttributeClass?.Name switch
        {
            "VstBoolParameterAttribute" => $"{fieldName}.Value",
            "VstRangeParameterAttribute" => $"({returnType}){fieldName}.Value",
            "VstStringListParameterAttribute" => $"({returnType}){fieldName}.SelectedItem",
            _ => $"({returnType}){fieldName}.NormalizedValue",
        };
        var valueSetter = attribute.AttributeClass?.Name switch
        {
            "VstBoolParameterAttribute" => $"{fieldName}.Value = value",
            "VstRangeParameterAttribute" => $"{fieldName}.Value = (double)value",
            "VstStringListParameterAttribute" => $"{fieldName}.SelectedItem = (int)value",
            _ => $"{fieldName}.NormalizedValue = (double)value",
        };

        sourceBuilder.AppendLine($"    public {returnType} {property.Name}");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine($"        get => {valueGetter};");
        sourceBuilder.AppendLine($"        set => {valueSetter};");
        sourceBuilder.AppendLine("    }");
    }

    private static string GetEnumFieldsString(INamedTypeSymbol enumType)
    {
        var fieldString = string.Join(", ", enumType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsConst)
            .Select(s => $"\"{s.Name}\""));
        return $"[{fieldString}]";
    }

    private static void GenerateParameterInitialization(StringBuilder sourceBuilder, IPropertySymbol property, int parameterIdOffset)
    {
        var attribute = property.GetAttributes().First(a => a.AttributeClass is not null && ParameterAttributeNameToAudioName.ContainsKey(a.AttributeClass.Name));

        var parameterClassName = ParameterAttributeNameToAudioName[attribute.AttributeClass!.Name];
        sourceBuilder.Append($"        {property.Name}Parameter = AddParameter(new {parameterClassName}(\"{property.Name}\"");

        for (var i = 0; i < attribute.AttributeConstructor!.Parameters.Length; i++)
        {
            var parameterName = attribute.AttributeConstructor!.Parameters[i].Name;
            var value = parameterName switch
            {
                "id" => ((int)attribute.ConstructorArguments[i].Value! + parameterIdOffset).ToString(),
                "items" => GetEnumFieldsString((INamedTypeSymbol)attribute.ConstructorArguments[i].Value!),
                _ => attribute.ConstructorArguments[i].ToCSharpString(),
            };

            sourceBuilder.Append($", {parameterName}: {value}");
        }

        foreach (var namedArgument in attribute.NamedArguments)
        {
            var value = namedArgument.Value.ToCSharpString().Replace("Tsumiki.Metadata.VstParameterFlags", "NPlug.AudioParameterFlags");
            sourceBuilder.Append($", {ToCamelCase(namedArgument.Key)}: {value}");
        }

        sourceBuilder.AppendLine("));");
    }

    private static string ToCamelCase(string name)
    {
        return string.IsNullOrEmpty(name) ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
