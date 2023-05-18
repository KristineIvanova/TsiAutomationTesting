using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace TSI.OCR.Auto.Tests.Misc.LogHTML {
    public class Error {
        public int id;
        public string documentName;
        public string errorType;
    }

    public class CompareErrorStatistic {
        public string expected;
        public string actual;
        public int count;

        public CompareErrorStatistic(string expected, string actual, int count) {
            this.expected = expected;
            this.actual = actual;
            this.count = count;
        }
    }
    
    public class CompareError : Error {
        public readonly string blockType;
        public readonly int pageNr;
        public readonly string expectedStr;
        public readonly string actualStr;
        public readonly string textType;
        public readonly Image image;
        public List<CompareErrorStatistic> compareErrorStatisticList = new List<CompareErrorStatistic>();

        public CompareError (int id, string documentName, string errorType, string textType, string blockType, int pageNr, string expectedStr, string actualStr, Image image) {
            this.id = id;
            this.documentName = documentName;
            this.errorType = errorType;
            this.blockType = blockType;
            this.pageNr = pageNr;
            this.expectedStr = expectedStr;
            this.actualStr = actualStr;
            this.textType = textType;
            this.image = image;
        }

        public void AddErrorStatistic(string str1, string str2) {
            var index = compareErrorStatisticList.FindIndex(x => x.expected == str1 && x.actual == str2);

            if (index == -1) {
                compareErrorStatisticList.Add(new CompareErrorStatistic(str1, str2, 1));
            }
            else {
                int count = compareErrorStatisticList[index].count;
                count++;
                compareErrorStatisticList[index].count = count;
            }
        }
    }

    public class BlockNotFoundError: Error {
        public readonly string blockType;
        public readonly int pageNr;
        public readonly string blockPositionText;
        public readonly string errorDescription;
        public readonly Image image;
        public readonly string text;
        public readonly string normalizedText;

        public BlockNotFoundError(int id, string documentName, string blockType, int pageNr, string blockPositionText, string errorDescription, Image image, string text, string normalizedText) {
            this.id = id;
            this.documentName = documentName;
            this.errorType = "BlockNotFound";
            this.blockType = blockType;
            this.pageNr = pageNr;
            this.blockPositionText = blockPositionText;
            this.errorDescription = errorDescription;
            this.image = image;
            this.text = text;
            this.normalizedText = normalizedText;
        }
    }

    public class PageWidthHeightError: Error {
        public readonly string blockType;
        public readonly int pageNr;
        public readonly string blockPositionText;
        public readonly string errorDescription;
        
        public PageWidthHeightError(int id, string documentName, string blockType, int pageNr, string blockPositionText, string errorDescription) {
            this.id = id;
            this.documentName = documentName;
            this.errorType = "PageHWError";
            this.blockType = blockType;
            this.pageNr = pageNr;
            this.blockPositionText = blockPositionText;
            this.errorDescription = errorDescription;
        }      
    }
}