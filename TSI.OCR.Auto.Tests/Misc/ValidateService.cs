using System;
using System.Collections.Generic;
using Serilog;
using TSI.OCR.Auto.Tests.Misc.LogHTML;
using TSI.OCR.Common.Config;

namespace TSI.OCR.Auto.Tests.Misc {
    public class ValidateService {
        private ILogger Logger { get; }

        public ValidateService(ILogger logger) {
            this.Logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentName">Document name</param>
        /// <param name="blockType">Block type</param>
        /// <param name="expectedBlockList"> APKG take from directory</param>
        /// <param name="activeBlockList"> APKG take from server (Sent PDF for analise and server return APKG for analise</param>
        /// <param name="expectedPageBlockList"></param>
        public void CompareExpectedAndActiveByBlock(string documentName, string blockType, List<BlocksTable> expectedBlockList, List<BlocksTable> activeBlockList,
            List<BlocksTable> expectedPageBlockList) {
            var activeCheckFlag = new List<bool>();

            for (var i = 0; i < activeBlockList.Count; i++) {
                activeCheckFlag.Add(false);
            }

            foreach (var expectedValue in expectedBlockList) {
                var index = activeBlockList.FindIndex(x => x.Page == expectedValue.Page
                                                           && Math.Abs(x.X - expectedValue.X) <= CommonTestConfigs.CoordinatesRange
                                                           && Math.Abs(x.Y - expectedValue.Y) <= CommonTestConfigs.CoordinatesRange
                                                           && Math.Abs(x.W - expectedValue.W) <= CommonTestConfigs.CoordinatesRange
                                                           && Math.Abs(x.H - expectedValue.H) <= CommonTestConfigs.CoordinatesRange);

                if (index == -1) { //One value from expected not found in Active list.
                    var errorDescription = "Expected value not found in Active result.";
                    var blockPositionText = expectedValue.ConvertToString();
                    
                    var imageForReport = expectedValue.Image ?? ImageService.CutImage(expectedPageBlockList, expectedValue, Logger);
                    
                    LogHtmlStaticHtmlError.AddBlockNotFoundError(documentName, expectedValue.BlockType, expectedValue.Page, blockPositionText, errorDescription,
                        imageForReport, expectedValue.Text, expectedValue.NormalizedText);
                }
                else { //expectedValue found in Active list.
                    // Value found, then need change flag.
                    activeCheckFlag[index] = true;
                    
                    //Compare text
                    if (expectedValue.Text != activeBlockList[index].Text) {
                        var imageForReport = expectedValue.Image ?? ImageService.CutImage(expectedPageBlockList, expectedValue, Logger);
                        LogHtmlStaticHtmlError.AddCompareError(documentName, "Text", blockType, expectedValue.Page, expectedValue.NormalizedText,
                            activeBlockList[index].NormalizedText, imageForReport);
                    } else if (expectedValue.NormalizedText != activeBlockList[index].NormalizedText) {
                        var imageForReport = expectedValue.Image ?? ImageService.CutImage(expectedPageBlockList, expectedValue, Logger);
                        LogHtmlStaticHtmlError.AddCompareError(documentName, "NormalizedText", blockType, expectedValue.Page, expectedValue.NormalizedText,
                            activeBlockList[index].NormalizedText, imageForReport);
                    } else  if (expectedValue.PageWidth != activeBlockList[index].PageWidth || expectedValue.PageHeight != activeBlockList[index].PageHeight) {
                        var errorDescription = $"Expected and Active page dimensions do not match.<br>Expected page value: {expectedValue.PrintPageSize()}" +
                                               $"<br>Active page value: {activeBlockList[index].PrintPageSize()}";
                        LogHtmlStaticHtmlError.AddBlockNotFoundError(documentName, blockType, expectedValue.Page, expectedValue.ConvertToString(), errorDescription);
                    }
                }
            }

            for (int i = 0; i < activeCheckFlag.Count; i++) {
                if (activeCheckFlag[i] == false) {
                    var errorDescription = "Active value not found in Expected result.";
                    var blockPositionText = activeBlockList[i].ConvertToString();
                    
                    var imageForReport = activeBlockList[i].Image ?? ImageService.CutImage(expectedPageBlockList, activeBlockList[i], Logger);
                    LogHtmlStaticHtmlError.AddBlockNotFoundError(documentName, activeBlockList[i].BlockType, activeBlockList[i].Page, blockPositionText,
                        errorDescription, imageForReport, activeBlockList[i].Text, activeBlockList[i].NormalizedText);
                }
            }
        }
    }
}