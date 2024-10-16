﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:    Stock chart requires 4 Y values High, Low, Open and Close.
//  :
//  The Stock chart displays opening and closing values by using 
//  markers, which are typically lines or triangles. “OpenCloseStyle” 
//  custom attribute may be used to control the style of the markers. 
//  The opening values are shown by the markers on the left, and the 
//  closing values are shown by the markers on the right.
//  :
//  A stock chart is typically used to illustrate significant stock 
//  price points including a stock's open, close, high, and low price 
//  points. However, this type of chart can also be used to analyze 
//  scientific data, because each series of data displays a high, low, 
//  open, and close value.
//


using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes;

#region Open/close marks style enumeration

/// <summary>
/// Style of the Open-Close marks in the stock chart
/// </summary>
internal enum StockOpenCloseMarkStyle
{
    /// <summary>
    /// Line
    /// </summary>
    Line,

    /// <summary>
    /// Triangle
    /// </summary>
    Triangle,

    /// <summary>
    /// CandleStick. Color of the bar depends if Open value was bigger than Close value.
    /// </summary>
    Candlestick
}

#endregion

/// <summary>
/// CandleStick class provides chart unique name and changes the marking 
/// style in the StockChart class to StockOpenCloseMarkStyle.CandleStick.
/// </summary>
internal sealed class CandleStickChart : StockChart
{
    #region Constructor

    /// <summary>
    /// CandleStick chart constructor.
    /// </summary>
    public CandleStickChart() : base(StockOpenCloseMarkStyle.Candlestick)
    {
        forceCandleStick = true;
    }

    #endregion

    #region IChartType interface implementation

    /// <summary>
    /// Chart type name
    /// </summary>
    public override string Name => ChartTypeNames.Candlestick;

    #endregion
}

/// <summary>
/// StockChart class provides 2D/3D drawing and hit testing 
/// functionality for the Stock and CandleStick charts.
/// </summary>
internal class StockChart : IChartType
{
    #region Fields

    /// <summary>
    /// Vertical axis
    /// </summary>
    internal Axis VAxis { get; set; }

    /// <summary>
    /// Horizontal axis
    /// </summary>
    internal Axis HAxis { get; set; }

    /// <summary>
    /// Default open-close style
    /// </summary>
    protected StockOpenCloseMarkStyle openCloseStyle = StockOpenCloseMarkStyle.Line;

    /// <summary>
    /// Indicates that only candle-stick type of the open-close marks should be used
    /// </summary>
    protected bool forceCandleStick;

    #endregion

    #region Constructor

    /// <summary>
    /// Stock chart constructor.
    /// </summary>
    public StockChart()
    {
    }

    /// <summary>
    /// Stock chart constructor.
    /// </summary>
    /// <param name="style">Open-close marks default style.</param>
    public StockChart(StockOpenCloseMarkStyle style)
    {
        this.openCloseStyle = style;
    }

    #endregion

    #region IChartType interface implementation

    /// <summary>
    /// Chart type name
    /// </summary>
    public virtual string Name => ChartTypeNames.Stock;

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
    /// True if chart type supports axeses
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
    /// True if chart type supports Logarithmic axes
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
    /// If the crossing value is auto Crossing value should be 
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
    /// Indicates that it's a hundredred percent chart.
    /// Axis scale from 0 to 100 percent should be used.
    /// </summary>
    public virtual bool HundredPercent => false;

    /// <summary>
    /// Indicates that it's a hundredred percent chart.
    /// Axis scale from 0 to 100 percent should be used.
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
        return LegendImageStyle.Line;
    }

    /// <summary>
    /// Number of supported Y value(s) per point 
    /// </summary>
    public virtual int YValuesPerPoint => 4;
    #endregion

    #region Painting and Selection methods

    /// <summary>
    /// Paint stock chart.
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

        // Prosess 3D chart type
        if (area.Area3DStyle.Enable3D)
        {
            ProcessChartType3D(selection, graph, common, area, seriesToDraw);
            return;
        }


        // All data series from chart area which have Stock chart type
        List<string> typeSeries = area.GetSeriesFromChartType(this.Name);

        // Zero X values mode.
        bool indexedSeries = ChartHelper.IndexedSeries(common, typeSeries);

        //************************************************************
        //** Loop through all series
        //************************************************************
        foreach (Series ser in common.DataManager.Series)
        {
            // Process non empty series of the area with stock chart type
            if (!string.Equals(ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase) || ser.ChartArea != area.Name || !ser.IsVisible())
            {
                continue;
            }

            // Check that we have at least 4 Y values
            if (ser.YValuesPerPoint < 4)
            {
                throw new ArgumentException(SR.ExceptionChartTypeRequiresYValues("StockChart", "4"));
            }

            // Set active horizontal/vertical axis
            HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
            VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

            // Get interval between points
            double interval = indexedSeries ? 1 : area.GetPointsInterval(HAxis.IsLogarithmic, HAxis.logarithmBase);

            // Calculates the width of the candles.
            float width = (float)ser.GetPointWidth(graph, HAxis, interval, 0.8);

            // Call Back Paint event
            if (!selection)
            {
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }


            //************************************************************
            //** Series data points loop
            //************************************************************
            int index = 1;
            foreach (DataPoint point in ser.Points)
            {
                // Reset pre-calculated point position
                point.positionRel = new PointF(float.NaN, float.NaN);

                // Get point X position
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                }

                float xPosition = (float)HAxis.GetPosition(xValue);

                double yValue0 = VAxis.GetLogValue(point.YValues[0]);
                double yValue1 = VAxis.GetLogValue(point.YValues[1]);
                xValue = HAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < HAxis.ViewMinimum ||
                    xValue > HAxis.ViewMaximum ||
                    (yValue0 < VAxis.ViewMinimum && yValue1 < VAxis.ViewMinimum) ||
                    (yValue0 > VAxis.ViewMaximum && yValue1 > VAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Make sure High/Low values are in data scaleView range						
                double high = VAxis.GetLogValue(point.YValues[0]);
                double low = VAxis.GetLogValue(point.YValues[1]);

                if (high > VAxis.ViewMaximum)
                {
                    high = VAxis.ViewMaximum;
                }

                if (high < VAxis.ViewMinimum)
                {
                    high = VAxis.ViewMinimum;
                }

                high = (float)VAxis.GetLinearPosition(high);

                if (low > VAxis.ViewMaximum)
                {
                    low = VAxis.ViewMaximum;
                }

                if (low < VAxis.ViewMinimum)
                {
                    low = VAxis.ViewMinimum;
                }

                low = VAxis.GetLinearPosition(low);

                // Remeber pre-calculated point position
                point.positionRel = new PointF(xPosition, (float)high);

                if (common.ProcessModePaint)
                {

                    // Check if chart is partially in the data scaleView
                    bool clipRegionSet = false;
                    if (xValue == HAxis.ViewMinimum || xValue == HAxis.ViewMaximum)
                    {
                        // Set clipping region for line drawing 
                        graph.SetClip(area.PlotAreaPosition.ToRectangleF());
                        clipRegionSet = true;
                    }

                    // Start Svg Selection mode
                    graph.StartHotRegion(point);

                    // Draw Hi-Low line
                    graph.DrawLineRel(
                        point.Color,
                        point.BorderWidth,
                        point.BorderDashStyle,
                        new PointF(xPosition, (float)high),
                        new PointF(xPosition, (float)low),
                        ser.ShadowColor,
                        ser.ShadowOffset);

                    // Draw Open-Close marks
                    DrawOpenCloseMarks(graph, area, ser, point, xPosition, width);

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
                    // Calculate rect around the hi-lo line and open-close marks
                    RectangleF areaRect = RectangleF.Empty;
                    areaRect.X = xPosition - width / 2f;
                    areaRect.Y = (float)Math.Min(high, low);
                    areaRect.Width = width;
                    areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

                    common.HotRegionsList.AddHotRegion(
                        areaRect,
                        point,
                        ser.Name,
                        index - 1);

                }

                ++index;
            }

            //************************************************************
            //** Second series data points loop, when markers and labels
            //** are drawn.
            //************************************************************

            int markerIndex = 0;
            index = 1;
            foreach (DataPoint point in ser.Points)
            {
                // Get point X position
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                }

                float xPosition = (float)HAxis.GetPosition(xValue);

                double yValue0 = VAxis.GetLogValue(point.YValues[0]);
                double yValue1 = VAxis.GetLogValue(point.YValues[1]);
                xValue = HAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < HAxis.ViewMinimum ||
                    xValue > HAxis.ViewMaximum ||
                    (yValue0 < VAxis.ViewMinimum && yValue1 < VAxis.ViewMinimum) ||
                    (yValue0 > VAxis.ViewMaximum && yValue1 > VAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Make sure High/Low values are in data scaleView range						
                double high = VAxis.GetLogValue(point.YValues[0]);
                double low = VAxis.GetLogValue(point.YValues[1]);

                if (high > VAxis.ViewMaximum)
                {
                    high = VAxis.ViewMaximum;
                }

                if (high < VAxis.ViewMinimum)
                {
                    high = VAxis.ViewMinimum;
                }

                high = (float)VAxis.GetLinearPosition(high);

                if (low > VAxis.ViewMaximum)
                {
                    low = VAxis.ViewMaximum;
                }

                if (low < VAxis.ViewMinimum)
                {
                    low = VAxis.ViewMinimum;
                }

                low = VAxis.GetLinearPosition(low);

                // Draw marker
                if (point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
                {
                    // Get marker size
                    SizeF markerSize = SizeF.Empty;
                    markerSize.Width = point.MarkerSize;
                    markerSize.Height = point.MarkerSize;
                    if (graph != null && graph.Graphics != null)
                    {
                        // Marker size is in pixels and we do the mapping for higher DPIs
                        markerSize.Width = point.MarkerSize * graph.Graphics.DpiX * Chart.DPIScale / 96;
                        markerSize.Height = point.MarkerSize * graph.Graphics.DpiY * Chart.DPIScale / 96;
                    }

                    if (point.MarkerImage.Length > 0)
                        common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);

                    // Get marker position
                    PointF markerPosition = PointF.Empty;
                    markerPosition.X = xPosition;
                    markerPosition.Y = (float)high - graph.GetRelativeSize(markerSize).Height / 2f;

                    // Draw marker
                    if (markerIndex == 0)
                    {
                        // Draw the marker
                        graph.DrawMarkerRel(markerPosition,
                            point.MarkerStyle,
                            (int)markerSize.Height,
                            (point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor,
                            (point.MarkerBorderColor == Color.Empty) ? point.BorderColor : point.MarkerBorderColor,
                            point.MarkerBorderWidth,
                            point.MarkerImage,
                            point.MarkerImageTransparentColor,
                            (point.series != null) ? point.series.ShadowOffset : 0,
                            (point.series != null) ? point.series.ShadowColor : Color.Empty,
                            new RectangleF(markerPosition.X, markerPosition.Y, markerSize.Width, markerSize.Height));

                        if (common.ProcessModeRegions)
                        {
                            // Get relative marker size
                            SizeF relativeMarkerSize = graph.GetRelativeSize(markerSize);

                            // Insert area just after the last custom area
                            //int insertIndex = common.HotRegionsList.FindInsertIndex();
                            //common.HotRegionsList.FindInsertIndex();

                            // Insert circle area
                            if (point.MarkerStyle == MarkerStyle.Circle)
                            {
                                float[] circCoord = [markerPosition.X, markerPosition.Y, relativeMarkerSize.Width / 2f];
                                common.HotRegionsList.AddHotRegion(
                                    graph,
                                    circCoord[0],
                                    circCoord[1],
                                    circCoord[2],
                                    point,
                                    ser.Name,
                                    index - 1);
                            }
                            else // All other markers represented as rectangles
                            {
                                common.HotRegionsList.AddHotRegion(
                                    new RectangleF(markerPosition.X - relativeMarkerSize.Width / 2f, markerPosition.Y - relativeMarkerSize.Height / 2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
                                    point,
                                    ser.Name,
                                    index - 1);
                            }
                        }
                    }

                    // Increase the markers counter
                    ++markerIndex;
                    if (ser.MarkerStep == markerIndex)
                    {
                        markerIndex = 0;
                    }
                }

                // Draw label
                DrawLabel(common, area, graph, ser, point, new PointF(xPosition, (float)Math.Min(high, low)), index);

                // Increase point counter
                ++index;
            }

            // Call Paint event
            if (!selection)
            {
                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }
        }
    }

    /// <summary>
    /// Draws stock chart open-close marks depending on selected style.
    /// </summary>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="xPosition">X position.</param>
    /// <param name="width">Point width.</param>
    protected virtual void DrawOpenCloseMarks(
        ChartGraphics graph,
        ChartArea area,
        Series ser,
        DataPoint point,
        float xPosition,
        float width)
    {
        double openY = VAxis.GetLogValue(point.YValues[2]);
        double closeY = VAxis.GetLogValue(point.YValues[3]);

        //// Check if mark is inside data scaleView
        //if ((openY > VAxis.ViewMaximum ||
        //    openY < VAxis.ViewMinimum) &&
        //    (closeY > VAxis.ViewMaximum ||
        //    closeY < VAxis.ViewMinimum))
        //{
        //    //return;
        //}

        // Calculate open-close position
        float open = (float)VAxis.GetLinearPosition(openY);
        float close = (float)VAxis.GetLinearPosition(closeY);
        SizeF absSize = graph.GetAbsoluteSize(new SizeF(width, width));
        float height = graph.GetRelativeSize(absSize).Height;

        // Detect style and get attribute which controls if open/close marks are shown
        StockOpenCloseMarkStyle style;
        bool showOpen;
        bool showClose;
        (style, showOpen, showClose) = GetOpenCloseMarksParams(point, ser);

        // Check if chart is partially in the data scaleView
        bool clipRegionSet = false;
        if (style == StockOpenCloseMarkStyle.Candlestick || xPosition - width / 2f < area.PlotAreaPosition.X || xPosition + width / 2f > area.PlotAreaPosition.Right)
        {
            // Set clipping region for line drawing 
            graph.SetClip(area.PlotAreaPosition.ToRectangleF());
            clipRegionSet = true;
        }


        // Draw open-close marks as bar
        if (forceCandleStick || style == StockOpenCloseMarkStyle.Candlestick)
        {
            // Colors used to draw bar of the open-close style
            ColorConverter colorConverter = new ColorConverter();
            Color priceUpColor = point.Color;
            Color priceDownColor = point.BackSecondaryColor;

            // Check if special color properties are set
            string attrValue = point[CustomPropertyName.PriceUpColor];
            if (attrValue != null && attrValue.Length > 0)
            {
                bool failed = false;
                try
                {
                    priceUpColor = (Color)colorConverter.ConvertFromString(attrValue);
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }

                if (failed)
                {
                    priceUpColor = (Color)colorConverter.ConvertFromInvariantString(attrValue);
                }
            }

            attrValue = point[CustomPropertyName.PriceDownColor];
            if (attrValue != null && attrValue.Length > 0)
            {
                bool failed = false;
                try
                {
                    priceDownColor = (Color)colorConverter.ConvertFromString(attrValue);
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }

                if (failed)
                {
                    priceDownColor = (Color)colorConverter.ConvertFromInvariantString(attrValue);
                }
            }

            // Calculate bar rectangle
            RectangleF rect = RectangleF.Empty;
            rect.Y = Math.Min(open, close);
            rect.X = xPosition - width / 2f;
            rect.Height = Math.Max(open, close) - rect.Y;
            rect.Width = width;

            // Bar and border color
            Color barColor = (open > close) ? priceUpColor : priceDownColor;
            Color barBorderColor = (point.BorderColor == Color.Empty) ? (barColor == Color.Empty) ? point.Color : barColor : point.BorderColor;

            // Get absolute height
            SizeF sizeOfHeight = new SizeF(rect.Height, rect.Height);
            sizeOfHeight = graph.GetAbsoluteSize(sizeOfHeight);

            // Draw open-close bar
            if (sizeOfHeight.Height > 1)
            {
                graph.FillRectangleRel(
                    rect,
                    barColor,
                    point.BackHatchStyle,
                    point.BackImage,
                    point.BackImageWrapMode,
                    point.BackImageTransparentColor,
                    point.BackImageAlignment,
                    point.BackGradientStyle,
                    point.BackSecondaryColor,
                    barBorderColor,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    ser.ShadowColor,
                    ser.ShadowOffset,
                    PenAlignment.Inset);
            }
            else
            {
                graph.DrawLineRel(barBorderColor, point.BorderWidth, point.BorderDashStyle,
                    new PointF(rect.X, rect.Y),
                    new PointF(rect.Right, rect.Y),
                    ser.ShadowColor, ser.ShadowOffset);
            }
        }

        // Draw open-close marks as triangals
        else if (style == StockOpenCloseMarkStyle.Triangle)
        {
            using GraphicsPath path = new GraphicsPath();
            PointF point1 = graph.GetAbsolutePoint(new PointF(xPosition, open));
            PointF point2 = graph.GetAbsolutePoint(new PointF(xPosition - width / 2f, open + height / 2f));
            PointF point3 = graph.GetAbsolutePoint(new PointF(xPosition - width / 2f, open - height / 2f));

            using Brush brush = new SolidBrush(point.Color);
            // Draw Open mark line
            if (showOpen)
            {
                if (openY <= VAxis.ViewMaximum && openY >= VAxis.ViewMinimum)
                {
                    path.AddLine(point2, point1);
                    path.AddLine(point1, point3);
                    path.AddLine(point3, point3);
                    graph.FillPath(brush, path);
                }
            }

            // Draw close mark line
            if (showClose)
            {
                if (closeY <= VAxis.ViewMaximum && closeY >= VAxis.ViewMinimum)
                {
                    path.Reset();
                    point1 = graph.GetAbsolutePoint(new PointF(xPosition, close));
                    point2 = graph.GetAbsolutePoint(new PointF(xPosition + width / 2f, close + height / 2f));
                    point3 = graph.GetAbsolutePoint(new PointF(xPosition + width / 2f, close - height / 2f));
                    path.AddLine(point2, point1);
                    path.AddLine(point1, point3);
                    path.AddLine(point3, point3);
                    graph.FillPath(brush, path);
                }
            }
        }

        // Draw ope-close marks as lines
        else
        {
            // Draw Open mark line
            if (showOpen)
            {
                if (openY <= VAxis.ViewMaximum && openY >= VAxis.ViewMinimum)
                {
                    graph.DrawLineRel(point.Color, point.BorderWidth, point.BorderDashStyle,
                        new PointF(xPosition - width / 2f, open),
                        new PointF(xPosition, open),
                        ser.ShadowColor, ser.ShadowOffset);
                }
            }

            // Draw Close mark line
            if (showClose)
            {
                if (closeY <= VAxis.ViewMaximum && closeY >= VAxis.ViewMinimum)
                {
                    graph.DrawLineRel(point.Color, point.BorderWidth, point.BorderDashStyle,
                        new PointF(xPosition, close),
                        new PointF(xPosition + width / 2f, close),
                        ser.ShadowColor, ser.ShadowOffset);
                }
            }
        }

        // Reset Clip Region
        if (clipRegionSet)
        {
            graph.ResetClip();
        }
    }

    /// <summary>
    /// Draws stock chart data point label.
    /// </summary>
    /// <param name="common">The Common elements object</param>
    /// <param name="area">Chart area for this chart</param>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="position">Label position.</param>
    /// <param name="pointIndex">Data point index in the series.</param>
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
                // Check what value to show (High, Low, Open, Close)
                int valueIndex = 3;
                string valueType = point.TryGetCustomProperty(CustomPropertyName.LabelValueType) ?? ser.TryGetCustomProperty(CustomPropertyName.LabelValueType);
                if (string.Equals(valueType, "High", StringComparison.OrdinalIgnoreCase))
                {
                    valueIndex = 0;
                }
                else if (string.Equals(valueType, "Low", StringComparison.OrdinalIgnoreCase))
                {
                    valueIndex = 1;
                }
                else if (string.Equals(valueType, "Open", StringComparison.OrdinalIgnoreCase))
                {
                    valueIndex = 2;
                }

                text = ValueConverter.FormatValue(
                    ser.Chart,
                    point,
                    point.Tag,
                    point.YValues[valueIndex],
                    point.LabelFormat,
                    ser.YValueType,
                    ChartElementType.DataPoint);
            }
            else
            {
                text = point.ReplaceKeywords(point.Label);
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
                    // Get marker size
                    SizeF markerSize = SizeF.Empty;
                    markerSize.Width = point.MarkerSize;
                    markerSize.Height = point.MarkerSize;
                    if (graph != null && graph.Graphics != null)
                    {
                        // Marker size is in pixels and we do the mapping for higher DPIs
                        markerSize.Width = point.MarkerSize * graph.Graphics.DpiX * Chart.DPIScale / 96;
                        markerSize.Height = point.MarkerSize * graph.Graphics.DpiY * Chart.DPIScale / 96;
                    }

                    if (point.MarkerImage.Length > 0)
                        common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);

                    // Get point label style attribute
                    markerSize = graph.GetRelativeSize(markerSize);
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
                        markerSize,
                        LabelAlignmentStyles.Top);

                    // Smart labels always use 0 degrees text angle
                    textAngle = 0;

                }

                // Draw label
                if (!position.IsEmpty)
                {
                    RectangleF labelBackPosition = RectangleF.Empty;
                    if (!point.LabelBackColor.IsEmpty ||
                        point.LabelBorderWidth > 0 ||
                        !point.LabelBorderColor.IsEmpty)
                    {
                        // Get text size
                        if (sizeFont.IsEmpty)
                        {
                            using var sf = StringFormat.GenericTypographic;
                            sizeFont = graph.GetRelativeSize(graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), sf));
                        }

                        // Adjust label y coordinate
                        position.Y -= sizeFont.Height / 8;

                        // Get label background position
                        SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                        sizeLabel.Height += sizeFont.Height / 8;
                        sizeLabel.Width += sizeLabel.Width / text.Length;
                        labelBackPosition = PointChart.GetLabelPosition(
                            graph,
                            position,
                            sizeLabel,
                            format,
                            true);
                    }

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

    private (StockOpenCloseMarkStyle Style, bool ShowOpen, bool ShowClose) GetOpenCloseMarksParams(DataPoint point, Series ser)
    {
        // Detect style
        StockOpenCloseMarkStyle style;
        string attr = point.TryGetCustomProperty(CustomPropertyName.OpenCloseStyle) ?? ser.TryGetCustomProperty(CustomPropertyName.OpenCloseStyle);
        if (!string.IsNullOrEmpty(attr))
        {
            if (string.Equals(attr, "Candlestick", StringComparison.OrdinalIgnoreCase))
            {
                style = StockOpenCloseMarkStyle.Candlestick;
            }
            else if (string.Equals(attr, "Triangle", StringComparison.OrdinalIgnoreCase))
            {
                style = StockOpenCloseMarkStyle.Triangle;
            }
            else if (string.Equals(attr, "Line", StringComparison.OrdinalIgnoreCase))
            {
                style = StockOpenCloseMarkStyle.Line;
            }
            else
            {
                style = openCloseStyle;
            }
        }
        else
        {
            style = openCloseStyle;
        }

        // Get attribute which controls if open/close marks are shown
        bool showOpen = true;
        bool showClose = true;
        attr = point.TryGetCustomProperty(CustomPropertyName.ShowOpenClose) ?? ser.TryGetCustomProperty(CustomPropertyName.ShowOpenClose);
        if (!string.IsNullOrEmpty(attr))
        {
            if (string.Equals(attr, "Both", StringComparison.OrdinalIgnoreCase))
            {
                showOpen = true;
                showClose = true;
            }
            else if (string.Equals(attr, "Open", StringComparison.OrdinalIgnoreCase))
            {
                showOpen = true;
                showClose = false;
            }
            else if (string.Equals(attr, "Close", StringComparison.OrdinalIgnoreCase))
            {
                showOpen = false;
                showClose = true;
            }
        }

        return (style, showOpen, showClose);
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

        // All data series from chart area which have Stock chart type
        List<string> typeSeries = area.GetSeriesFromChartType(this.Name);

        // Zero X values mode.
        bool indexedSeries = ChartHelper.IndexedSeries(common, typeSeries);

        //************************************************************
        //** Loop through all series
        //************************************************************
        foreach (Series ser in common.DataManager.Series)
        {
            // Process non empty series of the area with stock chart type
            if (!string.Equals(ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase) || ser.ChartArea != area.Name || !ser.IsVisible())
            {
                continue;
            }

            // Check if drawn series is specified
            if (seriesToDraw != null && seriesToDraw.Name != ser.Name)
            {
                continue;
            }

            // Check that we have at least 4 Y values
            if (ser.YValuesPerPoint < 4)
            {
                throw new ArgumentException(SR.ExceptionChartTypeRequiresYValues("StockChart", "4"));
            }

            // Set active horizontal/vertical axis
            HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
            VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

            // Get interval between points
            double interval = indexedSeries ? 1 : area.GetPointsInterval(HAxis.IsLogarithmic, HAxis.logarithmBase);

            // Calculates the width of the candles.
            float width = (float)ser.GetPointWidth(graph, HAxis, interval, 0.8);

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
                // Reset pre-calculated point position
                point.positionRel = new PointF(float.NaN, float.NaN);

                // Get point X position
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                }

                float xPosition = (float)HAxis.GetPosition(xValue);

                double yValue0 = VAxis.GetLogValue(point.YValues[0]);
                double yValue1 = VAxis.GetLogValue(point.YValues[1]);
                xValue = HAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < HAxis.ViewMinimum ||
                    xValue > HAxis.ViewMaximum ||
                    (yValue0 < VAxis.ViewMinimum && yValue1 < VAxis.ViewMinimum) ||
                    (yValue0 > VAxis.ViewMaximum && yValue1 > VAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Check if chart is partially in the data scaleView
                bool clipRegionSet = false;
                if (xValue == HAxis.ViewMinimum || xValue == HAxis.ViewMaximum)
                {
                    // Set clipping region for line drawing 
                    graph.SetClip(area.PlotAreaPosition.ToRectangleF());
                    clipRegionSet = true;
                }

                // Make sure High/Low values are in data scaleView range						
                double high = VAxis.GetLogValue(point.YValues[0]);
                double low = VAxis.GetLogValue(point.YValues[1]);

                if (high > VAxis.ViewMaximum)
                {
                    high = VAxis.ViewMaximum;
                }

                if (high < VAxis.ViewMinimum)
                {
                    high = VAxis.ViewMinimum;
                }

                high = (float)VAxis.GetLinearPosition(high);

                if (low > VAxis.ViewMaximum)
                {
                    low = VAxis.ViewMaximum;
                }

                if (low < VAxis.ViewMinimum)
                {
                    low = VAxis.ViewMinimum;
                }

                low = VAxis.GetLinearPosition(low);

                // Remeber pre-calculated point position
                point.positionRel = new PointF(xPosition, (float)high);

                // 3D Transform coordinates
                Point3D[] points =
                [
                    new Point3D(xPosition, (float)high, seriesZPosition + seriesDepth / 2f),
                    new Point3D(xPosition, (float)low, seriesZPosition + seriesDepth / 2f),
                ];
                area.matrix3D.TransformPoints(points);

                // Start Svg Selection mode
                graph.StartHotRegion(point);

                // Draw Hi-Low line
                graph.DrawLineRel(
                    point.Color,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    points[0].PointF,
                    points[1].PointF,
                    ser.ShadowColor,
                    ser.ShadowOffset);

                // Draw Open-Close marks
                DrawOpenCloseMarks3D(graph, area, ser, point, xPosition, width, seriesZPosition, seriesDepth);
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

                if (common.ProcessModeRegions)
                {
                    // Calculate rect around the hi-lo line and open-close marks
                    RectangleF areaRect = RectangleF.Empty;
                    areaRect.X = xPosition - width / 2f;
                    areaRect.Y = (float)Math.Min(high, low);
                    areaRect.Width = width;
                    areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

                    common.HotRegionsList.AddHotRegion(
                        areaRect,
                        point,
                        ser.Name,
                        index - 1);

                }

                ++index;
            }

            //************************************************************
            //** Second series data points loop, when markers and labels
            //** are drawn.
            //************************************************************
            int markerIndex = 0;
            index = 1;
            foreach (DataPoint point in ser.Points)
            {
                // Get point X position
                double xValue = point.XValue;
                if (indexedSeries)
                {
                    xValue = index;
                }

                float xPosition = (float)HAxis.GetPosition(xValue);

                double yValue0 = VAxis.GetLogValue(point.YValues[0]);
                double yValue1 = VAxis.GetLogValue(point.YValues[1]);
                xValue = HAxis.GetLogValue(xValue);

                // Check if chart is completly out of the data scaleView
                if (xValue < HAxis.ViewMinimum ||
                    xValue > HAxis.ViewMaximum ||
                    (yValue0 < VAxis.ViewMinimum && yValue1 < VAxis.ViewMinimum) ||
                    (yValue0 > VAxis.ViewMaximum && yValue1 > VAxis.ViewMaximum))
                {
                    ++index;
                    continue;
                }

                // Make sure High/Low values are in data scaleView range						
                double high = VAxis.GetLogValue(point.YValues[0]);
                double low = VAxis.GetLogValue(point.YValues[1]);

                if (high > VAxis.ViewMaximum)
                {
                    high = VAxis.ViewMaximum;
                }

                if (high < VAxis.ViewMinimum)
                {
                    high = VAxis.ViewMinimum;
                }

                high = (float)VAxis.GetLinearPosition(high);

                if (low > VAxis.ViewMaximum)
                {
                    low = VAxis.ViewMaximum;
                }

                if (low < VAxis.ViewMinimum)
                {
                    low = VAxis.ViewMinimum;
                }

                low = VAxis.GetLinearPosition(low);


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

                // Draw marker
                if (point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
                {
                    // Get marker size
                    SizeF markerSize = SizeF.Empty;
                    markerSize.Width = point.MarkerSize;
                    markerSize.Height = point.MarkerSize;
                    if (graph != null && graph.Graphics != null)
                    {
                        // Marker size is in pixels and we do the mapping for higher DPIs
                        markerSize.Width = point.MarkerSize * graph.Graphics.DpiX * Chart.DPIScale / 96;
                        markerSize.Height = point.MarkerSize * graph.Graphics.DpiY * Chart.DPIScale / 96;
                    }

                    if (point.MarkerImage.Length > 0)
                        common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);

                    // Get marker position
                    PointF markerPosition = PointF.Empty;
                    markerPosition.X = xPosition;
                    markerPosition.Y = (float)high - graph.GetRelativeSize(markerSize).Height / 2f;

                    // Draw marker
                    if (markerIndex == 0)
                    {
                        // Draw the marker
                        graph.DrawMarkerRel(markerPosition,
                            point.MarkerStyle,
                            (int)markerSize.Height,
                            (point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor,
                            (point.MarkerBorderColor == Color.Empty) ? point.BorderColor : point.MarkerBorderColor,
                            point.MarkerBorderWidth,
                            point.MarkerImage,
                            point.MarkerImageTransparentColor,
                            (point.series != null) ? point.series.ShadowOffset : 0,
                            (point.series != null) ? point.series.ShadowColor : Color.Empty,
                            new RectangleF(markerPosition.X, markerPosition.Y, markerSize.Width, markerSize.Height));

                        if (common.ProcessModeRegions)
                        {
                            // Get relative marker size
                            SizeF relativeMarkerSize = graph.GetRelativeSize(markerSize);

                            // Insert area just after the last custom area
                            //int insertIndex = common.HotRegionsList.FindInsertIndex();
                            //common.HotRegionsList.FindInsertIndex();

                            // Insert circle area
                            if (point.MarkerStyle == MarkerStyle.Circle)
                            {
                                float[] circCoord = [markerPosition.X, markerPosition.Y, relativeMarkerSize.Width / 2f];
                                common.HotRegionsList.AddHotRegion(
                                    graph,
                                    circCoord[0],
                                    circCoord[1],
                                    circCoord[2],
                                    point,
                                    ser.Name,
                                    index - 1);
                            }
                            // All other markers represented as rectangles
                            else
                            {
                                common.HotRegionsList.AddHotRegion(
                                    new RectangleF(markerPosition.X - relativeMarkerSize.Width / 2f, markerPosition.Y - relativeMarkerSize.Height / 2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
                                    point,
                                    ser.Name,
                                    index - 1);
                            }
                        }
                    }

                    // Increase the markers counter
                    ++markerIndex;
                    if (ser.MarkerStep == markerIndex)
                    {
                        markerIndex = 0;
                    }
                }

                ++index;
            }

            // Call Paint event
            if (!selection)
            {
                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
            }
        }
    }

    /// <summary>
    /// Draws stock chart open-close marks depending on selected style.
    /// </summary>
    /// <param name="graph">Chart graphics object.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="ser">Data point series.</param>
    /// <param name="point">Data point to draw.</param>
    /// <param name="xPosition">X position.</param>
    /// <param name="width">Point width.</param>
    /// <param name="zPosition">Series Z position.</param>
    /// <param name="depth">Series depth.</param>
    protected virtual void DrawOpenCloseMarks3D(
        ChartGraphics graph,
        ChartArea area,
        Series ser,
        DataPoint point,
        float xPosition,
        float width,
        float zPosition,
        float depth)
    {
        double openY = VAxis.GetLogValue(point.YValues[2]);
        double closeY = VAxis.GetLogValue(point.YValues[3]);

        //// Check if mark is inside data scaleView
        //if ((openY > VAxis.ViewMaximum ||
        //    openY < VAxis.ViewMinimum) &&
        //    (closeY > VAxis.ViewMaximum ||
        //    closeY < VAxis.ViewMinimum))
        //{
        //    //return;
        //}

        // Calculate open-close position
        float open = (float)VAxis.GetLinearPosition(openY);
        float close = (float)VAxis.GetLinearPosition(closeY);
        SizeF absSize = graph.GetAbsoluteSize(new SizeF(width, width));
        float height = graph.GetRelativeSize(absSize).Height;

        // Detect style and get attribute which controls if open/close marks are shown
        StockOpenCloseMarkStyle style;
        bool showOpen;
        bool showClose;
        (style, showOpen, showClose) = GetOpenCloseMarksParams(point, ser);

        // Check if chart is partially in the data scaleView
        bool clipRegionSet = false;
        if (xPosition - width / 2f < area.PlotAreaPosition.X || xPosition + width / 2f > area.PlotAreaPosition.Right)
        {
            // Set clipping region for line drawing 
            graph.SetClip(area.PlotAreaPosition.ToRectangleF());
            clipRegionSet = true;
        }


        // Draw open-close marks as bar
        if (forceCandleStick || style == StockOpenCloseMarkStyle.Candlestick)
        {
            // Colors used to draw bar of the open-close style
            ColorConverter colorConverter = new ColorConverter();
            Color priceUpColor = point.Color;
            Color priceDownColor = point.BackSecondaryColor;

            // Check if special color properties are set
            string attrValue = point[CustomPropertyName.PriceUpColor];
            if (attrValue != null && attrValue.Length > 0)
            {
                bool failed = false;
                try
                {
                    priceUpColor = (Color)colorConverter.ConvertFromString(attrValue);
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }
                catch (ArgumentException)
                {
                    failed = true;
                }

                if (failed)
                {
                    priceUpColor = (Color)colorConverter.ConvertFromInvariantString(attrValue);
                }
            }

            attrValue = point[CustomPropertyName.PriceDownColor];
            if (attrValue != null && attrValue.Length > 0)
            {
                bool failed = false;
                try
                {
                    priceDownColor = (Color)colorConverter.ConvertFromString(attrValue);
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }

                if (failed)
                {
                    priceDownColor = (Color)colorConverter.ConvertFromInvariantString(attrValue);
                }
            }

            // Calculate bar rectangle
            RectangleF rect = RectangleF.Empty;
            rect.Y = Math.Min(open, close);
            rect.X = xPosition - width / 2f;
            rect.Height = Math.Max(open, close) - rect.Y;
            rect.Width = width;

            // Bar and border color
            Color barColor = (open > close) ? priceUpColor : priceDownColor;
            Color barBorderColor = (point.BorderColor == Color.Empty) ? (barColor == Color.Empty) ? point.Color : barColor : point.BorderColor;

            // Translate coordinates
            Point3D[] points =
            [
                new Point3D(rect.X, rect.Y, zPosition + depth / 2f),
                new Point3D(rect.Right, rect.Bottom, zPosition + depth / 2f),
            ];
            area.matrix3D.TransformPoints(points);
            rect.Location = points[0].PointF;
            rect.Width = Math.Abs(points[1].X - points[0].X);
            rect.Height = Math.Abs(points[1].Y - points[0].Y);

            // Draw open-close bar
            if (rect.Height > 1)
            {
                graph.FillRectangleRel(
                    rect,
                    barColor,
                    point.BackHatchStyle,
                    point.BackImage,
                    point.BackImageWrapMode,
                    point.BackImageTransparentColor,
                    point.BackImageAlignment,
                    point.BackGradientStyle,
                    point.BackSecondaryColor,
                    barBorderColor,
                    point.BorderWidth,
                    point.BorderDashStyle,
                    ser.ShadowColor,
                    ser.ShadowOffset,
                    PenAlignment.Inset);
            }
            else
            {
                graph.DrawLineRel(barBorderColor, point.BorderWidth, point.BorderDashStyle,
                    new PointF(rect.X, rect.Y),
                    new PointF(rect.Right, rect.Y),
                    ser.ShadowColor, ser.ShadowOffset);
            }
        }

        // Draw open-close marks as triangals
        else if (style == StockOpenCloseMarkStyle.Triangle)
        {
            using GraphicsPath path = new GraphicsPath();

            // Translate coordinates
            Point3D[] points =
            [
                new Point3D(xPosition, open, zPosition + depth / 2f),
                new Point3D(xPosition - width / 2f, open + height / 2f, zPosition + depth / 2f),
                new Point3D(xPosition - width / 2f, open - height / 2f, zPosition + depth / 2f),
            ];
            area.matrix3D.TransformPoints(points);
            points[0].PointF = graph.GetAbsolutePoint(points[0].PointF);
            points[1].PointF = graph.GetAbsolutePoint(points[1].PointF);
            points[2].PointF = graph.GetAbsolutePoint(points[2].PointF);

            using Brush brush = new SolidBrush(point.Color);
            // Draw Open mark line
            if (showOpen)
            {
                if (openY <= VAxis.ViewMaximum && openY >= VAxis.ViewMinimum)
                {
                    path.AddLine(points[1].PointF, points[0].PointF);
                    path.AddLine(points[0].PointF, points[2].PointF);
                    path.AddLine(points[2].PointF, points[2].PointF);
                    graph.FillPath(brush, path);
                }
            }

            // Draw close mark line
            if (showClose)
            {
                if (closeY <= VAxis.ViewMaximum && closeY >= VAxis.ViewMinimum)
                {
                    points[0] = new Point3D(xPosition, close, zPosition + depth / 2f);
                    points[1] = new Point3D(xPosition + width / 2f, close + height / 2f, zPosition + depth / 2f);
                    points[2] = new Point3D(xPosition + width / 2f, close - height / 2f, zPosition + depth / 2f);
                    area.matrix3D.TransformPoints(points);
                    points[0].PointF = graph.GetAbsolutePoint(points[0].PointF);
                    points[1].PointF = graph.GetAbsolutePoint(points[1].PointF);
                    points[2].PointF = graph.GetAbsolutePoint(points[2].PointF);

                    path.Reset();
                    path.AddLine(points[1].PointF, points[0].PointF);
                    path.AddLine(points[0].PointF, points[2].PointF);
                    path.AddLine(points[2].PointF, points[2].PointF);
                    graph.FillPath(brush, path);
                }
            }
        }

        // Draw ope-close marks as lines
        else
        {
            // Draw Open mark line
            if (showOpen)
            {
                if (openY <= VAxis.ViewMaximum && openY >= VAxis.ViewMinimum)
                {
                    // Translate coordinates
                    Point3D[] points =
                    [
                        new Point3D(xPosition - width / 2f, open, zPosition + depth / 2f),
                        new Point3D(xPosition, open, zPosition + depth / 2f),
                    ];
                    area.matrix3D.TransformPoints(points);

                    graph.DrawLineRel(point.Color, point.BorderWidth, point.BorderDashStyle,
                        points[0].PointF,
                        points[1].PointF,
                        ser.ShadowColor, ser.ShadowOffset);
                }
            }

            // Draw Close mark line
            if (showClose)
            {
                if (closeY <= VAxis.ViewMaximum && closeY >= VAxis.ViewMinimum)
                {
                    // Translate coordinates
                    Point3D[] points =
                    [
                        new Point3D(xPosition, close, zPosition + depth / 2f),
                        new Point3D(xPosition + width / 2f, close, zPosition + depth / 2f),
                    ];
                    area.matrix3D.TransformPoints(points);

                    graph.DrawLineRel(point.Color, point.BorderWidth, point.BorderDashStyle,
                        points[0].PointF,
                        points[1].PointF,
                        ser.ShadowColor, ser.ShadowOffset);
                }
            }
        }

        // Reset Clip Region
        if (clipRegionSet)
        {
            graph.ResetClip();
        }
    }

    #endregion

    #region Y values related methods

    /// <summary>
    /// Helper function, which returns the Y value of the point.
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
        // Check if series is indexed
        bool indexedSeries = ChartHelper.IndexedSeries(common, area.GetSeriesFromChartType(this.Name));

        //************************************************************
        //** Set active horizontal/vertical axis
        //************************************************************
        Axis hAxis = area.GetAxis(AxisName.X, series.XAxisType, series.XSubAxisName);
        Axis vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);

        //************************************************************
        //** Loop through all data points in the series
        //************************************************************
        int markerIndex = 0;        // Marker index
        int index = 1;              // Data points loop
        foreach (DataPoint point in series.Points)
        {
            //************************************************************
            //** Check if point values are in the chart area
            //************************************************************

            // Check for min/max Y values
            double yValue = GetYValue(common, area, series, point, index - 1, 0);

            // Axis is Logarithmic
            yValue = vAxis.GetLogValue(yValue);

            if (yValue > vAxis.ViewMaximum || yValue < vAxis.ViewMinimum)
            {
                index++;
                continue;
            }

            // Check for min/max X values
            double xValue = indexedSeries ? index : point.XValue;
            xValue = hAxis.GetLogValue(xValue);
            if (xValue > hAxis.ViewMaximum || xValue < hAxis.ViewMinimum)
            {
                index++;
                continue;
            }

            //************************************************************
            //** Get marker position and size
            //************************************************************

            // Get marker position
            PointF markerPosition = PointF.Empty;
            markerPosition.Y = (float)vAxis.GetLinearPosition(yValue);
            if (indexedSeries)
            {
                // The formula for position is based on a distance 
                // from the grid line or nPoints position.
                markerPosition.X = (float)hAxis.GetPosition(index);
            }
            else
            {
                markerPosition.X = (float)hAxis.GetPosition(point.XValue);
            }

            // Get point some point properties and save them in variables
            string pointMarkerImage = point.MarkerImage;
            MarkerStyle pointMarkerStyle = point.MarkerStyle;

            // Get marker size
            SizeF markerSize = SizeF.Empty;
            markerSize.Width = point.MarkerSize;
            markerSize.Height = point.MarkerSize;
            if (common != null && common.graph != null && common.graph.Graphics != null)
            {
                // Marker size is in pixels and we do the mapping for higher DPIs
                markerSize.Width = point.MarkerSize * common.graph.Graphics.DpiX * Chart.DPIScale / 96;
                markerSize.Height = point.MarkerSize * common.graph.Graphics.DpiY * Chart.DPIScale / 96;
            }

            if (point.MarkerImage.Length > 0)
            {
                if (common.graph != null)
                    common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, common.graph.Graphics, ref markerSize);
            }

            // Transform marker position in 3D space
            if (area.Area3DStyle.Enable3D)
            {
                // Get series depth and Z position
                area.GetSeriesZPositionAndDepth(series, out float seriesDepth, out float seriesZPosition);

                Point3D[] marker3DPosition = [ new Point3D(
                    markerPosition.X,
                    markerPosition.Y,
                    seriesZPosition + seriesDepth / 2f) ];

                // Transform coordinates
                area.matrix3D.TransformPoints(marker3DPosition);
                markerPosition = marker3DPosition[0].PointF;
            }

            // Check if marker visible
            if (pointMarkerStyle != MarkerStyle.None ||
                pointMarkerImage.Length > 0)
            {
                // Check marker index
                if (markerIndex == 0)
                {
                    markerSize = common.graph.GetRelativeSize(markerSize);

                    // Add marker position into the list
                    RectangleF markerRect = new RectangleF(
                        markerPosition.X - markerSize.Width / 2f,
                        markerPosition.Y - markerSize.Height,
                        markerSize.Width,
                        markerSize.Height);
                    list.Add(markerRect);
                }

                // Increase the markers counter
                ++markerIndex;
                if (series.MarkerStep == markerIndex)
                {
                    markerIndex = 0;
                }
            }

            ++index;
        }
    }

    #endregion

    #region IDisposable interface implementation
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        //Nothing to dispose at the base class. 
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
