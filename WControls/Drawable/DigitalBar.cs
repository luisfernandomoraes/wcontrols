﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using WControls.Utils;

namespace WControls.Drawable
{
    public enum SegmentOrientation
    {
        Horizontal,
        Vertical
    }

    [Flags]
    public enum SegmentCorners
    {
        None = 0x0,
        TopLeft = 0x1,
        BottomLeft = 0x2,
        TopRight = 0x4,
        BottomRight = 0x8,
        All = 0xf,
        //two+ at once
        BothTop = 0x5,
        BothRight = 0xC,
        BothBottom = 0xA,
        BothLeft = 0x3
    }

    public class DigitalBar : IDisposable, IDrawable
    {
        #region Public Accessors

        /// <summary>
        /// The color of the bar
        /// </summary>
        [DefaultValue(typeof(Color), "Black")]
        [Description("The color of the bar")]
        [Category("Appearance")]
        public virtual Color Color { get; set; }

        /// <summary>
        /// How opaque the bar is when it is turned off
        /// </summary>
        [DefaultValue(0.1d)]
        [Description("How opaque the bar is when it is turned off")]
        [Category("Appearance")]
        public virtual double OpacityWhenOff { get; set; }

        /// <summary>
        /// Whether this segment is on or off
        /// </summary>
        [DefaultValue(true)]
        [Description("Whether this segment is on or off")]
        [Category("Appearance")]
        public virtual bool IsOn { get; set; }

        /// <summary>
        /// The corners that are drawn on this segment
        /// </summary>
        [DefaultValue(typeof(SegmentCorners), "None")]
        [Description("The corners that are drawn on this segment")]
        [Category("Appearance")]
        public virtual SegmentCorners Corners { get; set; }

        /// <summary>
        /// The horizontal or vertical padding
        /// </summary>
        [DefaultValue(0)]
        [Description("The horizontal or vertical padding")]
        [Category("Appearance")]
        public virtual float Padding { get; set; }

        /// <summary>
        /// The length of the tip that would intersect
        /// another segment. This is typically the width of the segment.
        /// </summary>
        [DefaultValue(0)]
        [Description("The length of the tip that would intersect another segment")]
        [Category("Appearance")]
        public virtual float TipLength { get; set; }

        #endregion

        protected Color OffColor
        {
            get
            {
                int alphaAmt = (int)(255 * OpacityWhenOff);
                return Color.FromArgb(alphaAmt, Color);
            }
        }

        protected GraphicsPath m_path;
        protected Region m_redrawRegion;
        protected SegmentOrientation m_orientation = SegmentOrientation.Horizontal;

        public DigitalBar(SegmentOrientation orientation)
        {
            Color = Color.Black;
            OpacityWhenOff = 0.1d;
            IsOn = true;
            m_orientation = orientation;
            Corners = SegmentCorners.None;
            Padding = 0;

            m_redrawRegion = new Region();
            CalculatePaths(new RectangleF());
        }

        public virtual void Draw(Graphics g)
        {
            Color color = IsOn ? Color : OffColor;
            using (Brush brush = new SolidBrush(color))
            {
                g.FillPath(brush, m_path);
            }

            //now these areas need to be redrawn if they change
            m_redrawRegion.Dispose();
            m_redrawRegion = new Region();
            m_redrawRegion.Union(m_path);
        }

        public virtual void CalculatePaths(RectangleF container)
        {
            DisposePaths();
            m_path = new GraphicsPath();

            RectangleF rect = container;
            bool bHorizontal = (m_orientation == SegmentOrientation.Horizontal);
            if (bHorizontal)
            {
                rect.Inflate(-Padding, 0);
            }
            else
            {
                rect.Inflate(0, -Padding);
            }

            PointF topLeft = new PointF(rect.Left, rect.Top);
            PointF topRight = new PointF(rect.Right, rect.Top);
            PointF bottomLeft = new PointF(rect.Left, rect.Bottom);
            PointF bottomRight = new PointF(rect.Right, rect.Bottom);

            float xOffset = bHorizontal ? TipLength : 0;
            float yOffset = bHorizontal ? 0 : TipLength;

            PointF topLeftOffset = new PointF(topLeft.X + xOffset, topLeft.Y + yOffset);
            PointF topRightOffset = new PointF(topRight.X - xOffset, topRight.Y + yOffset);
            PointF bottomLeftOffset = new PointF(bottomLeft.X + xOffset, bottomLeft.Y - yOffset);
            PointF bottomRightOffset = new PointF(bottomRight.X - xOffset, bottomRight.Y - yOffset);

            PointF topCenter = new PointF(rect.Left + (rect.Width / 2f), rect.Y + (yOffset / 2f));
            PointF leftCenter = new PointF(rect.Left + (xOffset / 2f), rect.Y + (rect.Height / 2f));
            PointF bottomCenter = new PointF(topCenter.X, rect.Bottom - (yOffset / 2f));
            PointF rightCenter = new PointF(rect.Right - (xOffset / 2f), leftCenter.Y);

            List<PointF> points = new List<PointF>();

            points.Add(leftCenter);
            if (Corners.IsFlagSet(SegmentCorners.TopLeft))
            {
                points.Add(topLeft);
            }
            else
            {
                points.Add(topLeftOffset);
                points.Add(topCenter);
            }

            if (Corners.IsFlagSet(SegmentCorners.TopRight))
            {
                points.Add(topRight);
            }
            else
            {
                points.Add(topRightOffset);
            }

            if (Corners.IsFlagSet(SegmentCorners.BottomRight))
            {
                points.Add(bottomRight);
            }
            else
            {
                points.Add(rightCenter);
                points.Add(bottomRightOffset);
            }

            if (Corners.IsFlagSet(SegmentCorners.BottomLeft))
            {
                points.Add(bottomLeft);
            }
            else
            {
                points.Add(bottomCenter);
                points.Add(bottomLeftOffset);
            }

            if (Corners.IsFlagSet(SegmentCorners.TopLeft))
            {
                points.Add(topLeft);
            }

            m_path.AddLines(points.ToArray());
            m_path.CloseFigure();

            //these areas need to be redrawn now
            m_redrawRegion.Union(m_path);
        }

        public virtual Region GetRedrawRegion()
        {
            return m_redrawRegion;
        }

        protected virtual void DisposePaths()
        {
            if (m_path != null)
            {
                m_path.Dispose();
                m_path = null;
            }
        }

        public virtual void Dispose()
        {
            DisposePaths();

            if (m_redrawRegion != null)
            {
                m_redrawRegion.Dispose();
                m_redrawRegion = null;
            }
        }
    }
}
