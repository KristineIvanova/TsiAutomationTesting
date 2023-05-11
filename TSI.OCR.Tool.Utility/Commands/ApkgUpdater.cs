using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using TSI.OCR.Auto.Tests;
using TSI.OCR.Auto.Tests.Misc;
using TSI.OCR.Common.Config;

namespace TSI.OCR.Main.Commands {
    public class  ApkgUpdater {
        private readonly ILogger logger;
        private IronOcrSampleTest.TestFilesRepository repo = new();

        public ApkgUpdater(ILogger logger) {
            this.logger = logger;
        }

        public async Task Update() {
            logger.Information("Updating apkg files...");
            var dirs = repo.GetDirectories();
            foreach (var dir in dirs) {
                logger.Information("Updating directory {Dir}", dir);

                if (dir.IndexOf("ru (1)", StringComparison.Ordinal) > 2) {
                    // Uncomment if need run only one document.
                    await ProcessDir(dir);
                }
            }
        }

        private async Task ProcessDir(string directory) {
            directory = Path.Combine(CommonTestConfigs.pathToRootDirectoryWithFiles, directory);
            var processor = new ApkgFileProcessor(logger);

            var fileService = new FileService(directory);
            var file = fileService.GetSourceFilesPath();
            var jsonFile = fileService.GetFeatureFlagsFilePath();
            var apkgFilePath = fileService.GetApkgFilePath();

            var target = await processor.ProcessFile(file, jsonFile);

            File.Delete(apkgFilePath);
            logger.Information("Deleting original file...");
            File.Move(target, apkgFilePath);
            logger.Information("Moving file ...");
            logger.Information("Everything is done (Updated)");
        }
    }
}