// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Series storage and manipulation class.
//


using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace System.Windows.Forms.DataVisualization.Charting.Data;

/// <summary>
/// Data Manager.
/// </summary>
internal sealed class DataManager : ChartElement, IServiceProvider, IDisposable
{
    #region Fields
    // Series collection

    // Service container reference
    internal IServiceContainer serviceContainer;

    /// <summary>
    /// Indicates if at least one series is indexed (in this case we tread that all series are indexed). null if not checked yet.
    /// </summary>
    internal bool? indexedSeries;

    /// <summary>
    /// Indicates if all series has all X values set to 0. null if not checked yet.
    /// </summary>
    internal bool? xValuesZeros;

    #endregion

    #region Constructors and initialization

    /// <summary>
    /// Data manager public constructor
    /// </summary>
    /// <param name="container">Service container object.</param>
    public DataManager(IServiceContainer container)
    {
        serviceContainer = container ?? throw new ArgumentNullException(SR.ExceptionInvalidServiceContainer);
        Common = new CommonElements(container);
        Series = new SeriesCollection(this);
    }

    /// <summary>
    /// Returns Data Manager service object.
    /// </summary>
    /// <param name="serviceType">Service type requested.</param>
    /// <returns>Data Manager service object.</returns>
    [EditorBrowsableAttribute(EditorBrowsableState.Never)]
    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(DataManager))
        {
            return this;
        }

        throw new ArgumentException(SR.ExceptionDataManagerUnsupportedType(serviceType.ToString()));
    }

    /// <summary>
    /// Initialize data manger object
    /// </summary>
    internal void Initialize()
    {
        // Attach to the Chart Picture painting events
        ChartImage chartPicture = (ChartImage)serviceContainer.GetService(typeof(ChartImage));
        chartPicture.BeforePaint += new EventHandler<ChartPaintEventArgs>(this.ChartPicture_BeforePaint);
        chartPicture.AfterPaint += new EventHandler<ChartPaintEventArgs>(this.ChartPicture_AfterPaint);
    }

    #endregion

    #region Chart picture painting events hanlers

    internal override void Invalidate()
    {
        base.Invalidate();

        Chart?.Invalidate();
    }


    /// <summary>
    /// Event fired when chart picture is going to be painted.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ChartPicture_BeforePaint(object sender, ChartPaintEventArgs e)
    {
        // Prepare series for drawing
        int markerIndex = 1;
        indexedSeries = null;
        xValuesZeros = null;

        for (int index = 0; index < this.Series.Count; index++)
        {
            Series series = this.Series[index];
            // Set series colors from palette
            IChartType chartType = e.CommonElements.ChartTypeRegistry.GetChartType(series.ChartTypeName);
            bool paletteColorsInPoints = chartType.ApplyPaletteColorsToPoints;
            // if the series palette is set the we can color all data points, even on column chart.
            if (series.Palette != ChartColorPalette.None)
            {
                paletteColorsInPoints = true;
            }

            this.PrepareData(
                paletteColorsInPoints,
                series.Name);

            // Clear temp. marker style
            if (series.tempMarkerStyleIsSet)
            {
                series.MarkerStyle = MarkerStyle.None;
                series.tempMarkerStyleIsSet = false;
            }

            // Set marker style for chart types based on markers
            if (chartType.GetLegendImageStyle(series) == LegendImageStyle.Marker && series.MarkerStyle == MarkerStyle.None)
            {
                series.MarkerStyle = (MarkerStyle)markerIndex++;
                series.tempMarkerStyleIsSet = true;

                if (markerIndex > 9)
                {
                    markerIndex = 1;
                }
            }
        }
    }

    /// <summary>
    /// Event fired after chart picture was painted.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Event arguments.</param>
    private void ChartPicture_AfterPaint(object sender, ChartPaintEventArgs e)
    {
        Chart control = (Chart)serviceContainer.GetService(typeof(Chart));
        if (control != null)
        {
            // Clean up series after drawing
            for (int index = 0; index < this.Series.Count; index++)
            {
                Series series = this.Series[index];
                if (series.UnPrepareData(control.Site))
                {
                    --index;
                }
            }
        }
    }

    #endregion

    #region Series data preparation methods

    /// <summary>
    /// Apply palette colors to the data series if UsePaletteColors property is set.
    /// </summary>
    internal void ApplyPaletteColors()
    {
        ChartColorPalette palette = this.Palette;
        // switch to default pallette if is none and custom collors array is empty.
        if (palette == ChartColorPalette.None && this.PaletteCustomColors.Length == 0)
        {
            palette = ChartColorPalette.BrightPastel;
        }

        // Get palette colors
        int colorIndex = 0;
        Color[] paletteColors = (palette == ChartColorPalette.None) ?
            this.PaletteCustomColors : ChartPaletteColors.GetPaletteColors(palette);

        foreach (Series dataSeries in Series)
        {
            // Check if chart area name is valid
            bool validAreaName = false;
            if (Chart != null)
            {
                validAreaName = Chart.ChartAreas.IsNameReferenceValid(dataSeries.ChartArea);
            }

            // Change color of the series only if valid chart area name is specified
            if (validAreaName)
            {
                // Change color of the series only if default color is set
                if (dataSeries.Color == Color.Empty || dataSeries.tempColorIsSet)
                {
                    dataSeries.color = paletteColors[colorIndex++];
                    dataSeries.tempColorIsSet = true;
                    if (colorIndex >= paletteColors.Length)
                    {
                        colorIndex = 0;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called just before the data from the series to be used to perform these operations:
    ///  - apply palette colors to the data series
    ///  - prepare data in series
    /// </summary>
    /// <param name="pointsApplyPaletteColors">If true each data point will be assigned a color from the palette (if it's set)</param>
    /// <param name="series">List of series indexes, which requires data preparation</param>
    internal void PrepareData(bool pointsApplyPaletteColors, params string[] series)
    {
        this.ApplyPaletteColors();

        // Prepare data in series
        Chart control = (Chart)serviceContainer.GetService(typeof(Chart));
        if (control != null)
        {
            foreach (string seriesName in series)
            {
                this.Series[seriesName].PrepareData(pointsApplyPaletteColors);
            }
        }
    }

    #endregion

    #region Series Min/Max values methods

    /// <summary>
    /// This method checks if data point should be skipped. This 
    /// method will return true if data point is empty.
    /// </summary>
    /// <param name="point">Data point</param>
    /// <returns>This method returns true if data point is empty.</returns>
    private bool IsPointSkipped(DataPoint point)
    {
        if (point.IsEmpty)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets max number of data points in specified series.
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum number of data points</returns>
    internal int GetNumberOfPoints(List<string> series)
    {
        int numberOfPoints = 0;
        foreach (string seriesName in series)
        {
            numberOfPoints = Math.Max(numberOfPoints, this.Series[seriesName].Points.Count);
        }

        return numberOfPoints;
    }

    /// <summary>
    /// Gets maximum Y value from many series
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum Y value</returns>
    internal double GetMaxYValue(int valueIndex, params string[] series)
    {
        double returnValue = double.MinValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.YValues[valueIndex]))
                {
                    returnValue = Math.Max(returnValue, seriesPoint.YValues[valueIndex]);
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Get Maximum value for Y and and Radius (Y2) ( used for bubble chart )
    /// </summary>
    /// <param name="area">Chart Area</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum Y value</returns>
    internal double GetMaxYWithRadiusValue(ChartArea area, List<string> series)
    {
        double returnValue = double.MinValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.YValues[0]))
                {
                    if (seriesPoint.YValues.Length > 1)
                    {
                        returnValue = Math.Max(returnValue, seriesPoint.YValues[0] + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                    }
                    else
                    {
                        returnValue = Math.Max(returnValue, seriesPoint.YValues[0]);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Get Maximum value for X and Radius (Y2) ( used for bubble chart )
    /// </summary>
    /// <param name="area">Chart Area</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum X value</returns>
    internal double GetMaxXWithRadiusValue(ChartArea area, List<string> series)
    {
        double returnValue = double.MinValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.XValue))
                {
                    if (seriesPoint.YValues.Length > 1)
                    {
                        returnValue = Math.Max(returnValue, seriesPoint.XValue + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.XValue, false));
                    }
                    else
                    {
                        returnValue = Math.Max(returnValue, seriesPoint.XValue);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Get Minimum value for X and Radius Y2 ( used for bubble chart )
    /// </summary>
    /// <param name="area">Chart Area</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum X value</returns>
    internal double GetMinXWithRadiusValue(ChartArea area, List<string> series)
    {
        double returnValue = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.XValue))
                {
                    if (seriesPoint.YValues.Length > 1)
                    {
                        returnValue = Math.Min(returnValue, seriesPoint.XValue - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], false));
                    }
                    else
                    {
                        returnValue = Math.Min(returnValue, seriesPoint.XValue);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets maximum Y value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum Y value</returns>
    internal double GetMaxYValue(params string[] series)
    {
        double returnValue = double.MinValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                foreach (double y in seriesPoint.YValues)
                {
                    if (!double.IsNaN(y))
                    {
                        returnValue = Math.Max(returnValue, y);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets maximum X value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum X value</returns>
    internal double GetMaxXValue(params string[] series)
    {
        double returnValue = double.MinValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                returnValue = Math.Max(returnValue, seriesPoint.XValue);
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum and maximum X value from many series.
    /// </summary>
    /// <param name="min">Returns maximum X value.</param>
    /// <param name="max">Returns minimum X value.</param>
    /// <param name="series">Series IDs</param>
    internal void GetMinMaxXValue(out double min, out double max, List<string> series)
    {
        max = double.MinValue;
        min = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                max = Math.Max(max, seriesPoint.XValue);
                min = Math.Min(min, seriesPoint.XValue);
            }
        }
    }

    /// <summary>
    /// Gets minimum and maximum Y value from many series.
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use.</param>
    /// <param name="min">Returns maximum Y value.</param>
    /// <param name="max">Returns minimum Y value.</param>
    /// <param name="series">Series IDs</param>
    internal void GetMinMaxYValue(int valueIndex, out double min, out double max, List<string> series)
    {
        max = double.MinValue;
        min = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // Skip empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                double yValue = seriesPoint.YValues[valueIndex];
                if (!double.IsNaN(yValue))
                {
                    max = Math.Max(max, yValue);
                    min = Math.Min(min, yValue);
                }
            }
        }
    }

    /// <summary>
    /// Gets minimum and maximum Y value from many series.
    /// </summary>
    /// <param name="min">Returns maximum Y value.</param>
    /// <param name="max">Returns minimum Y value.</param>
    /// <param name="series">Series IDs</param>
    internal void GetMinMaxYValue(out double min, out double max, List<string> series)
    {
        max = double.MinValue;
        min = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // Skip empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                // Iterate through all Y values
                foreach (double y in seriesPoint.YValues)
                {
                    if (!double.IsNaN(y))
                    {
                        max = Math.Max(max, y);
                        min = Math.Min(min, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets minimum and maximum Y value from many series.
    /// </summary>
    /// <param name="seriesList">Series objects list.</param>
    /// <param name="min">Returns maximum Y value.</param>
    /// <param name="max">Returns minimum Y value.</param>
    internal void GetMinMaxYValue(System.Collections.ArrayList seriesList, out double min, out double max)
    {
        max = double.MinValue;
        min = double.MaxValue;
        foreach (Series series in seriesList)
        {
            foreach (DataPoint seriesPoint in series.Points)
            {
                // Skip empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                // Iterate through all Y values
                foreach (double y in seriesPoint.YValues)
                {
                    if (!double.IsNaN(y))
                    {
                        max = Math.Max(max, y);
                        min = Math.Min(min, y);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets maximum stacked Y value from many series
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum stacked Y value</returns>
    internal double GetMaxStackedYValue(int valueIndex, List<string> series)
    {
        double returnValue = 0;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double stackedMax = 0;
            double noStackedMax = 0;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points.Count > pointIndex)
                {
                    // Take chart type from the series 
                    ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                    IChartType chartType = chartTypeRegistry.GetChartType(this.Series[seriesName].ChartTypeName);

                    // If stacked area
                    if (!chartType.StackSign)
                        continue;

                    if (chartType.Stacked)
                    {
                        if (this.Series[seriesName].Points[pointIndex].YValues[valueIndex] > 0)
                        {
                            stackedMax += this.Series[seriesName].Points[pointIndex].YValues[valueIndex];
                        }
                    }
                    else
                    {
                        noStackedMax = Math.Max(noStackedMax, this.Series[seriesName].Points[pointIndex].YValues[valueIndex]);
                    }
                }
            }

            stackedMax = Math.Max(stackedMax, noStackedMax);
            returnValue = Math.Max(returnValue, stackedMax);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets maximum Unsigned stacked Y value from many series ( Stacked Area chart )
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum stacked Y value</returns>
    internal double GetMaxUnsignedStackedYValue(int valueIndex, List<string> series)
    {
        double returnValue = 0;
        double maxValue = double.MinValue;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double stackedMax = 0;
            double noStackedMax = 0;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points.Count > pointIndex)
                {
                    // Take chart type from the series 
                    ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                    IChartType chartType = chartTypeRegistry.GetChartType(this.Series[seriesName].ChartTypeName);

                    // If stacked column and bar
                    if (chartType.StackSign || double.IsNaN(this.Series[seriesName].Points[pointIndex].YValues[valueIndex]))
                    {
                        continue;
                    }

                    if (chartType.Stacked)
                    {
                        maxValue = double.MinValue;
                        stackedMax += this.Series[seriesName].Points[pointIndex].YValues[valueIndex];
                        if (stackedMax > maxValue)
                            maxValue = stackedMax;
                    }
                    else
                    {
                        noStackedMax = Math.Max(noStackedMax, this.Series[seriesName].Points[pointIndex].YValues[valueIndex]);
                    }
                }
            }

            maxValue = Math.Max(maxValue, noStackedMax);
            returnValue = Math.Max(returnValue, maxValue);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets maximum stacked X value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Maximum stacked X value</returns>
    internal double GetMaxStackedXValue(List<string> series)
    {
        double returnValue = 0;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double doubleIndexValue = 0;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points.Count > pointIndex)
                {
                    if (this.Series[seriesName].Points[pointIndex].XValue > 0)
                    {
                        doubleIndexValue += this.Series[seriesName].Points[pointIndex].XValue;
                    }
                }
            }

            returnValue = Math.Max(returnValue, doubleIndexValue);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum Y value from many series
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum Y value</returns>
    internal double GetMinYValue(int valueIndex, params string[] series)
    {
        double returnValue = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.YValues[valueIndex]))
                {
                    returnValue = Math.Min(returnValue, seriesPoint.YValues[valueIndex]);
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Get Minimum value for Y and and Radius (Y2) ( used for bubble chart )
    /// </summary>
    /// <param name="area">Chart Area</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum Y value</returns>
    internal double GetMinYWithRadiusValue(ChartArea area, List<string> series)
    {
        double returnValue = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                if (!double.IsNaN(seriesPoint.YValues[0]))
                {
                    if (seriesPoint.YValues.Length > 1)
                    {
                        returnValue = Math.Min(returnValue, seriesPoint.YValues[0] - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                    }
                    else
                    {
                        returnValue = Math.Min(returnValue, seriesPoint.YValues[0]);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum Y value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum Y value</returns>
    internal double GetMinYValue(params string[] series)
    {
        double returnValue = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                // The empty point
                if (IsPointSkipped(seriesPoint))
                {
                    continue;
                }

                foreach (double y in seriesPoint.YValues)
                {
                    if (!double.IsNaN(y))
                    {
                        returnValue = Math.Min(returnValue, y);
                    }
                }
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum X value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum X value</returns>
    internal double GetMinXValue(params string[] series)
    {
        double returnValue = double.MaxValue;
        foreach (string seriesName in series)
        {
            foreach (DataPoint seriesPoint in this.Series[seriesName].Points)
            {
                returnValue = Math.Min(returnValue, seriesPoint.XValue);
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum stacked Y value from many series
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum stacked Y value</returns>
    internal double GetMinStackedYValue(int valueIndex, List<string> series)
    {
        double returnValue = double.MaxValue;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double stackedMin = 0;
            double noStackedMin = 0;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points.Count > pointIndex)
                {
                    // Take chart type from the series 
                    ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                    IChartType chartType = chartTypeRegistry.GetChartType(this.Series[seriesName].ChartTypeName);

                    // If stacked area
                    if (!chartType.StackSign || double.IsNaN(this.Series[seriesName].Points[pointIndex].YValues[valueIndex]))
                        continue;

                    if (chartType.Stacked)
                    {
                        if (this.Series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
                        {
                            stackedMin += this.Series[seriesName].Points[pointIndex].YValues[valueIndex];
                        }
                    }
                    else
                    {
                        noStackedMin = Math.Min(noStackedMin, this.Series[seriesName].Points[pointIndex].YValues[valueIndex]);
                    }
                }
            }

            stackedMin = Math.Min(stackedMin, noStackedMin);
            if (stackedMin == 0)
            {
                stackedMin = this.Series[series[0]].Points[^1].YValues[valueIndex];
            }

            returnValue = Math.Min(returnValue, stackedMin);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum Unsigned stacked Y value from many series
    /// </summary>
    /// <param name="valueIndex">Index of Y value to use</param>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum stacked Y value</returns>
    internal double GetMinUnsignedStackedYValue(int valueIndex, List<string> series)
    {
        double returnValue = double.MaxValue;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double stackedMin = 0;
            double noStackedMin = 0;
            double minValue = double.MaxValue;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points.Count > pointIndex)
                {
                    // Take chart type from the series 
                    ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                    IChartType chartType = chartTypeRegistry.GetChartType(this.Series[seriesName].ChartTypeName);

                    // If stacked column and bar
                    if (chartType.StackSign || double.IsNaN(this.Series[seriesName].Points[pointIndex].YValues[valueIndex]))
                    {
                        continue;
                    }

                    if (chartType.Stacked)
                    {
                        if (this.Series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
                        {
                            stackedMin += this.Series[seriesName].Points[pointIndex].YValues[valueIndex];
                            if (stackedMin < minValue)
                                minValue = stackedMin;
                        }
                    }
                    else
                    {
                        noStackedMin = Math.Min(noStackedMin, this.Series[seriesName].Points[pointIndex].YValues[valueIndex]);
                    }
                }
            }

            minValue = Math.Min(noStackedMin, minValue);
            returnValue = Math.Min(returnValue, minValue);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum stacked X value from many series
    /// </summary>
    /// <param name="series">Series IDs</param>
    /// <returns>Minimum stacked X value</returns>
    internal double GetMinStackedXValue(List<string> series)
    {
        double returnValue = 0;
        double numberOfPoints = GetNumberOfPoints(series);
        for (int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
        {
            double doubleIndexValue = 0;
            foreach (string seriesName in series)
            {
                if (this.Series[seriesName].Points[pointIndex].XValue < 0)
                {
                    doubleIndexValue += this.Series[seriesName].Points[pointIndex].XValue;
                }
            }

            returnValue = Math.Min(returnValue, doubleIndexValue);
        }

        return returnValue;
    }


    /// <summary>
    /// Gets maximum hundred percent stacked Y value
    /// </summary>
    /// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
    /// <param name="series">Series names</param>
    /// <returns>Maximum 100% stacked Y value</returns>
    internal double GetMaxHundredPercentStackedYValue(bool supportNegative, List<string> series)
    {
        double returnValue = 0;

        // Convert array of series names into array of series
        Series[] seriesArray = new Series[series.Count];
        int seriesIndex = 0;
        foreach (string seriesName in series)
        {
            seriesArray[seriesIndex++] = this.Series[seriesName];
        }

        // Loop through all dat points
        try
        {
            for (int pointIndex = 0; pointIndex < this.Series[series[0]].Points.Count; pointIndex++)
            {
                // Calculate the total for all series
                double totalPerPoint = 0;
                double positiveTotalPerPoint = 0;
                foreach (Series ser in seriesArray)
                {
                    if (supportNegative)
                    {
                        totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
                    }
                    else
                    {
                        totalPerPoint += ser.Points[pointIndex].YValues[0];
                    }

                    if (ser.Points[pointIndex].YValues[0] > 0 || supportNegative == false)
                    {
                        positiveTotalPerPoint += ser.Points[pointIndex].YValues[0];
                    }
                }

                totalPerPoint = Math.Abs(totalPerPoint);

                // Calculate percentage of total
                if (totalPerPoint != 0)
                {
                    returnValue = Math.Max(returnValue,
                        positiveTotalPerPoint / totalPerPoint * 100.0);
                }
            }
        }
        catch (System.Exception)
        {
            throw new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch);
        }

        return returnValue;
    }

    /// <summary>
    /// Gets minimum hundred percent stacked Y value
    /// </summary>
    /// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
    /// <param name="series">Series names</param>
    /// <returns>Minimum 100% stacked Y value</returns>
    internal double GetMinHundredPercentStackedYValue(bool supportNegative, List<string> series)
    {
        double returnValue = 0.0;

        // Convert array of series names into array of series
        Series[] seriesArray = new Series[series.Count];
        int seriesIndex = 0;
        foreach (string seriesName in series)
        {
            seriesArray[seriesIndex++] = this.Series[seriesName];
        }

        // Loop through all dat points
        try
        {
            for (int pointIndex = 0; pointIndex < this.Series[series[0]].Points.Count; pointIndex++)
            {
                // Calculate the total for all series
                double totalPerPoint = 0;
                double negativeTotalPerPoint = 0;
                foreach (Series ser in seriesArray)
                {
                    if (supportNegative)
                    {
                        totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
                    }
                    else
                    {
                        totalPerPoint += ser.Points[pointIndex].YValues[0];
                    }

                    if (ser.Points[pointIndex].YValues[0] < 0 || supportNegative == false)
                    {
                        negativeTotalPerPoint += ser.Points[pointIndex].YValues[0];
                    }
                }

                totalPerPoint = Math.Abs(totalPerPoint);

                // Calculate percentage of total
                if (totalPerPoint != 0)
                {
                    returnValue = Math.Min(returnValue,
                        negativeTotalPerPoint / totalPerPoint * 100.0);
                }
            }
        }
        catch (System.Exception)
        {
            throw new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch);
        }

        return returnValue;
    }

    #endregion

    #region DataManager Properties

    /// <summary>
    /// Chart series collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    Editor("SeriesCollectionEditor", typeof(UITypeEditor)),
    Bindable(true)
    ]
    public SeriesCollection Series { get; private set; }

    /// <summary>
    /// Color palette to use
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributePalette"),
    DefaultValue(ChartColorPalette.BrightPastel),
    Editor("ColorPaletteEditor", typeof(UITypeEditor))
    ]
    public ChartColorPalette Palette { get; set; } = ChartColorPalette.BrightPastel;

    /// <summary>
    /// Array of custom palette colors.
    /// </summary>
    /// <remarks>
    /// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
    /// </remarks>
    [
    SRCategory("CategoryAttributeAppearance"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    SerializationVisibilityAttribute(SerializationVisibility.Attribute),
    SRDescription("DescriptionAttributeDataManager_PaletteCustomColors"),
    TypeConverter(typeof(ColorArrayConverter))
    ]
    public Color[] PaletteCustomColors { set; get; } = Array.Empty<Color>();




    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        if (Series != null)
        {
            Series.Dispose();
            Series = null;
        }
    }
    #endregion
}