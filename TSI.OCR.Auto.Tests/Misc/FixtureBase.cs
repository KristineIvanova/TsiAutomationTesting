using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using TSI.OCR.Auto.Tests.Misc.LogHTML;
using TSI.OCR.Common.Config;
using Xunit.Abstractions;

namespace TSI.OCR.Auto.Tests.Misc {
    public class FixtureBase {
        protected FixtureBase(ITestOutputHelper output) {
            
            var config = new LoggerConfiguration()
                .WriteTo.TestOutput(output);
            Logger = config.CreateLogger();//.ForContext(GetType());
            
            LogHtmlStaticHtmlError.BindLogger(output);
        }

        protected ILogger Logger { get; }

        //public async Task TestClass(string[] blockTypes, string directory, bool isDebug = false) {
        public async Task TestClass(string directory, bool isDebug = false) {
            
            var fileService = new FileService(directory);
            var pdfFilePath = fileService.GetSourceFilesPath();
            var jsonFilePath = fileService.GetFeatureFlagsFilePath();
            var filePath = fileService.GetFilePath();

            var fileProcessor = new FileProcessor(Logger);
            
            string targetPackage;

            if (isDebug) {
                var pdfFileName = Path.GetFileName(pdfFilePath);
                targetPackage = Path.Combine(CommonTestConfigs.DebugFolder, $"{pdfFileName}.sqlite");
                if (File.Exists(targetPackage) == false) {
                    throw new FileNotFoundException($"File: '{targetPackage}' not found.");
                }
            }
            else {
                Logger.Information("Send file to server and waiting any answer");
                targetPackage = await fileProcessor.ProcessFile(pdfFilePath, jsonFilePath);
                Logger.Information("The response came from the server");
            }

            // Take all type block from active file (from server).
            var actualBlockTypesList = await PackageService.GetBlockTypeList(targetPackage);
            
            var expectedPageBlockList = await PackageService.GetPackageRecordsByBlockType(filePath, "PAGE_BLOCK"); 
            
            foreach (var blockType in actualBlockTypesList) {
                // get record count from target and active packages
                var targetPackageRecordsCountByBlockType = PackageService.GetPackageRecordsCountByBlockType(targetPackage, blockType);
                Logger.Information(
                    "In the target package record count with block type '{BlockType}' is: {TargetPackageRecordsCountByBlockType}", blockType, targetPackage);

                var activePackageRecordsCountByBlockType = PackageService.GetPackageRecordsCountByBlockType(filePath, blockType);
                Logger.Information(
                    "In the active package record count with block type '{BlockType}' is: {ActivePackageRecordsCountByBlockType}", blockType, activePackageRecordsCountByBlockType);

                // get all records from target and active packages
                var actualPackageRecordsByBlockType = await PackageService.GetPackageRecordsByBlockType(targetPackage, blockType);
                var expectedPackageRecordsByBlockType = await PackageService.GetPackageRecordsByBlockType(filePath, blockType);

                var validateService = new ValidateService(Logger);
                var documentName = Path.GetFileName(pdfFilePath);
                LogHtmlStaticHtmlError.AddDocumentNameAndFullPath(documentName, pdfFilePath);

                validateService.CompareExpectedAndActiveByBlock(documentName, blockType, expectedPackageRecordsByBlockType, 
                    actualPackageRecordsByBlockType, expectedPageBlockList);
            }

            var isErrorInErrorList = LogHtmlStaticHtmlError.IsErrorInErrorList(Path.GetFileName(pdfFilePath));

            if (isErrorInErrorList) {
                LogHtmlStaticHtmlError.PrintAllErrorToConsole();
                throw new Exception("An error occurred during the 'Compare' test. For more information please see the output logs or the HtmlLog report or error logs.");
            }
        }
    }
}