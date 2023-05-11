using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IronOcr;
using TSI.OCR.Common.Config;
using Xunit;

namespace TSI.OCR.Auto.Tests;

public class IronOcrSampleTest
{
    public const string File1 = "ru (1).pdf";
    public const string File2 = "ru (1).pdf";

    [Obsolete("Obsolete")]
    public IronOcrSampleTest()
    {
        Installation.LicenseKey =
            "IRONOCR.KRISTINEIVANOVA.6207-3AC6322919-IXBPPJWY2AWMR-K5ACUCUZBZC5-RU4X2Y2U7QYT-6CS7HC4K56FP-RO32SOBCKJV5-KTWREB-THNFP5SEDSWJUA-DEPLOYMENT.TRIAL-5OMJXI.TRIAL.EXPIRES.07.JUN.2023";
    }

    [Theory]
    [MemberData(nameof(GetFilesToTest))]
    public void IronOcrImageTest(string fileName)
    {
        var ocr = new IronTesseract();
        using var input = new OcrInput();
        var imgText = "";
        if (fileName == File1) imgText = "ru (1).pdf";
        if (fileName == File2) imgText = "ru (1).pdf";

        var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
        var path = $@"{dir}\Resource\{fileName}";

        input.AddImage(path);
        var result = ocr.Read(input);

        Assert.True(result.Confidence > 80, "The result needs to be of high confidence.");
        Assert.True(result.Text == imgText, $"The text needs to say '{imgText}'.");
    }

    [Fact]
    public void IronOcrPdfTest()
    {
        var Ocr = new IronTesseract();
        var result = new IronTesseract().Read(@"C:\Users\kristine.ivanova\RiderProjects\TsiAutomationTesting\TSI.OCR.Auto.Tests\Resource\ru (1).pdf");
        Console.WriteLine(result.Text);
        using (var input = new OcrInput())
        {
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
            var path = $@"{dir}\Resource\pdfs\ru (1).pdf";
            input.AddPdf(path);
            var Result = Ocr.Read(input);

            Assert.True(Result.Confidence > 80, "The result needs to be of high confidence.");
            Assert.True(Result.Text == "Test", "The text needs to say 'Test'.");
        }
    }

    public static IEnumerable<object[]> GetFilesToTest()
    {
        return new List<string[]> { new[] { File1 }, new[] { File2 } };
    }
    
    public class TestFilesRepository {
        public string[] GetDirectories() {
            return Directory.EnumerateDirectories(CommonTestConfigs.pathToRootDirectoryWithFiles).ToArray();
        }
    }
}