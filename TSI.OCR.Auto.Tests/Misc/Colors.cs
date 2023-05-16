using System.Collections.Generic;

namespace TSI.OCR.Auto.Tests.Misc;

public static class AnsiColors
{
    public const string Bold = "\u001b[37;1m";
    public const string Strike = "\u001b[37;9m";
    public const string Cyan = "\u001b[36m";
    public const string Green = "\u001b[32m";
    public const string Red = "\u001b[31m";
    public const string Yellow = "\u001b[33m";
    public const string Reset = "\u001b[0m";

    public static string Color(string message)
    {
        string coloredMessage = message;

        var colorDict = new Dictionary<string, string>()
        {
            { "bold", Bold },
            { "strike", Strike },
            { "cyan", Cyan },
            { "green", Green },
            { "red", Red },
            { "yellow", Yellow }
        };

        foreach (var color in colorDict)
        {
            string colorTag = $"<{color.Key}>";
            string colorEndTag = $"</{color.Key}>";

            coloredMessage = coloredMessage
                .Replace(colorTag, color.Value)
                .Replace(colorEndTag, Reset);
        }

        return coloredMessage;
    }
}
