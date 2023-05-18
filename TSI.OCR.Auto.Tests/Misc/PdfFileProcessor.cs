using System.Threading.Tasks;
using Serilog;
using TSI.OCR.Data;

namespace TSI.OCR.Auto.Tests.Misc; 

public class PdfFileProcessor {
    private readonly ILogger _logger;

    public PdfFileProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ProcessFile(string sourceFilePath) {
        await using var db = new DocumentContext();
        var pdf = new PdfFileReader(sourceFilePath);
        var document = await pdf.ParseFile();

        db.Documents.Add(document);
        await db.SaveChangesAsync();
    }
}