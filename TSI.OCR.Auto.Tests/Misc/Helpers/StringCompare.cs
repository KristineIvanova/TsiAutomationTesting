using System.Text;

namespace TSI.OCR.Auto.Tests.Misc.Helpers {
    public class StringCompareResult {
        public readonly string strA;
        public readonly string strB;
        public readonly string strMistake;

        public StringCompareResult(string strA, string strB, string strMistake) {
            this.strA = strA;
            this.strB = strB;
            this.strMistake = strMistake;
        }
    }

    public enum CompareExMode {
        None = 1,
        ConvertSpecChar = 2,
    }
    
    public static class StringCompare {
        
        /// <summary>
        /// Compare two string and return only error string.
        /// </summary>
        /// <param name="text1"></param>
        /// <param name="text2"></param>
        /// <returns></returns>
        public static string Compare(string text1, string text2) {
            var result = new StringBuilder();
            int maxLength;
            int difference;
            if (text1.Length <= text2.Length) {
                maxLength = text1.Length;
                difference = text2.Length - text1.Length;
            }
            else {
                maxLength = text2.Length;
                difference = text1.Length - text2.Length;
            }

            for (var i = 0; i < maxLength - 1; i++) {
                result.Append(text1[i] == text2[i] ? " " : "^");
            }

            for (var i = 0; i < difference; i++) {
                result.Append("^");
            }

            return result.ToString();
        }
        
        /// <summary>
        /// searches for the first match in the string two
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="startFrom"></param>
        /// <returns></returns>
        private static int FindFirstEquallyChar(string str1, string str2, int startFrom) {
            for (var i = startFrom; i < str1.Length; i++) {
                if (str1[i] == str2[startFrom]) {
                    return i;
                }
            }
            return int.MaxValue;
        }
        
        /// <summary>
        /// Check, char is special symbol or not ('\t', '\r', '\n')
        /// </summary>
        /// <param name="chr">Input char for check</param>
        /// <returns>T - char is special symbol; F - char is standard symbol</returns>
        private static bool IsSpecialChar(char chr) {
            switch (chr) {
                case '\t':
                case '\r':
                case '\n': {
                    return true;
                }

                default: {
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Compare string and return extra line with position mistake in two string.
        /// </summary>
        /// <param name="strA">String A for compare (original)</param>
        /// <param name="strB">String B for compare (new string)</param>
        /// <param name="compareExMode">Contain two mode, standard and move when in result special symbol convert to '\t', '\r', '\n'</param>
        /// <returns></returns>
        public static StringCompareResult CompareEx(string strA, string strB, CompareExMode compareExMode = CompareExMode.None) {
            var resultA = new StringBuilder();
            var resultB = new StringBuilder();
            var resultError = new StringBuilder();
            var positionStrA = 0;
            var positionStrB = 0;

            while ((strA.Length > positionStrA) && (strB.Length > positionStrB)) {
                //Console.WriteLine($"Compare char: {strA[positionStrA]} ::: {strB[positionStrB]}");

                if (strA[positionStrA] == strB[positionStrB]) {
                    resultA.Append(strA[positionStrA]);
                    resultB.Append(strB[positionStrB]);

                    positionStrA++;
                    positionStrB++;
                }
                else {
                    string newStrA = strA.Substring(positionStrA);
                    string newStrB = strB.Substring(positionStrB);

                    int a = FindFirstEquallyChar(newStrA, newStrB, 0);
                    int b = FindFirstEquallyChar(newStrB, newStrA, 0);

                    if (a < b) {
                        resultA.Append(strA[positionStrA]);
                        resultB.Append(" ");
                        positionStrA++;
                    } else { 
                        resultA.Append(" ");
                        resultB.Append(strB[positionStrB]);
                        positionStrB++;
                    }
                }
            }
            
            //Aligns lines to length
            if (strA.Length > positionStrA) {
                resultA.Append(strA.Substring(positionStrA));
                
                var endText = strA.Substring(positionStrA);
                for (var i = 0; i < endText.Length; i++) {
                    resultB.Append(" ");
                }
            }

            //Aligns lines to length
            if (strB.Length > positionStrB) {
                resultB.Append(strB.Substring(positionStrB));
                
                var endText = strB.Substring(positionStrB);
                for (var i = 0; i < endText.Length; i++) {
                    resultA.Append(" ");
                }
            }

            if (compareExMode == CompareExMode.ConvertSpecChar) {
                var resultAWithEscape = new StringBuilder();
                var resultBWithEscape = new StringBuilder();

                for (int i = 0; i < resultA.Length; i++) {
                    if (IsSpecialChar(resultA[i]) || IsSpecialChar(resultB[i])) {
                        
                        if (resultA[i] == resultB[i]) {
                            switch (resultA[i]) {
                                case '\t': {
                                    resultBWithEscape.Append("\\t");
                                    resultAWithEscape.Append("\\t");
                                    continue;
                                }
                                case '\r': {
                                    resultBWithEscape.Append("\\r");
                                    resultAWithEscape.Append("\\r");
                                    continue;
                                }
                                case '\n': {
                                    resultBWithEscape.Append("\\n");
                                    resultAWithEscape.Append("\\n");
                                    continue;
                                }
                            }
                        }

                        if (IsSpecialChar(resultA[i])) {
                            switch (resultA[i]) {
                                case '\t': {
                                    resultAWithEscape.Append("\\t");
                                    resultBWithEscape.Append("  ");
                                    continue;
                                }
                                case '\r': {
                                    resultAWithEscape.Append("\\r");
                                    resultBWithEscape.Append("  ");
                                    continue;
                                }
                                case '\n': {
                                    resultAWithEscape.Append("\\n");
                                    resultBWithEscape.Append("  ");
                                    continue;
                                }
                            }                            
                        }

                        if (IsSpecialChar(resultB[i])) {
                            switch (resultB[i]) {
                                case '\t': {
                                    resultAWithEscape.Append("  ");
                                    resultBWithEscape.Append("\\t");
                                    continue;
                                }
                                case '\r': {
                                    resultAWithEscape.Append("  ");
                                    resultBWithEscape.Append("\\r");
                                    continue;
                                }
                                case '\n': {
                                    resultAWithEscape.Append("  ");
                                    resultBWithEscape.Append("\\n");
                                    continue;
                                }
                            }                            
                        }
                    }
                    else {
                        resultAWithEscape.Append(resultA[i]);
                        resultBWithEscape.Append(resultB[i]);
                    }
                }

                resultA = resultAWithEscape;
                resultB = resultBWithEscape;
            }

            for (int i = 0; i < resultA.Length; i++) {
                resultError.Append(resultA[i] == resultB[i] ? " " : "^");

            }
            
            return new StringCompareResult(resultA.ToString(), resultB.ToString(), resultError.ToString());
        }
    }
}