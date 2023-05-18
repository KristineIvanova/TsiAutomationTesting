using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronSoftware.Drawing;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using TSI.OCR.Auto.Tests.Misc;
using TSI.OCR.Auto.Tests.Misc.Helpers;
using TSI.OCR.Auto.Tests.Misc.LogHTML;
using TSI.OCR.Common.Config;
using TSI.OCR.Data;
using TSI.OCR.Data.Entities;
using Xunit;
using Xunit.Abstractions;

namespace TSI.OCR.Auto.Tests;

[Collection("HtmlCollection")]
public class IronOcrSampleTest : FixtureBase, IClassFixture<IronOcrSampleTest.IronOcrFixture>
{
    private readonly string _rootPath;

    public class IronOcrFixture : IAsyncLifetime
    {
        public Task InitializeAsync()
        {
            
            
            // restore mongoDB
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            LogHtmlStaticHtmlError.CreatHtmlReport();
            return Task.CompletedTask;
        }
    }

    public IronOcrSampleTest(ITestOutputHelper output) : base(output)
    {
        _rootPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName, "Resource");
    }

    [Theory]
    [MemberData(nameof(GetFilesToTest))]
    public async Task IronOcrImageTest(string filePath)
    {
        LogHtmlStaticHtmlError.AddDocumentNameAndFullPath(Path.GetFileName(filePath), filePath);
        
        var srcDocument = await FindDocument(filePath);
        var pdfReader = new PdfFileReader(filePath);
        var targetDocument = await pdfReader.ParseFile();

        // Find fields that doesn't exist in target document
        var srcFields = srcDocument.Fields.Where(x =>
            !targetDocument.Fields.Any(y => y.Value == x.Value && x.OverlapArea(y) > 0.5 && x.Page == y.Page)).ToList();

        foreach (var src in srcFields) {
            var image = Image.Load(src.Image);
            LogHtmlStaticHtmlError.AddBlockNotFoundError(srcDocument.Name, "text", src.Page, src.Value, 
                "Block not found in target document", image, src.Value, src.Value);
        }

        // Find fields that doesn't exist in target document
        var targetFields = targetDocument.Fields.Where(x =>
            !srcDocument.Fields.Any(y => y.Value == x.Value && y.OverlapArea(x) > 0.5 
                                                            && x.Page == y.Page)).ToList();
        
        foreach (var target in targetFields) {
            var image = Image.Load(target.Image);
            LogHtmlStaticHtmlError.AddBlockNotFoundError(srcDocument.Name, "text", target.Page, target.Value,
                "Block not found in source document", image, target.Value, target.Value);
        }

        // Find fields that exist in both documents but have different values
        var differentFields = new List<(Field Src, Field Target)>();
        foreach (var src in srcDocument.Fields) {
            var target = targetDocument.Fields.FirstOrDefault(x => src.OverlapArea(x) > 0.5 && x.Page == src.Page);
            if (target == null) continue;
            if (src.Value == target.Value) continue;
            differentFields.Add((src, target));
        }
        
        foreach (var diff in differentFields) {
            var image = Image.Load(diff.Target.Image);
            LogHtmlStaticHtmlError.AddCompareError(srcDocument.Name, "text", "text", diff.Src.Page, diff.Src.Value,
                    diff.Target.Value, image);
        }

        var result = $"Source document has {srcFields.Count} fields that are not present in target document. " +
                     $"Target document has {targetFields.Count} fields that are not present in source document. " +
                     $"There are {differentFields.Count} fields that have different values in both documents.";

        Console.Error.WriteLine(AnsiColors.Color($"<red>{result}</red>"));

        Assert.True(srcFields.Count == 0, result);
        Assert.True(targetFields.Count == 0, result);
        Assert.True(differentFields.Count == 0, result);
    }

    private async Task<Document> FindDocument(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        await using var db = new DocumentContext(Path.Combine(_rootPath, "identifier.db"));
        var document = db.Documents.Include(x => x.Fields).Single(x => x.Name == fileName);
        return document;
    }

    public static IEnumerable<object[]> GetFilesToTest()
    {
        var dir = TestFilesRepository.GetDocumentsDir();
        var files = Directory.GetFiles(dir, "*.pdf");
        return files.Select(x => new object[] { x });
    }

    public class TestFilesRepository
    {
        public static string GetDocumentsDir() => CommonTestConfigs.pathToRootDirectoryWithFiles.Replace("Documents", "BrokenDocuments");
    }
}