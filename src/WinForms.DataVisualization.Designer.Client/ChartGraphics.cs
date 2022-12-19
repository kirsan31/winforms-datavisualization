// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Chart graphic class is used for drawing Chart
//				elements as Rectangles, Pie slices, lines, areas
//				etc. This class is used in all classes where
//				drawing is necessary. The GDI+ graphic class is
//				used throw this class. Encapsulates a GDI+ chart
//				drawing functionality
//

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// The ChartGraphics class provides some designer drawing capabilities.
    /// </summary>
    internal static class ChartGraphics
    {
        #region Markers

        /// <summary>
        /// Creates polygon for multi-corner star marker.
        /// </summary>
        /// <param name="rect">Marker rectangle.</param>
        /// <param name="numberOfCorners">Number of corners (4 and up).</param>
        /// <returns>Array of points.</returns>
        internal static PointF[] CreateStarPolygon(RectangleF rect, int numberOfCorners)
        {
            int numberOfCornersX2;
            checked
            {
                numberOfCornersX2 = numberOfCorners * 2;
            }

            bool outside = true;
            PointF[] points = new PointF[numberOfCornersX2];
            PointF[] tempPoints = new PointF[1];
            Matrix? matrix = null;

            // overflow check
            for (int pointIndex = 0; pointIndex < numberOfCornersX2; pointIndex++)
            {
                if (matrix is null)
                    matrix = new Matrix();
                else
                    matrix.Reset();

                tempPoints[0] = new PointF(rect.X + rect.Width / 2f, (outside == true) ? rect.Y : rect.Y + rect.Height / 4f);
                matrix.RotateAt(pointIndex * (360f / (numberOfCorners * 2f)), new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f));
                matrix.TransformPoints(tempPoints);
                points[pointIndex] = tempPoints[0];
                outside = !outside;
            }

            matrix?.Dispose();
            return points;
        }

        /// <summary>
        /// Draw marker using absolute coordinates of the center.
        /// </summary>
        /// <param name="point">Coordinates of the center.</param>
        /// <param name="markerStyle">Marker style.</param>
        /// <param name="markerSize">Marker size.</param>
        /// <param name="markerColor">Marker color.</param>
        /// <param name="markerBorderColor">Marker border color.</param>
        /// <param name="markerBorderSize">Marker border size.</param>
        /// <param name="markerImage">Marker image name.</param>
        /// <param name="markerImageTransparentColor">Marker image transparent color.</param>
        /// <param name="shadowSize">Marker shadow size.</param>
        /// <param name="shadowColor">Marker shadow color.</param>
        /// <param name="imageScaleRect">Rectangle to which marker image should be scaled.</param>
        /// <param name="forceAntiAlias">Always use anti aliasing when drawing the marker.</param>
        internal static void DrawMarkerAbs(
            Graphics graphics,
            PointF point,
            MarkerStyle markerStyle,
            int markerSize,
            Color markerColor,
            Color markerBorderColor,
            int markerBorderSize,
            int shadowSize,
            Color shadowColor,
            bool forceAntiAlias
            )
        {
            if (graphics is null)
                return;

            // Hide border when zero width specified
            if (markerBorderSize <= 0)
            {
                markerBorderColor = Color.Transparent;
            }

            // Draw standard marker using style, size and color
            if (markerStyle != MarkerStyle.None && markerSize > 0 && markerColor != Color.Empty)
            {
                Pen pen;
                // Enable AntiAliasing
                SmoothingMode oldSmoothingMode = graphics.SmoothingMode;
                if (forceAntiAlias)
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Create solid color brush
                using (SolidBrush brush = new SolidBrush(markerColor))
                {
                    // Calculate marker rectangle
                    RectangleF rect = RectangleF.Empty;
                    rect.X = point.X - markerSize / 2F;
                    rect.Y = point.Y - markerSize / 2F;
                    rect.Width = markerSize;
                    rect.Height = markerSize;

                    // Draw marker depending on style
                    switch (markerStyle)
                    {
                        case MarkerStyle.Star4:
                        case MarkerStyle.Star5:
                        case MarkerStyle.Star6:
                        case MarkerStyle.Star10:
                            {
                                // Set number of corners
                                int cornerNumber = 4;
                                if (markerStyle == MarkerStyle.Star5)
                                {
                                    cornerNumber = 5;
                                }
                                else if (markerStyle == MarkerStyle.Star6)
                                {
                                    cornerNumber = 6;
                                }
                                else if (markerStyle == MarkerStyle.Star10)
                                {
                                    cornerNumber = 10;
                                }

                                // Get star polygon
                                PointF[] points = CreateStarPolygon(rect, cornerNumber);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    using Matrix translateMatrix = graphics.Transform;
                                    translateMatrix.Translate(shadowSize, shadowSize);
                                    using Matrix oldMatrix = graphics.Transform;
                                    graphics.Transform = translateMatrix;

                                    using var sb = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor));
                                    graphics.FillPolygon(sb, points);

                                    graphics.Transform = oldMatrix;
                                }

                                // Draw star
                                graphics.FillPolygon(brush, points);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawPolygon(pen, points);
                                pen.Dispose();
                                break;
                            }
                        case MarkerStyle.Circle:
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    // Add circle to the graphics path
                                    using GraphicsPath path = new GraphicsPath();
                                    path.AddEllipse(rect.X + shadowSize - 1, rect.Y + shadowSize - 1, rect.Width + 2, rect.Height + 2);

                                    // Create path brush
                                    using PathGradientBrush shadowBrush = new PathGradientBrush(path);
                                    shadowBrush.CenterColor = shadowColor;

                                    // Set the color along the entire boundary of the path
                                    Color[] colors = { Color.Transparent };
                                    shadowBrush.SurroundColors = colors;
                                    shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                    // Define brush focus scale
                                    PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                    if (focusScale.X < 0)
                                    {
                                        focusScale.X = 0;
                                    }

                                    if (focusScale.Y < 0)
                                    {
                                        focusScale.Y = 0;
                                    }

                                    shadowBrush.FocusScales = focusScale;

                                    // Draw shadow
                                    graphics.FillPath(shadowBrush, path);
                                }

                                graphics.FillEllipse(brush, rect);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawEllipse(pen, rect);
                                pen.Dispose();
                                break;
                            }
                        case MarkerStyle.Square:
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                    FillRectangleShadowAbs(graphics, rect, shadowColor, shadowSize, shadowColor);

                                graphics.FillRectangle(brush, rect);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawRectangle(pen, (int)Math.Round(rect.X, 0), (int)Math.Round(rect.Y, 0), (int)Math.Round(rect.Width, 0), (int)Math.Round(rect.Height, 0));
                                pen.Dispose();
                                break;
                            }
                        case MarkerStyle.Cross:
                            {
                                // Calculate cross line width and size
                                float crossLineWidth = (float)Math.Ceiling(markerSize / 4F);
                                float crossSize = markerSize;// * (float)Math.Sin(45f/180f*Math.PI);

                                // Calculate cross coordinates
                                PointF[] points = new PointF[12];
                                points[0].X = point.X - crossSize / 2F;
                                points[0].Y = point.Y + crossLineWidth / 2F;
                                points[1].X = point.X - crossSize / 2F;
                                points[1].Y = point.Y - crossLineWidth / 2F;

                                points[2].X = point.X - crossLineWidth / 2F;
                                points[2].Y = point.Y - crossLineWidth / 2F;
                                points[3].X = point.X - crossLineWidth / 2F;
                                points[3].Y = point.Y - crossSize / 2F;
                                points[4].X = point.X + crossLineWidth / 2F;
                                points[4].Y = point.Y - crossSize / 2F;

                                points[5].X = point.X + crossLineWidth / 2F;
                                points[5].Y = point.Y - crossLineWidth / 2F;
                                points[6].X = point.X + crossSize / 2F;
                                points[6].Y = point.Y - crossLineWidth / 2F;
                                points[7].X = point.X + crossSize / 2F;
                                points[7].Y = point.Y + crossLineWidth / 2F;

                                points[8].X = point.X + crossLineWidth / 2F;
                                points[8].Y = point.Y + crossLineWidth / 2F;
                                points[9].X = point.X + crossLineWidth / 2F;
                                points[9].Y = point.Y + crossSize / 2F;
                                points[10].X = point.X - crossLineWidth / 2F;
                                points[10].Y = point.Y + crossSize / 2F;
                                points[11].X = point.X - crossLineWidth / 2F;
                                points[11].Y = point.Y + crossLineWidth / 2F;

                                // Rotate cross coordinates 45 degrees
                                using Matrix rotationMatrix = new Matrix();
                                rotationMatrix.RotateAt(45, point);
                                rotationMatrix.TransformPoints(points);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    // Create translation matrix
                                    using Matrix translateMatrix = graphics.Transform;
                                    translateMatrix.Translate(shadowSize + 1, shadowSize + 1);
                                    using Matrix oldMatrix = graphics.Transform;
                                    graphics.Transform = translateMatrix;

                                    // Add polygon to the graphics path
                                    using GraphicsPath path = new GraphicsPath();
                                    path.AddPolygon(points);

                                    // Create path brush
                                    using PathGradientBrush shadowBrush = new PathGradientBrush(path);
                                    shadowBrush.CenterColor = shadowColor;

                                    // Set the color along the entire boundary of the path
                                    Color[] colors = { Color.Transparent };
                                    shadowBrush.SurroundColors = colors;
                                    shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                    // Define brush focus scale
                                    PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                    if (focusScale.X < 0)
                                    {
                                        focusScale.X = 0;
                                    }

                                    if (focusScale.Y < 0)
                                    {
                                        focusScale.Y = 0;
                                    }

                                    shadowBrush.FocusScales = focusScale;

                                    // Draw shadow
                                    graphics.FillPath(shadowBrush, path);
                                    graphics.Transform = oldMatrix;
                                }

                                // Create translation matrix
                                using Matrix translateMatrixShape = graphics.Transform;
                                using Matrix oldMatrixShape = graphics.Transform;
                                graphics.Transform = translateMatrixShape;

                                graphics.FillPolygon(brush, points);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawPolygon(pen, points);
                                pen.Dispose();

                                graphics.Transform = oldMatrixShape;

                                break;
                            }
                        case MarkerStyle.Diamond:
                            {
                                PointF[] points = new PointF[4];
                                points[0].X = rect.X;
                                points[0].Y = rect.Y + rect.Height / 2F;
                                points[1].X = rect.X + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Y + rect.Height / 2F;
                                points[3].X = rect.X + rect.Width / 2F;
                                points[3].Y = rect.Bottom;

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    using Matrix translateMatrix = graphics.Transform;
                                    translateMatrix.Translate(0, 0);
                                    using Matrix oldMatrix = graphics.Transform;
                                    graphics.Transform = translateMatrix;

                                    // Calculate diamond size
                                    float diamondSize = markerSize * (float)Math.Sin(45f / 180f * Math.PI);

                                    // Calculate diamond rectangle position
                                    RectangleF diamondRect = RectangleF.Empty;
                                    diamondRect.X = point.X - (float)diamondSize / 2F;
                                    diamondRect.Y = point.Y - (float)diamondSize / 2F - shadowSize;
                                    diamondRect.Width = diamondSize;
                                    diamondRect.Height = diamondSize;

                                    // Set rotation matrix to 45
                                    translateMatrix.RotateAt(45, point);
                                    graphics.Transform = translateMatrix;

                                    FillRectangleShadowAbs(graphics, diamondRect, shadowColor, shadowSize, shadowColor);

                                    graphics.Transform = oldMatrix;
                                }

                                graphics.FillPolygon(brush, points);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawPolygon(pen, points);
                                pen.Dispose();
                                break;
                            }
                        case MarkerStyle.Triangle:
                            {
                                PointF[] points = new PointF[3];
                                points[0].X = rect.X;
                                points[0].Y = rect.Bottom;
                                points[1].X = rect.X + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Bottom;

                                // Draw image shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    using Matrix translateMatrix = graphics.Transform;
                                    translateMatrix.Translate(shadowSize - 1, shadowSize + 1);
                                    using Matrix oldMatrix = graphics.Transform;
                                    graphics.Transform = translateMatrix;

                                    // Add polygon to the graphics path
                                    using GraphicsPath path = new GraphicsPath();
                                    path.AddPolygon(points);

                                    // Create path brush
                                    using PathGradientBrush shadowBrush = new PathGradientBrush(path);
                                    shadowBrush.CenterColor = shadowColor;

                                    // Set the color along the entire boundary of the path
                                    Color[] colors = { Color.Transparent };
                                    shadowBrush.SurroundColors = colors;
                                    shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                    // Define brush focus scale
                                    PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                    if (focusScale.X < 0)
                                    {
                                        focusScale.X = 0;
                                    }

                                    if (focusScale.Y < 0)
                                    {
                                        focusScale.Y = 0;
                                    }

                                    shadowBrush.FocusScales = focusScale;

                                    // Draw shadow
                                    graphics.FillPath(shadowBrush, path);
                                    graphics.Transform = oldMatrix;
                                }

                                graphics.FillPolygon(brush, points);
                                pen = new Pen(markerBorderColor, markerBorderSize);
                                graphics.DrawPolygon(pen, points);
                                pen.Dispose();
                                break;
                            }
                        default:
                            {
                                throw new InvalidOperationException(SR.ExceptionGraphicsMarkerStyleUnknown);
                            }
                    }
                }

                // Restore SmoothingMode
                if (forceAntiAlias)
                {
                    graphics.SmoothingMode = oldSmoothingMode;
                }
            }
        }

        #endregion Markers

        /// <summary>
        /// Draw Shadow for a bar
        /// </summary>
        /// <param name="rect">Bar rectangle</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="circular">Draw circular shape inside the rectangle.</param>
        /// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
        internal static void FillRectangleShadowAbs(
            Graphics graphics,
            RectangleF rect,
            Color shadowColor,
            float shadowOffset,
            Color backColor,
            bool circular = false,
            int circularSectorsCount = 0)
        {
            if (graphics is null)
                return;

            // Do not draw shadow for empty rectangle
            if (rect.Height == 0 || rect.Width == 0 || shadowOffset == 0)
            {
                return;
            }

            // Do not draw  shadow if color is IsEmpty or offset is 0
            if (shadowOffset == 0 || shadowColor == Color.Empty)
            {
                return;
            }

            // For non-circular shadow with transparent background - use clipping
            bool clippingUsed = false;
            Region? oldClipRegion = null;
            if (!circular && backColor == Color.Transparent)
            {
                clippingUsed = true;
                oldClipRegion = graphics.Clip;
                Region region = new Region();
                region.MakeInfinite();
                region.Xor(rect);
                graphics.Clip = region;
            }

            // Draw usual or "soft" shadows
            if (circularSectorsCount > 2)
            {
                RectangleF absolute;
                RectangleF offset = RectangleF.Empty;

                absolute = Round(rect);

                // Change shadow color
                using SolidBrush shadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(backColor.A / 2, shadowColor));
                // Shadow Position
                offset.X = absolute.X + shadowOffset;
                offset.Y = absolute.Y + shadowOffset;
                offset.Width = absolute.Width;
                offset.Height = absolute.Height;

                // Draw rectangle
                if (circular)
                    DrawCircleAbs(graphics, null, shadowBrush, offset, circularSectorsCount, false);
                else
                    graphics.FillRectangle(shadowBrush, offset);
            }
            else
            {
                RectangleF absolute;
                RectangleF offset = RectangleF.Empty;

                absolute = Round(rect);

                // Shadow Position
                offset.X = absolute.X + shadowOffset - 1;
                offset.Y = absolute.Y + shadowOffset - 1;
                offset.Width = absolute.Width + 2;
                offset.Height = absolute.Height + 2;

                // Calculate rounded rect radius
                float radius = shadowOffset * 0.7f;
                radius = (float)Math.Max(radius, 2f);
                radius = (float)Math.Min(radius, offset.Width / 4f);
                radius = (float)Math.Min(radius, offset.Height / 4f);
                radius = (float)Math.Ceiling(radius);
                if (circular)
                {
                    radius = offset.Width / 2f;
                }

                // Create rounded rectangle path
                using GraphicsPath path = new GraphicsPath();
                if (circular && offset.Width != offset.Height)
                {
                    float radiusX = offset.Width / 2f;
                    float radiusY = offset.Height / 2f;
                    path.AddLine(offset.X + radiusX, offset.Y, offset.Right - radiusX, offset.Y);
                    path.AddArc(offset.Right - 2f * radiusX, offset.Y, 2f * radiusX, 2f * radiusY, 270, 90);
                    path.AddLine(offset.Right, offset.Y + radiusY, offset.Right, offset.Bottom - radiusY);
                    path.AddArc(offset.Right - 2f * radiusX, offset.Bottom - 2f * radiusY, 2f * radiusX, 2f * radiusY, 0, 90);
                    path.AddLine(offset.Right - radiusX, offset.Bottom, offset.X + radiusX, offset.Bottom);
                    path.AddArc(offset.X, offset.Bottom - 2f * radiusY, 2f * radiusX, 2f * radiusY, 90, 90);
                    path.AddLine(offset.X, offset.Bottom - radiusY, offset.X, offset.Y + radiusY);
                    path.AddArc(offset.X, offset.Y, 2f * radiusX, 2f * radiusY, 180, 90);
                }
                else
                {
                    path.AddLine(offset.X + radius, offset.Y, offset.Right - radius, offset.Y);
                    path.AddArc(offset.Right - 2f * radius, offset.Y, 2f * radius, 2f * radius, 270, 90);
                    path.AddLine(offset.Right, offset.Y + radius, offset.Right, offset.Bottom - radius);
                    path.AddArc(offset.Right - 2f * radius, offset.Bottom - 2f * radius, 2f * radius, 2f * radius, 0, 90);
                    path.AddLine(offset.Right - radius, offset.Bottom, offset.X + radius, offset.Bottom);
                    path.AddArc(offset.X, offset.Bottom - 2f * radius, 2f * radius, 2f * radius, 90, 90);
                    path.AddLine(offset.X, offset.Bottom - radius, offset.X, offset.Y + radius);
                    path.AddArc(offset.X, offset.Y, 2f * radius, 2f * radius, 180, 90);
                }

                using PathGradientBrush shadowBrush = new PathGradientBrush(path);
                shadowBrush.CenterColor = shadowColor;

                // Set the color along the entire boundary of the path
                Color[] colors = { Color.Transparent };
                shadowBrush.SurroundColors = colors;
                shadowBrush.CenterPoint = new PointF(offset.X + offset.Width / 2f, offset.Y + offset.Height / 2f);

                // Define brush focus scale
                PointF focusScale = new PointF(1 - 2f * shadowOffset / offset.Width, 1 - 2f * shadowOffset / offset.Height);
                if (focusScale.X < 0)
                    focusScale.X = 0;
                if (focusScale.Y < 0)
                    focusScale.Y = 0;
                shadowBrush.FocusScales = focusScale;

                // Draw rectangle
                graphics.FillPath(shadowBrush, path);
            }

            // Reset clip region
            if (clippingUsed)
            {
                Region region = graphics.Clip;
                graphics.Clip = oldClipRegion;
                region.Dispose();
            }
        }

        /// <summary>
        /// Fills and/or draws border as circle or polygon.
        /// </summary>
        /// <param name="pen">Border pen.</param>
        /// <param name="brush">Border brush.</param>
        /// <param name="position">Circle position.</param>
        /// <param name="polygonSectorsNumber">Number of sectors for the polygon.</param>
        /// <param name="circle3D">Indicates that circle should be 3D..</param>
        internal static void DrawCircleAbs(Graphics graphics, Pen? pen, Brush brush, RectangleF position, int polygonSectorsNumber, bool circle3D)
        {
            if (graphics is null)
                return;

            bool fill3DCircle = circle3D && brush != null;

            // Draw 2D circle
            if (polygonSectorsNumber <= 2 && !fill3DCircle)
            {
                if (brush != null)
                {
                    graphics.FillEllipse(brush, position);
                }

                if (pen != null)
                {
                    graphics.DrawEllipse(pen, position);
                }
            }

            // Draw circle as polygon with specified number of sectors
            else
            {
                PointF firstPoint = new PointF(position.X + position.Width / 2f, position.Y);
                PointF centerPoint = new PointF(position.X + position.Width / 2f, position.Y + position.Height / 2f);
                PointF prevPoint = PointF.Empty;
                using GraphicsPath path = new GraphicsPath();
                // Remember current smoothing mode
                SmoothingMode oldMode = graphics.SmoothingMode;
                if (fill3DCircle)
                {
                    graphics.SmoothingMode = SmoothingMode.None;
                }

                float sectorSize;
                // Get sector size
                if (polygonSectorsNumber <= 2)
                {
                    // Circle sector size
                    sectorSize = 1f;
                }
                else
                {
                    // Polygon sector size
                    sectorSize = 360f / polygonSectorsNumber;
                }

                Matrix? matrix = null;

                float curentSector;
                // Loop through all sectors
                for (curentSector = 0f; curentSector < 360f; curentSector += sectorSize)
                {
                    // Create matrix
                    if (matrix is null)
                        matrix = new Matrix();
                    else
                        matrix.Reset();

                    matrix.RotateAt(curentSector, centerPoint);

                    // Get point and rotate it
                    PointF[] points = new PointF[] { firstPoint };
                    matrix.TransformPoints(points);

                    // Add point into the path
                    if (!prevPoint.IsEmpty)
                    {
                        path.AddLine(prevPoint, points[0]);

                        // Fill each segment separately for the 3D look
                        if (fill3DCircle)
                        {
                            path.AddLine(points[0], centerPoint);
                            path.AddLine(centerPoint, prevPoint);
                            using (Brush sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                            {
                                graphics.FillPath(sectorBrush, path);
                            }

                            path.Reset();
                        }
                    }

                    // Remember last point
                    prevPoint = points[0];
                }

                matrix?.Dispose();
                path.CloseAllFigures();

                // Fill last segment for the 3D look
                if (!prevPoint.IsEmpty && fill3DCircle)
                {
                    path.AddLine(prevPoint, firstPoint);
                    path.AddLine(firstPoint, centerPoint);
                    path.AddLine(centerPoint, prevPoint);
                    using (Brush sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                    {
                        graphics.FillPath(sectorBrush, path);
                    }

                    path.Reset();
                }

                // Restore old mode
                if (fill3DCircle)
                {
                    graphics.SmoothingMode = oldMode;
                }

                if (brush != null && !circle3D)
                {
                    graphics.FillPath(brush, path);
                }

                if (pen != null)
                {
                    graphics.DrawPath(pen, path);
                }
            }
        }

        /// <summary>
        /// Find rounding coordinates for a rectangle
        /// </summary>
        /// <param name="rect">Rectangle which has to be rounded</param>
        /// <returns>Rounded rectangle</returns>
        internal static RectangleF Round(RectangleF rect)
        {
            float left = (float)Math.Round((double)rect.Left);
            float right = (float)Math.Round((double)rect.Right);
            float top = (float)Math.Round((double)rect.Top);
            float bottom = (float)Math.Round((double)rect.Bottom);

            return new RectangleF(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Creates 3D sector brush.
        /// </summary>
        /// <param name="brush">Original brush.</param>
        /// <param name="curentSector">Sector position.</param>
        /// <param name="sectorSize">Sector size.</param>
        /// <returns>3D brush.</returns>
        internal static Brush GetSector3DBrush(Brush? brush, float curentSector, float sectorSize)
        {
            // Get color from the brush
            Color brushColor = Color.Gray;
            switch (brush)
            {
                case HatchBrush br:
                    brushColor = br.BackgroundColor;
                    break;
                case LinearGradientBrush br:
                    brushColor = br.LinearColors[0];
                    break;
                case PathGradientBrush br:
                    brushColor = br.CenterColor;
                    break;
                case SolidBrush br:
                    brushColor = br.Color;
                    break;
            }

            // Adjust sector angle
            curentSector -= sectorSize / 2f;

            // Make adjustment for polygon circle with 5 segments
            // to avoid the issue that bottom segment is too dark
            if (sectorSize == 72f && curentSector == 180f)
            {
                curentSector *= 0.8f;
            }

            // No angles more than 180
            if (curentSector > 180)
            {
                curentSector = 360f - curentSector;
            }

            curentSector /= 180F;

            // Get brush
            brushColor = GetBrightGradientColor(brushColor, curentSector);

            // Get brush
            return new SolidBrush(brushColor);
        }

        /// <summary>
        /// This method creates gradient color with brightness
        /// </summary>
        /// <param name="beginColor">Start color for gradient.</param>
        /// <param name="position">Position used between Start and end color.</param>
        /// <returns>Calculated Gradient color from gradient position</returns>
        internal static Color GetBrightGradientColor(Color beginColor, double position)
        {
            double brightness = 0.5;
            if (position < brightness)
            {
                return GetGradientColor(Color.FromArgb(beginColor.A, 255, 255, 255), beginColor, 1 - brightness + position);
            }
            else if (-brightness + position < 1)
            {
                return GetGradientColor(beginColor, Color.Black, -brightness + position);
            }
            else
            {
                return Color.FromArgb(beginColor.A, 0, 0, 0);
            }
        }

        /// <summary>
        /// Returns the gradient color from a gradient position.
        /// </summary>
        /// <param name="beginColor">The color from the gradient beginning</param>
        /// <param name="endColor">The color from the gradient end.</param>
        /// <param name="relativePosition">The relative position.</param>
        /// <returns>Result color.</returns>
        internal static Color GetGradientColor(Color beginColor, Color endColor, double relativePosition)
        {
            // Check if position is valid
            if (relativePosition < 0 || relativePosition > 1 || double.IsNaN(relativePosition))
            {
                return beginColor;
            }

            // Extracts Begin color
            int nBRed = beginColor.R;
            int nBGreen = beginColor.G;
            int nBBlue = beginColor.B;

            // Extracts End color
            int nERed = endColor.R;
            int nEGreen = endColor.G;
            int nEBlue = endColor.B;

            // Gradient positions for Red, Green and Blue colors
            double dRRed = nBRed + (nERed - nBRed) * relativePosition;
            double dRGreen = nBGreen + (nEGreen - nBGreen) * relativePosition;
            double dRBlue = nBBlue + (nEBlue - nBBlue) * relativePosition;

            // Make sure colors are in range from 0 to 255
            if (dRRed > 255.0)
                dRRed = 255.0;
            if (dRRed < 0.0)
                dRRed = 0.0;
            if (dRGreen > 255.0)
                dRGreen = 255.0;
            if (dRGreen < 0.0)
                dRGreen = 0.0;
            if (dRBlue > 255.0)
                dRBlue = 255.0;
            if (dRBlue < 0.0)
                dRBlue = 0.0;

            // Return a gradient color position
            return Color.FromArgb(beginColor.A, (int)dRRed, (int)dRGreen, (int)dRBlue);
        }

        /// <summary>
        /// This method creates a gradient brush.
        /// </summary>
        /// <param name="rectangle">A rectangle which has to be filled with a gradient color.</param>
        /// <param name="firstColor">First color.</param>
        /// <param name="secondColor">Second color.</param>
        /// <param name="type ">Gradient type .</param>
        /// <returns>Gradient Brush</returns>
        internal static Brush GetGradientBrush(
            RectangleF rectangle,
            Color firstColor,
            Color secondColor,
            GradientStyle type
            )
        {
            // Increase the brush rectangle by 1 pixel to ensure the fit
            rectangle.Inflate(1f, 1f);
            float angle = 0;

            Brush gradientBrush;
            // Function which create gradient brush fires exception if
            // rectangle size is zero.
            if (rectangle.Height == 0 || rectangle.Width == 0)
            {
                gradientBrush = new SolidBrush(Color.Black);
                return gradientBrush;
            }

            // *******************************************
            // Linear Gradient
            // *******************************************
            // Check linear type .
            if (type == GradientStyle.LeftRight || type == GradientStyle.VerticalCenter)
            {
                angle = 0;
            }
            else if (type == GradientStyle.TopBottom || type == GradientStyle.HorizontalCenter)
            {
                angle = 90;
            }
            else if (type == GradientStyle.DiagonalLeft)
            {
                angle = (float)(Math.Atan(rectangle.Width / rectangle.Height) * 180 / Math.PI);
            }
            else if (type == GradientStyle.DiagonalRight)
            {
                angle = (float)(180 - Math.Atan(rectangle.Width / rectangle.Height) * 180 / Math.PI);
            }

            // Create a linear gradient brush
            if (type == GradientStyle.TopBottom || type == GradientStyle.LeftRight
                || type == GradientStyle.DiagonalLeft || type == GradientStyle.DiagonalRight
                || type == GradientStyle.HorizontalCenter || type == GradientStyle.VerticalCenter)
            {
                RectangleF tempRect = new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                // For Horizontal and vertical center gradient types
                if (type == GradientStyle.HorizontalCenter)
                {
                    // Resize and wrap gradient
                    tempRect.Height /= 2F;
                    LinearGradientBrush linearGradientBrush = new LinearGradientBrush(tempRect, firstColor, secondColor, angle);
                    gradientBrush = linearGradientBrush;
                    linearGradientBrush.WrapMode = WrapMode.TileFlipX;
                }
                else if (type == GradientStyle.VerticalCenter)
                {
                    // Resize and wrap gradient
                    tempRect.Width /= 2F;
                    LinearGradientBrush linearGradientBrush = new LinearGradientBrush(tempRect, firstColor, secondColor, angle);
                    gradientBrush = linearGradientBrush;
                    linearGradientBrush.WrapMode = WrapMode.TileFlipX;
                }
                else
                {
                    gradientBrush = new LinearGradientBrush(rectangle, firstColor, secondColor, angle);
                }

                return gradientBrush;
            }

            // *******************************************
            // Gradient is not linear : From Center.
            // *******************************************

            // Create a path
            GraphicsPath path = new GraphicsPath();

            // Add a rectangle to the path
            path.AddRectangle(rectangle);

            // Create a gradient brush
            PathGradientBrush pathGradientBrush = new PathGradientBrush(path);
            gradientBrush = pathGradientBrush;

            // Set the center color
            pathGradientBrush.CenterColor = firstColor;

            // Set the Surround color
            Color[] colors = { secondColor };
            pathGradientBrush.SurroundColors = colors;

            path?.Dispose();

            return gradientBrush;
        }

        /// <summary>
        /// Creates a Hatch Brush.
        /// </summary>
        /// <param name="hatchStyle">Chart Hatch style.</param>
        /// <param name="backColor">Back Color.</param>
        /// <param name="foreColor">Fore Color.</param>
        /// <returns>Brush</returns>
        internal static Brush GetHatchBrush(
            ChartHatchStyle hatchStyle,
            Color backColor,
            Color foreColor
            )
        {
            // Convert Chart Hatch Style enum
            // to Hatch Style enum.
            HatchStyle hatch;
            hatch = (HatchStyle)Enum.Parse(typeof(HatchStyle), hatchStyle.ToString());

            // Create Hatch Brush
            return new HatchBrush(hatch, foreColor, backColor);
        }
    }
}