﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;

namespace PythonSourceGenerator.Reflection;

public static class TypeReflection
{
    public static TypeSyntax AsPredefinedType(PythonTypeSpec pythonType)
    {
        // If type is an alias, e.g. "list[int]", "list[float]", etc.
        if (pythonType.HasArguments())
        {
            var genericName = pythonType.Name.ToLowerInvariant();
            // Get last occurrence of ] in pythonType
            return genericName switch
            {
                "list" => CreateListType(pythonType.Arguments[0]),
                "tuple" => CreateTupleType(pythonType.Arguments),
                "dict" => CreateDictionaryType(pythonType.Arguments[0], pythonType.Arguments[1]),
                // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
                _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
            };
        }
        return pythonType.Name switch
        {
            "int" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
            "str" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)),
            "float" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
            "bool" => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
            // Todo more types...
            _ => SyntaxFactory.ParseTypeName("PyObject"),// TODO : Should be nullable?
        };
    }

    private static TypeSyntax CreateDictionaryType(PythonTypeSpec keyType, PythonTypeSpec valueType) => CreateGenericType("IReadOnlyDictionary", [
            AsPredefinedType(keyType),
            AsPredefinedType(valueType)
            ]);

    private static TypeSyntax CreateTupleType(PythonTypeSpec[] tupleTypes)
    {
        if (tupleTypes.Length == 1)
        {
            return CreateGenericType("ValueTuple", tupleTypes.Select(AsPredefinedType));
        }

        IEnumerable<TupleElementSyntax> tupleTypeSyntaxGroups = tupleTypes.Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / 7)
            .Select(x => x.Select(v => v.Value))
            .Select(typeSpecs => typeSpecs.Select(AsPredefinedType))
            .SelectMany(item => item.Select(SyntaxFactory.TupleElement));

        return SyntaxFactory.TupleType(
            SyntaxFactory.Token(SyntaxKind.OpenParenToken),
            SyntaxFactory.SeparatedList(tupleTypeSyntaxGroups),
            SyntaxFactory.Token(SyntaxKind.CloseParenToken));
    }

    private static TypeSyntax CreateListType(PythonTypeSpec genericOf) => CreateGenericType("IEnumerable", [AsPredefinedType(genericOf)]);

    internal static TypeSyntax CreateGenericType(string typeName, IEnumerable<TypeSyntax> genericArguments) =>
        SyntaxFactory.GenericName(
            SyntaxFactory.Identifier(typeName))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(
                        genericArguments)));
}
