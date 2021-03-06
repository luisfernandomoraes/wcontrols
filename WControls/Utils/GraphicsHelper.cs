﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace WControls.Utils
{

    static class GraphicsHelper
    {
        private const double ROUNDED_RECT_RAD_PERCENT = .05d;//5 percent

        public  static GraphicsPath GetGraphicsPath(Rectangle container, ControlShape shape)
        {
            GraphicsPath path = new GraphicsPath();

            Rectangle pathRect = container;
            pathRect.Width -= 1;
            pathRect.Height -= 1;

            switch (shape)
            {
                case ControlShape.Rect:
                    path.AddRectangle(pathRect);
                    break;
                case ControlShape.RoundedRect:
                    //radius is 10% of smallest side
                    int rad = (int)(Math.Min(pathRect.Height, pathRect.Width) * ROUNDED_RECT_RAD_PERCENT);
                    path.AddRoundedRectangle(pathRect, rad);
                    break;
                case ControlShape.Circular:
                    path.AddEllipse(pathRect);
                    break;
            }

            return path;
        }

        public static GraphicsPath Get3DShinePath(Rectangle container, ControlShape shape)
        {
            GraphicsPath path = new GraphicsPath();

            Rectangle pathRect = container;
            pathRect.Width -= 1;
            pathRect.Height -= 1;

            RectangleF halfRect = new RectangleF(pathRect.X, pathRect.Y,
                                                    pathRect.Width, pathRect.Height / 2f);

            if (pathRect.Height > 0 && pathRect.Width > 0)
            {
                switch (shape)
                {
                    case ControlShape.Rect:
                        path.AddRectangle(halfRect);
                        break;
                    case ControlShape.RoundedRect:
                        //radius is 10% of smallest side
                        int rad = (int)(Math.Min(halfRect.Height, halfRect.Width) * ROUNDED_RECT_RAD_PERCENT);
                        path.AddRoundedRectangle(halfRect, rad);
                        break;
                    case ControlShape.Circular:
                        path.AddArc(pathRect, 180, 142);
                        PointF[] pts = new PointF[]
                    {
                        path.GetLastPoint(),
                        new PointF(container.Width * .70f, container.Height * .33f),
                        new PointF(container.Width * .25f, container.Height * .5f),
                        path.PathPoints[0]
                    };
                        path.AddCurve(pts);
                        path.CloseFigure();
                        break;
                }
            }

            return path;
        }

        public static Brush GetGradBrush(Rectangle container, ControlShape shape, Color color)
        {
            Brush brush = null;

            switch (shape)
            {
                case ControlShape.Rect:
                case ControlShape.RoundedRect:
                    brush = new LinearGradientBrush(container, color, Color.Transparent,
                        LinearGradientMode.Vertical);
                    break;
                case ControlShape.Circular:
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddEllipse(container);
                        PathGradientBrush pgb = new PathGradientBrush(path);
                        pgb.CenterColor = color;
                        pgb.SurroundColors = new Color[] { Color.Transparent };
                        pgb.CenterPoint = new PointF(container.Left + container.Width * .5f,
                                                     container.Bottom + container.Height);
                        brush = pgb;
                    }
                        
                    break;
            }

            return brush;
        }

        public static GraphicsPath GetArcPath(RectangleF container, double dStartDegrees, double dArcLengthDegrees)
        {
            GraphicsPath arcPath = new GraphicsPath();

            int nPoints = 100;
            double dEachPointDelta = dArcLengthDegrees / (double)(nPoints);
            List<PointF> arcPts = new List<PointF>();
            for (int i = 0; i <= nPoints; i++)
            {
                double curDeg = dStartDegrees - (i * dEachPointDelta);
                arcPts.Add(GetPointInArc(container, curDeg, 0));
            }

            arcPath.AddCurve(arcPts.ToArray());

            return arcPath;
        }

        public static PointF GetPointInArc(RectangleF rect, double degrees, double offset)
        {
            PointF center = new PointF(rect.Left + rect.Width / 2f,
                                       rect.Top + rect.Height / 2f);

            double rads = (Math.PI / 180d) * (degrees + 90);
            
            double xVal = center.X + (offset + (rect.Width / 2d)) * Math.Sin(rads);
            double yVal = center.Y + (offset + (rect.Height / 2d)) * Math.Cos(rads);

            return new PointF((float)xVal, (float)yVal);
        }

        public static Color GetMixedColor(Color start, Color end, double percentage)
        {
            //get distance between colors
            double aDif = end.A - start.A;
            double rDif = end.R - start.R;
            double gDif = end.G - start.G;
            double bDif = end.B - start.B;

            //shrink to percentage of total distance between colors
            aDif *= percentage;
            rDif *= percentage;
            gDif *= percentage;
            bDif *= percentage;

            //get the lowest of the two colors
            double aLow = Math.Min(start.A, end.A);
            double rLow = Math.Min(start.R, end.R);
            double gLow = Math.Min(start.G, end.G);
            double bLow = Math.Min(start.B, end.B);

            //get the highest of the two colors
            double aHigh = Math.Max(start.A, end.A);
            double rHigh = Math.Max(start.R, end.R);
            double gHigh = Math.Max(start.G, end.G);
            double bHigh = Math.Max(start.B, end.B);

            //new color will be percentage between start and end
            double alpha = aDif + ((aDif > 0) ? aLow : aHigh);
            double red = rDif + ((rDif > 0) ? rLow : rHigh);
            double green = gDif + ((gDif > 0) ? gLow : gHigh);
            double blue = bDif + ((bDif > 0) ? bLow : bHigh);

            return Color.FromArgb((int)alpha, (int)red, (int)green, (int)blue);
        }
    }
}
