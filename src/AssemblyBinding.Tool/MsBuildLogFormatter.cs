﻿using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace Oleander.Assembly.Binding.Tool;

internal static class MsBuildLogFormatter
{
    private const string messageFormat = "{origin}({line},{column}) : {subCategory} {category} {code} : {text}";

    public static string CreateMSBuildMessageFormat(string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var linePadRight = $"({line},0)".PadRight(9);
        return $"{FileName}{linePadRight} : {subCategory} message {code} : {text}";
    }

    public static string CreateMSBuildWarningFormat(string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var linePadRight = $"({line},0)".PadRight(9);
        return $"{FileName}{linePadRight} : {subCategory} warning {code} : {text}";
    }

    public static string CreateMSBuildErrorFormat(string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var linePadRight = $"({line},0)".PadRight(9);
        return $"{FileName}{linePadRight} : {subCategory} error {code} : {text}";
    }

    public static string CreateMSBuildMessage(this ILogger logger, string code, string text, int line, string subCategory)
    {
        return CreateMSBuildMessage(logger, code, text, subCategory, line);
    }

    public static string CreateMSBuildMessage(this ILogger logger, string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var message = CreateMSBuildMessageFormat(code, text, subCategory, line);

        Console.WriteLine(message);
        logger.LogInformation("MSBuildMessage: {message}", message);
        return message;
    }


    public static string CreateMSBuildWarning(string code, string text, int line, string subCategory)
    {
        return CreateMSBuildWarning(code, text, subCategory, line);
    }

    public static string CreateMSBuildWarning(string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var message = CreateMSBuildWarningFormat(code, text, subCategory, line);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();

        return message;
    }


    public static string CreateMSBuildError(string code, string text, int line, string subCategory)
    {
        return CreateMSBuildError(code, text, subCategory, line);
    }

    public static string CreateMSBuildError(string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        var message = CreateMSBuildErrorFormat(code, text, subCategory, line);

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();

        return message;
    }


    public static string CreateMSBuildWarning(this ILogger logger, string code, string text, int line, string subCategory)
    {
        return CreateMSBuildWarning(logger, code, text, subCategory, line);
    }

    public static string CreateMSBuildWarning(this ILogger logger, string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        logger.LogWarning(messageFormat, FileName, line, 0, subCategory, "warning", code, text);
        return CreateMSBuildWarning(code, text, subCategory, line);
    }


    public static string CreateMSBuildError(this ILogger logger, string code, string text, int line, string subCategory)
    {
        return CreateMSBuildError(logger, code, text, subCategory, line);
    }

    public static string CreateMSBuildError(this ILogger logger, string code, string text, string subCategory, [CallerLineNumber] int line = 0)
    {
        logger.LogError(messageFormat, FileName, line, 0, subCategory, "error", code, text);
        return CreateMSBuildError(code, text, subCategory, line);
    }

    private static readonly string FileName = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
}