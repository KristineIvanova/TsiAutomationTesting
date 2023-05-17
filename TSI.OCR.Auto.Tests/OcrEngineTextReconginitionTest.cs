using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronOcr;
using Microsoft.EntityFrameworkCore;
using TSI.OCR.Auto.Tests.Misc;
using TSI.OCR.Common.Config;
using TSI.OCR.Data;
using TSI.OCR.Data.Entities;
using Xunit;
using static IronOcr.Installation;

namespace TSI.OCR.Auto.Tests;

public class IronOcrSampleTest {
    private readonly string rootPath;
    
    public IronOcrSampleTest()
    {
        LicenseKey =
            "IRONOCR.KRISTINEIVANOVA.6207-3AC6322919-IXBPPJWY2AWMR-K5ACUCUZBZC5-RU4X2Y2U7QYT-6CS7HC4K56FP-RO32SOBCKJV5-KTWREB-THNFP5SEDSWJUA-DEPLOYMENT.TRIAL-5OMJXI.TRIAL.EXPIRES.07.JUN.2023";
        rootPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName, "Resource");
    }

    [Theory]
    [MemberData(nameof(GetFilesToTest))]
    public async Task IronOcrImageTest(string filePath) {
        var srcDocument = await FindDocument(filePath);
        var pdfReader = new PdfFileReader(filePath);
        var targetDocument = await pdfReader.ParseFile();

        // Find fields that doesn't exist in target document
        var srcFields = srcDocument.Fields.Where(x => 
            !targetDocument.Fields.Any(y => y.Value == x.Value && y.X == x.X &&
                                            y.Y == x.Y && y.Width == x.Width &&
                                            y.Height == x.Height && x.Page == y.Page)).ToList();
        
        // Find fields that doesn't exist in target document
        var targetFields = targetDocument.Fields.Where(x => 
            !srcDocument.Fields.Any(y => y.Value == x.Value && y.X == x.X &&
                                          y.Y == x.Y && y.Width == x.Width &&
                                          y.Height == x.Height && x.Page == y.Page)).ToList();
        
        // Find fields that exist in both documents but have different values
        var differentFields = srcDocument.Fields.Where(x => 
            targetDocument.Fields.Any(y => y.Value != x.Value && y.X == x.X &&
                                            y.Y == x.Y && y.Width == x.Width &&
                                            y.Height == x.Height && x.Page == y.Page)).ToList();
        
        var result = $"Source document has {srcFields.Count} fields that are not present in target document. " +
                        $"Target document has {targetFields.Count} fields that are not present in source document. " +
                        $"There are {differentFields.Count} fields that have different values in both documents.";
        
        Console.Error.WriteLine(AnsiColors.Color($"<red>{result}</red>"));
        
        Assert.True(srcFields.Count == 0, result);
        Assert.True(targetFields.Count == 0, result);
        Assert.True(differentFields.Count == 0, result);
    }

    // [Fact]
    // public void IronOcrPdfTest()
    // {
    //     var Ocr = new IronTesseract();
    //     var result = new IronTesseract().Read(@"C:\Users\kristine.ivanova\RiderProjects\TsiAutomationTesting\TSI.OCR.Auto.Tests\Resource\ru (1).pdf");
    //     Console.WriteLine(result.Text);
    //     using (var input = new OcrInput())
    //     {
    //         var dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
    //         var path = $@"{dir}\Resource\pdfs\ru (1).pdf";
    //         input.AddPdf(path);
    //         var Result = Ocr.Read(input);
    //
    //         Assert.True(Result.Confidence > 80, "The result needs to be of high confidence.");
    //         Assert.True(Result.Text == "Test", "The text needs to say 'Test'.");
    //     }
    // }

    private async Task<Document> FindDocument(string filePath) {
        var fileName = Path.GetFileName(filePath);
        await using var db = new DocumentContext(Path.Combine(rootPath, "identifier.db"));
        var document = db.Documents.Include(x => x.Fields).Single(x => x.Name == fileName);
        return document;
    }

    public static IEnumerable<object[]> GetFilesToTest()
    {
        var dir = TestFilesRepository.GetDocumentsDir();
        var files = Directory.GetFiles(dir, "*.pdf");
        return files.Select(x => new object[] {x});
    }
    
    public class TestFilesRepository {
        public static string GetDocumentsDir() => CommonTestConfigs.pathToRootDirectoryWithFiles;
    }
}