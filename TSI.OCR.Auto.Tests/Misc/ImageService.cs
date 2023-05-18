using System;
using System.Collections.Generic;
using System.Text;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TSI.OCR.Common.Config;

namespace TSI.OCR.Auto.Tests.Misc {
    public static class ImageService {
        public static Image CutImage(List<BlocksTable> expectedPageBlockList, BlocksTable expectedValue, ILogger logger) {
            
            //var actualValue = expectedPageBlockList[expectedValue.Page];
            var pageImage = expectedPageBlockList[expectedValue.Page].Image;
            var compressionRatio = expectedValue.IMG_SCALE;
            
            var newX = (int)(expectedValue.X * compressionRatio) - 5;
            //var newY = (int)((pageImage.Width - expectedValue.Y) * compressionRatio) - 5;
            var newY = (int)((expectedValue.PageHeight - expectedValue.Y) * compressionRatio) - 5;
            var newH = (int)(expectedValue.H * compressionRatio) + 5;
            var newW = (int)(expectedValue.W * compressionRatio) + 5;
            
            Image newImage;

            try {
                newImage =  pageImage.Clone(i => i.Crop(new Rectangle(newX, newY, newW, newH)));
            }
            catch (Exception) {
                newImage = Image.Load(CommonTestConfigs.errorImagePath);

                var errorDescription = new StringBuilder();
                errorDescription.Append("Page coordinate:\r\n");
                errorDescription.Append(expectedPageBlockList[expectedValue.Page].ConvertToString() + "\r\n");
                errorDescription.Append("Crop coordinate:\r\n");
                errorDescription.Append(expectedValue.ConvertToString());
                logger.Information(errorDescription.ToString());
            }

            return newImage;
        }
    }
}