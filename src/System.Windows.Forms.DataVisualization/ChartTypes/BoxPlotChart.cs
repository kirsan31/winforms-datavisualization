﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Provides 2D and 3D drawing and hit testing of the 
//              Box Plot chart.
//  ------------------
//  Box Plot Overview:
//  ------------------
//  The Box Plot chart type consists of one or more box symbols that 
//  summarize the distribution of the data within one or more data 
//  sets. A Box Chart displays a rectangle with whisker lines 
//  extending from both ends. What makes a Box Plot unique, in 
//  comparison to standard chart types, is that the values for a box 
//  plot most often are calculated values from data that is present 
//  in another series. One box symbol, or data point, is associated 
//  with one data series. The data for a Box Plot series may still 
//  be populated using Data Binding or the Points Collection.
//


using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes;

/// <summary>
/// BoxPlotChart class provides 2D and 3D drawing and hit testing of
/// the Box Plot chart.
/// </summary>
internal class BoxPlotChart : IChartType
{
    #region Fields

    /// <summary>
    /// Vertical axis
    /// </summary>
#pragma warning disable CA2213 // Disposable fields should be disposed
    protected Axis vAxis;

    /// <summary>
    /// Horizontal axis
    /// </summary>
    protected Axis hAxis;
#pragma warning restore CA2213 // Disposable fields should be disposed

    /// <summary>
    /// Side by side drawing flag.
    /// </summary>
    protected bool showSideBySide = true;

    #endregion

    #region Constructor

    /// <summary>
    /// Box Plot chart constructor.
    /// </summary>
    public BoxPlotChart()
    {
    }

    #endregion

    #region IChartType interface implementation

    /// <summary>
    /// Chart type name
    /// </summary>
    public virtual string Name => ChartTypeNames.BoxPlot;

    /// <summary>
    /// True if chart type is stacked
    /// </summary>
    public virtual bool Stacked => false;


    /// <summary>
    /// True if stacked chart type supports groups
    /// </summary>
    public virtual bool SupportStackedGroups => false;


    /// <summary>
    /// True if stacked chart type should draw separately positive and 
    /// negative data points ( Bar and column Stacked types ).
    /// </summary>
    public bool StackSign => false;

    /// <summary>
    /// True if chart type supports axes
    /// </summary>
    public virtual bool RequireAxes => true;

    /// <summary>
    /// Chart type with two y values used for scale ( bubble chart type )
    /// </summary>
    public bool SecondYScale => false;

    /// <summary>
    /// True if chart type requires circular chart area.
    /// </summary>
    public bool CircularChartArea => false;

    /// <summary>
    /// True if chart type supports logarithmic axes
    /// </summary>
    public virtual bool SupportLogarithmicAxes => true;

    /// <summary>
    /// True if chart type requires to switch the value (Y) axes position
    /// </summary>
    public virtual bool SwitchValueAxes => false;

    /// <summary>
    /// True if chart series can be placed side-by-side.
    /// </summary>
    public bool SideBySideSeries => false;

    /// <summary>
    /// True if each data point of a chart must be represented in the legend
    /// </summary>
    public virtual bool DataPointsInLegend => false;

    /// <summary>
    /// If the crossing value is auto Crossing value ZeroCrossing should be 
    /// automatically set to zero for some chart 
    /// types (Bar, column, area etc.)
    /// </summary>
    public virtual bool ZeroCrossing => false;

    /// <summary>
    /// True if palette colors should be applied for each data paoint.
    /// Otherwise the color is applied to the series.
    /// </summary>
    public virtual bool ApplyPaletteColorsToPoints => false;

    /// <summary>
    /// Indicates that extra Y values are connected to the scale of the Y axis
    /// </summary>
    public virtual bool ExtraYValuesConnectedToYAxis => true;

    /// <summary>
    /// Indicates that this is a one hundred percent chart.
    /// Axis scale from 0 to 100 percent should be used.
    /// </summary>
    public virtual bool HundredPercent => false;

    /// <summary>
    /// Indicates that negative 100% stacked values are shown on
    /// the other side of the X axis
    /// </summary>
    public virtual bool HundredPercentSupportNegative => false;

    /// <summary>
    /// How to draw series/points in legend:
    /// Filled rectangle, Line or Marker
    /// </summary>
    /// <param name="series">Legend item series.</param>
    /// <returns>Legend item style.</returns>
    public virtual LegendImageStyle GetLegendImageStyle(Series series)
    {
        return LegendImageStyle.Rectangle;
    }

    /// <summary>
    /// Number of supported Y value(s) per point 
    /// </summary>
    public virtual int YValuesPerPoint => 6;

    #endregion

    #region Painting and Selection methods

    /// <summary>
    /// Paint box plot chart.
    /// </summary>
    /// <param name="graph">The Chart Graphics object.</param>
    /// <param name="common">The Common elements object.</param>
    /// <param name="area">Chart area for this chart.</param>
    /// <param name="seriesToDraw">Chart series to draw.</param>
    public virtual void Paint(ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw)
    {
        ProcessChartType(false, graph, common, area, seriesToDraw);
    }

    /// <summary>
    /// This method recalculates size of the bars. This method is used 
    /// from Paint or Select method.
    /// </summary>
    /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
    /// <param name="graph">The Chart Graphics object.</param>
    /// <param name="common">The Common elements object.</param>
    /// <param name="area">Chart area for this chart.</param>
    /// <param name="seriesToDraw">Chart series to draw.</param>
    protected virtual void ProcessChartType(
        bool selection,
        ChartGraphics graph,
        CommonElements common,
        ChartArea area,
        Series seriesToDraw)
    {

        // Process 3D chart type
        if (area.Area3DStyle.Enable3D)
        {
            ProcessChartType3D(selection, graph, common, area, seriesToDraw);
            return;
        }

        // All data series from chart area which have Box Plot chart type
        List<string> typeSeries = area.GetSeriesFromChartType(this.Name);

        // Zero X values mode.
        bool indexedSeries = ChartHelper.IndexedSeries(area.Common, typeSeries);

        //************************************************************
        //** Loop through all series
        //************************************************************
        int seriesIndx = 0;
        foreach (Series ser in common.DataManager.Series)
        {
            // Process non empty series of the area with box plot chart type
            if (!string.Equals(ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase) || ser.ChartArea != area.Name || !ser.IsVisible())
            {
                continue;
            }

            // Set active horizontal/vertical axis
            hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
            vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

            // Get interval between points
            double interval = indexedSeries ? 1 : area.GetPointsInterval(hAxis.IsLogarithmic, hAxis.logarithmBase);

            // Check if side-by-side attribute is set
            bool currentShowSideBySide = showSideBySide;
            string attribValue = ser.TryGetCustomProperty(CustomPropertyName.DrawSideBySide);
            if (attribValue is not null)
            {
                if (string.Equals(attribValue, "False", StringComparison.OrdinalIgnoreCase))
                {
                    currentShowSideBySide = false;
                }
                else if (string.Equals(attribValue, "True", StringComparison.OrdinalIgnoreCase))
                {
                    currentShowSideBySide = true;
                }
                else if (string.Equals(attribValue, "Auto", StringComparison.OrdinalIgnoreCase))
                {
                }
                else
                {
                    throw new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid);
                }
            }

            // Find the number of "Column chart" data series
            double numOfSeries = typeSeries.Count;
            if (!currentShowSideBySide)
                numOfSeries = 1;

            // Calculates the width of the points.
            float width = (float)(ser.GetPointWidth(graph, hAxis, interval, 0.8) / numOfSeries);

            // Call Back Paint event
            if (!selection)
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));


            //************************************************************
            //** Series data points loop
            //************************************************************
            int index = 1;
            foreach (DataPoint point in ser.Points)
            {
                // Check required Y values number
                if (point.YValues.Length < this.YValuesPerPoint)
                {
                    throw new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));
                }

                // Reset pre-calculated point position
                point.positionRel = new PointF(float.NaN, float.NaN);

                // Get point X position
                float xPosition = 0f;
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                    xPosition = (float)(hAxis.GetPosition(index) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                }
                else if (currentShowSideBySide)
                {
                    xPosition = (float)(hAxis.GetPosition(xValue) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                }
                else
                {
                    xPosition = (float)hAxis.GetPosition(xValue);
                }


                double yValue0 = vAxis.GetLogValue(point.YValues[0]);
                double yValue1 = vAxis.GetLogValue(point.YValues[1]);
                xValue = hAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < hAxis.ViewMinimum ||
                    xValue > hAxis.ViewMaximum ||
                    (yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
                    (yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Make sure High/Low values are in data scaleView range						
                double low = vAxis.GetLogValue(point.YValues[0]);
                double high = vAxis.GetLogValue(point.YValues[1]);

                // Check if values are in range
                if (high > vAxis.ViewMaximum)
                {
                    high = vAxis.ViewMaximum;
                }

                if (high < vAxis.ViewMinimum)
                {
                    high = vAxis.ViewMinimum;
                }

                high = (float)vAxis.GetLinearPosition(high);

                if (low > vAxis.ViewMaximum)
                {
                    low = vAxis.ViewMaximum;
                }

                if (low < vAxis.ViewMinimum)
                {
                    low = vAxis.ViewMinimum;
                }

                low = vAxis.GetLinearPosition(low);

                // Remember pre-calculated point position
                point.positionRel = new PointF(xPosition, (float)Math.Min(high, low));

                if (common.ProcessModePaint)
                {

                    // Check if chart is partially in the data scaleView
                    bool clipRegionSet = false;
                    if (xValue == hAxis.ViewMinimum || xValue == hAxis.ViewMaximum)
                    {
                        // Set clipping region for line drawing 
                        graph.SetClip(area.PlotAreaPosition.ToRectangleF());
                        clipRegionSet = true;
                    }

                    // Define line color
                    Color lineColor = point.BorderColor;
                    if (lineColor == Color.Empty)
                    {
                        lineColor = point.Color;
                    }

                    // Start Svg Selection mode
                    graph.StartHotRegion(point);

                    // Draw lower whisker line
                    graph.DrawLineRel(
                        lineColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        new PointF(xPosition, (float)low),
                        new PointF(xPosition, (float)vAxis.GetPosition(point.YValues[2])),
                        ser.ShadowColor,
                        ser.ShadowOffset);

                    // Draw upper whisker line
                    graph.DrawLineRel(
                        lineColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        new PointF(xPosition, (float)high),
                        new PointF(xPosition, (float)vAxis.GetPosition(point.YValues[3])),
                        ser.ShadowColor,
                        ser.ShadowOffset);

                    // Draw Box
                    RectangleF rectSize = RectangleF.Empty;
                    rectSize.X = xPosition - width / 2;
                    rectSize.Width = width;
                    rectSize.Y = (float)vAxis.GetPosition(point.YValues[3]);
                    rectSize.Height = (float)Math.Abs(rectSize.Y - vAxis.GetPosition(point.YValues[2]));
                    graph.FillRectangleRel(rectSize,
                        point.Color,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        point.BackSecondaryColor,
                        point.BorderColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        ser.ShadowColor,
                        ser.ShadowOffset,
                        PenAlignment.Inset);


                    // Check if average line should be drawn
                    bool showAverage = true;
                    string showAverageValue = point.TryGetCustomProperty(CustomPropertyName.BoxPlotShowAverage) ?? ser.TryGetCustomProperty(CustomPropertyName.BoxPlotShowAverage);
                    if (showAverageValue is not null)
                    {
                        if (string.Equals(showAverageValue, "True", StringComparison.OrdinalIgnoreCase))
                        {
                            // default - do nothing
                        }
                        else if (string.Equals(showAverageValue, "False", StringComparison.OrdinalIgnoreCase))
                        {
                            showAverage = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.BoxPlotShowAverage], "BoxPlotShowAverage"));
                        }
                    }


                    // Draw average line
                    SizeF relBorderWidth = graph.GetRelativeSize(new SizeF(point.BorderWidth, point.BorderWidth));
                    if (point.BorderColor == Color.Empty)
                    {
                        relBorderWidth.Height = 0;
                        relBorderWidth.Width = 0;
                    }

                    Color markerLinesColor = lineColor;
                    if (markerLinesColor == point.Color)
                    {
                        double brightness = Math.Sqrt(point.Color.R * point.Color.R + point.Color.G * point.Color.G + point.Color.B * point.Color.B);
                        if (brightness > 220)
                        {
                            markerLinesColor = ChartGraphics.GetGradientColor(point.Color, Color.Black, 0.4);
                        }
                        else
                        {
                            markerLinesColor = ChartGraphics.GetGradientColor(point.Color, Color.White, 0.4);
                        }
                    }

                    if (!double.IsNaN(point.YValues[4]) && showAverage)
                    {
                        graph.DrawLineRel(
                            markerLinesColor,
                            1,
                            ChartDashStyle.Solid,
                            new PointF(rectSize.Left + relBorderWidth.Width, (float)vAxis.GetPosition(point.YValues[4])),
                            new PointF(rectSize.Right - relBorderWidth.Width, (float)vAxis.GetPosition(point.YValues[4])),
                            Color.Empty,
                            0);
                    }

                    // Check if median line should be drawn
                    bool showMedian = true;
                    string showMedianValue = point.TryGetCustomProperty(CustomPropertyName.BoxPlotShowMedian) ?? ser.TryGetCustomProperty(CustomPropertyName.BoxPlotShowMedian);
                    if (showMedianValue is not null)
                    {
                        if (string.Equals(showMedianValue, "True", StringComparison.OrdinalIgnoreCase))
                        {
                            // default - do nothing
                        }
                        else if (string.Equals(showMedianValue, "False", StringComparison.OrdinalIgnoreCase))
                        {
                            showMedian = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.BoxPlotShowMedian], "BoxPlotShowMedian"));
                        }
                    }

                    // Draw median line
                    if (!double.IsNaN(point.YValues[5]) && showMedian)
                    {
                        float medianValue = (float)vAxis.GetPosition(point.YValues[5]);
                        float dashWidth = (rectSize.Width - relBorderWidth.Width * 2) / 9f;

                        // Dash width should not be less than 2 pixels
                        SizeF minSize = graph.GetRelativeSize(new SizeF(2, 2));
                        dashWidth = Math.Max(dashWidth, minSize.Width);

                        for (float curPosition = rectSize.Left + relBorderWidth.Width; curPosition < rectSize.Right - relBorderWidth.Width; curPosition += dashWidth * 2f)
                        {
                            graph.DrawLineRel(
                                markerLinesColor,
                                1,
                                ChartDashStyle.Solid,
                                new PointF(curPosition, medianValue),
                                new PointF(Math.Min(rectSize.Right, curPosition + dashWidth), medianValue),
                                Color.Empty,
                                0);
                        }
                    }

                    // Draw Box Plot marks
                    DrawBoxPlotMarks(graph, area, ser, point, xPosition, width);

                    // End Svg Selection mode
                    graph.EndHotRegion();

                    // Reset Clip Region
                    if (clipRegionSet)
                    {
                        graph.ResetClip();
                    }
                }

                if (common.ProcessModeRegions)
                {
                    // Calculate rect around the box plot marks
                    RectangleF areaRect = RectangleF.Empty;
                    areaRect.X = xPosition - width / 2f;
                    areaRect.Y = (float)Math.Min(high, low);
                    areaRect.Width = width;
                    areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

                    // Add area
                    common.HotRegionsList.AddHotRegion(areaRect, point, ser.Name, index - 1);
                }

                ++index;
            }

            //************************************************************
            //** Second series data points loop, when labels are drawn.
            //************************************************************
            if (!selection)
            {
                index = 1;
                foreach (DataPoint point in ser.Points)
                {
                    // Get point X position
                    float xPosition = 0f;
                    double xValue = point.XValue;
                    if (indexedSeries)
                    {
                        xValue = index;
                        xPosition = (float)(hAxis.GetPosition(index) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                    }
                    else if (currentShowSideBySide)
                    {
                        xPosition = (float)(hAxis.GetPosition(xValue) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                    }
                    else
                    {
                        xPosition = (float)hAxis.GetPosition(xValue);
                    }

                    double yValue0 = vAxis.GetLogValue(point.YValues[0]);
                    double yValue1 = vAxis.GetLogValue(point.YValues[1]);
                    xValue = hAxis.GetLogValue(xValue);

                    // Check if chart is completly out of the data scaleView
                    if (xValue < hAxis.ViewMinimum ||
                        xValue > hAxis.ViewMaximum ||
                        (yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
                        (yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum))
                    {
                        ++index;
                        continue;
                    }

                    // Make sure High/Low values are in data scaleView range	
                    double high = double.MaxValue;
                    for (int valueIndex = 0; valueIndex < point.YValues.Length; valueIndex++)
                    {
                        if (!double.IsNaN(point.YValues[valueIndex]))
                        {
                            double currentValue = vAxis.GetLogValue(point.YValues[valueIndex]);
                            if (currentValue > vAxis.ViewMaximum)
                            {
                                currentValue = vAxis.ViewMaximum;
                            }

                            if (currentValue < vAxis.ViewMinimum)
                            {
                                currentValue = vAxis.ViewMinimum;
                            }

                            currentValue = (float)vAxis.GetLinearPosition(currentValue);

                            high = Math.Min(high, currentValue);
                        }
                    }

                    // Adjust label position by marker size
                    SizeF relMarkerSize = graph.GetRelativeSize(new SizeF(point.MarkerSize, point.MarkerSize));
                    high -= relMarkerSize.Height / 2f;

                    // Start Svg Selection mode
                    graph.StartHotRegion(point, true);

                    // Draw label
                    DrawLabel(common, area, graph, ser, point, new PointF(xPosition, (float)high), index);

                    // End Svg Selection mode
                    graph.EndHotRegion();


                    ++index;
                }
            }

            // Call Paint event
            if (!selection)
            {
                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }

            // Data series index
            if (currentShowSideBySide)
            {
                seriesIndx++;
            }
        }
    }

    /// <summary>
    /// Draws box plot markers.
    /// </summary>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="xPosition">X position.</param>
    /// <param name="width">Point width.</param>
    protected virtual void DrawBoxPlotMarks(
        ChartGraphics graph,
        ChartArea area,
        Series ser,
        DataPoint point,
        float xPosition,
        float width)
    {
        // Get markers style
        string markerStyle = "LINE";
        if (point.MarkerStyle != MarkerStyle.None)
        {
            markerStyle = point.MarkerStyle.ToString();
        }

        // Draw lower marker
        double yPosition = vAxis.GetLogValue(point.YValues[0]);
        DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);

        // Draw upper marker
        yPosition = vAxis.GetLogValue(point.YValues[1]);
        DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);

        // Draw unusual points if any
        markerStyle = "CIRCLE";
        if (point.MarkerStyle != MarkerStyle.None)
        {
            markerStyle = point.MarkerStyle.ToString();
        }

        for (int valueIndex = 6; valueIndex < point.YValues.Length; valueIndex++)
        {
            if (!double.IsNaN(point.YValues[valueIndex]))
            {
                yPosition = vAxis.GetLogValue(point.YValues[valueIndex]);
                DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);
            }
        }
    }

    /// <summary>
    /// Draws single marker on the box plot.
    /// </summary>
    /// <param name="graph">Chart graphics.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="point">Series point.</param>
    /// <param name="markerStyle">Marker style name.</param>
    /// <param name="xPosition">X position.</param>
    /// <param name="yPosition">Y position.</param>
    /// <param name="zPosition">Z position.</param>
    /// <param name="width">Point width.</param>
    /// <param name="draw3D">Used for 3d drawing.</param>
    private void DrawBoxPlotSingleMarker(
        ChartGraphics graph,
        ChartArea area,
        DataPoint point,
        string markerStyle,
        float xPosition,
        float yPosition,
        float zPosition,
        float width,
        bool draw3D)
    {
        if (markerStyle.Length > 0 && !string.Equals(markerStyle, "None", StringComparison.OrdinalIgnoreCase))
        {
            // Make sure Y value is in range
            if (yPosition > vAxis.ViewMaximum || yPosition < vAxis.ViewMinimum)
            {
                return;
            }

            yPosition = (float)vAxis.GetLinearPosition(yPosition);

            // 3D Transform coordinates
            if (draw3D)
            {
                Point3D[] points = [new Point3D(xPosition, yPosition, zPosition)];
                area.matrix3D.TransformPoints(points);
                xPosition = points[0].X;
                yPosition = points[0].Y;
            }

            // Define line color
            Color lineColor = point.BorderColor;
            if (lineColor == Color.Empty)
            {
                lineColor = point.Color;
            }

            // Draw horizontal line marker
            if (string.Equals(markerStyle, "Line", StringComparison.OrdinalIgnoreCase))
            {
                graph.DrawLineRel(
                    lineColor,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    new PointF(xPosition - width / 4f, yPosition),
                    new PointF(xPosition + width / 4f, yPosition),
                    (point.series != null) ? point.series.ShadowColor : Color.Empty,
                    (point.series != null) ? point.series.ShadowOffset : 0);
            }

            // Draw standard marker
            else
            {
                MarkerStyle marker = Enum.Parse<MarkerStyle>(markerStyle, true);
                // Get marker size
                SizeF markerSize = GetMarkerSize(
                    graph,
                    area.Common,
                    area,
                    point,
                    point.MarkerSize,
                    point.MarkerImage);

                // Get marker color
                Color markerColor = (point.MarkerColor == Color.Empty) ? point.BorderColor : point.MarkerColor;
                if (markerColor == Color.Empty)
                {
                    markerColor = point.Color;
                }

                // Draw the marker
                graph.DrawMarkerRel(
                    new PointF(xPosition, yPosition),
                    marker,
                    point.MarkerSize,
                    markerColor,
                    point.MarkerBorderColor,
                    point.MarkerBorderWidth,
                    point.MarkerImage,
                    point.MarkerImageTransparentColor,
                    (point.series != null) ? point.series.ShadowOffset : 0,
                    (point.series != null) ? point.series.ShadowColor : Color.Empty,
                    new RectangleF(xPosition, yPosition, markerSize.Width, markerSize.Height));
            }
        }
    }

    /// <summary>
    /// Returns marker size.
    /// </summary>
    /// <param name="graph">The Chart Graphics object.</param>
    /// <param name="common">The Common elements object.</param>
    /// <param name="area">Chart area for this chart.</param>
    /// <param name="point">Data point.</param>
    /// <param name="markerSize">Marker size.</param>
    /// <param name="markerImage">Marker image.</param>
    /// <returns>Marker width and height.</returns>
    protected virtual SizeF GetMarkerSize(
        ChartGraphics graph,
        CommonElements common,
        ChartArea area,
        DataPoint point,
        int markerSize,
        string markerImage)
    {
        SizeF size = new SizeF(markerSize, markerSize);
        if (graph != null && graph.Graphics != null)
        {
            // Marker size is in pixels and we do the mapping for higher DPIs
            size.Width = markerSize * graph.Graphics.DpiX * Chart.DPIScale / 96;
            size.Height = markerSize * graph.Graphics.DpiY * Chart.DPIScale / 96;
        }

        if (markerImage.Length > 0)
            common.ImageLoader.GetAdjustedImageSize(markerImage, graph.Graphics, ref size);

        return size;
    }


    /// <summary>
    /// Draws box plot chart data point label.
    /// </summary>
    /// <param name="common">The Common elements object</param>
    /// <param name="area">Chart area for this chart</param>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="position">Label position.</param>
    /// <param name="pointIndex">Data point index.</param>
    protected virtual void DrawLabel(
        CommonElements common,
        ChartArea area,
        ChartGraphics graph,
        Series ser,
        DataPoint point,
        PointF position,
        int pointIndex)
    {
        if (ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0)
        {
            // Label text format
            using StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Center;
            if (point.LabelAngle == 0)
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Far;
            }

            // Get label text
            string text;
            if (point.Label.Length == 0)
            {
                text = ValueConverter.FormatValue(
                    ser.Chart,
                    point,
                    point.Tag,
                    point.YValues[0],
                    point.LabelFormat,
                    ser.YValueType,
                    ChartElementType.DataPoint);
            }
            else
            {
                text = point.ReplaceKeywords(point.Label);
            }

            // Adjust label positio to the marker size
            SizeF markerSizes = new SizeF(0f, 0f);
            if (point.MarkerStyle != MarkerStyle.None)
            {
                markerSizes = graph.GetRelativeSize(new SizeF(point.MarkerSize, point.MarkerSize));
                position.Y -= markerSizes.Height / 2f;
            }

            // Get text angle
            int textAngle = point.LabelAngle;

            // Check if text contains white space only
            if (!string.IsNullOrWhiteSpace(text))
            {
                SizeF sizeFont = SizeF.Empty;
                // Check if Smart Labels are enabled
                if (ser.SmartLabelStyle.Enabled)
                {
                    // Get text size
                    using var sf = StringFormat.GenericTypographic;
                    sizeFont = graph.GetRelativeSize(graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), sf));

                    // Adjust label position using SmartLabelStyle algorithm
                    position = area.smartLabels.AdjustSmartLabelPosition(
                        common,
                        graph,
                        area,
                        ser.SmartLabelStyle,
                        position,
                        sizeFont,
                        format,
                        position,
                        markerSizes,
                        LabelAlignmentStyles.Top);

                    // Smart labels always use 0 degrees text angle
                    textAngle = 0;
                }

                // Draw label
                if (!position.IsEmpty)
                {
                    // Get text size
                    if (sizeFont.IsEmpty)
                    {
                        using var sf = StringFormat.GenericTypographic;
                        sizeFont = graph.GetRelativeSize(graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), sf));
                    }

                    // Get label background position
                    RectangleF labelBackPosition = RectangleF.Empty;
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Height += sizeFont.Height / 8;
                    sizeLabel.Width += sizeLabel.Width / text.Length;
                    labelBackPosition = PointChart.GetLabelPosition(
                        graph,
                        position,
                        sizeLabel,
                        format,
                        true);

                    // Draw label text
                    using Brush brush = new SolidBrush(point.LabelForeColor);
                    graph.DrawPointLabelStringRel(
                        common,
                        text,
                        point.Font,
                        brush,
                        position,
                        format,
                        textAngle,
                        labelBackPosition,
                        point.LabelBackColor,
                        point.LabelBorderColor,
                        point.LabelBorderWidth,
                        point.LabelBorderDashStyle,
                        ser,
                        point,
                        pointIndex - 1);
                }
            }
        }
    }

    #endregion

    #region 3D Drawing and Selection methods

    /// <summary>
    /// This method recalculates size of the bars. This method is used 
    /// from Paint or Select method.
    /// </summary>
    /// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
    /// <param name="graph">The Chart Graphics object.</param>
    /// <param name="common">The Common elements object.</param>
    /// <param name="area">Chart area for this chart.</param>
    /// <param name="seriesToDraw">Chart series to draw.</param>
    protected virtual void ProcessChartType3D(
        bool selection,
        ChartGraphics graph,
        CommonElements common,
        ChartArea area,
        Series seriesToDraw)
    {
        // All data series from chart area which have Error Bar chart type
        List<string> typeSeries = area.GetSeriesFromChartType(this.Name);

        // Zero X values mode.
        bool indexedSeries = ChartHelper.IndexedSeries(common, typeSeries);

        //************************************************************
        //** Loop through all series
        //************************************************************
        int seriesIndx = 0;
        foreach (Series ser in common.DataManager.Series)
        {
            // Process non empty series of the area with stock chart type
            if (!string.Equals(ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase) || ser.ChartArea != area.Name || !ser.IsVisible())
            {
                continue;
            }

            // Check that we have at least 6 Y values
            if (ser.YValuesPerPoint < 6)
            {
                throw new ArgumentException(SR.ExceptionChartTypeRequiresYValues(ChartTypeNames.BoxPlot, "6"));
            }

            // Check if side-by-side attribute is set
            bool currentShowSideBySide = showSideBySide;
            string attribValue = ser.TryGetCustomProperty(CustomPropertyName.DrawSideBySide);
            if (attribValue is not null)
            {
                if (string.Equals(attribValue, "False", StringComparison.OrdinalIgnoreCase))
                {
                    currentShowSideBySide = false;
                }
                else if (string.Equals(attribValue, "True", StringComparison.OrdinalIgnoreCase))
                {
                    currentShowSideBySide = true;
                }
                else if (string.Equals(attribValue, "Auto", StringComparison.OrdinalIgnoreCase))
                {
                }
                else
                {
                    throw new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid);
                }
            }

            // Find the number of "Column chart" data series
            double numOfSeries = typeSeries.Count;
            if (!currentShowSideBySide)
            {
                numOfSeries = 1;
            }

            // Set active horizontal/vertical axis
            hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
            vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

            // Get interval between points
            double interval = indexedSeries ? 1 : area.GetPointsInterval(hAxis.IsLogarithmic, hAxis.logarithmBase);

            // Calculates the width of the candles.
            float width = (float)(ser.GetPointWidth(graph, hAxis, interval, 0.8) / numOfSeries);

            // Call Back Paint event
            if (!selection)
            {
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }

            //************************************************************
            //** Get series depth and Z position
            //************************************************************
            area.GetSeriesZPositionAndDepth(ser, out float seriesDepth, out float seriesZPosition);

            //************************************************************
            //** Series data points loop
            //************************************************************
            int index = 1;
            foreach (DataPoint point in ser.Points)
            {
                // Check required Y values number
                if (point.YValues.Length < this.YValuesPerPoint)
                {
                    throw new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));
                }

                // Reset pre-calculated point position
                point.positionRel = new PointF(float.NaN, float.NaN);

                // Get point X position
                float xPosition = 0f;
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                    xPosition = (float)(hAxis.GetPosition(index) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                }
                else if (currentShowSideBySide)
                {
                    xPosition = (float)(hAxis.GetPosition(xValue) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                }
                else
                {
                    xPosition = (float)hAxis.GetPosition(xValue);
                }

                double yValue0 = vAxis.GetLogValue(point.YValues[0]);
                double yValue1 = vAxis.GetLogValue(point.YValues[1]);
                xValue = hAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < hAxis.ViewMinimum ||
                    xValue > hAxis.ViewMaximum ||
                    (yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
                    (yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Make sure High/Low values are in data scaleView range						
                double high = vAxis.GetLogValue(point.YValues[1]);
                double low = vAxis.GetLogValue(point.YValues[0]);

                if (high > vAxis.ViewMaximum)
                {
                    high = vAxis.ViewMaximum;
                }

                if (high < vAxis.ViewMinimum)
                {
                    high = vAxis.ViewMinimum;
                }

                high = (float)vAxis.GetLinearPosition(high);

                if (low > vAxis.ViewMaximum)
                {
                    low = vAxis.ViewMaximum;
                }

                if (low < vAxis.ViewMinimum)
                {
                    low = vAxis.ViewMinimum;
                }

                low = vAxis.GetLinearPosition(low);

                // Remember pre-calculated point position
                point.positionRel = new PointF(xPosition, (float)Math.Min(high, low));

                // 3D Transform coordinates
                Point3D[] points =
                [
                    new Point3D(xPosition, (float)low, seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)high, seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)vAxis.GetPosition(point.YValues[2]), seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)vAxis.GetPosition(point.YValues[3]), seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)vAxis.GetPosition(point.YValues[4]), seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)vAxis.GetPosition(point.YValues[5]), seriesZPosition + seriesDepth / 2f),
                ];
                area.matrix3D.TransformPoints(points);

                if (common.ProcessModePaint)
                {
                    // Check if chart is partially in the data scaleView
                    bool clipRegionSet = false;
                    if (xValue == hAxis.ViewMinimum || xValue == hAxis.ViewMaximum)
                    {
                        // Set clipping region for line drawing 
                        graph.SetClip(area.PlotAreaPosition.ToRectangleF());
                        clipRegionSet = true;
                    }

                    // Define line color
                    Color lineColor = point.BorderColor;
                    if (lineColor == Color.Empty)
                    {
                        lineColor = point.Color;
                    }

                    // Start Svg Selection mode
                    graph.StartHotRegion(point);

                    // Draw lower whisker line
                    graph.DrawLineRel(
                        lineColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        points[0].PointF,
                        points[2].PointF,
                        ser.ShadowColor,
                        ser.ShadowOffset);

                    // Draw upper whisker line
                    graph.DrawLineRel(
                        lineColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        points[1].PointF,
                        points[3].PointF,
                        ser.ShadowColor,
                        ser.ShadowOffset);

                    // Draw Box
                    RectangleF rectSize = RectangleF.Empty;
                    rectSize.X = points[0].X - width / 2;
                    rectSize.Width = width;
                    rectSize.Y = points[3].Y;
                    rectSize.Height = Math.Abs(rectSize.Y - points[2].Y);
                    graph.FillRectangleRel(rectSize,
                        point.Color,
                        point.BackHatchStyle,
                        point.BackImage,
                        point.BackImageWrapMode,
                        point.BackImageTransparentColor,
                        point.BackImageAlignment,
                        point.BackGradientStyle,
                        point.BackSecondaryColor,
                        point.BorderColor,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        ser.ShadowColor,
                        ser.ShadowOffset,
                        PenAlignment.Inset);

                    // Check if average line should be drawn
                    bool showAverage = true;
                    string showAverageValue = point.TryGetCustomProperty(CustomPropertyName.BoxPlotShowAverage) ?? ser.TryGetCustomProperty(CustomPropertyName.BoxPlotShowAverage);
                    if (showAverageValue is not null)
                    {
                        if (string.Equals(showAverageValue, "True", StringComparison.OrdinalIgnoreCase))
                        {
                            // default - do nothing
                        }
                        else if (string.Equals(showAverageValue, "False", StringComparison.OrdinalIgnoreCase))
                        {
                            showAverage = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.BoxPlotShowAverage], "BoxPlotShowAverage"));
                        }
                    }

                    // Draw average line
                    Color markerLinesColor = lineColor;
                    if (markerLinesColor == point.Color)
                    {
                        double brightness = Math.Sqrt(point.Color.R * point.Color.R + point.Color.G * point.Color.G + point.Color.B * point.Color.B);
                        if (brightness > 220)
                        {
                            markerLinesColor = ChartGraphics.GetGradientColor(point.Color, Color.Black, 0.4);
                        }
                        else
                        {
                            markerLinesColor = ChartGraphics.GetGradientColor(point.Color, Color.White, 0.4);
                        }
                    }

                    if (!double.IsNaN(point.YValues[4]) && showAverage)
                    {
                        graph.DrawLineRel(
                            markerLinesColor,
                            1,
                            ChartDashStyle.Solid,
                            new PointF(rectSize.Left, points[4].Y),
                            new PointF(rectSize.Right, points[4].Y),
                            Color.Empty,
                            0);
                    }

                    // Check if median line should be drawn
                    bool showMedian = true;
                    string showMedianValue = point.TryGetCustomProperty(CustomPropertyName.BoxPlotShowMedian) ?? ser.TryGetCustomProperty(CustomPropertyName.BoxPlotShowMedian);
                    if (showMedianValue is not null)
                    {
                        if (string.Equals(showMedianValue, "True", StringComparison.OrdinalIgnoreCase))
                        {
                            // default - do nothing
                        }
                        else if (string.Equals(showMedianValue, "False", StringComparison.OrdinalIgnoreCase))
                        {
                            showMedian = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.BoxPlotShowMedian], "BoxPlotShowMedian"));
                        }
                    }

                    // Draw median line
                    if (!double.IsNaN(point.YValues[5]) && showMedian)
                    {
                        float medianValue = points[5].Y;
                        float dashWidth = rectSize.Width / 9f;

                        // Dash width should not be less than 2 pixels
                        SizeF minSize = graph.GetRelativeSize(new SizeF(2, 2));
                        dashWidth = Math.Max(dashWidth, minSize.Width);

                        for (float curPosition = rectSize.Left; curPosition < rectSize.Right; curPosition += dashWidth * 2f)
                        {
                            graph.DrawLineRel(
                                markerLinesColor,
                                1,
                                ChartDashStyle.Solid,
                                new PointF(curPosition, medianValue),
                                new PointF(Math.Min(rectSize.Right, curPosition + dashWidth), medianValue),
                                Color.Empty,
                                0);
                        }
                    }

                    // Draw Box Plot marks
                    DrawBoxPlotMarks3D(graph, area, ser, point, xPosition, width, seriesZPosition, seriesDepth);
                    xPosition = points[0].X;
                    high = points[0].Y;
                    low = points[1].Y;

                    // End Svg Selection mode
                    graph.EndHotRegion();

                    // Reset Clip Region
                    if (clipRegionSet)
                    {
                        graph.ResetClip();
                    }
                }

                if (common.ProcessModeRegions)
                {
                    xPosition = points[0].X;
                    high = points[0].Y;
                    low = points[1].Y;

                    // Calculate rect around the error bar marks
                    RectangleF areaRect = RectangleF.Empty;
                    areaRect.X = xPosition - width / 2f;
                    areaRect.Y = (float)Math.Min(high, low);
                    areaRect.Width = width;
                    areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

                    // Add area
                    common.HotRegionsList.AddHotRegion(areaRect, point, ser.Name, index - 1);
                }

                ++index;
            }

            //************************************************************
            //** Second series data points loop, when labels are drawn.
            //************************************************************
            if (!selection)
            {
                index = 1;
                foreach (DataPoint point in ser.Points)
                {
                    // Get point X position
                    float xPosition = 0f;
                    double xValue = point.XValue;
                    if (indexedSeries)
                    {
                        xValue = index;
                        xPosition = (float)(hAxis.GetPosition(index) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                    }
                    else if (currentShowSideBySide)
                    {
                        xPosition = (float)(hAxis.GetPosition(xValue) - width * numOfSeries / 2.0 + width / 2 + seriesIndx * width);
                    }
                    else
                    {
                        xPosition = (float)hAxis.GetPosition(xValue);
                    }


                    double yValue0 = vAxis.GetLogValue(point.YValues[0]);
                    double yValue1 = vAxis.GetLogValue(point.YValues[1]);
                    xValue = hAxis.GetLogValue(xValue);

                    // Check if chart is completly out of the data scaleView
                    if (xValue < hAxis.ViewMinimum ||
                        xValue > hAxis.ViewMaximum ||
                        (yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
                        (yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum))
                    {
                        ++index;
                        continue;
                    }

                    // Make sure High/Low values are in data scaleView range						
                    double high = vAxis.GetLogValue(point.YValues[1]);
                    double low = vAxis.GetLogValue(point.YValues[0]);
                    if (high > vAxis.ViewMaximum)
                    {
                        high = vAxis.ViewMaximum;
                    }

                    if (high < vAxis.ViewMinimum)
                    {
                        high = vAxis.ViewMinimum;
                    }

                    high = (float)vAxis.GetLinearPosition(high);

                    if (low > vAxis.ViewMaximum)
                    {
                        low = vAxis.ViewMaximum;
                    }

                    if (low < vAxis.ViewMinimum)
                    {
                        low = vAxis.ViewMinimum;
                    }

                    low = vAxis.GetLinearPosition(low);


                    // 3D Transform coordinates
                    Point3D[] points =
                    [
                        new Point3D(xPosition, (float)high, seriesZPosition + seriesDepth / 2f),
                        new Point3D(xPosition, (float)low, seriesZPosition + seriesDepth / 2f),
                    ];
                    area.matrix3D.TransformPoints(points);
                    xPosition = points[0].X;
                    high = points[0].Y;
                    low = points[1].Y;

                    // Draw label
                    DrawLabel(common, area, graph, ser, point, new PointF(xPosition, (float)Math.Min(high, low)), index);

                    ++index;
                }
            }

            // Call Paint event
            if (!selection)
            {
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }
        }
    }

    /// <summary>
    /// Draws 3D box plot markers.
    /// </summary>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="xPosition">X position.</param>
    /// <param name="width">Point width.</param>
    /// <param name="zPosition">Series Z position.</param>
    /// <param name="depth">Series depth.</param>
    protected virtual void DrawBoxPlotMarks3D(
        ChartGraphics graph,
        ChartArea area,
        Series ser,
        DataPoint point,
        float xPosition,
        float width,
        float zPosition,
        float depth)
    {
        // Get markers style
        string markerStyle = "LINE";
        if (point.MarkerStyle != MarkerStyle.None)
        {
            markerStyle = point.MarkerStyle.ToString();
        }

        // Draw lower marker
        double yPosition = vAxis.GetLogValue(point.YValues[0]);
        DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, zPosition + depth / 2f, width, true);

        // Draw upper marker
        yPosition = vAxis.GetLogValue(point.YValues[1]);
        DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, zPosition + depth / 2f, width, true);

        // Draw unusual points if any
        markerStyle = "CIRCLE";
        if (point.MarkerStyle != MarkerStyle.None)
        {
            markerStyle = point.MarkerStyle.ToString();
        }

        for (int valueIndex = 6; valueIndex < point.YValues.Length; valueIndex++)
        {
            if (!double.IsNaN(point.YValues[valueIndex]))
            {
                yPosition = vAxis.GetLogValue(point.YValues[valueIndex]);
                DrawBoxPlotSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, zPosition + depth / 2f, width, true);
            }
        }
    }

    #endregion

    #region Y values related methods

    /// <summary>
    /// Helper function that returns the Y value of the point.
    /// </summary>
    /// <param name="common">Chart common elements.</param>
    /// <param name="area">Chart area the series belongs to.</param>
    /// <param name="series">Sereis of the point.</param>
    /// <param name="point">Point object.</param>
    /// <param name="pointIndex">Index of the point.</param>
    /// <param name="yValueIndex">Index of the Y value to get.</param>
    /// <returns>Y value of the point.</returns>
    public virtual double GetYValue(
        CommonElements common,
        ChartArea area,
        Series series,
        DataPoint point,
        int pointIndex,
        int yValueIndex)
    {
        return point.YValues[yValueIndex];
    }

    #endregion

    #region Automatic Values Calculation methods

    /// <summary>
    /// Populates box plot chart type using series data specified in "BoxPlotSeries" custom attribute.
    /// </summary>
    /// <param name="boxPlotSeries">Box Plot chart type series.</param>
    internal static void CalculateBoxPlotFromLinkedSeries(Series boxPlotSeries)
    {
        // Check input parameters
        if (!string.Equals(boxPlotSeries.ChartTypeName, ChartTypeNames.BoxPlot, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Check if "BoxPlotSeries" custom attribute is set for the series
        string attr = boxPlotSeries.TryGetCustomProperty(CustomPropertyName.BoxPlotSeries);
        if (attr is not null)
        {
            // Create as many data points as series in attribute
            string[] attrValues = attr.Split(';', StringSplitOptions.TrimEntries);

            // Clear and and new points
            boxPlotSeries.Points.Clear();
            int pointIndex = 0;
            foreach (string val in attrValues)
            {
                boxPlotSeries.Points.AddY(0.0);
                boxPlotSeries.Points[pointIndex++][CustomPropertyName.BoxPlotSeries] = val;
            }
        }

        // Calculate box plot for every data point
        for (int pointIndex = 0; pointIndex < boxPlotSeries.Points.Count; pointIndex++)
        {
            DataPoint point = boxPlotSeries.Points[pointIndex];
            attr = point.TryGetCustomProperty(CustomPropertyName.BoxPlotSeries);
            if (attr is null)
                continue;

#if NET9_0_OR_GREATER
            // Get series and value name
            ReadOnlySpan<char> valueName;
            ReadOnlySpan<char> attrSpan = attr.AsSpan();
            int valueTypeIndex = attrSpan.IndexOf(':');
            if (valueTypeIndex >= 0)
            {
                valueName = attrSpan[(valueTypeIndex + 1)..];
                attrSpan = attrSpan[..valueTypeIndex];
            }
            else
            {
                valueName = ['Y'];
            }

            // Get reference to the chart control
            Chart control = boxPlotSeries.Chart;
            if (control is not null)
            {
                // Get linked series and check existence
                if (control.Series.IndexOf(attrSpan) == -1)
                    throw new InvalidOperationException(SR.ExceptionCustomAttributeSeriesNameNotFound("BoxPlotSeries", attrSpan.ToString()));

                // Calculate box point values
                CalculateBoxPlotValues(ref point, control.Series[attrSpan], valueName);
            }
#else
            // Get series and value name
            string valueName;
            int valueTypeIndex = attr.IndexOf(':');
            if (valueTypeIndex >= 0)
            {
                valueName = attr[(valueTypeIndex + 1)..];
                attr = attr[..valueTypeIndex];
            }
            else
            {
                valueName = "Y";
            }

            // Get reference to the chart control
            Chart control = boxPlotSeries.Chart;
            if (control is not null)
            {
                // Get linked series and check existence
                if (control.Series.IndexOf(attr) == -1)
                    throw new InvalidOperationException(SR.ExceptionCustomAttributeSeriesNameNotFound("BoxPlotSeries", attr));

                // Calculate box point values
                CalculateBoxPlotValues(ref point, control.Series[attr], valueName);
            }
#endif
        }
    }

    /// <summary>
    /// Calculates values for single Box Plot point using specified data series.
    /// </summary>
    /// <param name="boxPoint">Data Point.</param>
    /// <param name="linkedSeries">Linked data series.</param>
    /// <param name="valueName">Name of the point value to link to.</param>
#if NET9_0_OR_GREATER
    private static void CalculateBoxPlotValues(ref DataPoint boxPoint, Series linkedSeries, ReadOnlySpan<char> valueName)
#else
    private static void CalculateBoxPlotValues(ref DataPoint boxPoint, Series linkedSeries, string valueName)
#endif
    {
        // Linked series must be non-empty
        if (linkedSeries.Points.Count == 0)
        {
            return;
        }

        // Calculate an average value for all the data points
        double averageValue = 0.0;
        int valueCount = 0;
        foreach (DataPoint point in linkedSeries.Points)
        {
            if (!point.IsEmpty)
            {
                averageValue += point.GetValueByName(valueName);
                ++valueCount;
            }
        }

        averageValue /= valueCount;

        // Fill array of Y values
        List<double> yValues = new List<double>(valueCount);
        foreach (DataPoint point in linkedSeries.Points)
        {
            if (!point.IsEmpty)
            {
                yValues.Add(point.IsEmpty ? double.NaN : point.GetValueByName(valueName));
            }
        }

        // Get required percentiles
        double[] requiredPercentile = [10.0, 90.0, 25.0, 75.0, 50.0];
        string boxPercentile = (boxPoint.TryGetCustomProperty(CustomPropertyName.BoxPlotPercentile) ?? boxPoint.series?.TryGetCustomProperty(CustomPropertyName.BoxPlotPercentile)) ?? string.Empty;
        string boxWhiskerPercentile = (boxPoint.TryGetCustomProperty(CustomPropertyName.BoxPlotWhiskerPercentile) ?? boxPoint.series?.TryGetCustomProperty(CustomPropertyName.BoxPlotWhiskerPercentile)) ?? string.Empty;

        // Check specified 
        if (boxPercentile.Length > 0)
        {
            bool parseSucceed = double.TryParse(boxPercentile, NumberStyles.Any, CultureInfo.InvariantCulture, out double percentile);
            if (parseSucceed)
            {
                requiredPercentile[2] = percentile;
            }

            if (!parseSucceed || requiredPercentile[2] < 0 || requiredPercentile[2] > 50)
            {
                throw new InvalidOperationException(SR.ExceptionCustomAttributeIsNotInRange0to50("BoxPlotPercentile"));
            }

            requiredPercentile[3] = 100.0 - requiredPercentile[2];
        }

        if (boxWhiskerPercentile.Length > 0)
        {

            bool parseSucceed = double.TryParse(boxWhiskerPercentile, NumberStyles.Any, CultureInfo.InvariantCulture, out double percentile);
            if (parseSucceed)
            {
                requiredPercentile[0] = percentile;
            }


            if (!parseSucceed || requiredPercentile[0] < 0 || requiredPercentile[0] > 50)
            {
                throw new InvalidOperationException(SR.ExceptionCustomAttributeIsNotInRange0to50("BoxPlotPercentile"));
            }

            requiredPercentile[1] = 100.0 - requiredPercentile[0];
        }

        // Calculate 5 required percentile values
        double[] percentileValues = CalculatePercentileValues(yValues, requiredPercentile);

        // Set data points values
        boxPoint.YValues[0] = percentileValues[0];
        boxPoint.YValues[1] = percentileValues[1];
        boxPoint.YValues[2] = percentileValues[2];
        boxPoint.YValues[3] = percentileValues[3];
        boxPoint.YValues[4] = averageValue;
        boxPoint.YValues[5] = percentileValues[4];

        // Check if unusual values should be added
        bool addUnusualValues = false;
        string showUnusualValues = (boxPoint.TryGetCustomProperty(CustomPropertyName.BoxPlotShowUnusualValues) ?? boxPoint.series?.TryGetCustomProperty(CustomPropertyName.BoxPlotShowUnusualValues)) ?? string.Empty;
        if (showUnusualValues.Length > 0)
        {
            if (string.Equals(showUnusualValues, "True", StringComparison.OrdinalIgnoreCase))
            {
                addUnusualValues = true;
            }
            else if (string.Equals(showUnusualValues, "False", StringComparison.OrdinalIgnoreCase))
            {
                addUnusualValues = false;
            }
            else
            {
                throw new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("BoxPlotShowUnusualValues"));
            }
        }

        // Add unusual point 
        if (addUnusualValues)
        {
            BoxPlotAddUnusual(ref boxPoint, yValues);
        }
    }

    /// <summary>
    /// Add unusual data point 
    /// </summary>
    /// <param name="boxPoint">Data Point.</param>
    /// <param name="yValues">Y values array.</param>
    private static void BoxPlotAddUnusual(ref DataPoint boxPoint, List<double> yValues)
    {
        // Get unusual values
        List<double> unusualValuesList = [];
        foreach (double yValue in yValues)
        {
            if (yValue < boxPoint.YValues[0] || yValue > boxPoint.YValues[1])
            {
                unusualValuesList.Add(yValue);
            }
        }

        // Update point's values
        if (unusualValuesList.Count > 0)
        {
            // Create new arry of values for the data pont
            double[] newYValues = new double[6 + unusualValuesList.Count];

            // Copy original data
            for (int index = 0; index < 6; index++)
            {
                newYValues[index] = boxPoint.YValues[index];
            }

            // Add unusual values
            for (int index = 0; index < unusualValuesList.Count; index++)
            {
                newYValues[6 + index] = unusualValuesList[index];
            }

            // Set new values array
            boxPoint.YValues = newYValues;
        }
    }

    /// <summary>
    /// Calculates required percentile values from the data
    /// </summary>
    /// <returns>Array of 5 values</returns>
    /// <param name="yValues">Y values array.</param>
    /// <param name="requiredPercentile">Required percentile</param>
    /// <returns>Array of 5 values</returns>
    private static double[] CalculatePercentileValues(List<double> yValues, double[] requiredPercentile)
    {
        // Create results array
        double[] result = new double[5];

        // Sort Y values array
        yValues.Sort();

        // Calculate required percentile
        int index = 0;
        foreach (double percentile in requiredPercentile)
        {
            // Get percentile point index 
            double percentPointIndex = (yValues.Count - 1.0) / 100.0 * percentile;
            double percentPointIndexInteger = Math.Floor(percentPointIndex);
            double percentPointIndexReminder = percentPointIndex - percentPointIndexInteger;

            result[index] = 0.0;
            if ((int)percentPointIndexInteger < yValues.Count)
            {
                result[index] += (1.0 - percentPointIndexReminder) * yValues[(int)percentPointIndexInteger];
            }

            if ((int)(percentPointIndexInteger + 1) < yValues.Count)
            {
                result[index] += percentPointIndexReminder * yValues[(int)percentPointIndexInteger + 1];
            }

            ++index;
        }


        return result;
    }

#endregion

    #region SmartLabelStyle methods

    /// <summary>
    /// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
    /// </summary>
    /// <param name="common">Common chart elements.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="series">Series values to be used.</param>
    /// <param name="list">List to add to.</param>
    public void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, List<RectangleF> list)
    {
        // No data point markers supported for SmartLabelStyle
    }

    #endregion

    #region IDisposable interface implementation
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            hAxis = null;
            vAxis = null;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

