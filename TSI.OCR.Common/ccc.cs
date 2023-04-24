using IronOcr;
using Xunit;
using static IronOcr.Installation;

namespace TSI.OCR.Common;

public class IronOcrSampleTest
{
    public const string File1 = "Group menu_RU.pdf";
    public const string File2 = "Group menu_RU.pdf";

    [Obsolete("Obsolete")]
    public IronOcrSampleTest()
    {
        LicenseKey =
            "IRONOCR.KRISTINEIVANOVA.6207-27D880053B-AMA6RG6HC4R4PZK-TKOH4CVAOVC6-4ABBFTHCUUB3-3XCIZEXLTLXY-5W2H6PREKRW5-7QLRKF-TUHO232GO52JEA-DEPLOYMENT.TRIAL-PQTBXI.TRIAL.EXPIRES.26.APR.2023";
    }

    [Theory]
    [MemberData(nameof(GetFilesToTest))]
    public void IronOcrImageTest(string fileName)
    {
        var ocr = new IronTesseract();
        using var input = new OcrInput();
        var imgText = "";
        if (fileName == File1) imgText = "Test";
        if (fileName == File2) imgText = "GRANDE LARGE";

        var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
        var path = $@"{dir}\images\{fileName}";

        input.AddImage(path);
        var result = ocr.Read(input);

        Assert.True(result.Confidence > 80, "The result needs to be of high confidence.");
        Assert.True(result.Text == imgText, $"The text needs to say '{imgText}'.");
    }

    [Fact]
    public void IronOcrPdfTest()
    {
        var Ocr = new IronTesseract();
        var result = new IronTesseract().Read(@"images\556cad0b-f931-4e70-b497-789b86d8aea9.png");
        Console.WriteLine(result.Text);
        using (var input = new OcrInput())
        {
            var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
            var path = $@"{dir}\pdfs\Group menu_RU.pdf";
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
}