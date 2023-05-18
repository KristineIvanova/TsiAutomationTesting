using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using SixLabors.ImageSharp;
using TSI.OCR.Auto.Tests.Misc.Helpers;
using TSI.OCR.Common.Config;
using Xunit.Abstractions;

namespace TSI.OCR.Auto.Tests.Misc.LogHTML {
    
    public static class LogHtmlStaticHtmlError{
        private static List<Error> errorList = new List<Error>();
        private static Dictionary<string, string> documentNameAndFullPath = new Dictionary<string, string>();
        private static ILogger logger;

        public static void BindLogger(ITestOutputHelper output) {
            logger = new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }
        
        public static void AddCompareError(string documentName, string textType, string blockType, int pageNr, string expectedStr, string actualStr, Image image) {
            var id = errorList.Count;
            var newError = new CompareError(id, documentName, "CompareError", textType, blockType, pageNr, expectedStr, actualStr, image);
            errorList.Add(newError);
        }

        public static void AddBlockNotFoundError(string documentName, string blockType, int pageNr, string blockPositionText, string errorDescription, Image image, string text, string normalizeText) {
            var id = errorList.Count;
            var newError = new BlockNotFoundError(id, documentName, blockType, pageNr, blockPositionText, errorDescription, image, text, normalizeText);
            errorList.Add(newError);
        }

        public static void AddBlockNotFoundError(string documentName, string blockType, int pageNr, string blockPositionText, string errorDescription) {
            var id = errorList.Count;
            var newError = new PageWidthHeightError(id, documentName, blockType, pageNr, blockPositionText, errorDescription);
            errorList.Add(newError);
        }

        /// <summary>
        /// If no any in list then return false, if have error, then return true.
        /// </summary>
        /// <returns></returns>
        public static bool IsErrorInErrorList (string documentName) {
            var errorListCount = errorList.FindAll(x => x.documentName == documentName).Count;

            return errorListCount != 0;
        }
        
        public static void AddDocumentNameAndFullPath(string documentName, string pdfFullPath) {
            if (documentNameAndFullPath.ContainsKey(documentName) == false) {
                documentNameAndFullPath.Add(documentName, pdfFullPath);
            }
        }
        
        private static void CheckHtmlLogFolder() {
            if (errorList.Count != 0) {
                var path = Path.Combine(CommonTestConfigs.HtmlLogFolder, "Files");
                Directory.CreateDirectory(path);
            }
        }

        public static void DeleteHtmlLogFolder() {
            if (Directory.Exists(Path.Combine(CommonTestConfigs.HtmlLogFolder, "Files"))) {
                Directory.Delete (Path.Combine(CommonTestConfigs.HtmlLogFolder, "Files"), true);
                
                var di = new DirectoryInfo(CommonTestConfigs.HtmlLogFolder);
                foreach (var file in di.GetFiles()) {
                    file.Delete(); 
                }
            }
        }
        
        /// <summary>
        /// Create html report.
        /// </summary>
        public static void CreatHtmlReport() {
            CheckHtmlLogFolder();
            CreateErrorPages();
            CreateMainPage();
        }

        private static void CreateMainPage() {
            if (errorList.Count == 0) return;
           
            //Prepare document list.
            var htmlCode = new StringBuilder();
            
            var documentList = GetDocumentList();
            foreach (var document in documentList) {
                var compareErrorCount = errorList.FindAll(x => x.errorType == "CompareError" && x.documentName == document).Count;
                var blockNotFoundErrorCount = errorList.FindAll(x => x.errorType == "BlockNotFound" && x.documentName == document).Count;
                var pageWidthHeightErrorCount = errorList.FindAll(x => x.errorType == "PageHWError" && x.documentName == document).Count;
                
                var strSummaryErrorMessage = new StringBuilder();
                
                if (compareErrorCount != 0) strSummaryErrorMessage.Append($"CompareErrorCount: {compareErrorCount}<br>");
                if (blockNotFoundErrorCount != 0) {
                    strSummaryErrorMessage.Append($"BlockNotFoundErrorCount: {blockNotFoundErrorCount}<br>");
                }
                if (pageWidthHeightErrorCount != 0) strSummaryErrorMessage.Append($"PageHWError: {pageWidthHeightErrorCount}<br>");

                var compareErrorList = errorList.FindAll(x => x.errorType == "CompareError" && x.documentName == document).OfType<CompareError>().ToList();
                var summaryStaticErrorList = new List<CompareErrorStatistic>();
                
                foreach (var oneError in compareErrorList) {
                    foreach (var oneStaticError in oneError.compareErrorStatisticList) {
      
                        var index = summaryStaticErrorList.FindIndex(x => x.expected == oneStaticError.expected && x.actual == oneStaticError.actual);

                        if (index == -1) {
                            summaryStaticErrorList.Add(new CompareErrorStatistic(oneStaticError.expected, oneStaticError.actual, oneStaticError.count));
                        }
                        else {
                            int count = summaryStaticErrorList[index].count;
                            count += oneStaticError.count;
                            summaryStaticErrorList[index].count = count;
                        }
                    }
                }

                var compareErrorMessage = new StringBuilder();
                int iMax = 0;
                foreach (var error in summaryStaticErrorList) {
                    iMax++;
                    compareErrorMessage.Append($"'{error.expected}' -> '{error.actual}' (x{error.count})<br>");
                    
                    if (iMax > 14) {
                        compareErrorMessage.Append($"......... Count: '{summaryStaticErrorList.Count.ToString()}'");
                        break;
                    }
                }
                
                htmlCode.Append("<tr>");
                htmlCode.Append($"   <td class=\"tg-0pky\"><a href=\"Files\\{document}.html\">{document}</a></td>");
                htmlCode.Append($"   <td class=\"tg-0pky\">{strSummaryErrorMessage}</td>");
                htmlCode.Append($"   <td class=\"tg-0pky\">{compareErrorMessage}</td>");
                htmlCode.Append("</tr>");
            }            

            var fullHtmlCode = HtmlTemplateMainPage(htmlCode.ToString()).ToString();
            File.WriteAllText(Path.Combine(CommonTestConfigs.HtmlLogFolder, "index.html"), fullHtmlCode);
        }

        private static StringBuilder HtmlTemplateErrorPage(string documentName, string documentLink, StringBuilder compareErrorHtmlCode, StringBuilder blockNotFoundErrorHtmlCode, StringBuilder pageHWErrorHtmlCode) {
            var result = new StringBuilder();
            result.Append("<html><head><style type=\"text/css\">");
            result.Append(".tg  {border-collapse:collapse;border-color:#ccc;border-spacing:0;}");
            result.Append(".tg td{background-color:#fff;border-color:#ccc;border-style:solid;border-width:1px;color:#333;");
            result.Append("  font-family:Arial, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;word-break:normal;}");
            result.Append(".tg th{background-color:#f0f0f0;border-color:#ccc;border-style:solid;border-width:1px;color:#333;");
            result.Append("  font-family:Arial, sans-serif;font-size:14px;font-weight:normal;overflow:hidden;padding:10px 5px;word-break:normal;}");
            result.Append(".tg .tg-lboi{border-color:inherit;text-align:left;vertical-align:middle}");
            result.Append(".tg .tg-0pky{border-color:inherit;text-align:left;vertical-align:top}");
            result.Append("</style>");
            result.Append("</head><body>");
            result.Append($"<a href=\"{documentLink}\">{documentName}</a>");

            if (compareErrorHtmlCode.Length != 0) {
                // Second table, with compare result.
                result.Append("<h1>Compare error list:</h1>");
                result.Append("<table class=\"tg\">");
                result.Append("<thead>");
                result.Append("  <tr>");
                result.Append("    <th class=\"tg-0pky\" style=\"width: 200px;\">Information</th>");
                result.Append("    <th class=\"tg-0pky\" style=\"width: 100px;\">Error</th>");
                result.Append("    <th class=\"tg-0pky\">Expected</th>");
                result.Append("    <th class=\"tg-0pky\">Actual </th>");
                result.Append("    <th class=\"tg-0pky\">Image</th>");
                result.Append("  </tr>");
                result.Append("</thead>");
                result.Append("<tbody>");
                result.Append(compareErrorHtmlCode);
                result.Append("</tbody>");
                result.Append("</table>");
            }

            if (blockNotFoundErrorHtmlCode.Length != 0) {
                result.Append("<h1>Block not found:</h1>");
                result.Append("<table class=\"tg\">");
                result.Append("	<thead>");
                result.Append("		<tr>");
                result.Append("			<th class=\"tg-0pky\" style=\"width: 100px;\">Block type</th>");
                result.Append("			<th class=\"tg-0pky\" style=\"width: 1000px;\">Error description</th>");
                result.Append("			<th class=\"tg-0pky\">Image</th>");
                result.Append("		</tr>");
                result.Append("	</thead>");
                result.Append("	<tbody>");
                result.Append(blockNotFoundErrorHtmlCode);
                result.Append("	</tbody>");
                result.Append("</table>");                  
            }

            if (pageHWErrorHtmlCode.Length != 0) {
                result.Append("<h1>Page PageWidth/PageHeight compare error:</h1>");
                result.Append("<table class=\"tg\">");
                result.Append("	<thead>");
                result.Append("		<tr>");
                result.Append("			<th class=\"tg-0pky\" style=\"width: 50px;\">Page Nr</th>");
                result.Append("			<th class=\"tg-0pky\" style=\"width: 1000px;\">Error description</th>");
                result.Append("		</tr>");
                result.Append("	</thead>");
                result.Append("	<tbody>");
                result.Append(pageHWErrorHtmlCode);
                result.Append("	</tbody>");
                result.Append("</table>");  
            }
            
            result.Append("</body></html>");
            return result;
        }

        private static StringBuilder HtmlTemplateMainPage(string tableResult) {
            var result = new StringBuilder();

            result.Append("<html><head><style type=\"text/css\">");
            result.Append(".tg  {border-collapse:collapse;border-color:#ccc;border-spacing:0;}");
            result.Append(".tg td{background-color:#fff;border-color:#ccc;border-style:solid;border-width:1px;color:#333;");
            result.Append("  font-family:Arial, sans-serif;font-size:14px;overflow:hidden;padding:10px 5px;word-break:normal;}");
            result.Append(".tg th{background-color:#f0f0f0;border-color:#ccc;border-style:solid;border-width:1px;color:#333;");
            result.Append("  font-family:Arial, sans-serif;font-size:14px;font-weight:normal;overflow:hidden;padding:10px 5px;word-break:normal;}");
            result.Append(".tg .tg-lboi{border-color:inherit;text-align:left;vertical-align:middle}");
            result.Append(".tg .tg-0pky{border-color:inherit;text-align:left;vertical-align:top}");
            result.Append("</style>");
            result.Append("</head><body>");
            
            result.Append("<table class=\"tg\">");
            result.Append("	<thead>");
            result.Append("		<tr>");
            result.Append("			<th class=\"tg-0pky\" style=\"width: 500px;\">Document name</th>");
            result.Append("			<th class=\"tg-0pky\" style=\"width: 300px;\">Error summary</th>");
            result.Append("			<th class=\"tg-0pky\" style=\"width: 300px;\">Compare error summary (Max: 15)</th>");            
            result.Append("		</tr>");
            result.Append("	</thead>");
            result.Append("	<tbody>");
            result.Append(tableResult);
            result.Append("	</tbody>");
            result.Append("</table>");
            
            result.Append("</body></html>");
            return result;
        }
        
        /// <summary>
        /// By document name take all errors
        /// </summary>
        /// <param name="documentName"></param>
        /// <returns></returns>
        private static List<Error> GetAllErrorByDocumentName(string documentName) {
            var result = errorList.FindAll(x => x.documentName == documentName).ToList();
            return result;
        }        
        
        /// <summary>
        /// Return full document list (All document in a single copy).
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDocumentList() {
            var documentList = new List<string>();

            foreach (var item in errorList) {
                var index = documentList.FindIndex(x => x == item.documentName);
                if (index == -1) {
                    documentList.Add(item.documentName);
                }
            }

            return documentList;
        }

        /// <summary>
        /// for every error document create error page.
        /// </summary>
        private static void CreateErrorPages() {
            //Get document list.
            var documentList = GetDocumentList();

            foreach (var documentName in documentList) {
                var documentErrorList = GetAllErrorByDocumentName(documentName);

                CreateErrorPageByDocument(CommonTestConfigs.HtmlLogFolder, documentName, documentErrorList);
            }
        }

        private static StringBuilder GetHtmlCodeForCompareError(string folderPath, CompareError compareError, int imageNumber) {
            var result = new StringBuilder();
            var expectedStrWithColor = new StringBuilder();
            var actualStrWithColor = new StringBuilder();
    
            var compareResult = StringCompare.CompareEx(compareError.expectedStr, compareError.actualStr, CompareExMode.ConvertSpecChar);

            var lastCharIsError = false;
            var str1 = new StringBuilder();
            var str2 = new StringBuilder();
            
            for (int i = 0; i < compareResult.strA.Length; i++) {
                if (compareResult.strMistake[i] == '^') {

                    if (lastCharIsError == false) {
                        expectedStrWithColor.Append(CommonTestConfigs.ColorMistakeBegin);
                        actualStrWithColor.Append(CommonTestConfigs.ColorMistakeBegin);
                    }

                    expectedStrWithColor.Append(compareResult.strA[i]);
                    actualStrWithColor.Append(compareResult.strB[i]);

                    str1.Append(compareResult.strA[i]);
                    str2.Append(compareResult.strB[i]);
                    
                    lastCharIsError = true;
                }
                else {

                    if (lastCharIsError) {
                        expectedStrWithColor.Append(CommonTestConfigs.ColorMistakeEnd);
                        actualStrWithColor.Append(CommonTestConfigs.ColorMistakeEnd);

                        ((CompareError)errorList[compareError.id]).AddErrorStatistic(str1.ToString(), str2.ToString());
                        str1.Clear();
                        str2.Clear();
                    }

                    expectedStrWithColor.Append(compareResult.strA[i]);
                    actualStrWithColor.Append(compareResult.strB[i]);
            
                    lastCharIsError = false;
                }
            }            
            
            if (compareResult.strMistake[^1] == '^') {
                //Console.WriteLine(compareResult.strA);
                //Console.WriteLine(compareResult.strB);
        
                expectedStrWithColor.Append(CommonTestConfigs.ColorMistakeEnd);
                actualStrWithColor.Append(CommonTestConfigs.ColorMistakeEnd);
                
                ((CompareError)errorList[compareError.id]).AddErrorStatistic(str1.ToString(), str2.ToString());
                str1.Clear();
                str2.Clear();
            }
            
            //Create image
            var imageFileName = Path.Combine(folderPath, "Files", $"{compareError.documentName}-{imageNumber}.png");
            var imageFileLink = $"{compareError.documentName}-{imageNumber}.png";

            if (compareError.image != null) {
                compareError.image.SaveAsJpeg(imageFileName);
            }
            
            var errorStatisticHtml = new StringBuilder();
            foreach (var item in ((CompareError)errorList[compareError.id]).compareErrorStatisticList) {
                errorStatisticHtml.Append($"'{item.expected}' -> '{item.actual}' (x{item.count})<br>");
            }
            
            result.Append("<tr>");
            result.Append($"<td class=\"tg-0pky\"> Text type name: {compareError.textType}<br> Block type: {compareError.blockType}<br> Page Nr: {compareError.pageNr}<br> </td>");
            //result.Append($"<td class=\"tg-0pky\">{errorStatisticHtml}</td>");
            result.Append($"<td class=\"tg-0pky\">Remove in code</td>");
            result.Append($"<td class=\"tg-0pky\">{expectedStrWithColor}</td>");
            result.Append($"<td class=\"tg-0pky\">{actualStrWithColor}</td>");
            //htmlResult.Append($"<td class=\"tg-lboi\"><img src=\"test.png\" width=\"400\" height=\"100\"></td>");
            result.Append(compareError.image == null ? $"<td class=\"tg-lboi\"></td>" : $"<td class=\"tg-lboi\"><img src=\"{imageFileLink}\"></td>");
            result.Append("</tr>");

            return result;
        }

        /// <summary>
        /// Create one HTML page by one document
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="documentName">Document name</param>
        /// <param name="errorListForDocument">All errors for one document</param>
        /// <returns>return HTML code ( With out begin HTML code and end) only td and tr</returns>
        private static void CreateErrorPageByDocument(string folderPath, string documentName, List<Error> errorListForDocument) {
            StringBuilder htmlResult;
            
            var compareErrorResult = new StringBuilder();
            var blockNotFoundError = new StringBuilder();
            var pageWidthHeightError = new StringBuilder();
            
            var imageNumber = 0;

            foreach (var oneError in errorListForDocument) {
                switch (oneError) {
                    case CompareError error:
                        imageNumber++;
                        var resultCompareError = GetHtmlCodeForCompareError(folderPath, error, imageNumber);
                        compareErrorResult.Append(resultCompareError);
                        break;

                    case BlockNotFoundError error:
                        imageNumber++;
                        var imageFileName = Path.Combine(folderPath, "Files", $"{error.documentName}-{imageNumber}.png");
                        var imageFileLink = $"{error.documentName}-{imageNumber}.png";
                        if (error.image != null) {
                            error.image.SaveAsJpeg(imageFileName);
                        }

                        blockNotFoundError.Append("<tr>");
                        blockNotFoundError.Append($"<td class=\"tg-0pky\">{error.blockType}</td>");
                        blockNotFoundError.Append($"<td class=\"tg-0pky\">{error.errorDescription}<br>{error.blockPositionText}</td>");
                        blockNotFoundError.Append(error.image == null
                            ? $"<td class=\"tg-0pky\"></td>"
                            : $"<td class=\"tg-0pky\"><img src=\"{imageFileLink}\"></td>");
                        blockNotFoundError.Append("</tr>");    
                        break;
                    
                    case PageWidthHeightError error:
                        pageWidthHeightError.Append("<tr>");
                        pageWidthHeightError.Append($"<td class=\"tg-0pky\">{error.pageNr}</td>");
                        pageWidthHeightError.Append($"<td class=\"tg-0pky\">{error.errorDescription}");
                        pageWidthHeightError.Append($"<td class=\"tg-0pky\">Image");
                        pageWidthHeightError.Append("</tr>");   
                        break;
                }
            }
            
            //Copy PDF document
            var pdfPath = documentNameAndFullPath[documentName];
            var pdfNewPath = Path.Combine(folderPath, "Files", Path.GetFileName(pdfPath));
            var pdfLink = Path.GetFileName(pdfPath);
            File.Copy(pdfPath, pdfNewPath, true);

            htmlResult = HtmlTemplateErrorPage(documentName, pdfLink, compareErrorResult, blockNotFoundError, pageWidthHeightError);
            File.WriteAllText(Path.Combine(folderPath, "Files", $"{documentName}.html"), htmlResult.ToString());
        }

        public static void PrintAllErrorToConsole() {
            var errorMessageList = new List<string>();

            var compareErrorCount = 0;
            var blockNotFoundErrorCount = 0;
            var pageWidthHeightErrorCount = 0;
            
            foreach (var oneError in errorList) {
                switch (oneError) {
                    case CompareError error: {
                        var errorMessage = new StringBuilder();
                        errorMessage.Append("Compare error: ");
                        errorMessage.Append(error.textType + " | " + error.blockType + " | " + error.pageNr);
                        errorMessage.Append("\r\n");
                        errorMessage.Append("Expected:" + error.expectedStr.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\r\n", "\\r\\n") + "\r\n");
                        errorMessage.Append("Actual  :" + error.actualStr.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\r\n", "\\r\\n") + "\r\n");
                        errorMessage.Append("\r\n");
                        errorMessageList.Add(errorMessage.ToString());

                        compareErrorCount++;
                        break;
                    }

                    case BlockNotFoundError error: {
                        var errorMessage = new StringBuilder();
                        errorMessage.Append("Block Not Found Error: ");
                        errorMessage.Append(error.blockType + "|" + error.errorDescription + "|" + error.blockPositionText);
                        errorMessageList.Add(errorMessage.ToString());

                        blockNotFoundErrorCount++;
                        break;
                    }

                    case PageWidthHeightError error: {
                        var errorMessage = new StringBuilder();
                        errorMessage.Append("Page Width Height Error: ");
                        errorMessage.Append(error.blockType + "|" + error.errorDescription + "|" + error.blockPositionText);
                        errorMessageList.Add(errorMessage.ToString());

                        pageWidthHeightErrorCount++;
                        break;
                    }
                }
            }
            
            var errorStatistic = new StringBuilder();
            errorStatistic.AppendLine();
            errorStatistic.AppendLine("------------------------------------------");
            errorStatistic.AppendLine(" Num| Error name                 | count |");
            errorStatistic.AppendLine("------------------------------------------");
            errorStatistic.AppendLine($" 1  | CompareErrorCount          | {compareErrorCount.ToString().PadRight(6)}|");
            errorStatistic.AppendLine($" 2  | BlockNotFoundErrorCount    | {blockNotFoundErrorCount.ToString().PadRight(6)}|");
            errorStatistic.AppendLine($" 3  | PageWidthHeightErrorCount  | {pageWidthHeightErrorCount.ToString().PadRight(6)}|");
            errorStatistic.AppendLine("------------------------------------------");
            logger.Error(errorStatistic.ToString());    

            foreach (var errorMessage in errorMessageList) {
                logger.Error(errorMessage);    
            }
        }
    }
}