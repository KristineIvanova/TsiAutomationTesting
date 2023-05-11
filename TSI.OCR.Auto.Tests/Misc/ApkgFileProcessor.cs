using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Serilog;
using TSI.OCR.Common.Config;

namespace TSI.OCR.Auto.Tests.Misc
{
    public class ApkgFileProcessor
    {
        private readonly ILogger logger;

        public ApkgFileProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task<string> ProcessFile(string sourceFilePath, string jsonFilePath)
        {
            //send POST-request with pdf file
            var response = await UploadFileToServer(sourceFilePath);

            //get apkg file
            var targetPackageName = response.Split(new[]
            {
                "\""
            }, StringSplitOptions.None)[1].Split("\"")[0];
            logger.Information("Target package name is: {TargetPackageName}", targetPackageName);

            return await WaitApkgFileFromServer(targetPackageName, 30, 2);
        }

        private async Task<string> WaitApkgFileFromServer(string fileName, int retries, int time)
        {
            var attempt = 0;
            var path = Path.Combine(CommonTestConfigs.PathToNewApkgFiles, fileName);

            do
            {
                try
                {
                    if (File.Exists(path))
                    {
                        logger.Information("File '{FileName}' already exists in the target directory: {TargetDirectory}", fileName,
                            CommonTestConfigs.PathToNewApkgFiles);
                        return path;
                    }

                    attempt++;
                    if (attempt >= retries)
                    {
                        var errorMessage = new StringBuilder();
                        errorMessage.AppendLine($"retries: {retries}, time: {time}: Sum: {retries * time} seconds");
                        errorMessage.AppendLine($"DateTime.Now: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine($"DateTime.UtcNow: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine("Too many retries to get the file.");
                        throw new Exception(errorMessage.ToString());
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(time));
                }
                catch (Exception)
                {
                    attempt++;
                    if (attempt >= retries)
                    {
                        var errorMessage = new StringBuilder();
                        errorMessage.AppendLine($"retries: {retries}, time: {time}: Sum: {retries * time} seconds");
                        errorMessage.AppendLine($"DateTime.Now: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine($"DateTime.UtcNow: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine("Too many retries to get the file.");
                        throw new Exception(errorMessage.ToString());
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(time));
                }
            } while (true);
        }


        private async Task<string> UploadFileToServer(string pdfFilePath)
        {
            logger.Information("Test will try to upload file: {PdfFile}", pdfFilePath);

            try
            {
                using (var fs = File.OpenRead(pdfFilePath))
                {
                    using (var ms = new MemoryStream())
                    {
                        await fs.CopyToAsync(ms); // Copy the file stream to a memory stream

                        // TODO: Implement your file processing logic here
                        // You can save the file to a local directory or perform any other operations

                        // Example: Save the file to a specific directory
                        var targetDirectory = "C:\\Users\\kristine.ivanova\\RiderProjects\\TsiAutomationTesting\\TSI.OCR.Auto.Tests\\Resource";
                        var targetFilePath = Path.Combine(targetDirectory, Path.GetFileName(pdfFilePath));
                        using (var targetStream = File.Create(targetFilePath))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            await ms.CopyToAsync(targetStream);
                        }

                        logger.Information("File successfully uploaded to: {TargetFilePath}", targetFilePath);
                        return "Upload successful";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred during file upload: {ErrorMessage}", ex.Message);
                return "Upload failed";
            }
        }
    }
}