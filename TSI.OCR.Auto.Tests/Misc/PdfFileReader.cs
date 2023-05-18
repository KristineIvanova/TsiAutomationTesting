using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronOcr;
using TSI.OCR.Data.Entities;
using static IronOcr.Installation;

namespace TSI.OCR.Auto.Tests.Misc;

public class PdfFileReader
{
    private readonly string _filePath;
    private readonly IronTesseract _ocr;

    public PdfFileReader(string filePath)
    {
        _filePath = filePath;
        LicenseKey =
            "IRONOCR.KRISTINEIVANOVA.6207-3AC6322919-IXBPPJWY2AWMR-K5ACUCUZBZC5-RU4X2Y2U7QYT-6CS7HC4K56FP-RO32SOBCKJV5-KTWREB-THNFP5SEDSWJUA-DEPLOYMENT.TRIAL-5OMJXI.TRIAL.EXPIRES.07.JUN.2023";
        _ocr = new IronTesseract(new TesseractConfiguration());
    }

    public async Task<Document> ParseFile()
    {
        Console.WriteLine($"Parsing file {_filePath}...");
        var ocrInput = OcrInput.FromPdf(_filePath, 72);
        var ocrResult = await _ocr.ReadAsync(ocrInput);
        var result = new List<Field>();

        foreach (var page in ocrResult.Pages)
        {
            var fields = page.Paragraphs
                .SelectMany(paragraph => paragraph.Words)
                .Select(word => new Field
                {
                    Value = word.Text,
                    X = word.Location.X,
                    Y = word.Location.Y,
                    Width = word.Location.Width,
                    Height = word.Location.Height,
                    Page = page.PageNumber,
                    Image = word.ToBitmap(ocrInput).GetBytes()
                }).ToList();

            result.AddRange(fields);
        }

        var name = Path.GetFileName(_filePath);
        Console.WriteLine($"Parsed file {_filePath} successfully.");

        return new Document
        {
            Name = name,
            Fields = result
        };
    }
}