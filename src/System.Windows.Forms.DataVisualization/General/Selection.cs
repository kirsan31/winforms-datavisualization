﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	This file contains methods used for Win Form selection
//


using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;

using Point = System.Drawing.Point;

namespace System.Windows.Forms.DataVisualization.Charting;

#region Enumerations


// Please keep the following enumeration in chart layering order - ex. ChartArea is under DataPoint 
/// <summary>
/// An enumeration of types of Chart Element.
/// </summary>
public enum ChartElementType
{
    /// <summary>
    /// No chart element.
    /// </summary>
    Nothing,

    /// <summary>
    /// The title of a chart.
    /// </summary>
    Title,

    /// <summary>
    /// Plotting area (chart area excluding axes, labels, etc.).  
    /// Also excludes the regions that data points may occupy.
    /// </summary>
    PlottingArea,

    /// <summary>
    /// An Axis object.
    /// </summary>
    Axis,

    /// <summary>
    /// Any major or minor tick mark.
    /// </summary>
    TickMarks,

    /// <summary>
    /// Any major or minor grid line (both vertical or horizontal).
    /// </summary>
    Gridlines,

    /// <summary>
    /// A StripLine object.
    /// </summary>
    StripLines,

    /// <summary>
    /// Axis label Image.
    /// </summary>
    AxisLabelImage,

    /// <summary>
    /// Axis labels
    /// </summary>
    AxisLabels,

    /// <summary>
    /// Axis title
    /// </summary>
    AxisTitle,

    /// <summary>
    /// A scrollbar tracking thumb.
    /// </summary>
    ScrollBarThumbTracker,

    /// <summary>
    /// A scrollbar small decrement button.  A "down arrow" 
    /// button for a vertical scrollbar, or a "left arrow" 
    /// button for a horizontal scroll bar.
    /// </summary>
    ScrollBarSmallDecrement,

    /// <summary>
    /// A scrollbar small increment button.  An "up arrow" 
    /// button for a vertical scrollbar, or a "right arrow" 
    /// button for a horizontal scroll bar.
    /// </summary>
    ScrollBarSmallIncrement,

    /// <summary>
    /// The background of a scrollbar that will result in 
    /// a large decrement in the scale view size when clicked.  
    /// This is the background below the thumb for 
    /// a vertical scrollbar, and to the left of 
    /// the thumb for a horizontal scrollbar.
    /// </summary>
    ScrollBarLargeDecrement,

    /// <summary>
    /// The background of a scrollbar that will result in 
    /// a large increment in the scale view size when clicked.  
    /// This is the background above the thumb for 
    /// a vertical scrollbar, and to the right of 
    /// the thumb for a horizontal scrollbar.
    /// </summary>
    ScrollBarLargeIncrement,

    /// <summary>
    /// The zoom reset button of a scrollbar.
    /// </summary>
    ScrollBarZoomReset,

    /// <summary>
    /// A DataPoint object.
    /// </summary>
    DataPoint,

    /// <summary>
    /// Series data point label.
    /// </summary>
    DataPointLabel,

    /// <summary>
    /// The area inside a Legend object.  Does not include 
    /// the space occupied by legend items.
    /// </summary>
    LegendArea,

    /// <summary>
    /// Legend title.
    /// </summary>
    LegendTitle,

    /// <summary>
    /// Legend header.
    /// </summary>
    LegendHeader,

    /// <summary>
    /// A LegendItem object.
    /// </summary>
    LegendItem,


    /// <summary>
    /// Chart annotation object.
    /// </summary>
    Annotation,


}

/// <summary>
/// Enumeration (Flag) used for processing chart types.
/// </summary>
[Flags]
internal enum ProcessMode
{
    /// <summary>
    /// Paint mode
    /// </summary>
    Paint = 1,

    /// <summary>
    /// Selection mode. Collection of hot regions has to be created.
    /// </summary>
    HotRegions = 2,

    /// <summary>
    /// Used for image maps
    /// </summary>
    ImageMaps = 4
}

#endregion

/// <summary>
/// This class presents item in
/// the collection of hot regions.
/// </summary>
internal class HotRegion : IDisposable
{
    #region Fields

    // Private data members, which store properties values
    private RectangleF _boundingRectangle = RectangleF.Empty;
    private string _seriesName = string.Empty;
    private object _selectedSubObject;


    #endregion // Fields

    #region Properties

    /// <summary>
    /// Region is Graphics path
    /// </summary>
    internal GraphicsPath Path { get; set; }

    /// <summary>
    /// Relative coordinates are used 
    /// to define region
    /// </summary>
    internal bool RelativeCoordinates { get; set; } = true;

    /// <summary>
    /// Bounding Rectangle of an shape
    /// </summary>
    internal RectangleF BoundingRectangle
    {
        get => _boundingRectangle;
        set => _boundingRectangle = value;
    }

    /// <summary>
    /// Object which is presented with this region
    /// </summary>
    internal object SelectedObject { get; set; }



    /// <summary>
    /// Sub-Object which is presented with this region
    /// </summary>
    internal object SelectedSubObject
    {
        get => _selectedSubObject;
        set => _selectedSubObject = value;
    }



    /// <summary>
    /// Index of the data point which is presented with this region
    /// </summary>
    internal int PointIndex { get; set; } = -1;

    /// <summary>
    /// Name of the series which is presented with the region
    /// </summary>
    internal string SeriesName
    {
        get => _seriesName;
        set => _seriesName = value;
    }

    /// <summary>
    /// Chart Element AxisName
    /// </summary>
    internal ChartElementType Type { get; set; } = ChartElementType.Nothing;

    #endregion // Properties

    #region IDisposable members
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (Path != null)
            {
                Path.Dispose();
                Path = null;
            }
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

    #region Methods

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
        string objectType = this.SelectedObject != null ? this.SelectedObject.ToString() : "null";
        if (this.SelectedObject == null && !string.IsNullOrEmpty(this.SeriesName))
        {
            objectType = this.SeriesName;
        }

        return string.Format(CultureInfo.CurrentCulture, "{0} of {1}", this.Type, objectType);
    }

    #endregion //Methods
}

/// <summary>
/// This class is used to fill and 
/// manage collection with Hot Regions
/// </summary>
internal class HotRegionsList : IDisposable
{
    #region Fields

    /// <summary>
    /// Process chart mode Flag
    /// </summary>
    private ProcessMode _processChartMode = ProcessMode.Paint;

    /// <summary>
    /// Reference to the common elements object
    /// </summary>
    private readonly CommonElements _common;

    /// <summary>
    /// True if hit test function is called
    /// </summary>
    internal bool hitTestCalled;

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Flag used for processing chart types. It could 
    /// be Paint, HotRegion or both mode.
    /// </summary>
    internal ProcessMode ProcessChartMode
    {
        get => _processChartMode;
        set
        {
            _processChartMode = value;
            if (this._common != null)
            {
                this._common.processModePaint =
                    (_processChartMode & ProcessMode.Paint) == ProcessMode.Paint;
                this._common.processModeRegions =
                    (_processChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions ||
                    (_processChartMode & ProcessMode.ImageMaps) == ProcessMode.ImageMaps;
            }
        }
    }

    /// <summary>
    /// Collection with Hor Region Elements
    /// </summary>
    internal List<HotRegion> List { get; } = new List<HotRegion>();

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="common">Reference to the CommonElements</param>
    internal HotRegionsList(CommonElements common)
    {
        this._common = common;
    }

    /// <summary>
    /// Add hot region to the collection.
    /// </summary>
    /// <param name="rectSize">Rectangle which presents an Hot Region</param>
    /// <param name="point">Data Point</param>
    /// <param name="seriesName">Data Series</param>
    /// <param name="pointIndex">Index of an Data Point in the series</param>
    public void AddHotRegion(
        RectangleF rectSize,
        DataPoint point,
        string seriesName,
        int pointIndex)
    {

        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                BoundingRectangle = rectSize,
                SeriesName = seriesName,
                PointIndex = pointIndex,
                Type = ChartElementType.DataPoint,
                RelativeCoordinates = true
            };

            // Use index of the original data point
            if (point?.IsCustomPropertySet("OriginalPointIndex") == true)
                region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);

            List.Add(region);
        }
    }


    /// <summary>
    /// Adds the hot region.
    /// </summary>
    /// <param name="path">Bounding GraphicsPath.</param>
    /// <param name="relativePath">if set to <c>true</c> the is relative path.</param>
    /// <param name="point">Selected data point</param>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="pointIndex">Index of the point.</param>
    internal void AddHotRegion(
            GraphicsPath path,
            bool relativePath,
            DataPoint point,
            string seriesName,
            int pointIndex
            )
    {
        if (path is null)
            return;

        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                SeriesName = seriesName,
                PointIndex = pointIndex,
                Type = ChartElementType.DataPoint,
                Path = (GraphicsPath)path.Clone(),
                BoundingRectangle = path.GetBounds(),
                RelativeCoordinates = relativePath
            };
            // Use index of the original data point
            if (point?.IsCustomPropertySet("OriginalPointIndex") == true)
                region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);

            List.Add(region);
        }
    }

    /// <summary>
    /// Adds the hot region.
    /// </summary>
    /// <param name="path">Bounding GraphicsPath.</param>
    /// <param name="boundingRectangle">The bounding rectangle.</param>
    /// <param name="relativePath">If set to <c>true</c> the is relative path.</param>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="pointIndex">Index of the point.</param>
    internal void AddHotRegion(
            GraphicsPath path,
            RectangleF boundingRectangle,
            bool relativePath,
            string seriesName,
            int pointIndex
            )
    {
        if (path is null || (ProcessChartMode & ProcessMode.HotRegions) != ProcessMode.HotRegions)
            return;

        HotRegion region = new HotRegion
        {
            SeriesName = seriesName,
            PointIndex = pointIndex,
            Type = ChartElementType.DataPoint,
            Path = (GraphicsPath)path.Clone(),
            BoundingRectangle = boundingRectangle,
            RelativeCoordinates = relativePath
        };
        List.Add(region);
    }

    /// <summary>
    /// Adds the hot region.
    /// </summary>
    /// <param name="path">Bounding GraphicsPath.</param>
    /// <param name="relativePath">If set to <c>true</c> the is relative path.</param>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="pointIndex">Index of the point.</param>
    internal void AddHotRegion(
            GraphicsPath path,
            bool relativePath,
            string seriesName,
            int pointIndex
            )
    {
        if (path is null || (ProcessChartMode & ProcessMode.HotRegions) != ProcessMode.HotRegions)
            return;

        HotRegion region = new HotRegion
        {
            SeriesName = seriesName,
            PointIndex = pointIndex,
            Type = ChartElementType.DataPoint,
            Path = (GraphicsPath)path.Clone(),
            BoundingRectangle = path.GetBounds(),
            RelativeCoordinates = relativePath
        };
        List.Add(region);
    }

    /// <summary>
    /// Adds the hot region.
    /// </summary>
    /// <param name="path">Bounding GraphicsPath.</param>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="pointIndex">Index of the point.</param>
    /// <param name="yGoDown">If set to <see langword="true" /> then points y value is increasing like system coordinates from top to down.</param>
    /// <param name="xGoRight">If set to <see langword="true" /> then points x value is increasing like system coordinates from left to right.</param>
    /// <param name="yDiffMax">If set to <see langword="true" /> then points y values difference is grater than x values difference.</param>
    internal void AddHotRegion(
            GraphicsPath path,
            string seriesName,
            int pointIndex,
            bool yGoDown,
            bool xGoRight,
            bool yDiffMax
            )
    {
        if (path is null || (ProcessChartMode & ProcessMode.HotRegions) != ProcessMode.HotRegions)
            return;

        var bounds = path.GetBounds();
        var pathClone = (GraphicsPath)path.Clone();
        // To correct HitTest between points we simply divide the rectangle in half along the axis where the greatest change occurred.
        HotRegion region = new HotRegion
        {
            SeriesName = seriesName,
            PointIndex = yDiffMax ? (yGoDown ? pointIndex - 1 : pointIndex) : (xGoRight ? pointIndex - 1 : pointIndex),
            Type = ChartElementType.DataPoint,
            Path = pathClone,
            BoundingRectangle = new RectangleF(bounds.X, bounds.Y, !yDiffMax ? bounds.Width / 2 : bounds.Width, !yDiffMax ? bounds.Height : bounds.Height / 2),
            RelativeCoordinates = false
        };
        List.Add(region);

        region = new HotRegion
        {
            SeriesName = seriesName,
            PointIndex = yDiffMax ? (yGoDown ? pointIndex : pointIndex - 1) : (xGoRight ? pointIndex : pointIndex - 1),
            Type = ChartElementType.DataPoint,
            Path = pathClone,
            BoundingRectangle = new RectangleF(!yDiffMax ? bounds.X + bounds.Width / 2 : bounds.X, !yDiffMax ? bounds.Y : bounds.Y + bounds.Height / 2,
                !yDiffMax ? bounds.Width / 2 : bounds.Width, !yDiffMax ? bounds.Height : bounds.Height / 2),
            RelativeCoordinates = false
        };
        List.Add(region);
    }

    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="graph">Chart Graphics Object</param>
    /// <param name="x">x coordinate.</param>
    /// <param name="y">y coordinate.</param>
    /// <param name="radius">The radius.</param>
    /// <param name="point">Selected data point</param>
    /// <param name="seriesName">Data Series</param>
    /// <param name="pointIndex">Index of an Data Point in the series</param>
    internal void AddHotRegion(ChartGraphics graph, float x, float y, float radius, DataPoint point, string seriesName, int pointIndex)
    {
        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            PointF circleCenter = graph.GetAbsolutePoint(new PointF(x, y));
            SizeF circleRadius = graph.GetAbsoluteSize(new SizeF(radius, radius));
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(
                circleCenter.X - circleRadius.Width,
                circleCenter.Y - circleRadius.Width,
                2 * circleRadius.Width,
                2 * circleRadius.Width);

            HotRegion region = new HotRegion
            {
                BoundingRectangle = path.GetBounds(),
                SeriesName = seriesName,
                Type = ChartElementType.DataPoint,
                PointIndex = pointIndex,
                Path = path,
                RelativeCoordinates = false
            };

            // Use index of the original data point
            if (point?.IsCustomPropertySet("OriginalPointIndex") == true)
                region.PointIndex = int.Parse(point["OriginalPointIndex"], CultureInfo.InvariantCulture);

            List.Add(region);
        }
    }


    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="rectArea">Hot Region rectangle</param>
    /// <param name="selectedObject">Object which present hot region</param>
    /// <param name="type">AxisName of the object which present hot region</param>
    /// <param name="series">Selected series</param>
    internal void AddHotRegion(RectangleF rectArea, object selectedObject, ChartElementType type, string series)
    {

        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                BoundingRectangle = rectArea,
                RelativeCoordinates = true,
                Type = type,
                SelectedObject = selectedObject
            };

            if (!string.IsNullOrEmpty(series))
                region.SeriesName = series;

            List.Add(region);
        }
    }



    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="rectArea">Hot Region rectangle</param>
    /// <param name="selectedObject">Object which present hot region</param>
    /// <param name="selectedSubObject">Sub-Object which present hot region</param>
    /// <param name="type">AxisName of the object which present hot region</param>
    /// <param name="series">Selected series</param>
    internal void AddHotRegion(
        RectangleF rectArea,
        object selectedObject,
        object selectedSubObject,
        ChartElementType type,
        string series)
    {

        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                BoundingRectangle = rectArea,
                RelativeCoordinates = true,
                Type = type,
                SelectedObject = selectedObject,
                SelectedSubObject = selectedSubObject
            };

            if (!string.IsNullOrEmpty(series))
                region.SeriesName = series;

            List.Add(region);
        }
    }

    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="rectArea">Hot Region rectangle</param>
    /// <param name="selectedObject">Object which present hot region</param>
    /// <param name="type">AxisName of the object which present hot region</param>
    /// <param name="relativeCoordinates">Coordinates for rectangle are relative</param>
    internal void AddHotRegion(RectangleF rectArea, object selectedObject, ChartElementType type, bool relativeCoordinates)
    {
        this.AddHotRegion(rectArea, selectedObject, type, relativeCoordinates, false);
    }


    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="rectArea">Hot Region rectangle</param>
    /// <param name="selectedObject">Object which present hot region</param>
    /// <param name="type">AxisName of the object which present hot region</param>
    /// <param name="relativeCoordinates">Coordinates for rectangle are relative</param>
    /// <param name="insertAtBeginning">Insert the hot region at the beginning of the collection</param>
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
    internal void AddHotRegion(RectangleF rectArea, object selectedObject, ChartElementType type, bool relativeCoordinates, bool insertAtBeginning)
    {
        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                BoundingRectangle = rectArea,
                RelativeCoordinates = relativeCoordinates,
                Type = type,
                SelectedObject = selectedObject
            };

            if (insertAtBeginning)
                List.Insert(List.Count - 1, region);
            else
                List.Add(region);
        }
    }

    /// <summary>
    /// Add Hot region to the collection.
    /// </summary>
    /// <param name="path">Graphics path</param>
    /// <param name="relativePath">Used relative coordinates for graphics path.</param>
    /// <param name="type">Type of the object which present hot region</param>
    /// <param name="selectedObject">Object which present hot region</param>
    internal void AddHotRegion(
        GraphicsPath path,
        bool relativePath,
        ChartElementType type,
        object selectedObject)
    {
        if ((ProcessChartMode & ProcessMode.HotRegions) == ProcessMode.HotRegions)
        {
            HotRegion region = new HotRegion
            {
                SelectedObject = selectedObject,
                Type = type,
                Path = (GraphicsPath)path.Clone(),
                BoundingRectangle = path.GetBounds(),
                RelativeCoordinates = relativePath
            };

            List.Add(region);
        }
    }

    ///// <summary>
    ///// This method search for position in Map Areas which is the first 
    ///// position after Custom areas.
    ///// </summary>
    ///// <returns>Insert Index</returns>
    //internal int FindInsertIndex()
    //{
    //    int insertIndex = 0;

    //    return insertIndex;
    //}

    /// <summary>
    /// Clears this instance.
    /// </summary>
    public void Clear()
    {
        foreach (HotRegion hotRegion in this.List)
            hotRegion.Dispose();

        this.List.Clear();
    }

    #endregion // Methods

    #region IDisposable members
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.List != null)
            {
                foreach (HotRegion hotRegion in this.List)
                    hotRegion.Dispose();

                this.List.Clear();
            }
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
/// The HitTestResult class contains the result of the hit test function.
/// </summary>
public class HitTestResult
{
    #region Fields

    // Private members
    private ChartArea _chartArea;
    private object _subObject;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data series object.
    /// </summary>
    public Series Series { get; set; }

    /// <summary>
    /// Gets or sets the data point index.
    /// </summary>
    public int PointIndex { get; set; } = -1;

    /// <summary>
    /// Gets or sets the chart area object.
    /// </summary>
    public ChartArea ChartArea
    {
        get => _chartArea;
        set => _chartArea = value;
    }

    /// <summary>
    /// Gets or sets the axis object.
    /// </summary>
    public Axis Axis { get; set; }

    /// <summary>
    /// Gets or sets the chart element type.
    /// </summary>
    public ChartElementType ChartElementType { get; set; } = ChartElementType.Nothing;

    /// <summary>
    ///  Gets or sets the selected object.
    /// </summary>
    public object Object { get; set; }



    /// <summary>
    ///  Gets or sets the selected sub object.
    /// </summary>
    public object SubObject
    {
        get => _subObject;
        set => _subObject = value;
    }

    #endregion
}


/// <summary>
/// This class represents an array of marker points and 
/// the outline path used for visual object selection in the chart.
/// </summary>
/// <remarks>
/// <see cref="OutlinePath"/> may be null for complex objects or objects with two points or fewer.
/// </remarks>
public class ChartElementOutline : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChartElementOutline"/> class.
    /// </summary>
    internal ChartElementOutline()
    {
        this.Markers = new ReadOnlyCollection<PointF>(Array.Empty<PointF>());
    }

    /// <summary>
    /// Gets the markers.  
    /// </summary>
    /// <value>The markers.</value>
    public ReadOnlyCollection<PointF> Markers { get; internal set; }

    /// <summary>
    /// Gets or sets the outline path. The result could be null for complex objects and objects with two points or fewer.
    /// </summary>
    /// <value>The outline path.</value>
    public GraphicsPath OutlinePath { get; internal set; }


    #region IDisposable Members

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (this.OutlinePath != null)
            {
                this.OutlinePath.Dispose();
                this.OutlinePath = null;
            }

            this.Markers = null;
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}


/// <summary>
/// This class contains methods used for Windows Forms selection.
/// </summary>
internal sealed class Selection : IServiceProvider
, IDisposable
{
    #region Fields

    /// <summary>
    /// The chart service container
    /// </summary>
    private readonly IServiceContainer _service;

    /// <summary>
    /// Stores the tooltip of the control.
    /// </summary>
    private ToolTip _toolTip = new ToolTip();

    /// <summary>
    /// Used by the tooltip - stores the time when the tooltip is activated.
    /// </summary>
    private DateTime _toolTipActivationTime = DateTime.Now;

    /// <summary>
    /// Stores the last mouse move X and Y coordinates, so we can ignore false calls to
    /// OnMouseMove generated my the tooltip.
    /// </summary>
    private Point _lastMouseMove = new Point(int.MinValue, int.MinValue);


    // ToolTips enabled or disabled from series or legend
    private bool _toolTipsEnabled;

    // Tool tips enabled flag checked
    internal bool enabledChecked;

    #endregion

    #region Constructors 

    /// <summary>
    /// Initializes a new instance of the <see cref="Selection"/> class.
    /// </summary>
    /// <param name="service">The service.</param>
    internal Selection(IServiceContainer service)
    {
        this._service = service;
        this._chartControl = this.ChartControl;

        // Set up the tooltip
        this._toolTip.Active = true;
        this._toolTip.AutoPopDelay = 30000; // maximum delay possible
        this._toolTip.InitialDelay = 500;
        this._toolTip.ReshowDelay = 50;
        this._toolTip.ShowAlways = true;
        this._toolTip.Active = false;

    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_toolTip != null)
            {
                _toolTip.Dispose();
                _toolTip = null;
            }
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

    #endregion //Constructors

    #region Properties

    private Chart _chartControl;
    /// <summary>
    /// Returns the attached chart control
    /// </summary>
    internal Chart ChartControl
    {
        get
        {
            if (this._chartControl == null)
            {
                if (this.ChartPicture != null)
                {
                    this._chartControl = this.ChartPicture.Chart;
                }
            }

            return this._chartControl;
        }
    }

    private ChartPicture _chartPicture;
    /// <summary>
    /// Returns the attached ChartPicture
    /// </summary>
    internal ChartPicture ChartPicture
    {
        get
        {
            if (this._chartPicture == null)
            {
                this._chartPicture = ((IServiceProvider)this).GetService(typeof(ChartImage)) as ChartPicture;
                this._chartPicture ??= ((IServiceProvider)this).GetService(typeof(ChartPicture)) as ChartPicture;
            }

            return this._chartPicture;
        }
    }

    private Data.DataManager _dataManager;
    /// <summary>
    /// Gets the chart data manager ( for series access )
    /// </summary>
    internal Data.DataManager DataManager
    {
        get
        {
            this._dataManager ??= ((IServiceProvider)this).GetService(typeof(Data.DataManager)) as Data.DataManager;
            return this._dataManager;
        }
    }

    /// <summary>
    /// Gets the chart ChartGraphics
    /// </summary>
    internal ChartGraphics Graph
    {
        get
        {
            if (this.ChartPicture != null)
            {
                return this.ChartPicture.Common.graph;
            }

            return null;
        }
    }

    #endregion //Private Properties

    #region Methods

    #region Tooltips
    /// <summary>
    /// Checks if tooltips are enabled
    /// </summary>
    /// <returns>true if tooltips enabled</returns>
    private bool IsToolTipsEnabled()
    {
        // Enabled checked. Don’t check every time series 
        // and data points for tooltips.
        if (enabledChecked)
        {
            return _toolTipsEnabled;
        }

        enabledChecked = true;

        // Annotations loop
        foreach (Annotation annotation in _chartControl.Annotations)
        {
            // ToolTip empty
            if (annotation.ToolTip.Length > 0)
            {
                // ToolTips enabled
                _toolTipsEnabled = true;
                return true;
            }
        }

        // Data series loop
        foreach (Series series in _chartControl.Series)
        {
            // Check series tooltips
            if (series.ToolTip.Length > 0 ||
                series.LegendToolTip.Length > 0 ||
                series.LabelToolTip.Length > 0)
            {
                // ToolTips enabled
                _toolTipsEnabled = true;
                return true;
            }

            // Check if custom properties (Pie collected slice) that create tooltips are used
            if (series.IsCustomPropertySet(Utilities.CustomPropertyName.CollectedToolTip))
            {
                // ToolTips enabled
                _toolTipsEnabled = true;
                return true;
            }

            // Check point tooltips only for "non-Fast" chart types
            if (!series.IsFastChartType())
            {
                // Data point loop
                foreach (DataPoint point in series.Points)
                {
                    // ToolTip empty
                    if (point.ToolTip.Length > 0 ||
                        point.LegendToolTip.Length > 0 ||
                        point.LabelToolTip.Length > 0)
                    {
                        // ToolTips enabled
                        _toolTipsEnabled = true;
                        return true;
                    }
                }
            }
        }

        // Legend items loop
        foreach (Legend legend in _chartControl.Legends)
        {
            // Check custom legend items
            foreach (LegendItem legendItem in legend.CustomItems)
            {
                // ToolTip empty
                if (legendItem.ToolTip.Length > 0)
                {
                    _toolTipsEnabled = true;
                    return true;
                }


                // Check all custom cells in the legend item
                foreach (LegendCell legendCell in legendItem.Cells)
                {
                    if (legendCell.ToolTip.Length > 0)
                    {
                        _toolTipsEnabled = true;
                        return true;
                    }
                }

            }


            // Iterate through legend columns
            foreach (LegendCellColumn legendColumn in legend.CellColumns)
            {
                if (legendColumn.ToolTip.Length > 0)
                {
                    _toolTipsEnabled = true;
                    return true;
                }
            }

        }

        // Title items loop
        foreach (Title title in _chartControl.Titles)
        {
            // ToolTip empty
            if (title.ToolTip.Length > 0)
            {
                _toolTipsEnabled = true;
                return true;
            }
        }

        // Chart areas loop
        foreach (ChartArea area in _chartControl.ChartAreas)
        {

            // Check if chart area is visible
            if (area.Visible)

            {
                // Axis loop
                foreach (Axis axis in area.Axes)
                {

                    // Check ToolTip
                    if (axis.ToolTip.Length > 0)
                    {
                        _toolTipsEnabled = true;
                        return true;
                    }


                    // Strip lines loop
                    foreach (StripLine stripLine in axis.StripLines)
                    {
                        // Check ToolTip
                        if (stripLine.ToolTip.Length > 0)
                        {
                            _toolTipsEnabled = true;
                            return true;
                        }
                    }
                    // Check custom labels
                    foreach (CustomLabel customLabel in axis.CustomLabels)
                    {
                        if (customLabel.ToolTip.Length > 0)
                        {
                            _toolTipsEnabled = true;
                            return true;
                        }
                    }
                }
            }
        }

        // ToolTips disabled
        _toolTipsEnabled = false;
        return false;
    }

    [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
        Justification = "Too large of a code change to justify making this change")]
    internal string EvaluateToolTip(System.Windows.Forms.MouseEventArgs e)
    {
        object obj;
        object subObj;
        ChartElementType type;
        int dataPointIndex;
        string seriesName;
        string toolTipText = " ";

        HitTestResult hitTest = this.HitTest(e.X, e.Y, true);

        type = hitTest.ChartElementType;
        dataPointIndex = hitTest.PointIndex;
        seriesName = hitTest.Series != null ? hitTest.Series.Name : string.Empty;
        obj = hitTest.Object;
        subObj = hitTest.SubObject;


        // Get tooltips from data points
        if (type == ChartElementType.DataPoint)
        {
            if (_chartControl.Series.IndexOf(seriesName) >= 0 &&
                dataPointIndex >= 0 &&
                dataPointIndex < _chartControl.Series[seriesName].Points.Count)
            {
                // Take tool tip from data point
                toolTipText = _chartControl.Series[seriesName].Points[dataPointIndex].ReplaceKeywords(_chartControl.Series[seriesName].Points[dataPointIndex].ToolTip);
            }
            else
            {
                if (obj is DataPoint dataPoint)
                {
                    // Take tool tip from data point
                    toolTipText = dataPoint.ReplaceKeywords(dataPoint.ToolTip);
                }
            }
        }



        // Get tooltips from data points
        if (type == ChartElementType.DataPointLabel)
        {
            if (_chartControl.Series.IndexOf(seriesName) >= 0 &&
                dataPointIndex >= 0 &&
                dataPointIndex < _chartControl.Series[seriesName].Points.Count)
            {
                // Take tool tip from data point
                toolTipText = _chartControl.Series[seriesName].Points[dataPointIndex].ReplaceKeywords(_chartControl.Series[seriesName].Points[dataPointIndex].LabelToolTip);
            }
        }


        // Get tooltips from custom label
        if (type == ChartElementType.AxisLabels &&
            obj is CustomLabel)
        {
            toolTipText = ((CustomLabel)obj).ToolTip;
        }




        // Get tooltips from data points
        else if (type == ChartElementType.Annotation && obj != null && obj is Annotation)
        {
            // Take tool tip from data point
            toolTipText = ((Annotation)obj).ReplaceKeywords(((Annotation)obj).ToolTip);

        }
        // Get tooltips from axis
        else if (type == ChartElementType.Axis && obj != null && obj is Axis)
        {
            // Take tool tip from strip line
            toolTipText = ((Axis)obj).ToolTip;
        }

        // Get tooltips from strip lines
        else if (type == ChartElementType.StripLines && obj != null && obj is StripLine)
        {
            // Take tool tip from strip line
            toolTipText = ((StripLine)obj).ToolTip;

        }
        // Get tooltips from data points
        else if (type == ChartElementType.Title && obj != null && obj is Title)
        {
            // Take tool tip from data point
            toolTipText = ((Title)obj).ToolTip;

        } // Get tooltips for legend items

        // Get tooltips for legend items
        else if (type == ChartElementType.LegendItem)
        {
            // Take tool tip from legend item
            toolTipText = ((LegendItem)obj).ToolTip;


            // Check if cell has it's own tooltip
            if (subObj is LegendCell legendCell && legendCell.ToolTip.Length > 0)
            {
                toolTipText = legendCell.ToolTip;
            }


            // Check if series is associated with legend item
            if (toolTipText.Length == 0 &&
                seriesName.Length > 0 &&
                _chartControl.Series.IndexOf(seriesName) >= 0)
            {
                // Take tool tip from data point
                if (dataPointIndex == -1)
                {
                    if (seriesName.Length > 0)
                    {
                        // Take tool tip from series
                        toolTipText = _chartControl.Series[seriesName].ReplaceKeywords(_chartControl.Series[seriesName].LegendToolTip);
                    }
                }
                else
                {
                    if (dataPointIndex >= 0 &&
                        dataPointIndex < _chartControl.Series[seriesName].Points.Count)
                    {
                        // Take tool tip from data point
                        toolTipText = _chartControl.Series[seriesName].Points[dataPointIndex].ReplaceKeywords(_chartControl.Series[seriesName].Points[dataPointIndex].LegendToolTip);
                    }
                }
            }
        }

        // Set event arguments
        ToolTipEventArgs args = new ToolTipEventArgs(e.X, e.Y, toolTipText, hitTest);

        // Event
        _chartControl.OnGetToolTipText(args);

        return args.Text.Trim();

    }


    /// <summary>
    /// Mouse move event handler.
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Arguments</param>
    internal void Selection_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
    {

        // Ignore false calls to OnMouseMove caused by the tootip control.
        if (e.X == this._lastMouseMove.X && e.Y == this._lastMouseMove.Y)
        {
            return;
        }
        else
        {
            this._lastMouseMove.X = e.X;
            this._lastMouseMove.Y = e.Y;
        }

        // Event is not active and tooltip properties are nor set.
        if (!IsToolTipsEnabled() && !_chartControl.IsToolTipEventUsed())
        {
            return;
        }

        string newToolTipText = this.EvaluateToolTip(e);

        if (!string.IsNullOrEmpty(newToolTipText))
        {
            string oldToolTipText = this._toolTip.GetToolTip(this._chartControl);
            TimeSpan timeSpan = DateTime.Now.Subtract(this._toolTipActivationTime);
            if (oldToolTipText != newToolTipText || timeSpan.Milliseconds > 600)
            {
                // Activate the tooltip
                this._toolTip.Active = false;
                this._toolTip.SetToolTip(this._chartControl, newToolTipText);
                this._toolTip.Active = true;
                this._toolTipActivationTime = DateTime.Now;
            }
        }
        else
        {
            // We do not have a tooltip, so deactivate it
            this._toolTip.Active = false;
            this._toolTip.SetToolTip(this._chartControl, string.Empty);
        }
    }


    #endregion //Tooltips

    #region HitTest


    /// <summary>
    /// Call this method to determine the  chart element,
    /// if any, that is located at a point defined by the given X and Y 
    /// coordinates.
    /// <seealso cref="HitTestResult"/></summary>
    /// <param name="x">The X coordinate for the point in question.
    /// Often obtained from a parameter in an event
    /// (e.g. the X parameter value in the MouseDown event).</param>
    /// <param name="y">The Y coordinate for the point in question.
    /// Often obtained from a parameter in an event
    /// (e.g. the Y parameter value in the MouseDown event).</param>
    /// <param name="requestedElementTypes">
    /// An array of type which specify the types                  
    /// to test for, on order to filter the result. If omitted checking for                 
    /// elementTypes will be ignored and all kind of elementTypes will be 
    /// valid.
    ///  </param>
    /// <param name="ignoreTransparent">Indicates that transparent 
    /// elements should be ignored.</param>
    /// <returns>
    /// A array of <see cref="HitTestResult"/> objects,
    /// which provides information concerning the  chart element
    /// (if any) that is at the specified location. Result contains at least
    /// one element, which could be ChartElementType.Nothing. 
    /// The objects in the result are sorted in from top to bottom of 
    /// different layers of control. </returns>
    /// <remarks>Call this method to determine the  gauge element
    /// (if any) that is located at a specified point. Often this method is used in
    /// some mouse-related event (e.g. MouseDown)
    /// to determine what  gauge element the end-user clicked on.
    /// The X and Y mouse coordinates obtained from the
    /// event parameters are then used for the X and Y parameter              
    /// values of this method call.   The returned 
    /// <see cref="HitTestResult"/> object's properties
    /// can then be used to determine what  chart element was clicked on,
    /// and also provides a reference to the actual object selected (if 
    /// any).</remarks>
    internal List<HitTestResult> HitTest(int x, int y, bool ignoreTransparent, params ChartElementType[] requestedElementTypes)
    {
        List<HitTestResult> result = new List<HitTestResult>();
        var regionList = this.ChartPicture.Common.HotRegionsList.List;

        if (regionList.Count == 0)
        {
            this.ChartPicture.PaintOffScreen();
        }

        float newX;
        float newY;
        float relativeX;
        float relativeY;
        RectangleF newMouseRect;

        // Find mouse position in relative and absolute coordinates
        RectangleF mouseRect = new RectangleF(x - 1, y - 1, 2, 2);
        relativeX = this.Graph.GetRelativePoint(new PointF(x, y)).X;
        relativeY = this.Graph.GetRelativePoint(new PointF(x, y)).Y;
        RectangleF relativeMouseRect = this.Graph.GetRelativeRectangle(mouseRect);

        // Try to pass through series object in design time.
        // The series usually contain auto-generated points with short lifetime - during painting;
        // This hit test result will be used in VS2005 design time click.
        for (int index = regionList.Count - 1; index >= 0; index--)
        {
            HotRegion region = regionList[index];
            if (requestedElementTypes.Length > 0 && !requestedElementTypes.Contains(region.Type))
                continue;

            // Change coordinates if relative path is used
            if (region.RelativeCoordinates)
            {
                newX = relativeX;
                newY = relativeY;
                newMouseRect = relativeMouseRect;
            }
            else
            {
                newX = x;
                newY = y;
                newMouseRect = mouseRect;
            }

            // Check if series name and point index are valid
            if (region.SeriesName.Length > 0 &&
               (this.ChartControl.Series.IndexOf(region.SeriesName) < 0 || region.PointIndex >= this.ChartControl.Series[region.SeriesName].Points.Count))
            {
                continue;
            }

            // Check if transparent chart elements should be ignored
            if (ignoreTransparent && IsElementTransparent(region))
            {
                continue;
            }
            // Check intersection with bounding rectangle
            if (region.BoundingRectangle.IntersectsWith(newMouseRect))
            {
                bool pointVisible = false;
                if (region.Path is not null)
                {
                    // If there is more then one graphical path split them and create 
                    // image maps for every graphical path separately.
                    using GraphicsPathIterator iterator = new GraphicsPathIterator(region.Path);
                    // There is more then one path.
                    if (iterator.SubpathCount > 1)
                    {
                        using GraphicsPath subPath = new GraphicsPath();
                        while (iterator.NextMarker(subPath) > 0)
                        {
                            if (subPath.IsVisible(newX, newY))
                            {
                                pointVisible = true;
                                break;
                            }

                            subPath.Reset();
                        }
                    }
                    else if (region.Path.IsVisible(newX, newY)) // There is only one path
                    {
                        pointVisible = true;
                    }
                }
                else
                {
                    // Point is inside bounding rectangle and path is not specified
                    pointVisible = true;
                }

                // Check if point is inside the hot region
                if (pointVisible)
                {
                    HitTestResult hitTestToAdd = this.GetHitTestResult(
                        region.SeriesName,
                        region.PointIndex,
                        region.Type,
                        region.SelectedObject,
                        region.SelectedSubObject
                    );

                    int elementIndex = result.FindIndex(
                        delegate (HitTestResult test)
                        {
                            return (test.ChartElementType == hitTestToAdd.ChartElementType) &&
                                (test.Object == hitTestToAdd.Object) &&
                                (test.SubObject == hitTestToAdd.SubObject) &&
                                (test.Series == hitTestToAdd.Series) &&
                                (test.PointIndex == hitTestToAdd.PointIndex);
                        });

                    if (elementIndex == -1)
                    {
                        result.Add(hitTestToAdd);
                    }
                }
            }
        }

        if (result.Count == 0)
        {
            result.Add(this.GetHitTestResult(string.Empty, 0, ChartElementType.Nothing, null, null));
        }

        return result;
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult objects.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Hit test result object</returns>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        Justification = "X and Y are cartesian coordinates and well understood")]
    internal HitTestResult HitTest(int x, int y)
    {
        return this.HitTest(x, y, false, Array.Empty<ChartElementType>())[0];
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult object.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="ignoreTransparent">Indicates that transparent elements should be ignored.</param>
    /// <returns>Hit test result object</returns>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        Justification = "X and Y are cartesian coordinates and well understood")]
    public HitTestResult HitTest(int x, int y, bool ignoreTransparent)
    {
        return this.HitTest(x, y, ignoreTransparent, Array.Empty<ChartElementType>())[0];
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult object.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="requestedElement">Only this chart element will be hit tested.</param>
    /// <returns>Hit test result object</returns>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        Justification = "X and Y are cartesian coordinates and well understood")]
    public HitTestResult HitTest(int x, int y, ChartElementType requestedElement)
    {
        return this.HitTest(x, y, false, requestedElement)[0];
    }

    /// <summary>
    /// Checks if chart element associated with hot region has transparent background.
    /// </summary>
    /// <param name="region">Element hot region.</param>
    /// <returns>True if chart element is transparent.</returns>
    private bool IsElementTransparent(HotRegion region)
    {
        bool isTransparent = false;

        if (region.Type == ChartElementType.DataPoint)
        {
            if (this.ChartControl != null)
            {
                DataPoint dataPoint = region.SelectedObject as DataPoint;
                if (region.SeriesName.Length > 0)
                {
                    dataPoint = this.ChartControl.Series[region.SeriesName].Points[region.PointIndex];
                }

                if (dataPoint != null && dataPoint.Color == Color.Transparent)
                {
                    isTransparent = true;
                }
            }
        }
        else if (region.SelectedObject is Axis)
        {
            if (((Axis)region.SelectedObject).LineColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is ChartArea)
        {
            if (((ChartArea)region.SelectedObject).BackColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is Legend)
        {
            if (((Legend)region.SelectedObject).BackColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is Grid)
        {
            if (((Grid)region.SelectedObject).LineColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is StripLine)
        {
            if (((StripLine)region.SelectedObject).BackColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is TickMark)
        {
            if (((TickMark)region.SelectedObject).LineColor == Color.Transparent)
            {
                isTransparent = true;
            }
        }
        else if (region.SelectedObject is Title title)
        {
            if ((title.Text.Length == 0 || title.ForeColor == Color.Transparent) &&
                (title.BackColor == Color.Transparent || title.BackColor.IsEmpty))
            {
                isTransparent = true;
            }
        }

        return isTransparent;
    }

    /// <summary>
    /// Returns Hit Test Result object
    /// </summary>
    /// <param name="seriesName">Data series Name</param>
    /// <param name="pointIndex">Data point index</param>
    /// <param name="type">Selected Chart element type</param>
    /// <param name="obj">Selected object</param>
    /// <param name="subObject">Selected sub object</param>
    /// <returns>Hit test result object</returns>
    [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
        Justification = "Too large of a code change to justify making this change")]
    internal HitTestResult GetHitTestResult(
        string seriesName,
        int pointIndex,
        ChartElementType type,
        object obj,
        object subObject)
    {
        HitTestResult result = new HitTestResult();
        Chart chart = this.ChartControl;
        // If data point is selected convert series 
        // name to series object.
        if (seriesName.Length > 0)
        {
            result.Series = chart.Series[seriesName];
        }

        // Selected Object
        result.Object = obj;

        result.SubObject = subObject;

        result.PointIndex = pointIndex;
        result.ChartElementType = type;

        AxisScrollBar scrollBar;

        switch (type)
        {
            case ChartElementType.Axis:
                Axis axis = (Axis)obj;
                result.Axis = axis;
                if (axis != null)
                {
                    result.ChartArea = axis.ChartArea;
                }

                break;
            case ChartElementType.DataPoint:
                {
                    if (chart.Series.IndexOf(seriesName) >= 0 &&
                        pointIndex < chart.Series[seriesName].Points.Count)
                    {
                        DataPoint dataPoint = chart.Series[seriesName].Points[pointIndex];
                        result.Axis = null;
                        result.ChartArea = chart.ChartAreas[dataPoint.series.ChartArea];
                        result.Object = dataPoint;
                    }

                    break;
                }

            case ChartElementType.DataPointLabel:
                {
                    if (chart.Series.IndexOf(seriesName) >= 0 &&
                        pointIndex < chart.Series[seriesName].Points.Count)
                    {
                        DataPoint dataPoint = chart.Series[seriesName].Points[pointIndex];
                        result.Axis = null;
                        result.ChartArea = chart.ChartAreas[dataPoint.series.ChartArea];
                        result.Object = dataPoint;
                    }

                    break;
                }

            case ChartElementType.Gridlines:
                Grid gridLines = (Grid)obj;
                result.Axis = gridLines.Axis;
                if (gridLines.Axis != null)
                {
                    result.ChartArea = gridLines.Axis.ChartArea;
                }

                break;
            case ChartElementType.LegendArea:
                result.Axis = null;
                result.ChartArea = null;
                break;
            case ChartElementType.LegendItem:
                result.PointIndex = ((LegendItem)obj).SeriesPointIndex;
                result.Axis = null;
                result.ChartArea = null;
                break;
            case ChartElementType.PlottingArea:
                ChartArea area = (ChartArea)obj;
                result.Axis = null;
                result.ChartArea = area;
                break;
            case ChartElementType.StripLines:
                StripLine stripLines = (StripLine)obj;
                result.Axis = stripLines.Axis;
                if (stripLines.Axis != null)
                {
                    result.ChartArea = stripLines.Axis.ChartArea;
                }

                break;
            case ChartElementType.TickMarks:
                TickMark tickMarks = (TickMark)obj;
                result.Axis = tickMarks.Axis;
                if (tickMarks.Axis != null)
                {
                    result.ChartArea = tickMarks.Axis.ChartArea;
                }

                break;
            case ChartElementType.Title:
                result.Axis = null;
                result.ChartArea = null;
                break;
            case ChartElementType.AxisLabels:
            case ChartElementType.AxisLabelImage:
                if (obj is CustomLabel label)
                {
                    result.Axis = label.Axis;
                    result.ChartArea = label.Axis?.ChartArea;
                }

                break;
            case ChartElementType.AxisTitle:
                if (obj is Axis)
                {
                    result.Axis = (Axis)obj;
                    result.ChartArea = result.Axis.ChartArea;
                }

                break;
            case ChartElementType.ScrollBarLargeDecrement:
                scrollBar = (AxisScrollBar)obj;
                result.Axis = scrollBar.axis;
                if (scrollBar.axis != null)
                {
                    result.ChartArea = scrollBar.axis.ChartArea;
                }

                break;
            case ChartElementType.ScrollBarLargeIncrement:
                scrollBar = (AxisScrollBar)obj;
                result.Axis = scrollBar.axis;
                if (scrollBar.axis != null)
                {
                    result.ChartArea = scrollBar.axis.ChartArea;
                }

                break;
            case ChartElementType.ScrollBarSmallDecrement:
                scrollBar = (AxisScrollBar)obj;
                result.Axis = scrollBar.axis;
                if (scrollBar.axis != null)
                {
                    result.ChartArea = scrollBar.axis.ChartArea;
                }

                break;
            case ChartElementType.ScrollBarSmallIncrement:
                scrollBar = (AxisScrollBar)obj;
                result.Axis = scrollBar.axis;
                if (scrollBar.axis != null)
                {
                    result.ChartArea = scrollBar.axis.ChartArea;
                }

                break;
            case ChartElementType.ScrollBarThumbTracker:
                scrollBar = (AxisScrollBar)obj;
                result.Axis = scrollBar.axis;
                if (scrollBar.axis != null)
                {
                    result.ChartArea = scrollBar.axis.ChartArea;
                }

                break;

            case ChartElementType.Annotation:
                result.Axis = null;
                result.ChartArea = null;
                break;
        }

        return result;
    }

    #endregion //HitTest

    #region Outline

    /// <summary>
    /// Gets the chart element outline.
    /// </summary>
    /// <param name="chartObject">The chart object.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns></returns>
    internal ChartElementOutline GetChartElementOutline(object chartObject, ChartElementType elementType)
    {
        // Check arguments
        if (chartObject == null)
            throw new ArgumentNullException(nameof(chartObject));

        // Get outline
        ChartElementOutline result = new ChartElementOutline();
        chartObject = this.GetAutoGeneratedObject(chartObject);
        List<PointF> list = this.GetMarkers(chartObject, elementType);
        result.Markers = list.AsReadOnly();
        result.OutlinePath = GetGraphicsPath(list, chartObject, elementType);
        return result;
    }

    #endregion //Outline

    #region Selection

    /// <summary>
    /// Gets the graphics path.
    /// </summary>
    /// <param name="markers">The markers.</param>
    /// <param name="chartObject">The chart object.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns></returns>
    private GraphicsPath GetGraphicsPath(IList markers, object chartObject, ChartElementType elementType)
    {
        bool chartArea3D = false;
        if (chartObject is ChartArea chartArea && elementType == ChartElementType.PlottingArea)
        {
            chartArea3D = IsArea3D(chartArea);
        }

        if (elementType != ChartElementType.DataPoint &&
            elementType != ChartElementType.Gridlines &&
            elementType != ChartElementType.StripLines &&
            elementType != ChartElementType.TickMarks &&
            !chartArea3D
            )
        {
            GraphicsPath path = new GraphicsPath();
            PointF[] points = new PointF[markers.Count];
            markers.CopyTo(points, 0);
            if (points.Length > 3)
            {
                if (elementType == ChartElementType.DataPointLabel)
                {
                    for (int i = 0; i < points.Length; i += 4)
                    {
                        RectangleF rect = RectangleF.FromLTRB(points[i].X, points[i].Y, points[i + 2].X, points[i + 2].Y);
                        path.Reset();
                        path.AddRectangle(Rectangle.Round(rect));
                    }
                }
                else
                {
                    if (points.Length == 4)
                    {
                        Point[] pointsAlligned = new Point[points.Length];
                        for (int i = 0; i < points.Length; i++)
                        {
                            pointsAlligned[i] = Point.Round(points[i]);
                        }

                        path.AddPolygon(pointsAlligned);
                    }
                    else
                    {
                        path.AddPolygon(points);
                    }
                }
            }

            return path;
        }

        return null;
    }

    private static int GetDataPointIndex(DataPoint dataPoint)
    {
        int pointIndex = -1;
        if (dataPoint != null && dataPoint.series != null)
        {
            pointIndex = dataPoint.series.Points.IndexOf(dataPoint);
            if (pointIndex == -1 && dataPoint.IsCustomPropertySet("OriginalPointIndex"))
            {
                if (!int.TryParse(dataPoint.GetCustomProperty("OriginalPointIndex"), out pointIndex))
                    return -1;
            }
        }

        return pointIndex;
    }

    /// <summary>
    /// Gets the auto generated object.
    /// </summary>
    /// <param name="chartObject">The chart object.</param>
    /// <returns></returns>
    private object GetAutoGeneratedObject(object chartObject)
    {
        if (chartObject is DataPoint dataPoint)
        {
            if (dataPoint.series != null)
            {
                string seriesName = dataPoint.series.Name;
                int pointIndex = dataPoint.series.Points.IndexOf(dataPoint);
                if (this.ChartControl.Series.IndexOf(seriesName) != -1)
                {
                    Series series = this.ChartControl.Series[seriesName];
                    if (series.Points.Contains(dataPoint))
                    {
                        return chartObject;
                    }

                    if (pointIndex >= 0)
                    {
                        if (series.Points.Count > pointIndex)
                        {
                            return series.Points[pointIndex];
                        }
                    }
                }
            }
        }

        if (chartObject is Series asSeries)
        {
            if (this.ChartControl.Series.Contains(asSeries))
            {
                return chartObject;
            }

            if (this.ChartControl.Series.IndexOf(asSeries.Name) != -1)
            {
                return this.ChartControl.Series[asSeries.Name];
            }
        }

        return chartObject;
    }

    /// <summary>
    /// Gets the hot regions.
    /// </summary>
    /// <param name="cntxObj">The CNTX obj.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns></returns>
    private List<HotRegion> GetHotRegions(object cntxObj, ChartElementType elementType)
    {
        List<HotRegion> result = new List<HotRegion>();
        HotRegionsList hrList = this.ChartPicture.Common.HotRegionsList;
        string dataPointSeries = string.Empty;
        int dataPointIndex = -1;

        for (int i = hrList.List.Count - 1; i >= 0; i--)
        {
            HotRegion rgn = hrList.List[i];
            if (rgn.Type == elementType)
            {
                switch (rgn.Type)
                {
                    case ChartElementType.LegendItem:
                        if (cntxObj is LegendItem legendItem)
                        {
                            if (((LegendItem)rgn.SelectedObject).Name == legendItem.Name)
                            {
                                result.Add(rgn);
                            }
                        }

                        break;
                    case ChartElementType.AxisLabelImage:
                    case ChartElementType.AxisLabels:
                        CustomLabel label2 = rgn.SelectedObject as CustomLabel;
                        if (cntxObj is CustomLabel label1)
                        {
                            if (label1 != null && label2 != null)
                            {
                                if (label1.Axis == label2.Axis)
                                {
                                    if (label1.FromPosition == label2.FromPosition &&
                                        label1.ToPosition == label2.ToPosition &&
                                        label1.RowIndex == label2.RowIndex)
                                    {
                                        if (rgn.Path == null)
                                        {
                                            result.Add(rgn);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (cntxObj is Axis axis)
                            {
                                if (axis == label2.Axis)
                                {
                                    if (rgn.Path == null)
                                    {
                                        result.Add(rgn);
                                    }
                                }
                            }
                        }

                        break;
                    case ChartElementType.DataPointLabel:
                    case ChartElementType.DataPoint:
                        if (cntxObj is DataPoint dataPoint)
                        {
                            if (string.IsNullOrEmpty(dataPointSeries) || dataPointIndex == -1)
                            {
                                dataPointSeries = dataPoint.series.Name;
                                dataPointIndex = GetDataPointIndex(dataPoint);
                            }

                            if (rgn.PointIndex == dataPointIndex && rgn.SeriesName == dataPointSeries)
                            {
                                result.Add(rgn);
                            }
                        }
                        else
                        {
                            if (cntxObj is DataPointCollection dataPointCollection)
                            {
                                cntxObj = dataPointCollection.series;
                            }

                            if (cntxObj is Series series)
                            {
                                if (string.IsNullOrEmpty(dataPointSeries) || dataPointIndex == -1)
                                {
                                    dataPointSeries = series.Name;
                                }

                                if (rgn.SeriesName == dataPointSeries)
                                {
                                    result.Add(rgn);
                                }
                            }
                        }

                        break;

                    default:
                        if (rgn.SelectedObject == cntxObj)
                        {
                            result.Add(rgn);
                        }

                        break;
                }
            }
        }

        return result;
    }



    /// <summary>
    /// Gets the markers from regions.
    /// </summary>
    /// <param name="chartObject">The chart object.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns></returns>
    private List<PointF> GetMarkersFromRegions(object chartObject, ChartElementType elementType)
    {
        List<PointF> list = new List<PointF>();
        List<HotRegion> regions = this.GetHotRegions(chartObject, elementType);
        ChartGraphics graph = this.Graph;
        RectangleF rect;

        if (chartObject is Grid grid)
        {
            foreach (HotRegion rgn in regions)
            {
                if (!IsArea3D(grid.Axis.ChartArea))
                {
                    if (IsChartAreaCircular(grid.Axis.ChartArea))
                    {
                        using GraphicsPathIterator iterator = new GraphicsPathIterator(rgn.Path);
                        // There is more then one path.
                        if (iterator.SubpathCount > 1)
                        {
                            using GraphicsPath subPath = new GraphicsPath();
                            while (iterator.NextMarker(subPath) > 0)
                            {
                                rect = subPath.GetBounds();
                                list.Add(new PointF(rect.Left + rect.Width / 2, rect.Top));
                                list.Add(new PointF(rect.Right, rect.Top + rect.Height / 2));
                                list.Add(new PointF(rect.Right - rect.Width / 2, rect.Bottom));
                                list.Add(new PointF(rect.Left, rect.Bottom - rect.Height / 2));
                                subPath.Reset();
                            }
                        }
                    }
                    else
                    {
                        // 2D
                        rect = this.GetHotRegionRectangle(rgn, RectangleF.Empty, elementType);
                        if (grid != null)
                        {
                            if (grid.GetAxis().AxisPosition == AxisPosition.Bottom ||
                                grid.GetAxis().AxisPosition == AxisPosition.Top)
                            {
                                rect.Offset(rect.Width / 2, 0);
                                rect.Width = 0;
                            }
                            else
                            {
                                rect.Offset(0, rect.Height / 2);
                                rect.Height = 0;
                            }
                        }

                        list.AddRange(this.GetMarkers(rect, false));
                    }
                }
                else
                {   // 3D
                    PointF[] points = rgn.Path.PathPoints;
                    for (int i = 0; i < points.Length - 3; i += 4)
                    {   //Each grid line has a corresponding set of 4 points in the path
                        //One of the ends of a grid line is in the middle the line between points #0 and #3
                        //Another ends of a grid line is in the middle the line between points #1 and #2
                        //So we find those middles and put a marks to the ends of the grid line.
                        PointF middleP0P3 = new PointF((points[i].X + points[i + 3].X) / 2f, (points[i].Y + points[i + 3].Y) / 2f);
                        PointF middleP1P2 = new PointF((points[i + 1].X + points[i + 2].X) / 2f, (points[i + 1].Y + points[i + 2].Y) / 2f);
                        list.Add(graph.GetAbsolutePoint(middleP0P3));
                        list.Add(graph.GetAbsolutePoint(middleP1P2));
                    }
                }
            }

            return list;
        }

        if (chartObject is DataPoint dataPoint && elementType != ChartElementType.DataPointLabel)
        {
            Series series = dataPoint.series;
            if (this.ChartControl.ChartAreas.IndexOf(series.ChartArea) == -1)
            {
                return list;
            }

            ChartArea area = this.ChartControl.ChartAreas[series.ChartArea];
            PointF pp = this.Transform3D(area, dataPoint);
            if (!(float.IsNaN(pp.X) || float.IsNaN(pp.Y)))
            {
                list.Add(graph.GetAbsolutePoint(pp));
            }

            return list;
        }

        if (chartObject is Axis axis && elementType == ChartElementType.AxisTitle)
        {
            foreach (HotRegion rgn in regions)
            {
                if (!IsArea3D(axis.ChartArea))
                {   // 2D
                    rect = this.GetHotRegionRectangle(rgn, RectangleF.Empty, elementType);
                    list.AddRange(this.GetMarkers(rect, elementType));
                }
                else
                {   // 3D
                    PointF[] points = rgn.Path.PathPoints;
                    list.AddRange(points);
                }
            }

            return list;
        }

        if (chartObject is LegendItem)
        {
            rect = Rectangle.Empty;
            foreach (HotRegion rgn in regions)
            {
                rect = this.GetHotRegionRectangle(rgn, rect, elementType);
            }

            if (!rect.IsEmpty)
            {
                list.AddRange(this.GetMarkers(rect, elementType));
            }

            return list;
        }
        else if (chartObject is Annotation)
        {
            rect = Rectangle.Empty;
            foreach (HotRegion rgn in regions)
            {
                rect = this.GetHotRegionRectangle(rgn, rect, elementType);
            }

            if (!rect.IsEmpty)
            {
                list.AddRange(this.GetMarkers(rect, elementType));
            }

            return list;
        }

        foreach (HotRegion rgn in regions)
        {
            rect = this.GetHotRegionRectangle(rgn, RectangleF.Empty, elementType);
            list.AddRange(this.GetMarkers(rect, elementType));
        }

        return list;

    }


    /// <summary>
    /// Gets the markers.
    /// </summary>
    /// <param name="chartObject">The chart object.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns></returns>
    private List<PointF> GetMarkers(object chartObject, ChartElementType elementType)
    {
        if (chartObject is ChartArea chartArea)
        {
            return this.GetAreaMarkers(this.Graph, chartArea);
        }


        if (chartObject is Axis axis)
        {
            if (
                elementType == ChartElementType.AxisLabelImage ||
                elementType == ChartElementType.AxisLabels ||
                elementType == ChartElementType.AxisTitle
                )
            {
                return this.GetMarkersFromRegions(chartObject, elementType);
            }

            return this.GetAxisMarkers(this.Graph, axis);
        }

        if (chartObject is DataPoint)
        {
            return this.GetMarkersFromRegions(chartObject, elementType);
        }

        if (chartObject is Series series)
        {
            if (elementType == ChartElementType.DataPointLabel)
            {
                return this.GetMarkersFromRegions(chartObject, elementType);
            }

            return this.GetSeriesMarkers(series);
        }

        return this.GetMarkersFromRegions(chartObject, elementType);
    }

    /// <summary>
    /// Determines whether specified chart area is circular or not have axes. These chart areas contain pie, doughnut, polar, radar
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns>
    /// 	<c>true</c> if specified chart area is circular; otherwise, <c>false</c>.
    /// </returns>
    private bool IsChartAreaCircular(ChartArea area)
    {
        foreach (object o in area.ChartTypes)
        {
            ChartTypes.IChartType chartType = area.Common.ChartTypeRegistry.GetChartType(o.ToString());
            if (chartType != null && (chartType.CircularChartArea || !chartType.RequireAxes))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the chart area is in 3D mode.
    /// </summary>
    /// <param name="area">The area.</param>
    /// <returns>
    /// 	<c>true</c> if the chart area is in 3D mode; otherwise, <c>false</c>.
    /// </returns>
    private bool IsArea3D(ChartArea area)
    {
        return area.Area3DStyle.Enable3D && !this.IsChartAreaCircular(area) && area.matrix3D != null && area.matrix3D.IsInitialized();
    }

    /// <summary>
    /// Gets the series markers.
    /// </summary>
    /// <param name="series">The series.</param>
    /// <returns>List of PointF.</returns>
    private List<PointF> GetSeriesMarkers(Series series)
    {
        List<PointF> list = new List<PointF>();
        if (series != null)
        {
            string areaName = series.ChartArea;

            if (string.IsNullOrEmpty(areaName))
            {
                areaName = ChartPicture.ChartAreas.DefaultNameReference;
            }

            if (ChartPicture.ChartAreas.IndexOf(areaName) != -1 && series.Enabled)
            {

                ChartArea chartArea = ChartPicture.ChartAreas[areaName];

                if (ChartControl.Series.IndexOf(series.Name) != -1)
                {
                    series = ChartControl.Series[series.Name];
                }

                DataPointCollection points = series.Points;
                // in design mode we have usually fake points
                if (points.Count == 0)
                {
                    points = series.fakeDataPoints;
                }
                // transform points in 3D
                foreach (DataPoint point in points)
                {
                    PointF pp = this.Transform3D(chartArea, point);
                    if (float.IsNaN(pp.X) || float.IsNaN(pp.Y))
                    {
                        continue;
                    }

                    list.Add(this.Graph.GetAbsolutePoint(pp));
                }
            }
        }

        return list;
    }

    /// <summary>
    /// Gets the axis markers - list of points where markers are drawn.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="axis">The axis.</param>
    /// <returns>List of PointF.</returns>
    private List<PointF> GetAxisMarkers(ChartGraphics graph, Axis axis)
    {
        List<PointF> list = new List<PointF>();
        if (axis == null)
        {
            return list;
        }

        PointF first = PointF.Empty;
        PointF second = PointF.Empty;

        // Set the position of axis
        switch (axis.AxisPosition)
        {

            case AxisPosition.Left:

                first.X = (float)axis.GetAxisPosition();
                first.Y = axis.PlotAreaPosition.Y;
                second.X = (float)axis.GetAxisPosition();
                second.Y = axis.PlotAreaPosition.Bottom;
                first.X -= axis.labelSize + axis.markSize;
                break;

            case AxisPosition.Right:

                first.X = (float)axis.GetAxisPosition();
                first.Y = axis.PlotAreaPosition.Y;
                second.X = (float)axis.GetAxisPosition();
                second.Y = axis.PlotAreaPosition.Bottom;
                second.X += axis.labelSize + axis.markSize;
                break;

            case AxisPosition.Bottom:

                first.X = axis.PlotAreaPosition.X;
                first.Y = (float)axis.GetAxisPosition();
                second.X = axis.PlotAreaPosition.Right;
                second.Y = (float)axis.GetAxisPosition();
                second.Y += axis.labelSize + axis.markSize;
                break;

            case AxisPosition.Top:

                first.X = axis.PlotAreaPosition.X;
                first.Y = (float)axis.GetAxisPosition();
                second.X = axis.PlotAreaPosition.Right;
                second.Y = (float)axis.GetAxisPosition();
                first.Y -= axis.labelSize + axis.markSize;
                break;
        }

        // Update axis line position for circular area
        if (axis.ChartArea.chartAreaIsCurcular)
        {
            second.Y = axis.PlotAreaPosition.Y + axis.PlotAreaPosition.Height / 2f;
        }

        RectangleF rect1 = new RectangleF(first.X, first.Y, second.X - first.X, second.Y - first.Y);

        SizeF size = graph.GetRelativeSize(new SizeF(3, 3));
        if (axis.AxisPosition == AxisPosition.Top || axis.AxisPosition == AxisPosition.Bottom)
        {
            rect1.Inflate(2, size.Height);
        }
        else
        {
            rect1.Inflate(size.Width, 2);
        }

        IList list1 = this.GetMarkers(rect1, ChartElementType.Axis);
        ChartArea area = axis.ChartArea;
        if (this.IsArea3D(area))
        {
            float zPositon = axis.GetMarksZPosition(out _);

            // Transform coordinates
            Point3D[] points = new Point3D[list1.Count];
            for (int i = 0; i < list1.Count; i++)
            {
                points[i] = new Point3D(((PointF)list1[i]).X, ((PointF)list1[i]).Y, zPositon);
            }

            axis.ChartArea.matrix3D.TransformPoints(points);
            for (int i = 0; i < list1.Count; i++)
            {
                list1[i] = points[i].PointF;
            }
        }

        foreach (PointF p in list1)
        {
            list.Add(graph.GetAbsolutePoint(p));
        }

        return list;
    }

    /// <summary>
    /// Gets the area markers.
    /// </summary>
    /// <param name="graph">The graph.</param>
    /// <param name="area">The area.</param>
    /// <returns>List of PointF.</returns>
    private List<PointF> GetAreaMarkers(ChartGraphics graph, ChartArea area)
    {
        List<PointF> list = new List<PointF>();
        if (area == null)
        {
            return list;
        }

        IList list1 = this.GetMarkers(area.PlotAreaPosition.ToRectangleF(), ChartElementType.PlottingArea);
        if (this.IsChartAreaCircular(area))
        {
            list1 = this.GetMarkers(area.lastAreaPosition, ChartElementType.PlottingArea);
        }

        if (IsArea3D(area))
        {
            float zPositon = 0; // area.areaSceneDepth;
            // Transform coordinates
            Point3D[] points = new Point3D[list1.Count];
            for (int i = 0; i < list1.Count; i++)
            {
                points[i] = new Point3D(((PointF)list1[i]).X, ((PointF)list1[i]).Y, zPositon);
            }

            area.matrix3D.TransformPoints(points);
            for (int i = 0; i < list1.Count; i++)
            {
                list1[i] = points[i].PointF;
            }
        }

        foreach (PointF p in list1)
        {
            list.Add(graph.GetAbsolutePoint(p));
        }

        return list;
    }

    /// <summary>
    /// Builds list of markers (PointF) based on rectangle
    /// </summary>
    /// <param name="rect">The rectangle</param>
    /// <param name="elementType">The type of chart elements to retrieve.</param>
    /// <returns>List of PointF</returns>
    private List<PointF> GetMarkers(RectangleF rect, ChartElementType elementType)
    {
        if (elementType.ToString().StartsWith("Legend", StringComparison.Ordinal) || elementType.ToString().StartsWith("Title", StringComparison.Ordinal))
        {
            rect.Inflate(4f, 4f);
        }

        if (elementType.ToString().StartsWith("PlottingArea", StringComparison.Ordinal))
        {
            SizeF relSize = this.Graph.GetRelativeSize(new SizeF(4f, 4f));
            rect.Inflate(relSize.Width, relSize.Height);
        }

        if ((elementType != ChartElementType.Nothing) && (elementType != ChartElementType.PlottingArea))
        {
            return this.GetMarkers(rect, false);
        }

        return this.GetMarkers(rect, true);
    }


    /// <summary>
    /// Builds list of markers (PointF) based on rectangle
    /// </summary>
    /// <param name="rect">The rectangle</param>
    /// <param name="addAdditionalMarkers">Add additional markers to the rectangle.</param>
    /// <returns>List of PointF</returns>
    private List<PointF> GetMarkers(RectangleF rect, bool addAdditionalMarkers)
    {
        List<PointF> list = new List<PointF>();
        if (!addAdditionalMarkers)
        {
            if (rect.Width > 0 && rect.Height > 0)
            {
                list.Add(new PointF(rect.Left, rect.Top));
                list.Add(new PointF(rect.Right, rect.Top));
                list.Add(new PointF(rect.Right, rect.Bottom));
                list.Add(new PointF(rect.Left, rect.Bottom));
            }
            else if (rect.Width > 0)
            {
                list.Add(new PointF(rect.Left, rect.Top));
                list.Add(new PointF(rect.Right, rect.Top));
            }
            else if (rect.Height > 0)
            {
                list.Add(new PointF(rect.Left, rect.Top));
                list.Add(new PointF(rect.Left, rect.Bottom));
            }
        }
        else
        {
            if (rect.Width > 0 && rect.Height > 0)
            {
                list.Add(new PointF(rect.Left, rect.Top));
                if (rect.Width > 30)
                {
                    list.Add(new PointF(rect.Left + rect.Width / 2, rect.Top));
                }

                list.Add(new PointF(rect.Right, rect.Top));
                if (rect.Height > 30)
                {
                    list.Add(new PointF(rect.Right, rect.Top + rect.Height / 2));
                }

                list.Add(new PointF(rect.Right, rect.Bottom));
                if (rect.Width > 30)
                {
                    list.Add(new PointF(rect.Left + rect.Width / 2, rect.Bottom));
                }

                list.Add(new PointF(rect.Left, rect.Bottom));
                if (rect.Height > 30)
                {
                    list.Add(new PointF(rect.Left, rect.Top + rect.Height / 2));
                }
            }
            else if (rect.Width > 0)
            {
                list.Add(new PointF(rect.Left, rect.Top));
                if (rect.Width > 30)
                {
                    list.Add(new PointF(rect.Left + rect.Width / 2, rect.Top));
                }

                list.Add(new PointF(rect.Right, rect.Top));
            }
            else if (rect.Height > 0)
            {
                list.Add(new PointF(rect.Left, rect.Bottom));
                if (rect.Height > 30)
                {
                    list.Add(new PointF(rect.Left, rect.Top + rect.Height / 2));
                }

                list.Add(new PointF(rect.Left, rect.Top));
            }
        }

        return list;
    }

    /// <summary>
    /// Calculates a DataPoint of 3D area into PointF to draw.
    /// </summary>
    /// <param name="chartArea">3D chart area</param>
    /// <param name="point">The DataPoint</param>
    /// <returns>Calculated PointF</returns>
    private PointF Transform3D(ChartArea chartArea, DataPoint point)
    {
        if (chartArea is not null && IsArea3D(chartArea))
        {
            // Get annotation Z coordinate (use scene depth or anchored point Z position)
            float positionZ = chartArea.areaSceneDepth;
            if (point != null && point.series != null)
            {
                chartArea.GetSeriesZPositionAndDepth(
                    point.series,
                    out float depth,
                    out positionZ);
                positionZ += depth / 2f;
            }

            PointF pf = point.positionRel;

            // Define 3D points of annotation object
            Point3D[] annot3DPoints = new Point3D[1];
            annot3DPoints[0] = new Point3D(pf.X, pf.Y, positionZ);

            // Transform cube coordinates
            chartArea.matrix3D.TransformPoints(annot3DPoints);

            return annot3DPoints[0].PointF;
        }

        return point.positionRel;
    }

    /// <summary>
    /// Gets the hot region rectangle.
    /// </summary>
    /// <param name="rgn">The hot region.</param>
    /// <param name="unionWith">The rectangle to union with.</param>
    /// <param name="elementType">The type of the element.</param>
    /// <returns>Returns the rectangle around the hot region.</returns>
    private RectangleF GetHotRegionRectangle(HotRegion rgn, RectangleF unionWith, ChartElementType elementType)
    {
        RectangleF rect;
        if (rgn.Path != null)
        {
            rect = rgn.Path.GetBounds();
        }
        else
        {
            rect = rgn.BoundingRectangle;
        }

        if (rgn.RelativeCoordinates)
        {
            rect = this.Graph.GetAbsoluteRectangle(rect);
        }

        if (elementType == ChartElementType.AxisLabels)
        {
            if (rect.Width > rect.Height)
            {
                rect.Inflate(-5, -2);
            }
            else if (rect.Width < rect.Height)
            {
                rect.Inflate(-2, -5);
            }
        }

        if (!unionWith.IsEmpty)
        {
            return RectangleF.Union(unionWith, rect);
        }

        return rect;
    }

    #endregion //Selection

    #endregion //Tooltips

    #region IServiceProvider Members

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>
    /// A service object of type <paramref name="serviceType"/>.  It returns null 
    /// if there is no service object of type <paramref name="serviceType"/>.
    /// </returns>
    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(Selection))
        {
            return this;
        }

        if (_service != null)
        {
            return _service.GetService(serviceType);
        }

        return null;
    }

    #endregion

}


/// <summary>
/// The ToolTipEventArgs class stores the tool tips event arguments.
/// </summary>
public class ToolTipEventArgs : EventArgs
{
    #region Private fields

    // Private fields for properties values storage
    private readonly int y;
    private readonly HitTestResult result = new HitTestResult();

    #endregion

    #region Constructors

    /// <summary>
    /// ToolTipEventArgs constructor.  Creates ToolTip event arguments.
    /// </summary>
    /// <param name="x">X-coordinate of mouse.</param>
    /// <param name="y">Y-coordinate of mouse.</param>
    /// <param name="text">Tooltip text.</param>
    /// <param name="result">Hit test result object.</param>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
        Justification = "X and Y are cartesian coordinates and well understood")]
    public ToolTipEventArgs(int x, int y, string text, HitTestResult result)
    {
        this.X = x;
        this.y = y;
        this.Text = text;
        this.result = result;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the x-coordinate of the mouse.
    /// </summary>
    [
    SRDescription("DescriptionAttributeToolTipEventArgs_X"),
    ]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
    public int X { get; }

    /// <summary>
    /// Gets the result of the hit test.
    /// </summary>
    [
    SRDescription("DescriptionAttributeToolTipEventArgs_HitTestResult"),
    ]
    public HitTestResult HitTestResult => result;

    /// <summary>
    /// Gets the y-coordinate of the mouse.
    /// </summary>
    [
    SRDescription("DescriptionAttributeToolTipEventArgs_Y"),
    ]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
    public int Y => y;

    /// <summary>
    /// Gets the text of the tooltip.
    /// </summary>
    [
    SRDescription("DescriptionAttributeToolTipEventArgs_Text"),
    ]
    public string Text { get; set; } = string.Empty;

    #endregion
}
