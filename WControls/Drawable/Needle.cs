﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WControls.Utils;
using System.Drawing.Drawing2D;

namespace WControls.Drawable
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Category("Appearance")]
    [Description("The needle associated with this control")]
    public class Needle : IDisposable, IDrawable
    {
        #region Public Properties

        /// <summary>
        /// Raised when the appearance (color, etc) of the axis has changed
        /// </summary>
        [Description("Raised when the appearance (color, etc) of the axis has changed")]
        public event EventHandler AppearanceChanged;

        /// <summary>
        /// Raised when the underlying graphics paths need to be recalculated
        /// </summary>
        [Description("Raised when the underlying graphics paths need to be recalculated")]
        public event EventHandler LayoutChanged;

        private double m_orientation;
        private bool m_bDrawShadows = true;

        /// <summary>
        /// The orientation of the needle:
        /// <para>
        /// 0 = East, 90 = North, 180 = West, 270 = South
        /// </para>
        /// </summary>
        [Description("The orientation of the needle")]
        public double Orientation 
        {
            get { return m_orientation; }
            set 
            {
                m_orientation = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// Whether or not shadows are drawn for the needle and hub
        /// </summary>
        [DefaultValue(true)]
        [Description("Whether or not shadows are drawn for the needle and hub")]
        public bool ShadowsVisible
        {
            get { return m_bDrawShadows; }
            set
            {
                m_bDrawShadows = value;
                OnAppearanceChanged();
            }
        }

        #region Needle

        private float m_radiusPercent = .85f;
        private Color m_needleColor = Color.Red;
        private float m_baseSizePercent = .02f;
        private float m_tipWidthDegrees = 1f;
        private float m_tipExtensionPercent = .01f;
        private bool m_bIsNeedleAboveHub = false;

        /// <summary>
        /// The percent of the whole control to make the needle length
        /// </summary>
        [DefaultValue(.85f)]
        [Description("The percent of the whole control to make the needle length")]
        public float RadiusPercent
        {
            get { return m_radiusPercent; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_radiusPercent = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// The color of the main needle
        /// </summary>
        [DefaultValue(typeof(Color), "Red")]
        [Description("The color of the main needle")]
        public Color NeedleColor
        {
            get { return m_needleColor; }
            set
            {
                m_needleColor = value;
                OnAppearanceChanged();
            }
        }

        /// <summary>
        /// The percent of the whole control to use as the base width for the needle
        /// </summary>
        [DefaultValue(.02f)]
        [Description("The percent of the whole control to use as the base width for the needle")]
        public float BaseSizePercent
        {
            get { return m_baseSizePercent; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_baseSizePercent = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// The width (in degrees) of the tip of the needle. This is necessary so that the
        /// tip comes to a blunt point
        /// </summary>
        [DefaultValue(1f)]
        [Description("The width (in degrees) of the tip of the needle")]
        public float TipWidthDegrees
        {
            get { return m_tipWidthDegrees; }
            set
            {
                m_tipWidthDegrees = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// The percent of the whole control to extend past the RadiusPercent for
        /// the needle to come to a finite point (blunt point)
        /// </summary>
        [DefaultValue(.01f)]
        [Description("0 makes tip more square, larger values make it more pointed")]
        public float TipExtensionPercent
        {
            get { return m_tipExtensionPercent; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                m_tipExtensionPercent = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// Whether or not the needle is drawn on top of the hub
        /// </summary>
        [DefaultValue(false)]
        [Description("Whether or not the needle is drawn on top of the hub")]
        public bool IsNeedleAboveHub
        {
            get { return m_bIsNeedleAboveHub; }
            set
            {
                m_bIsNeedleAboveHub = value;
                OnAppearanceChanged();
            }
        }



        #endregion

        #region Hub

        private Color m_hubColor = Color.Black;
        private float m_hubSizePercent = .13f;
        private Color m_hubShadeColor = Color.Black;

        /// <summary>
        /// The color of the center circle on which the needle rotates
        /// </summary>
        [DefaultValue(typeof(Color), "Black")]
        [Description("The color of the center circle on which the needle rotates")]
        public Color HubColor
        {
            get { return m_hubColor; }
            set
            {
                m_hubColor = value;
                OnAppearanceChanged();
            }
        }

        /// <summary>
        /// The percent of the whole control to use as the hub size
        /// </summary>
        [DefaultValue(.13f)]
        [Description("The percent of the whole control to use as the hub size")]
        public float HubSizePercent
        {
            get { return m_hubSizePercent; }
            set
            {
                m_hubSizePercent = value;
                OnLayoutChanged();
            }
        }

        /// <summary>
        /// The shade color around the hub of the needle
        /// </summary>
        [DefaultValue(typeof(Color), "Black")]
        [Description("The shade color around the hub of the needle")]
        public Color HubShadeColor
        {
            get { return m_hubShadeColor; }
            set
            {
                m_hubShadeColor = value;
                OnAppearanceChanged();
            }
        }

        #endregion

        #endregion

        private GraphicsPath m_needlePath;
        private GraphicsPath m_hubPath;
        private GraphicsPath m_shadowPath;
        private Region m_redrawRegion;

        private const float DROP_SHADOW_X = .01f;
        private const float DROP_SHADOW_Y = .01f;

        private const float HUB_BEVEL_PERCENT = .8f;

        public Needle()
        {
            NeedleColor = Color.Red;
            HubColor = Color.Black;
            HubShadeColor = Color.Black;
            HubSizePercent = .13f;
            Orientation = 90d;
            RadiusPercent = .85f;
            BaseSizePercent = .02f;
            TipWidthDegrees = 1f;
            TipExtensionPercent = .01f;
            IsNeedleAboveHub = false;
            ShadowsVisible = true;

            m_redrawRegion = new Region();

            CalculatePaths(new RectangleF());
        }

        public Region GetRedrawRegion()
        {
            return m_redrawRegion;
        }

        public void Draw(Graphics g)
        {
            if (m_hubPath != null && m_needlePath != null && m_shadowPath != null)
            {
                using (Brush shadowBrush = new SolidBrush(Color.FromArgb(100, Color.Black)))
                using (Brush needleBrush = new SolidBrush(NeedleColor))
                using (PathGradientBrush hubBrush = new PathGradientBrush(m_hubPath))
                using (Pen hubPen = new Pen(HubShadeColor, 1f))
                using (Pen needlePen = new Pen(GraphicsHelper.GetMixedColor(NeedleColor, Color.Black, .75d)))
                {
                    hubBrush.SurroundColors = new Color[] { HubShadeColor };
                    hubBrush.CenterColor = HubColor;
                    hubBrush.FocusScales = new PointF(HUB_BEVEL_PERCENT, HUB_BEVEL_PERCENT);

                    if (m_bDrawShadows)
                    {
                        //draw shadow lowest
                        g.FillPath(shadowBrush, m_shadowPath);
                    }

                    if (IsNeedleAboveHub)
                    {
                        //then hub
                        g.FillPath(hubBrush, m_hubPath);
                        g.DrawPath(hubPen, m_hubPath);

                        //then needle on top
                        g.FillPath(needleBrush, m_needlePath);
                        g.DrawPath(needlePen, m_needlePath);
                    }
                    else
                    {
                        //then needle
                        g.FillPath(needleBrush, m_needlePath);
                        g.DrawPath(needlePen, m_needlePath);

                        //and hub on very top
                        g.FillPath(hubBrush, m_hubPath);
                        g.DrawPath(hubPen, m_hubPath);
                    }
                }

                //now these areas need to be redrawn if they change
                m_redrawRegion.Dispose();
                m_redrawRegion = new Region();
                m_redrawRegion.Union(m_shadowPath);
                m_redrawRegion.Union(m_needlePath);
                m_redrawRegion.Union(m_hubPath);
            }
        }

        public void CalculatePaths(RectangleF container)
        {
            DisposePaths();

            m_needlePath = new GraphicsPath();
            m_hubPath = new GraphicsPath();
            m_shadowPath = new GraphicsPath(FillMode.Winding);

            float hubDeltaX = ((container.Width * (1 - HubSizePercent)) / 2f);
            float hubDeltaY = ((container.Height * (1 - HubSizePercent)) / 2f);
            RectangleF centerRect = container;
            centerRect.Inflate(-hubDeltaX, -hubDeltaY);

            float deltaX = ((container.Width * (1 - RadiusPercent)) / 2f);
            float deltaY = ((container.Height * (1 - RadiusPercent)) / 2f);
            RectangleF needleRect = container;
            needleRect.Inflate(-deltaX, -deltaY);

            float baseDeltaX = ((container.Width * (1 - BaseSizePercent)) / 2f);
            float baseDeltaY = ((container.Height * (1 - BaseSizePercent)) / 2f);
            RectangleF baseNeedleRect = container;
            baseNeedleRect.Inflate(-baseDeltaX, -baseDeltaY);

            float tipDeltaX = ((container.Width * TipExtensionPercent) / 2f);
            float tipDeltaY = ((container.Height * TipExtensionPercent) / 2f);
            RectangleF tipNeedleRect = needleRect;
            tipNeedleRect.Inflate(tipDeltaX, tipDeltaY);

            PointF tip = GraphicsHelper.GetPointInArc(tipNeedleRect, Orientation, 0);
            PointF tipMore = GraphicsHelper.GetPointInArc(needleRect, Orientation + (TipWidthDegrees / 2f), 0);
            PointF tipLess = GraphicsHelper.GetPointInArc(needleRect, Orientation - (TipWidthDegrees / 2f), 0);

            PointF baseMore = GraphicsHelper.GetPointInArc(baseNeedleRect, Orientation + 90, 0);
            PointF baseLess = GraphicsHelper.GetPointInArc(baseNeedleRect, Orientation - 90, 0);
            PointF baseExt = GraphicsHelper.GetPointInArc(baseNeedleRect, Orientation + 180, 0);

            m_needlePath.AddCurve(new PointF[] { baseLess, baseExt, baseMore });
            m_needlePath.AddLine(baseMore, tipMore);
            m_needlePath.AddCurve(new PointF[] { tipMore, tip, tipLess });
            m_needlePath.CloseFigure();

            m_hubPath.AddEllipse(centerRect);

            centerRect.Width += (container.Width * DROP_SHADOW_X);
            centerRect.Height += (container.Height * DROP_SHADOW_Y);

            m_shadowPath.AddEllipse(centerRect);
            using (GraphicsPath tempPath = m_needlePath.Clone() as GraphicsPath)
            using (Matrix trans = new Matrix())
            {
                trans.Translate(container.Width * DROP_SHADOW_X,
                                container.Height * DROP_SHADOW_Y);
                tempPath.Transform(trans);
                m_shadowPath.AddPath(tempPath, true);
            }

            //these areas need to be redrawn now
            m_redrawRegion.Union(m_shadowPath);
            m_redrawRegion.Union(m_needlePath);
            m_redrawRegion.Union(m_hubPath);
        }

        protected virtual void OnLayoutChanged()
        {
            if (LayoutChanged != null)
            {
                LayoutChanged(this, EventArgs.Empty);
            }
        }

        protected virtual void OnAppearanceChanged()
        {
            if (AppearanceChanged != null)
            {
                AppearanceChanged(this, EventArgs.Empty);
            }
        }

        private void DisposePaths()
        {
            if (m_needlePath != null)
            {
                m_needlePath.Dispose();
                m_needlePath = null;
            }
            if (m_hubPath != null)
            {
                m_hubPath.Dispose();
                m_hubPath = null;
            }
            if (m_shadowPath != null)
            {
                m_shadowPath.Dispose();
                m_shadowPath = null;
            }
        }

        public void Dispose()
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
