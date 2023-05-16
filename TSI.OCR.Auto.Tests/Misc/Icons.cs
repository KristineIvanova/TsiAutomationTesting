using System;

namespace TSI.OCR.Auto.Tests.Misc;

public static class Test {
    private static int successes;
    private static int failures;

    public static string Tick = "\u2713";
    public static string Cross = "\u2717";

    public static void It(string name, Action body) {
        try {
            body();
            Console.WriteLine(AnsiColors.Color($"<green>{Tick}</green> {name}"));
            successes++;
        }
        catch (Exception e) {
            Console.WriteLine(AnsiColors.Color($"<red>{Cross}</red> {name}"));
            Console.Error.WriteLine(e);
            failures++;
        }
    }
}