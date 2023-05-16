using System.Threading.Tasks;
using Serilog;
using TSI.OCR.Auto.Tests;
using TSI.OCR.Auto.Tests.Misc;
using TSI.OCR.Data;

namespace TSI.OCR.Main.Commands {
    public class  ReferenceDatabaseUpdater {
        private readonly ILogger logger;

        public ReferenceDatabaseUpdater(ILogger logger) {
            this.logger = logger;
        }

        public async Task Update() {
            logger.Information("Updating reference database...");
            var dir = IronOcrSampleTest.TestFilesRepository.GetDocumentsDir();
            await ProcessDir(dir);
        }

        private async Task ProcessDir(string directory) {
            var processor = new PdfFileProcessor(logger);

            var fileService = new FileService(directory);
            var files = fileService.GetSourceFilesPaths();
            
            // Clean database 
            await using var db = new DocumentContext();
            db.Documents.RemoveRange(db.Documents);
            await db.SaveChangesAsync();

            foreach (var file in files) {
                await processor.ProcessFile(file);
            }
        }
    }
}