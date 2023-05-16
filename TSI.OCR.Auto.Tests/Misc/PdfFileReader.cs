using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronOcr;
using TSI.OCR.Data.Entities;

namespace TSI.OCR.Auto.Tests.Misc;

public class PdfFileReader {
    private readonly string filePath;
    private readonly IronTesseract ocr;

    public PdfFileReader(string filePath) {
        this.filePath = filePath;
        ocr = new IronTesseract();
    }

    public async Task<Document> ParseFile() {
        var ocrResult = await ocr.ReadAsync(filePath);
        var result = new List<Field>();

        foreach (var page in ocrResult.Pages) {
            var fields = page.Paragraphs
                .SelectMany(paragraph => paragraph.Words)
                .Select(word => new Field {
                    Value = word.Text,
                    X = word.Location.X,
                    Y = word.Location.Y,
                    Width = word.Location.Width,
                    Height = word.Location.Height,
                    Page = page.PageNumber
                }).ToList();

            result.AddRange(fields);
        }

        var name = Path.GetFileName(filePath);

        return new Document {
            Name = name,
            Fields = result
        };
    }
}