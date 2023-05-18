using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Serilog;
using TSI.OCR.Auto.Tests.Misc.JsonConvertObject;
using TSI.OCR.Common.Config;

namespace TSI.OCR.Auto.Tests.Misc {
    public class FileProcessor {
        private readonly ILogger logger;

        public FileProcessor(ILogger logger) {
            this.logger = logger;
        }

        public async Task<string> ProcessFile(string sourceFilePath, string jsonFilePath) {
            //send POST-request with pdf file
            var response = await UploadFileToServer(sourceFilePath, jsonFilePath);
            
            var targetPackageName = response.Split(new[] {
                "\""
            }, StringSplitOptions.None)[1].Split("\"")[0];
            logger.Information("Target package name is: {TargetPackageName}", targetPackageName);

            return await WaitFileFromServer(targetPackageName, 30, 2);
        }

        private async Task<string> WaitFileFromServer(string fileName, int retries, int time) {
            var attempt = 0;
            do {
                try {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(CommonTestConfigs.ApiGetFile + fileName);

                    if (response.StatusCode.Equals(HttpStatusCode.OK)) {
                        var path = Path.Combine(CommonTestConfigs.PathToNewFiles, fileName);
                        using (var fileStream = new FileStream(path, FileMode.Create,
                                   FileAccess.Write, FileShare.None)) {
                            logger.Information("Saving file: {PathToNewFiles}", CommonTestConfigs.PathToNewFiles + fileName);
                            await response.Content.CopyToAsync(fileStream);
                            return path;
                        }
                    }

                    attempt++;
                    if (attempt >= retries) {
                        throw new Exception();
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(time));
                }
                catch (Exception) {
                    attempt++;
                    if (attempt >= retries) {
                        var errorMessage = new StringBuilder();
                        errorMessage.AppendLine($"retries: {retries}, time: {time}: Sum: {retries * time} seconds");
                        errorMessage.AppendLine($"DateTime.Now: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine($"DateTime.UtcNow: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}");
                        errorMessage.AppendLine("Too many retries to get status code 'OK' while sending GET-request.");
                        throw new Exception(errorMessage.ToString());
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(time));
                }
            } while (true);
        }

        private async Task<string> UploadFileToServer(string pdfFilePth, string featureFlagsFilePath) {
            logger.Information("Test will try to send POST-request with file: {PdfFile}", pdfFilePth);
            var httpClient = new HttpClient();

            var jsonFeatureFlags = JsonConvertService.GetFeatureFlagsFromFile(featureFlagsFilePath);
            
            using (var fs = File.OpenRead(pdfFilePth)) {
                using (var file = new StreamContent(fs)) {

                    HttpContent content;
                    if (jsonFeatureFlags == null) {
                        content = new MultipartFormDataContent {
                            { file, "file", Path.GetFileName(pdfFilePth) },
                        };
                    }
                    else {
                        content = new MultipartFormDataContent {
                            { file, "file", Path.GetFileName(pdfFilePth) },
                            {new StringContent(jsonFeatureFlags), "feature_flags"}
                        };
                    }

                    var response = await httpClient.PostAsync(CommonTestConfigs.ApiPostFile, content);
                    response.StatusCode.Should().Be(HttpStatusCode.OK, "Status code should be 'OK'");
                    logger.Information("Status code is: {ResponseStatusCode}", response.StatusCode);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    logger.Information("Data in response: {Trim}", responseContent.Trim());

                    return responseContent;
                }
            }
        }
    }
}