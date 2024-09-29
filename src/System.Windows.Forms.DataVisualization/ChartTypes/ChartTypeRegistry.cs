﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	ChartTypeRegistry is a repository for all standard 
//              and custom chart types. Each chart type has unique 
//              name and IChartType derived class which provides
//              behavior information about the chart type and
//              also contains drawing functionality.
//              ChartTypeRegistry can be used by user for custom 
//              chart type registering and can be retrieved using 
//              Chart.GetService(typeof(ChartTypeRegistry)) method.
//


using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes;

/// <summary>
/// ChartTypeRegistry class is a repository for all standard and custom 
/// chart types. In order for the chart control to display the chart 
/// type, it first must be registered using unique name and IChartType 
/// derived class which provides the description of the chart type and 
/// also responsible for all drawing and hit testing.
/// 
/// ChartTypeRegistry can be used by user for custom chart type registering 
/// and can be retrieved using Chart.GetService(typeof(ChartTypeRegistry)) 
/// method.
/// </summary>
internal class ChartTypeRegistry : IServiceProvider, IDisposable
{
    #region Fields

    // Storage for registered/created chart types
    internal readonly Dictionary<string, Type> registeredChartTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IChartType> _createdChartTypes = new(StringComparer.OrdinalIgnoreCase);

    #endregion

    #region Constructor and Services

    /// <summary>
    /// Chart types registry public constructor.
    /// </summary>
    public ChartTypeRegistry()
    {
    }

    /// <summary>
    /// Returns chart type registry service object.
    /// </summary>
    /// <param name="serviceType">Service type to get.</param>
    /// <returns>Chart type registry service.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(ChartTypeRegistry))
        {
            return this;
        }

        throw new ArgumentException(SR.ExceptionChartTypeRegistryUnsupportedType(serviceType.ToString()));
    }

    #endregion

    #region Registry methods

    /// <summary>
    /// Adds chart type into the registry.
    /// </summary>
    /// <param name="name">Chart type name.</param>
    /// <param name="chartType">Chart class type.</param>
    public void Register(string name, Type chartType)
    {
        // First check if chart type with specified name already registered
        if (registeredChartTypes.TryGetValue(name, out var curT))
        {
            // If same type provided - ignore
            if (curT == chartType)
                return;

            // Error - throw exception
            throw new ArgumentException(SR.ExceptionChartTypeNameIsNotUnique(name));
        }

        // Make sure that specified class support IChartType interface
        bool found = false;
        Type[] interfaces = chartType.GetInterfaces();
        foreach (Type type in interfaces)
        {
            if (type == typeof(IChartType))
            {
                found = true;
                break;
            }
        }

        if (!found)
            throw new ArgumentException(SR.ExceptionChartTypeHasNoInterface);

        // Add chart type to the hash table
        registeredChartTypes[name] = chartType;
    }

    /// <summary>
    /// Returns chart type object by name.
    /// </summary>
    /// <param name="chartType">Chart type.</param>
    /// <returns>Chart type object derived from IChartType.</returns>
    public IChartType GetChartType(SeriesChartType chartType)
    {
        return this.GetChartType(ChartTypeNames.GetChartTypeName(chartType));
    }

    /// <summary>
    /// Returns chart type object by name.
    /// </summary>
    /// <param name="name">Chart type name.</param>
    /// <returns>Chart type object derived from IChartType.</returns>
    public IChartType GetChartType(string name)
    {
        // Check if the chart type object is already created
        if (_createdChartTypes.TryGetValue(name, out var curT))
            return curT;

        // Check if chart type with specified name registered
        if (!registeredChartTypes.TryGetValue(name, out var regT))
            throw new ArgumentException(SR.ExceptionChartTypeUnknown(name));

        // Create chart type object
        var res = (IChartType)regT.Assembly.CreateInstance(regT.ToString());
        _createdChartTypes[name] = res;
        return res;
    }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose managed resource
            foreach (string name in this._createdChartTypes.Keys)
            {
                IChartType chartType = _createdChartTypes[name];
                chartType.Dispose();
            }

            this._createdChartTypes.Clear();
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

/// <summary>
/// IChartType interface must be implemented for any standard or custom 
/// chart type displayed in the chart control. This interface defines 
/// properties which provide information on chart type behavior including 
/// how many Y values supported, is it a stacked chart type, how it 
/// interacts with axes and much more.
/// 
/// IChartType interface methods define how to draw series data point, 
/// calculate Y values and process SmartLabelStyle.
/// </summary>
internal interface IChartType : IDisposable
{
    #region Properties

    /// <summary>
    /// Chart type name
    /// </summary>
    string Name { get; }


    /// <summary>
    /// True if chart type is stacked
    /// </summary>
    bool Stacked { get; }


    /// <summary>
    /// True if stacked chart type supports groups
    /// </summary>
    bool SupportStackedGroups { get; }


    /// <summary>
    /// True if stacked chart type should draw separately positive and 
    /// negative data points ( Bar and column Stacked types ).
    /// </summary>
    bool StackSign { get; }

    /// <summary>
    /// True if chart type supports axeses
    /// </summary>
    bool RequireAxes { get; }

    /// <summary>
    /// True if chart type requires circular chart area.
    /// </summary>
    bool CircularChartArea { get; }

    /// <summary>
    /// True if chart type supports logarithmic axes
    /// </summary>
    bool SupportLogarithmicAxes { get; }

    /// <summary>
    /// True if chart type requires to switch the value (Y) axes position
    /// </summary>
    bool SwitchValueAxes { get; }

    /// <summary>
    /// True if chart series can be placed side-by-side.
    /// </summary>
    bool SideBySideSeries { get; }

    /// <summary>
    /// True if each data point of a chart must be represented in the legend
    /// </summary>
    bool DataPointsInLegend { get; }

    /// <summary>
    /// True if palette colors should be applied for each data paoint.
    /// Otherwise the color is applied to the series.
    /// </summary>
    bool ApplyPaletteColorsToPoints { get; }

    /// <summary>
    /// Indicates that extra Y values are connected to the scale of the Y axis
    /// </summary>
    bool ExtraYValuesConnectedToYAxis { get; }

    /// <summary>
    /// If the crossing value is auto Crossing value should be 
    /// automatically set to zero for some chart 
    /// types (Bar, column, area etc.)
    /// </summary>
    bool ZeroCrossing { get; }

    /// <summary>
    /// Number of supported Y value(s) per point 
    /// </summary>
    int YValuesPerPoint { get; }

    /// <summary>
    /// Chart type with two y values used for scale ( bubble chart type )
    /// </summary>
    bool SecondYScale { get; }

    /// <summary>
    /// Indicates that it's a hundredred percent chart.
    /// Axis scale from 0 to 100 percent should be used.
    /// </summary>
    bool HundredPercent { get; }

    /// <summary>
    /// Indicates that negative 100% stacked values are shown on
    /// the other side of the X axis
    /// </summary>
    bool HundredPercentSupportNegative { get; }

    /// <summary>
    /// How to draw series/points in legend:
    /// Filled rectangle, Line or Marker
    /// </summary>
    /// <param name="series">Legend item series.</param>
    /// <returns>Legend item style.</returns>
    LegendImageStyle GetLegendImageStyle(Series series);

    #endregion

    #region Painting and Selection methods

    /// <summary>
    /// Draw chart on specified chart graphics.
    /// </summary>
    /// <param name="graph">Chart grahhics object.</param>
    /// <param name="common">Common elements.</param>
    /// <param name="area">Chart area to draw on.</param>
    /// <param name="seriesToDraw">Chart series to draw.</param>
    void Paint(ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw);

    #endregion

    #region Y values methods

    /// <summary>
    /// Helper function, which returns the Y value of the data point.
    /// </summary>
    /// <param name="common">Chart common elements.</param>
    /// <param name="area">Chart area the series belongs to.</param>
    /// <param name="series">Sereis of the point.</param>
    /// <param name="point">Point object.</param>
    /// <param name="pointIndex">Index of the point.</param>
    /// <param name="yValueIndex">Index of the Y value to get.</param>
    /// <returns>Y value of the point.</returns>
    double GetYValue(CommonElements common, ChartArea area, Series series, DataPoint point, int pointIndex, int yValueIndex);

    #endregion

    #region SmartLabelStyle methods

    /// <summary>
    /// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
    /// </summary>
    /// <param name="common">Common chart elements.</param>
    /// <param name="area">Chart area.</param>
    /// <param name="series">Series values to be used.</param>
    /// <param name="list">List to add to.</param>
    void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, List<RectangleF> list);

    #endregion
}
