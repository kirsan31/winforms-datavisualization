// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	DoughnutChart class provide only the behavior 
//              information for the Doughnut chart, all the drawing 
//              routines are located in the PieChart base class.
//

namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes;

/// <summary>
/// DoughnutChart class provide only the behavior information for the 
/// Doughnut chart, all the drawing routines are located in the PieChart 
/// base class.
/// </summary>
internal sealed class DoughnutChart : PieChart
{
    #region IChartType interface implementation

    /// <summary>
    /// Chart type name
    /// </summary>
    public override string Name => ChartTypeNames.Doughnut;


    /// <summary>
    /// True if chart type is stacked
    /// </summary>
    public override bool Stacked => false;

    /// <summary>
    /// True if chart type supports axeses
    /// </summary>
    public override bool RequireAxes => false;

    /// <summary>
    /// True if chart type supports logarithmic axes
    /// </summary>
    public override bool SupportLogarithmicAxes => false;

    /// <summary>
    /// True if chart type requires to switch the value (Y) axes position
    /// </summary>
    public override bool SwitchValueAxes => false;

    /// <summary>
    /// True if chart series can be placed side-by-side.
    /// </summary>
    public override bool SideBySideSeries => false;

    /// <summary>
    /// If the crossing value is auto Crossing value should be 
    /// automatically set to zero for some chart 
    /// types (Bar, column, area etc.)
    /// </summary>
    public override bool ZeroCrossing => false;

    /// <summary>
    /// True if each data point of a chart must be represented in the legend
    /// </summary>
    public override bool DataPointsInLegend => true;

    /// <summary>
    /// Indicates that extra Y values are connected to the scale of the Y axis
    /// </summary>
    public override bool ExtraYValuesConnectedToYAxis => false;

    /// <summary>
    /// True if palette colors should be applied for each data paint.
    /// Otherwise the color is applied to the series.
    /// </summary>
    public override bool ApplyPaletteColorsToPoints => true;

    /// <summary>
    /// How to draw series/points in legend:
    /// Filled rectangle, Line or Marker
    /// </summary>
    /// <param name="series">Legend item series.</param>
    /// <returns>Legend item style.</returns>
    public override LegendImageStyle GetLegendImageStyle(Series series)
    {
        return LegendImageStyle.Rectangle;
    }

    /// <summary>
    /// Number of supported Y value(s) per point 
    /// </summary>
    public override int YValuesPerPoint => 1;

    /// <summary>
    /// Chart is Doughnut or Pie type
    /// </summary>
    public override bool Doughnut => true;

    #endregion

    #region Methods

    /// <summary>
    /// Default constructor
    /// </summary>
    public DoughnutChart()
    {
    }

    #endregion
}
