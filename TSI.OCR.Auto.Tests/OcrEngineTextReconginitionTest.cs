using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TSI.OCR.Auto.Tests.Misc;
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
            !targetDocument.Fields.Any(y => y.Value == x.Value && y.X == x.X &&
                                            y.Y == x.Y && y.Width == x.Width &&
                                            y.Height == x.Height && x.Page == y.Page)).ToList();

        foreach (var src in srcFields)
        {
            LogHtmlStaticHtmlError.AddBlockNotFoundError(srcDocument.Name, "text", src.Page, src.Value, 
                "Block not found in target document");
        }

        // Find fields that doesn't exist in target document
        var targetFields = targetDocument.Fields.Where(x =>
            !srcDocument.Fields.Any(y => y.Value == x.Value && y.X == x.X &&
                                         y.Y == x.Y && y.Width == x.Width &&
                                         y.Height == x.Height && x.Page == y.Page)).ToList();
        
        foreach (var target in targetFields)
        {
            LogHtmlStaticHtmlError.AddBlockNotFoundError(srcDocument.Name, "text", target.Page, target.Value, 
                "Block not found in source document");
        }

        // Find fields that exist in both documents but have different values
        var differentFields = (from src in srcDocument.Fields
            join target in targetDocument.Fields on new
                {
                   
                    src.X,
                    src.Y,
                    src.Width,
                    src.Height,
                    src.Page
                } equals new
                {
                    target.X,
                    target.Y,
                    target.Width,
                    target.Height,
                    target.Page
                }
            where src.Value != target.Value
            select new {
                Src = src, Target = target
            }).ToArray();
        
        foreach (var diff in differentFields)
        {
            LogHtmlStaticHtmlError.AddCompareError(srcDocument.Name, "text", "text", diff.Src.Page, diff.Src.Value,
                    diff.Target.Value, null);
        }

        var result = $"Source document has {srcFields.Count} fields that are not present in target document. " +
                     $"Target document has {targetFields.Count} fields that are not present in source document. " +
                     $"There are {differentFields.Length} fields that have different values in both documents.";

        Console.Error.WriteLine(AnsiColors.Color($"<red>{result}</red>"));

        Assert.True(srcFields.Count == 0, result);
        Assert.True(targetFields.Count == 0, result);
        Assert.True(differentFields.Length == 0, result);
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
        public static string GetDocumentsDir() => CommonTestConfigs.pathToRootDirectoryWithFiles;
    }
}