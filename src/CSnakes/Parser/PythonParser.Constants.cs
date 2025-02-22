﻿using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.Globalization;

namespace PythonSourceGenerator.Parser;
public static partial class PythonParser
{
    public static TextParser<char> UnderScoreOrDigit { get; } =
        Character.Matching(char.IsDigit, "digit").Or(Character.EqualTo('_'));

    public static TextParser<Unit> IntegerConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from firstdigit in Character.Digit
        from digits in UnderScoreOrDigit.Many().OptionalOrDefault([])
        select Unit.Value;

    public static TextParser<Unit> DecimalConstantToken { get; } =
            from sign in Character.EqualTo('-').OptionalOrDefault()
            from first in Character.Digit
            from rest in UnderScoreOrDigit.Or(Character.In('.', 'e', 'E', '+', '-')).IgnoreMany()
            select Unit.Value;

    public static TextParser<Unit> HexidecimalConstantToken { get; } =
        from prefix in Span.EqualTo("0x")
        from digits in Character.EqualTo('_').Or(Character.HexDigit).AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> BinaryConstantToken { get; } =
        from prefix in Span.EqualTo("0b")
        from digits in Character.In('0', '1', '_').AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    public static TokenListParser<PythonToken, PythonConstant> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Select(s => new PythonConstant(s))
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Select(s => new PythonConstant(s))
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonToken, PythonConstant> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Decimal)
        .Select(token => new PythonConstant(double.Parse(token.ToStringValue().Replace("_", ""), NumberStyles.Float, CultureInfo.InvariantCulture)))
        .Named("Decimal Constant");

    public static TokenListParser<PythonToken, PythonConstant> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.Integer)
        .Select(d => new PythonConstant(long.Parse(d.ToStringValue().Replace("_", ""), NumberStyles.Integer)))
        .Named("Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant> HexidecimalIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.HexidecimalInteger)
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.HexidecimalInteger, IntegerValue = long.Parse(d.ToStringValue().Substring(2).Replace("_", ""), NumberStyles.HexNumber) })
        .Named("Hexidecimal Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant> BinaryIntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.BinaryInteger)
        // TODO: Consider Binary Format specifier introduced in .NET 8 https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings#binary-format-specifier-b
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.BinaryInteger, IntegerValue = (long)Convert.ToUInt64(d.ToStringValue().Substring(2).Replace("_", ""), 2) })
        .Named("Binary Integer Constant");

    public static TokenListParser<PythonToken, PythonConstant> BoolConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.True).Or(Token.EqualTo(PythonToken.False))
        .Select(d => new PythonConstant(d.Kind == PythonToken.True))
        .Named("Bool Constant");

    public static TokenListParser<PythonToken, PythonConstant> NoneConstantTokenizer { get; } =
        Token.EqualTo(PythonToken.None)
        .Select(d => new PythonConstant { Type = PythonConstant.ConstantType.None })
        .Named("None Constant");

    // Any constant value
    public static TokenListParser<PythonToken, PythonConstant?> ConstantValueTokenizer { get; } =
        DecimalConstantTokenizer.AsNullable()
        .Or(IntegerConstantTokenizer.AsNullable())
        .Or(HexidecimalIntegerConstantTokenizer.AsNullable())
        .Or(BinaryIntegerConstantTokenizer.AsNullable())
        .Or(BoolConstantTokenizer.AsNullable())
        .Or(NoneConstantTokenizer.AsNullable())
        .Or(DoubleQuotedStringConstantTokenizer.AsNullable())
        .Or(SingleQuotedStringConstantTokenizer.AsNullable())
        .Named("Constant");

    static class ConstantParsers
    {
        public static TextParser<string> DoubleQuotedString { get; } =
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('"'))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('"')
            select new string(chars);

        public static TextParser<string> SingleQuotedString { get; } =
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('\''))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('\'')
            select new string(chars);
    }
}
