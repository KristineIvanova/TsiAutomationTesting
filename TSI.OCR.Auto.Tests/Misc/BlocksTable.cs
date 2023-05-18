using System;
using System.Text;
using SixLabors.ImageSharp;

namespace TSI.OCR.Auto.Tests.Misc {
    
    public enum ConvertType {
        Full,
        OnlyCoordinate,
    }

    public class BlocksTable {
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string Text { get; set; }
        public string NormalizedText { get; set; }
        public int PageWidth { get; set; }
        public int PageHeight { get; set; }
        public string BlockType { get; set; }
        private byte[] imageArray;
        private Image image;
        public Image Image {
            get {
                if (imageArray == null) {
                    return null;
                }
                    
                if (image == null) {
                    image = Image.Load(imageArray);
                }

                return image;
            }
        }
        public int NormLength { get; set; }
        public float IMG_SCALE { get; set; }

        public string ConvertToString(ConvertType convertType = ConvertType.Full) {
            var result = new StringBuilder();

            result.Append($"{Page} ");
            result.Append($"\t |  {BlockType}");
            result.Append($"\t |  {X} ");
            result.Append($"\t |  {Y} ");
            result.Append($"\t |  {W} ");
            result.Append($"\t |  {H} ");
            result.Append($"\t |  {PageWidth} ");
            result.Append($"\t |  {PageHeight} ");

            switch (convertType) {
                case ConvertType.Full:
                    result.Append($"\t |  {Text?.Replace("\n", "\\n")?.Replace("\t", "\\t")?.Replace("\r\n", "\\r\\n")} ");
                    result.Append($"\t |  {NormalizedText?.Replace("\n", "\\n")?.Replace("\t", "\\t")?.Replace("\r\n", "\\r\\n")} ");
                    break;
                case ConvertType.OnlyCoordinate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(convertType), convertType, null);
            }
            result.Append($"\t |  {NormLength} ");
            result.Append($"\t |  {IMG_SCALE} ");
            
            return result.ToString();
        }

        public string PrintPageSize() {
            return $"PageWidth: {PageWidth}; PageHeight: {PageHeight}";
        }
    }
}