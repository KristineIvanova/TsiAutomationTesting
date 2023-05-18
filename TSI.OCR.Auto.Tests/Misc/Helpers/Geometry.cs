using System.Drawing;
using TSI.OCR.Data.Entities;

namespace TSI.OCR.Auto.Tests.Misc.Helpers; 

public static class Geometry {
    public static double OverlapArea(this Field a, Field b) {
        Rectangle rect1 = new Rectangle(a.X, a.Y, a.Width, a.Height);
        Rectangle rect2 = new Rectangle(b.X, b.Y, b.Width, b.Height);
        
        if (rect1.IntersectsWith(rect2))
        {
            Rectangle overlapRectangle = Rectangle.Intersect(rect1, rect2);
            double intersectArea = overlapRectangle.Width * overlapRectangle.Height;
            double unionArea = rect1.Width * rect1.Height + rect2.Width * rect2.Height - overlapRectangle.Width * overlapRectangle.Height;
            double ioU = intersectArea / unionArea;
            return ioU;
        }

        return 0;
    }
}