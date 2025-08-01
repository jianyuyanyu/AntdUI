// THIS FILE IS PART OF SVG PROJECT
// THE SVG PROJECT IS AN OPENSOURCE LIBRARY LICENSED UNDER THE MS-PL License.
// COPYRIGHT (C) svg-net. ALL RIGHTS RESERVED.
// GITHUB: https://github.com/svg-net/SVG

using AntdUI.Svg.DataTypes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace AntdUI.Svg
{
    public class SvgMarker : SvgPathBasedElement, ISvgViewPort
    {
        public override string ClassName => "marker";

        private SvgOrient _svgOrient = new SvgOrient();
        private SvgVisualElement _markerElement = null;

        /// <summary>
        /// Return the child element that represent the marker
        /// </summary>
        private SvgVisualElement MarkerElement
        {
            get
            {
                _markerElement ??= (SvgVisualElement)Children.FirstOrDefault(x => x is SvgVisualElement);
                return _markerElement;
            }
        }

        [SvgAttribute("refX")]
        public virtual SvgUnit RefX
        {
            get { return Attributes.GetAttribute<SvgUnit>("refX"); }
            set { Attributes["refX"] = value; }
        }

        [SvgAttribute("refY")]
        public virtual SvgUnit RefY
        {
            get { return Attributes.GetAttribute<SvgUnit>("refY"); }
            set { Attributes["refY"] = value; }
        }


        [SvgAttribute("orient")]
        public virtual SvgOrient Orient
        {
            get { return _svgOrient; }
            set { _svgOrient = value; }
        }


        [SvgAttribute("overflow")]
        public virtual SvgOverflow Overflow
        {
            get { return Attributes.GetAttribute<SvgOverflow>("overflow"); }
            set { Attributes["overflow"] = value; }
        }


        [SvgAttribute("viewBox")]
        public virtual SvgViewBox ViewBox
        {
            get { return Attributes.GetAttribute<SvgViewBox>("viewBox"); }
            set { Attributes["viewBox"] = value; }
        }


        [SvgAttribute("preserveAspectRatio")]
        public virtual SvgAspectRatio AspectRatio
        {
            get { return Attributes.GetAttribute<SvgAspectRatio>("preserveAspectRatio"); }
            set { Attributes["preserveAspectRatio"] = value; }
        }


        [SvgAttribute("markerWidth")]
        public virtual SvgUnit MarkerWidth
        {
            get { return Attributes.GetAttribute<SvgUnit>("markerWidth"); }
            set { Attributes["markerWidth"] = value; }
        }

        [SvgAttribute("markerHeight")]
        public virtual SvgUnit MarkerHeight
        {
            get { return Attributes.GetAttribute<SvgUnit>("markerHeight"); }
            set { Attributes["markerHeight"] = value; }
        }

        [SvgAttribute("markerUnits")]
        public virtual SvgMarkerUnits MarkerUnits
        {
            get { return Attributes.GetAttribute<SvgMarkerUnits>("markerUnits"); }
            set { Attributes["markerUnits"] = value; }
        }

        /// <summary>
        /// If not set set in the marker, consider the attribute in the drawing element.
        /// </summary>
        public override SvgPaintServer Fill
        {
            get
            {
                if (MarkerElement != null)
                    return MarkerElement.Fill;
                return base.Fill;
            }
        }

        /// <summary>
        /// If not set set in the marker, consider the attribute in the drawing element.
        /// </summary>
        public override SvgPaintServer Stroke
        {
            get
            {
                if (MarkerElement != null)
                    return MarkerElement.Stroke;
                return base.Stroke;
            }
        }

        public SvgMarker()
        {
            MarkerUnits = SvgMarkerUnits.StrokeWidth;
            MarkerHeight = 3;
            MarkerWidth = 3;
            Overflow = SvgOverflow.Hidden;
        }

        public override GraphicsPath Path(ISvgRenderer renderer)
        {
            if (MarkerElement != null)
                return MarkerElement.Path(renderer);
            return null;
        }

        /// <summary>
        /// Render this marker using the slope of the given line segment
        /// </summary>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pRefPoint"></param>
        /// <param name="pMarkerPoint1"></param>
        /// <param name="pMarkerPoint2"></param>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2)
        {
            float fAngle1 = 0f;
            if (Orient.IsAuto)
            {
                // Only calculate this if needed.
                float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
                float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
                fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);
            }

            RenderPart2(fAngle1, pRenderer, pOwner, pRefPoint);
        }

        /// <summary>
        /// Render this marker using the average of the slopes of the two given line segments
        /// </summary>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pRefPoint"></param>
        /// <param name="pMarkerPoint1"></param>
        /// <param name="pMarkerPoint2"></param>
        /// <param name="pMarkerPoint3"></param>
        public void RenderMarker(ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pRefPoint, PointF pMarkerPoint1, PointF pMarkerPoint2, PointF pMarkerPoint3)
        {
            float xDiff = pMarkerPoint2.X - pMarkerPoint1.X;
            float yDiff = pMarkerPoint2.Y - pMarkerPoint1.Y;
            float fAngle1 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            xDiff = pMarkerPoint3.X - pMarkerPoint2.X;
            yDiff = pMarkerPoint3.Y - pMarkerPoint2.Y;
            float fAngle2 = (float)(Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI);

            RenderPart2((fAngle1 + fAngle2) / 2, pRenderer, pOwner, pRefPoint);
        }

        /// <summary>
        /// Common code for rendering a marker once the orientation angle has been calculated
        /// </summary>
        /// <param name="fAngle"></param>
        /// <param name="pRenderer"></param>
        /// <param name="pOwner"></param>
        /// <param name="pMarkerPoint"></param>
        private void RenderPart2(float fAngle, ISvgRenderer pRenderer, SvgVisualElement pOwner, PointF pMarkerPoint)
        {
            using (var pRenderPen = CreatePen(pOwner, pRenderer))
            {
                using (var markerPath = GetClone(pOwner))
                {
                    using (var transMatrix = new Matrix())
                    {
                        transMatrix.Translate(pMarkerPoint.X, pMarkerPoint.Y);
                        if (Orient.IsAuto)
                            transMatrix.Rotate(fAngle);
                        else
                            transMatrix.Rotate(Orient.Angle);
                        switch (MarkerUnits)
                        {
                            case SvgMarkerUnits.StrokeWidth:
                                if (ViewBox.Width > 0 && ViewBox.Height > 0)
                                {
                                    transMatrix.Scale(MarkerWidth, MarkerHeight);
                                    var strokeWidth = pOwner.StrokeWidth.ToDeviceValue(pRenderer, UnitRenderingType.Other, this);
                                    transMatrix.Translate(AdjustForViewBoxWidth(-RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this) *
                                                            strokeWidth),
                                                          AdjustForViewBoxHeight(-RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this) *
                                                            strokeWidth));
                                }
                                else
                                {
                                    // SvgMarkerUnits.UserSpaceOnUse
                                    //	TODO: We know this isn't correct.
                                    //        But use this until the TODOs from AdjustForViewBoxWidth and AdjustForViewBoxHeight are done.
                                    //  MORE see Unit Test "MakerEndTest.TestArrowCodeCreation()"
                                    transMatrix.Translate(-RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this),
                                                         -RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
                                }
                                break;
                            case SvgMarkerUnits.UserSpaceOnUse:
                                transMatrix.Translate(-RefX.ToDeviceValue(pRenderer, UnitRenderingType.Horizontal, this),
                                                      -RefY.ToDeviceValue(pRenderer, UnitRenderingType.Vertical, this));
                                break;
                        }

                        if (MarkerElement != null && MarkerElement.Transforms != null)
                        {
                            foreach (var transformation in MarkerElement.Transforms)
                            {
                                transMatrix.Multiply(transformation.Matrix(0, 0));
                            }
                        }
                        markerPath.Transform(transMatrix);
                        if (pRenderPen != null) pRenderer.DrawPath(pRenderPen, markerPath);

                        SvgPaintServer pFill = Children.First().Fill;
                        SvgFillRule pFillRule = FillRule;								// TODO: What do we use the fill rule for?
                        float fOpacity = FillOpacity;

                        if (pFill != null)
                        {
                            using (var pBrush = pFill.GetBrush(this, pRenderer, fOpacity))
                            {
                                pRenderer.FillPath(pBrush, markerPath);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a pen that can be used to render this marker
        /// </summary>
        /// <returns></returns>
        private Pen CreatePen(SvgVisualElement pPath, ISvgRenderer renderer)
        {
            if (Stroke == null) return null;
            Brush pBrush = Stroke.GetBrush(this, renderer, Opacity);
            switch (MarkerUnits)
            {
                case SvgMarkerUnits.StrokeWidth:
                    // TODO: have to multiply with marker stroke width if it is not inherted from the
                    // same ancestor as owner path stroke width
                    return (new Pen(pBrush, pPath.StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)));
                case SvgMarkerUnits.UserSpaceOnUse:
                    return (new Pen(pBrush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)));
            }
            return (new Pen(pBrush, StrokeWidth.ToDeviceValue(renderer, UnitRenderingType.Other, this)));
        }

        /// <summary>
        /// Get a clone of the current path, scaled for the stroke width
        /// </summary>
        /// <returns></returns>
        private GraphicsPath GetClone(SvgVisualElement pPath)
        {
            GraphicsPath pRet = Path(null).Clone() as GraphicsPath;
            switch (MarkerUnits)
            {
                case SvgMarkerUnits.StrokeWidth:
                    using (var transMatrix = new Matrix())
                    {
                        transMatrix.Scale(AdjustForViewBoxWidth(pPath.StrokeWidth), AdjustForViewBoxHeight(pPath.StrokeWidth));
                        pRet.Transform(transMatrix);
                    }
                    break;
                case SvgMarkerUnits.UserSpaceOnUse:
                    break;
            }
            return (pRet);
        }

        /// <summary>
        /// Adjust the given value to account for the width of the viewbox in the viewport
        /// </summary>
        /// <param name="fWidth"></param>
        /// <returns></returns>
        private float AdjustForViewBoxWidth(float fWidth)
        {
            //	TODO: We know this isn't correct
            return (ViewBox.Width <= 0 ? 1 : fWidth / ViewBox.Width);
        }

        /// <summary>
        /// Adjust the given value to account for the height of the viewbox in the viewport
        /// </summary>
        /// <param name="fHeight"></param>
        /// <returns></returns>
        private float AdjustForViewBoxHeight(float fHeight)
        {
            //	TODO: We know this isn't correct
            return (ViewBox.Height <= 0 ? 1 : fHeight / ViewBox.Height);
        }
    }
}