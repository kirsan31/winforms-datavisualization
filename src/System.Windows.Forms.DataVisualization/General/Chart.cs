// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	This file contains classes, which are used for Image
//				creation and chart painting. This file has also a
//				class, which is used for Paint events arguments.
//

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

using FontStyle = System.Drawing.FontStyle;

namespace System.Windows.Forms.DataVisualization.Charting;

/// <summary>
/// ChartImage class adds image type and data binding functionality to
/// the base ChartPicture class.
/// </summary>
internal sealed class ChartImage : ChartPicture
{
    #region Fields

    // Private data members, which store properties values
    private int _compression;

    // Chart data source object
    private object _dataSource;

    // Indicates that control was bound to the data source
    internal bool boundToDataSource;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Chart internal constructor.
    /// </summary>
    /// <param name="container">Service container</param>
    internal ChartImage(IServiceContainer container)
        : base(container)
    {
    }

    #endregion Constructor

    #region Properties

    /// <summary>
    /// Gets or sets the data source for the Chart object.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    Bindable(true),
    SRDescription("DescriptionAttributeDataSource"),
    DefaultValue(null),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibility(SerializationVisibility.Hidden)
    ]
    public object DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource != value)
            {
                _dataSource = value;
                this.boundToDataSource = false;
            }
        }
    }

    /// <summary>
    /// Image compression value
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(0),
    SRDescription("DescriptionAttributeChartImage_Compression"),
    ]
    public int Compression
    {
        get => _compression;
        set
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionChartCompressionInvalid);
            }

            _compression = value;
        }
    }

    #endregion Properties

    #region Methods

    #region Image Manipulation

    /// <summary>
    /// Saves image into the metafile stream.
    /// </summary>
    /// <param name="imageStream">Image stream.</param>
    /// <param name="emfType">Image stream.</param>
    [SecuritySafeCritical]
    public void SaveIntoMetafile(Stream imageStream, EmfType emfType)
    {
        // Check arguments
        if (imageStream == null)
            throw new ArgumentNullException(nameof(imageStream));

        // Create temporary Graphics object for metafile
        using Bitmap bitmap = new Bitmap(this.Width, this.Height);
        using Graphics newGraphics = Graphics.FromImage(bitmap);
        IntPtr hdc = IntPtr.Zero;
        try
        {
            hdc = newGraphics.GetHdc();

            // Create metafile object to record.
            using Metafile metaFile = new Metafile(
                imageStream,
                hdc,
                new Rectangle(0, 0, this.Width, this.Height),
                MetafileFrameUnit.Pixel,
                emfType);

            // Create graphics object to record metaFile.
            using Graphics metaGraphics = Graphics.FromImage(metaFile);

            // Note: Fix for issue #3674. Some 3D borders shadows may be drawn outside
            // of image boundaries. This causes issues when generated EMF file
            // is placed in IE. Image looks shifted down and hot areas do not align.
            if (this.BorderSkin.SkinStyle != BorderSkinStyle.None)
            {
                metaGraphics.Clip = new Region(new Rectangle(0, 0, this.Width, this.Height));
            }

            // Draw chart in the metafile
            this.ChartGraph.IsMetafile = true;
            this.Paint(metaGraphics, false);
            this.ChartGraph.IsMetafile = false;
        }
        finally
        {
            if (hdc != IntPtr.Zero)
            {
                newGraphics.ReleaseHdc(hdc);
            }
        }
    }


    /// <summary>
    /// Create Image and draw chart picture with <see cref="Control.DeviceDpi"/> resolution.
    /// </summary>
    public Bitmap GetImage()
    {
        return this.GetImage(this.Chart.DeviceDpi);
    }

    /// <summary>
    /// Create Image and draw chart picture
    /// </summary>
    /// <param name="resolution">The resolution (DPI).</param>
    public Bitmap GetImage(float resolution)
    {
        // Create a new bitmap
        Bitmap image = null;
        int oldW = -1, oldH = -1;
        resolution = Math.Max(resolution, 96);
        while (image is null)
        {
            bool failed;
            try
            {
                image = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));
                image.SetResolution(resolution, resolution);
                failed = false;
            }
            catch (ArgumentException)
            {
                if (resolution == 96)
                    throw;

                failed = true;
            }
            catch (OverflowException)
            {
                if (resolution == 96)
                    throw;

                failed = true;
            }
            catch (InvalidOperationException)
            {
                if (resolution == 96)
                    throw;

                failed = true;
            }
            catch (ExternalException)
            {
                if (resolution == 96)
                    throw;

                failed = true;
            }

            if (failed)
            {
                // if failed to create the image, decrease the size and the resolution of the chart
                image = null;
                float newResolution = Math.Max(resolution / 2, 96);
                if (oldW == -1 && oldH == -1)
                {
                    oldW = Width;
                    oldH = Height;
                }

                Width = (int)Math.Ceiling(Width * newResolution / resolution);
                Height = (int)Math.Ceiling(Height * newResolution / resolution);
                resolution = newResolution;
            }
        }

        // Creates a new Graphics object from the
        // specified Image object.
        Graphics offScreen = Graphics.FromImage(image);
        Color backGroundColor;
        if (this.BackColor != Color.Empty)
            backGroundColor = this.BackColor;
        else
            backGroundColor = Color.White;

        // Get the page color if border skin is visible.
        if (GetBorderSkinVisibility() && this.BorderSkin.PageColor != Color.Empty)
            backGroundColor = this.BorderSkin.PageColor;

        // draw a rectangle first with the size of the control, this prevent strange behavior when printing in the reporting services,
        // without this rectangle, the printed picture is blurry
        Pen pen = new Pen(backGroundColor);
        offScreen.DrawRectangle(pen, 0, 0, Width, Height);
        pen.Dispose();

        // Paint the chart
        Paint(offScreen, false);

        // Dispose Graphic object
        offScreen.Dispose();

        if (oldW != -1 && oldH != -1)
        {
            Width = oldW;
            Height = oldH;
        }

        // Return reference to the image
        return image;
    }

    #endregion Image Manipulation

    #region Data Binding

    /// <summary>
    /// Checks if the type of the data source is valid.
    /// </summary>
    /// <param name="dataSource">Data source object to test.</param>
    /// <returns>True if valid data source object.</returns>
    internal static bool IsValidDataSource(object dataSource)
    {
        if (null != dataSource &&
            (
            dataSource is IEnumerable ||
            dataSource is DataSet ||
            dataSource is DataView ||
            dataSource is DataTable ||
            // ADDED: for VS2005 compatibility, DT Nov 25, 2005
            dataSource.GetType().GetInterface("IDataSource") != null
            // END ADDED
            )
          )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets an list of the data source member names.
    /// </summary>
    /// <param name="dataSource">Data source object to get the members for.</param>
    /// <param name="usedForYValue">Indicates that member will be used for Y values.</param>
    /// <returns>List of member names.</returns>
    internal static List<string> GetDataSourceMemberNames(object dataSource, bool usedForYValue)
    {
        List<string> names = new List<string>();
        if (dataSource != null)
        {
            // ADDED: for VS2005 compatibility, DT Nov 25, 2004
            if (dataSource.GetType().GetInterface("IDataSource") != null)
            {
                try
                {
                    MethodInfo m = dataSource.GetType().GetMethod("Select");
                    if (m != null)
                    {
                        if (m.GetParameters().Length == 1)
                        {
                            // SQL derived datasource
                            Type selectArgsType = dataSource.GetType().Assembly.GetType("System.Web.UI.DataSourceSelectArguments", true);
                            ConstructorInfo ci = selectArgsType.GetConstructor(Array.Empty<Type>());
                            dataSource = m.Invoke(dataSource, new object[] { ci.Invoke(Array.Empty<object>()) });
                        }
                        else
                        {
                            // object data source
                            dataSource = m.Invoke(dataSource, Array.Empty<object>());
                        }
                    }
                }
                catch (TargetException)
                {
                }
                catch (TargetInvocationException)
                {
                }
            }
            // END ADDED

            // Check all DataTable based data sources
            DataTable dataTable = null;

            if (dataSource is DataTable table)
            {
                dataTable = table;
            }
            else if (dataSource is DataView view)
            {
                dataTable = view.Table;
            }
            else if (dataSource is DataSet set && set.Tables.Count > 0)
            {
                dataTable = set.Tables[0];
            }

            // Check if DataTable was set
            if (dataTable != null)
            {
                // Add table columns names
                foreach (DataColumn column in dataTable.Columns)
                {
                    if (!usedForYValue || column.DataType != typeof(string))
                    {
                        names.Add(column.ColumnName);
                    }
                }
            }
            else if (names.Count == 0 && dataSource is ITypedList list)
            {
                foreach (PropertyDescriptor pd in list.GetItemProperties(null))
                {
                    if (!usedForYValue || pd.PropertyType != typeof(string))
                    {
                        names.Add(pd.Name);
                    }
                }
            }
            else if (names.Count == 0 && dataSource is IEnumerable enumerable)
            {
                // .Net 2.0 ObjectDataSource processing
                IEnumerator e = enumerable.GetEnumerator();
                e.Reset();
                e.MoveNext();
                foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(e.Current))
                {
                    if (!usedForYValue || pd.PropertyType != typeof(string))
                    {
                        names.Add(pd.Name);
                    }
                }
            }

            // Check if list still empty
            if (names.Count == 0)
            {
                // Add first column or any data member name
                names.Add("0");
            }
        }

        return names;
    }

    /// <summary>
    /// Data binds control to the data source
    /// </summary>
    internal void DataBind()
    {
        // Set bound flag
        this.boundToDataSource = true;

        object dataSource = this.DataSource;
        if (dataSource is null)
            return;

        DataView dataView = null;
        // Convert data source to recognizable source for the series
        if (dataSource is DataSet set && set.Tables.Count > 0)
        {
            dataView = set.DefaultViewManager.CreateDataView(set.Tables[0]);
            dataSource = dataView;
        }
        else if (dataSource is DataTable table)
        {
            dataView = new DataView(table);
            dataSource = dataView;
        }
        else if (dataSource is IListSource source)
        {
            if (source.ContainsListCollection && source.GetList().Count > 0)
            {
                dataSource = source.GetList()[0] as IEnumerable;
            }
            else
            {
                dataSource = source.GetList();
            }
        }

        // Data bind
        DataBind(dataSource as IEnumerable, null);
        dataView?.Dispose();
    }

    /// <summary>
    /// Data binds control to the data source
    /// </summary>
    /// <param name="dataSource">Data source to bind to.</param>
    /// <param name="seriesList">List of series to bind.</param>
    internal void DataBind(IEnumerable dataSource, ArrayList seriesList)
    {
        // Data bind series
        if (dataSource != null && this.Common != null)
        {
            //************************************************************
            //** If list of series is not provided - bind all of them.
            //************************************************************
            if (seriesList == null)
            {
                seriesList = new ArrayList();
                foreach (Series series in this.Common.Chart.Series)
                {
                    // note: added for design time data binding
                    if (this.Common.Chart.IsDesignMode())
                    {
                        if (series.YValueMembers.Length > 0)
                        {
                            seriesList.Add(series);
                        }
                    }
                    else
                    {
                        seriesList.Add(series);
                    }
                }
            }

            //************************************************************
            //** Clear all data points in data bound series
            //************************************************************
            foreach (Series series in seriesList)
            {
                if (series.XValueMember.Length > 0 || series.YValueMembers.Length > 0)
                {
                    series.Points.Clear();
                }
            }

            //************************************************************
            //** Get and reset data enumerator.
            //************************************************************
            IEnumerator enumerator = dataSource.GetEnumerator();
            if (enumerator.GetType() != typeof(System.Data.Common.DbEnumerator))
            {
                try
                {
                    enumerator.Reset();
                }
                // Some enumerators may not support Resetting
                catch (InvalidOperationException)
                {
                }
                catch (NotImplementedException)
                {
                }
                catch (NotSupportedException)
                {
                }
            }

            bool autoDetectType = true;

            //************************************************************
            //** Loop through the enumerator.
            //************************************************************
            bool valueExsists;
            do
            {
                // Move to the next item
                valueExsists = enumerator.MoveNext();

                // Loop through all series
                foreach (Series series in seriesList)
                {
                    if (series.XValueMember.Length > 0 || series.YValueMembers.Length > 0)
                    {
                        //************************************************************
                        //** Check and convert fields names.
                        //************************************************************

                        // Convert comma separated field names string to array of names
                        string[] yFieldNames = null;
                        if (series.YValueMembers.Length > 0)
                        {
                            yFieldNames = series.YValueMembers.Replace(",,", "\n").Split(',', StringSplitOptions.TrimEntries);
                            for (int index = 0; index < yFieldNames.Length; index++)
                            {
                                yFieldNames[index] = yFieldNames[index].Replace('\n', ',');
                            }
                        }

                        // Double check that a string object is not provided for data binding
                        if (dataSource is string)
                        {
                            throw new ArgumentException(SR.ExceptionDataBindYValuesToString, nameof(dataSource));
                        }

                        // Check number of fields
                        if (yFieldNames == null || yFieldNames.GetLength(0) > series.YValuesPerPoint)
                        {
                            throw new ArgumentOutOfRangeException(nameof(dataSource), SR.ExceptionDataPointYValuesCountMismatch(series.YValuesPerPoint.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                        }

                        //************************************************************
                        //** Create new data point.
                        //************************************************************
                        if (valueExsists)
                        {
                            // Auto detect values type
                            if (autoDetectType)
                            {
                                autoDetectType = false;

                                // Make sure Y field is not empty
                                string yField = yFieldNames[0];
                                int fieldIndex = 1;
                                while (yField.Length == 0 && fieldIndex < yFieldNames.Length)
                                {
                                    yField = yFieldNames[fieldIndex++];
                                }

                                DataPointCollection.AutoDetectValuesType(series, enumerator, series.XValueMember, enumerator, yField);
                            }

                            // Create new point
                            DataPoint newDataPoint = new DataPoint(series);
                            bool emptyValues = false;
                            bool xValueIsNull = false;

                            //************************************************************
                            //** Get new point X and Y values.
                            //************************************************************
                            object[] yValuesObj = new object[yFieldNames.Length];
                            object xValueObj = null;

                            // Set X to the value provided or use sequence numbers starting with 1
                            if (series.XValueMember.Length > 0)
                            {
                                xValueObj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, series.XValueMember);
                                if (xValueObj is DBNull || xValueObj == null)
                                {
                                    xValueIsNull = true;
                                    emptyValues = true;
                                    xValueObj = 0.0;
                                }
                            }

                            if (yFieldNames.Length == 0)
                            {
                                yValuesObj[0] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, null);
                                if (yValuesObj[0] is DBNull || yValuesObj[0] == null)
                                {
                                    emptyValues = true;
                                    yValuesObj[0] = 0.0;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < yFieldNames.Length; i++)
                                {
                                    if (yFieldNames[i].Length > 0)
                                    {
                                        yValuesObj[i] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, yFieldNames[i]);
                                        if (yValuesObj[i] is DBNull || yValuesObj[i] == null)
                                        {
                                            emptyValues = true;
                                            yValuesObj[i] = 0.0;
                                        }
                                    }
                                    else
                                    {
                                        yValuesObj[i] = ((Series)seriesList[0]).IsYValueDateTime() ? DateTime.Now.Date.ToOADate() : 0.0;
                                    }
                                }
                            }

                            // Add data point if X value is not Null
                            if (!xValueIsNull)
                            {
                                if (emptyValues)
                                {
                                    if (xValueObj != null)
                                    {
                                        newDataPoint.SetValueXY(xValueObj, yValuesObj);
                                    }
                                    else
                                    {
                                        newDataPoint.SetValueXY(0, yValuesObj);
                                    }

                                    series.Points.DataPointInit(ref newDataPoint);
                                    newDataPoint.IsEmpty = true;
                                    series.Points.Add(newDataPoint);
                                }
                                else
                                {
                                    if (xValueObj != null)
                                    {
                                        newDataPoint.SetValueXY(xValueObj, yValuesObj);
                                    }
                                    else
                                    {
                                        newDataPoint.SetValueXY(0, yValuesObj);
                                    }

                                    series.Points.DataPointInit(ref newDataPoint);
                                    series.Points.Add(newDataPoint);
                                }
                            }

                            if (this.Common.Chart.IsDesignMode())
                            {
                                series["TempDesignData"] = "true";
                            }
                        }
                    }
                }
            } while (valueExsists);
        }
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    /// <param name="sortAxisLabels">Indicates if points should be sorted by axis labels.</param>
    /// <param name="sortingOrder">Sorting pointSortOrder.</param>
    internal void AlignDataPointsByAxisLabel(bool sortAxisLabels, PointSortOrder sortingOrder)
    {
        // Find series which are attached to the same X axis in the same chart area
        foreach (ChartArea chartArea in this.ChartAreas)
        {
            // Check if chart area is visible
            if (chartArea.Visible)

            {
                // Create series list for primary and secondary X axis
                List<Series> chartAreaSeriesPrimary = [];
                List<Series> chartAreaSeriesSecondary = [];
                foreach (Series series in this.Common.Chart.Series)
                {
                    // Check if series belongs to the chart area
                    if (series.ChartArea == chartArea.Name)
                    {
                        if (series.XSubAxisName.Length == 0)
                        {
                            if (series.XAxisType == AxisType.Primary)
                            {
                                chartAreaSeriesPrimary.Add(series);
                            }
                            else
                            {
                                chartAreaSeriesSecondary.Add(series);
                            }
                        }
                    }
                }

                // Align series
                AlignDataPointsByAxisLabel(chartAreaSeriesPrimary, sortAxisLabels, sortingOrder);
                AlignDataPointsByAxisLabel(chartAreaSeriesSecondary, sortAxisLabels, sortingOrder);
            }
        }
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    /// <param name="seriesList">List of series to align.</param>
    /// <param name="sortAxisLabels">Indicates if points should be sorted by axis labels.</param>
    /// <param name="sortingOrder">Sorting order.</param>
    internal static void AlignDataPointsByAxisLabel(
        List<Series> seriesList,
        bool sortAxisLabels,
        PointSortOrder sortingOrder)
    {
        // List is empty
        if (seriesList.Count == 0)
        {
            return;
        }

        // Collect information about all points in all series
        bool indexedX = true;
        bool uniqueAxisLabels = true;
        List<string> axisLabels = [];
        foreach (Series series in seriesList)
        {
            HashSet<string> seriesAxisLabels = [];
            foreach (DataPoint point in series.Points)
            {
                // Check if series has indexed X values
                if (!series.IsXValueIndexed && point.XValue != 0.0)
                {
                    indexedX = false;
                    break;
                }

                // Add axis label to the list and make sure it's non-empty and unique
                if (point.AxisLabel.Length == 0)
                {
                    uniqueAxisLabels = false;
                    break;
                }
                else if (seriesAxisLabels.Contains(point.AxisLabel))
                {
                    uniqueAxisLabels = false;
                    break;
                }
                else if (!axisLabels.Contains(point.AxisLabel))
                {
                    axisLabels.Add(point.AxisLabel);
                }

                seriesAxisLabels.Add(point.AxisLabel);
            }
        }

        // Sort axis labels
        if (sortAxisLabels)
        {
#pragma warning disable CA1310 // Specify StringComparison for correctness
            if (sortingOrder == PointSortOrder.Descending)
                axisLabels.Sort((a, b) => b.CompareTo(a));
            else
                axisLabels.Sort();
#pragma warning restore CA1310 // Specify StringComparison for correctness
        }

        // All series must be indexed
        if (!indexedX)
        {
            throw new InvalidOperationException(SR.ExceptionChartDataPointsAlignmentFaild);
        }

        // AxisLabel can't be empty or duplicated
        if (!uniqueAxisLabels)
        {
            throw new InvalidOperationException(SR.ExceptionChartDataPointsAlignmentFaildAxisLabelsInvalid);
        }

        // Assign unique X values for data points in all series with same axis LabelStyle
        if (indexedX && uniqueAxisLabels)
        {
            foreach (Series series in seriesList)
            {
                foreach (DataPoint point in series.Points)
                {
                    point.XValue = axisLabels.IndexOf(point.AxisLabel) + 1;
                }

                // Sort points by X value
                series.Sort(PointSortOrder.Ascending, "X");
            }

            // Make sure there are no missing points
            foreach (Series series in seriesList)
            {
                series.IsXValueIndexed = true;
                for (int index = 0; index < axisLabels.Count; index++)
                {
                    if (index >= series.Points.Count || series.Points[index].XValue != index + 1)
                    {
                        DataPoint newPoint = new DataPoint(series)
                        {
                            AxisLabel = axisLabels[index],
                            XValue = index + 1
                        };
                        newPoint.YValues[0] = 0.0;
                        newPoint.IsEmpty = true;
                        series.Points.Insert(index, newPoint);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Data bind chart to the table. Series will be automatically added to the chart depending on
    /// the number of unique values in the seriesGroupByField column of the data source.
    /// Data source can be the DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
    /// <param name="xField">Name of the field for X values.</param>
    /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
    /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
    /// <param name="sort">Indicates that series should be sorted by group field.</param>
    /// <param name="sortingOrder">Series sorting order by group field.</param>
    internal void DataBindCrossTab(
    IEnumerable dataSource,
    string seriesGroupByField,
    string xField,
    string yFields,
    string otherFields,
    bool sort,
    PointSortOrder sortingOrder)
    {
        // Check arguments
        if (dataSource == null)
            throw new ArgumentNullException(nameof(dataSource), SR.ExceptionDataPointInsertionNoDataSource);

        if (dataSource is string)
            throw new ArgumentException(SR.ExceptionDataBindSeriesToString, nameof(dataSource));

        if (string.IsNullOrEmpty(yFields))
            throw new ArgumentException(SR.ExceptionChartDataPointsInsertionFailedYValuesEmpty, nameof(yFields));

        if (string.IsNullOrEmpty(seriesGroupByField))
            throw new ArgumentException(SR.ExceptionDataBindSeriesGroupByParameterIsEmpty, nameof(seriesGroupByField));

        // List of series and group by field values
        ArrayList seriesList = new ArrayList();
        ArrayList groupByValueList = new ArrayList();

        // Convert comma separated Y values field names string to array of names
        string[] yFieldNames = null;
        if (yFields != null)
        {
            yFieldNames = yFields.Replace(",,", "\n").Split(',');
            for (int index = 0; index < yFieldNames.Length; index++)
            {
                yFieldNames[index] = yFieldNames[index].Replace('\n', ',');
            }
        }

        // Convert other fields/properties names to two arrays of names
        string[] otherAttributeNames = null;
        string[] otherFieldNames = null;
        string[] otherValueFormat = null;
        DataPointCollection.ParsePointFieldsParameter(
            otherFields,
            ref otherAttributeNames,
            ref otherFieldNames,
            ref otherValueFormat);

        // Get and reset enumerator
        IEnumerator enumerator = DataPointCollection.GetDataSourceEnumerator(dataSource);
        if (enumerator.GetType() != typeof(System.Data.Common.DbEnumerator))
        {
            try
            {
                enumerator.Reset();
            }
            // Some enumerators may not support Resetting
            catch (NotSupportedException)
            {
            }
            catch (NotImplementedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        // Add data points
        bool valueExsist = true;
        object[] yValuesObj = new object[yFieldNames.Length];
        object xValueObj = null;
        bool autoDetectType = true;

        do
        {
            // Move to the next objects in the enumerations
            if (valueExsist)
            {
                valueExsist = enumerator.MoveNext();
            }

            // Create and initialize data point
            if (valueExsist)
            {
                // Get value of the group by field
                object groupObj = DataPointCollection.ConvertEnumerationItem(
                    enumerator.Current,
                    seriesGroupByField);
                int seriesIndex = groupByValueList.IndexOf(groupObj);

                // Check series group by field and create new series if required
                Series series;
                if (seriesIndex >= 0)
                {
                    // Select existing series from the list
                    series = (Series)seriesList[seriesIndex];
                }
                else
                {
                    // Create new series
                    series = new Series
                    {
                        YValuesPerPoint = yFieldNames.GetLength(0)
                    };

                    // If not the first series in the list copy some properties
                    if (seriesList.Count > 0)
                    {
                        series.XValueType = ((Series)seriesList[0]).XValueType;
                        series.autoXValueType = ((Series)seriesList[0]).autoXValueType;
                        series.YValueType = ((Series)seriesList[0]).YValueType;
                        series.autoYValueType = ((Series)seriesList[0]).autoYValueType;
                    }

                    // Try to set series name based on grouping value
                    if (groupObj is string groupObjStr)
                    {
                        series.Name = groupObjStr;
                    }
                    else
                    {
                        series.Name = seriesGroupByField + " - " + groupObj.ToString();
                    }

                    // Add series and group value into the lists
                    groupByValueList.Add(groupObj);
                    seriesList.Add(series);
                }

                // Auto detect value(s) type
                if (autoDetectType)
                {
                    autoDetectType = false;
                    DataPointCollection.AutoDetectValuesType(series, enumerator, xField, enumerator, yFieldNames[0]);
                }

                // Create new data point
                DataPoint newDataPoint = new DataPoint(series);
                bool emptyValues = false;

                // Set X to the value provided
                if (xField.Length > 0)
                {
                    xValueObj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, xField);
                    if (DataPointCollection.IsEmptyValue(xValueObj))
                    {
                        emptyValues = true;
                        xValueObj = 0.0;
                    }
                }

                // Set Y values
                if (yFieldNames.Length == 0)
                {
                    yValuesObj[0] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, null);
                    if (DataPointCollection.IsEmptyValue(yValuesObj[0]))
                    {
                        emptyValues = true;
                        yValuesObj[0] = 0.0;
                    }
                }
                else
                {
                    for (int i = 0; i < yFieldNames.Length; i++)
                    {
                        yValuesObj[i] = DataPointCollection.ConvertEnumerationItem(enumerator.Current, yFieldNames[i]);
                        if (DataPointCollection.IsEmptyValue(yValuesObj[i]))
                        {
                            emptyValues = true;
                            yValuesObj[i] = 0.0;
                        }
                    }
                }

                // Set other values
                if (otherAttributeNames != null &&
                    otherAttributeNames.Length > 0)
                {
                    for (int i = 0; i < otherFieldNames.Length; i++)
                    {
                        // Get object by field name
                        object obj = DataPointCollection.ConvertEnumerationItem(enumerator.Current, otherFieldNames[i]);
                        if (!DataPointCollection.IsEmptyValue(obj))
                        {
                            newDataPoint.SetPointCustomProperty(
                                obj,
                                otherAttributeNames[i],
                                otherValueFormat[i]);
                        }
                    }
                }

                // IsEmpty value was detected
                if (emptyValues)
                {
                    if (xValueObj != null)
                    {
                        newDataPoint.SetValueXY(xValueObj, yValuesObj);
                    }
                    else
                    {
                        newDataPoint.SetValueXY(0, yValuesObj);
                    }

                    DataPointCollection.DataPointInit(series, ref newDataPoint);
                    newDataPoint.IsEmpty = true;
                    series.Points.Add(newDataPoint);
                }
                else
                {
                    if (xValueObj != null)
                    {
                        newDataPoint.SetValueXY(xValueObj, yValuesObj);
                    }
                    else
                    {
                        newDataPoint.SetValueXY(0, yValuesObj);
                    }

                    DataPointCollection.DataPointInit(series, ref newDataPoint);
                    series.Points.Add(newDataPoint);
                }
            }
        } while (valueExsist);

        // Sort series using values of group by field
        if (sort)
        {
            // Duplicate current list
            ArrayList oldList = (ArrayList)groupByValueList.Clone();

            // Sort list
            groupByValueList.Sort();
            if (sortingOrder == PointSortOrder.Descending)
            {
                groupByValueList.Reverse();
            }

            // Change order of series in collection
            ArrayList sortedSeriesList = new ArrayList();
            foreach (object obj in groupByValueList)
            {
                sortedSeriesList.Add(seriesList[oldList.IndexOf(obj)]);
            }

            seriesList = sortedSeriesList;
        }

        // Add all series from the list into the series collection
        foreach (Series series in seriesList)
        {
            this.Common.Chart.Series.Add(series);
        }
    }

    /// <summary>
    /// Automatically creates and binds series to specified data table.
    /// Each column of the table becomes a Y value in a separate series.
    /// Series X value field may also be provided.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="xField">Name of the field for series X values.</param>
    internal void DataBindTable(
        IEnumerable dataSource,
        string xField)
    {
        // Check arguments
        if (dataSource == null)
            throw new ArgumentNullException(nameof(dataSource));

        // Get list of member names from the data source
        var dataSourceFields = GetDataSourceMemberNames(dataSource, true);

        // Remove X value field if it's there
        if (!string.IsNullOrEmpty(xField))
        {
            int index = -1;
            for (int i = 0; i < dataSourceFields.Count; i++)
            {
                if (string.Equals(dataSourceFields[i], xField, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                dataSourceFields.RemoveAt(index);
            }
            else
            {
                // Check if field name passed as index
                bool parseSucceed = int.TryParse(xField, NumberStyles.Any, CultureInfo.InvariantCulture, out index);
                if (parseSucceed && index >= 0 && index < dataSourceFields.Count)
                {
                    dataSourceFields.RemoveAt(index);
                }
            }
        }

        // Get number of series
        int seriesNumber = dataSourceFields.Count;
        if (seriesNumber > 0)
        {
            // Create as many series as fields in the data source
            ArrayList seriesList = new ArrayList();
            int index = 0;
            foreach (string fieldName in dataSourceFields)
            {
                Series series = new Series(fieldName)
                {
                    // Set binding properties
                    YValueMembers = fieldName,
                    XValueMember = xField
                };

                // Add to list
                seriesList.Add(series);
                ++index;
            }

            // Data bind series
            this.DataBind(dataSource, seriesList);

            // Add all series from the list into the series collection
            foreach (Series series in seriesList)
            {
                // Clear binding properties
                series.YValueMembers = string.Empty;
                series.XValueMember = string.Empty;

                // Add series into the list
                this.Common.Chart.Series.Add(series);
            }
        }
    }

    #endregion Data Binding

    #endregion Methods
}

/// <summary>
/// ChartPicture class represents chart content like legends, titles,
/// chart areas and series. It provides methods for positioning and
/// drawing all chart elements.
/// </summary>
internal class ChartPicture : ChartElement, IServiceProvider, IDisposable
{
    #region Fields

    /// <summary>
    /// Indicates that chart exceptions should be suppressed.
    /// </summary>

    // Chart Graphic object
    internal ChartGraphics ChartGraph { get; }

    private int _borderWidth = 1;
    private int _width = 300;
    private int _height = 300;
    internal readonly HotRegionsList hotRegionsList;

    // Annotation smart labels class
    internal AnnotationSmartLabel annotationSmartLabel = new AnnotationSmartLabel();

    // Chart picture events
    internal event EventHandler<ChartPaintEventArgs> BeforePaint;

    internal event EventHandler<ChartPaintEventArgs> AfterPaint;

    // Chart title position rectangle
    private RectangleF _titlePosition = RectangleF.Empty;

    // Element spacing size
    internal const float elementSpacing = 3F;

    // Maximum size of the font in percentage
    internal const float maxTitleSize = 15F;

    // Printing indicator
    internal bool isPrinting;

    // Indicates chart selection mode
    internal bool isSelectionMode;

    // Position of the chart 3D border
    private RectangleF _chartBorderPosition = RectangleF.Empty;

    // Saving As Image indicator
    internal bool isSavingAsImage;

    // Indicates that chart background is restored from the double buffer
    // prior to drawing top level objects like annotations, cursors and selection.
    internal bool backgroundRestored;

    // Buffered image of non-top level chart elements
    internal Bitmap nonTopLevelChartBuffer;

    private bool _disposedValue;

    #endregion Fields

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="container">Service container</param>
    public ChartPicture(IServiceContainer container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(SR.ExceptionInvalidServiceContainer);
        }

        // Create and set Common Elements
        Common = new CommonElements(container);
        ChartGraph = new ChartGraphics(Common);
        hotRegionsList = new HotRegionsList(Common);

        // Create border properties class
        BorderSkin = new BorderSkin(this);

        // Create a collection of chart areas
        ChartAreas = new ChartAreaCollection(this);

        // Create a collection of legends
        Legends = new LegendCollection(this);

        // Create a collection of titles
        Titles = new TitleCollection(this);

        // Create a collection of annotations
        Annotations = new AnnotationCollection(this);

        // Set Common elements for data manipulator
        DataManipulator.Common = Common;
    }

    /// <summary>
    /// Returns Chart service object
    /// </summary>
    /// <param name="serviceType">Service AxisName</param>
    /// <returns>Chart picture</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    object IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType == typeof(ChartPicture))
        {
            return this;
        }

        throw new ArgumentException(SR.ExceptionChartPictureUnsupportedType(serviceType.ToString()));
    }

    #endregion Constructors

    #region Painting and selection methods

    /// <summary>
    /// Performs empty painting.
    /// </summary>
    internal void PaintOffScreen()
    {
        // Check chart size
        // NOTE: Fixes issue #4733
        if (this.Width <= 0 || this.Height <= 0)
        {
            return;
        }

        // Set process Mode to hot regions
        this.Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;
        this.Common.HotRegionsList.hitTestCalled = true;

        // Enable selection mode
        this.isSelectionMode = true;

        // Hot Region list does not exist. Create the list.
        //this.common.HotRegionsList.List = new ArrayList();
        this.Common.HotRegionsList.Clear();

        // Create a new bitmap
        Bitmap image = new Bitmap(Math.Max(1, Width), Math.Max(1, Height));

        // Creates a new Graphics object from the
        // specified Image object.
        Graphics offScreen = Graphics.FromImage(image);

        // Connect Graphics object with Chart Graphics object
        ChartGraph.Graphics = offScreen;

        // Remember the previous dirty flag
        bool oldDirtyFlag = this.Common.Chart.dirtyFlag;

        Paint(ChartGraph.Graphics, false);

        image.Dispose();

        // Restore the previous dirty flag
        this.Common.Chart.dirtyFlag = oldDirtyFlag;

        // Disable selection mode
        this.isSelectionMode = false;

        // Set process Mode to hot regions
        this.Common.HotRegionsList.ProcessChartMode |= ProcessMode.HotRegions;
    }

    /// <summary>
    /// Gets text rendering quality.
    /// </summary>
    /// <returns>Text rendering quality.</returns>
    internal TextRenderingHint GetTextRenderingHint()
    {
        TextRenderingHint result;
        if ((this.AntiAliasing & AntiAliasingStyles.Text) == AntiAliasingStyles.Text)
        {
            result = TextRenderingHint.ClearTypeGridFit;
            if (this.TextAntiAliasingQuality == TextAntiAliasingQuality.Normal)
            {
                result = TextRenderingHint.AntiAlias;
            }
            else if (this.TextAntiAliasingQuality == TextAntiAliasingQuality.SystemDefault)
            {
                result = TextRenderingHint.SystemDefault;
            }
        }
        else
        {
            result = TextRenderingHint.SingleBitPerPixelGridFit;
        }

        return result;
    }

    internal bool GetBorderSkinVisibility()
    {
        return BorderSkin.SkinStyle != BorderSkinStyle.None && this.Width > 20 && this.Height > 20;
    }

    /// <summary>
    /// This function paints a chart.
    /// </summary>
    /// <param name="graph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
    /// <param name="paintTopLevelElementOnly">Indicates that only chart top level elements like cursors, selection or annotation objects must be redrawn.</param>
    internal void Paint(
        Graphics graph,
        bool paintTopLevelElementOnly)
    {
        // Reset restored and saved background flags
        this.backgroundRestored = false;

        // Reset Annotation Smart Labels
        this.annotationSmartLabel.Reset();

        // Do not draw the control if size is less than 5 pixel
        if (this.Width < 5 || this.Height < 5)
        {
            return;
        }

        bool resetHotRegionList = false;

        if (
            this.Common.HotRegionsList.hitTestCalled
            || IsToolTipsEnabled()
            )
        {
            Common.HotRegionsList.ProcessChartMode = ProcessMode.HotRegions | ProcessMode.Paint;

            this.Common.HotRegionsList.hitTestCalled = false;

            // Clear list of hot regions
            if (paintTopLevelElementOnly)
            {
                // If repainting only top level elements (annotations) -
                // clear top level objects hot regions only
                for (int index = 0; index < this.Common.HotRegionsList.List.Count; index++)
                {
                    HotRegion region = this.Common.HotRegionsList.List[index];
                    if (region.Type == ChartElementType.Annotation)
                    {
                        this.Common.HotRegionsList.List.RemoveAt(index);
                        --index;
                        region.Dispose();
                    }
                }
            }
            else
            {
                // If repainting whole chart - clear all hot regions
                resetHotRegionList = true;
            }
        }
        else
        {
            Common.HotRegionsList.ProcessChartMode = ProcessMode.Paint;

            // If repainting whole chart - clear all hot regions
            resetHotRegionList = true;
        }

        // Reset hot region list
        if (resetHotRegionList)
        {
            this.Common.HotRegionsList.Clear();
        }

        // Check if control was data bound
        if (this is ChartImage chartImage && !chartImage.boundToDataSource)
        {
            if (this.Common != null && this.Common.Chart != null && !this.Common.Chart.IsDesignMode())
            {
                this.Common.Chart.DataBind();
            }
        }

        // Connect Graphics object with Chart Graphics object
        ChartGraph.Graphics = graph;

        Common.graph = ChartGraph;

        // Set anti alias mode
        ChartGraph.AntiAliasing = AntiAliasing;
        ChartGraph.softShadows = IsSoftShadows;
        ChartGraph.TextRenderingHint = GetTextRenderingHint();

        try
        {
            // Check if only chart area cursors and annotations must be redrawn
            if (!paintTopLevelElementOnly)
            {
                // Fire Before Paint event
                OnBeforePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

                // Flag indicates that resize method should be called
                // after adjusting the intervals in 3D charts
                bool resizeAfterIntervalAdjusting = false;

                // RecalculateAxesScale paint chart areas
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.Set3DAnglesAndReverseMode();
                        area.SetTempValues();
                        area.ReCalcInternal();

                        // Resize should be called the second time
                        if (area.Area3DStyle.Enable3D && !area.chartAreaIsCurcular)
                        {
                            resizeAfterIntervalAdjusting = true;
                        }
                    }
                }

                // Call Customize event
                this.Common.Chart.CallOnCustomize();

                // Resize picture
                Resize(ChartGraph, resizeAfterIntervalAdjusting);

                // This code is introduce because labels has to
                // be changed when scene is rotated.
                bool intervalReCalculated = false;
                foreach (ChartArea area in ChartAreas)
                {
                    if (area.Area3DStyle.Enable3D &&
                        !area.chartAreaIsCurcular

                        && area.Visible

                        )

                    {
                        // Make correction for interval in 3D space
                        intervalReCalculated = true;
                        area.Estimate3DInterval(ChartGraph);
                        area.ReCalcInternal();
                    }
                }

                // Resize chart areas after updating 3D interval
                if (resizeAfterIntervalAdjusting)
                {
                    // NOTE: Fixes issue #6808.
                    // In 3D chart area interval will be changed to compensate for the axis rotation angle.
                    // This will cause all standard labels to be changed. We need to call the customize event
                    // the second time to give user a chance to modify those labels.
                    if (intervalReCalculated)
                    {
                        // Call Customize event
                        this.Common.Chart.CallOnCustomize();
                    }

                    // Resize chart elements
                    Resize(ChartGraph);
                }

                //***********************************************************************
                //** Draw chart 3D border
                //***********************************************************************
                if (GetBorderSkinVisibility())
                {
                    // Fill rectangle with page color
                    ChartGraph.FillRectangleAbs(new RectangleF(0, 0, Width - 1, Height - 1),
                        BorderSkin.PageColor,
                        ChartHatchStyle.None,
                        string.Empty,
                        ChartImageWrapMode.Tile,
                        Color.Empty,
                        ChartImageAlignmentStyle.Center,
                        GradientStyle.None,
                        Color.Empty,
                        BorderSkin.PageColor,
                        1,
                        ChartDashStyle.Solid,
                        PenAlignment.Inset);

                    // Draw 3D border
                    ChartGraph.Draw3DBorderAbs(
                        BorderSkin,
                        this._chartBorderPosition,
                        BackColor,
                        BackHatchStyle,
                        BackImage,
                        BackImageWrapMode,
                        BackImageTransparentColor,
                        BackImageAlignment,
                        BackGradientStyle,
                        BackSecondaryColor,
                        BorderColor,
                        BorderWidth,
                        BorderDashStyle);
                }

                // Paint Background
                else
                {
                    ChartGraph.FillRectangleAbs(new RectangleF(0, 0, Width - 1, Height - 1),
                        BackColor,
                        BackHatchStyle,
                        BackImage,
                        BackImageWrapMode,
                        BackImageTransparentColor,
                        BackImageAlignment,
                        BackGradientStyle,
                        BackSecondaryColor,
                        BorderColor,
                        BorderWidth,
                        BorderDashStyle,
                        PenAlignment.Inset);
                }

                // Call BackPaint event
                this.Chart.CallOnPrePaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

                // Call paint function for each chart area.
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.Paint(ChartGraph);
                    }
                }

                // This code is introduced because of GetPointsInterval method,
                // which is very time consuming. There is no reason to calculate
                // interval after painting.
                foreach (ChartArea area in ChartAreas)
                {
                    // Reset interval data
                    area.intervalData = double.NaN;
                }

                // Draw Legends
                foreach (Legend legendCurrent in this.Legends)
                {
                    legendCurrent.Paint(ChartGraph);
                }

                // Draw chart titles from the collection
                foreach (Title titleCurrent in this.Titles)
                {
                    titleCurrent.Paint(ChartGraph);
                }

                // Call Paint event
                this.Chart.CallOnPostPaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));
            }

            // Draw annotation objects
            this.Annotations.Paint(ChartGraph, paintTopLevelElementOnly);

            // Draw chart areas cursors in all areas.
            // Only if not in selection
            if (!this.isSelectionMode)
            {
                foreach (ChartArea area in ChartAreas)
                {
                    // Check if area is visible
                    if (area.Visible)

                    {
                        area.PaintCursors(ChartGraph, paintTopLevelElementOnly);
                    }
                }
            }

            // Return default values
            foreach (ChartArea area in ChartAreas)
            {
                // Check if area is visible
                if (area.Visible)

                {
                    area.Restore3DAnglesAndReverseMode();
                    area.GetTempValues();
                }
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            // Fire After Paint event
            OnAfterPaint(new ChartPaintEventArgs(this.Chart, this.ChartGraph, this.Common, new ElementPosition(0, 0, 100, 100)));

            // Restore temp values for each chart area
            foreach (ChartArea area in ChartAreas)
            {
                // Check if area is visible
                if (area.Visible)

                {
                    area.Restore3DAnglesAndReverseMode();
                    area.GetTempValues();
                }
            }
        }
    }

    /// <summary>
    /// Invoke before paint delegates.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnBeforePaint(ChartPaintEventArgs e)
    {
        //Invokes the delegates.
        BeforePaint?.Invoke(this, e);
    }

    /// <summary>
    /// Invoke after paint delegates.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnAfterPaint(ChartPaintEventArgs e)
    {
        //Invokes the delegates.
        AfterPaint?.Invoke(this, e);
    }

    internal override void Invalidate()
    {
        base.Invalidate();

        Chart?.Invalidate();
    }

    #endregion Painting and selection methods

    #region Resizing methods

    /// <summary>
    /// Resize the chart picture.
    /// </summary>
    /// <param name="chartGraph">Chart graphics.</param>
    public void Resize(ChartGraphics chartGraph)
    {
        Resize(chartGraph, false);
    }

    /// <summary>
    /// Resize the chart picture.
    /// </summary>
    /// <param name="chartGraph">Chart graphics.</param>
    /// <param name="calcAreaPositionOnly">Indicates that only chart area position is calculated.</param>
    public void Resize(ChartGraphics chartGraph, bool calcAreaPositionOnly)
    {
        // Set the chart size for Common elements
        Common.Width = _width;
        Common.Height = _height;

        // Set the chart size for Chart graphics
        chartGraph.SetPictureSize(_width, _height);

        // Initialize chart area(s) rectangle
        RectangleF chartAreasRectangle = new RectangleF(0, 0, _width - 1, _height - 1);
        chartAreasRectangle = chartGraph.GetRelativeRectangle(chartAreasRectangle);

        //******************************************************
        //** Get the 3D border interface
        //******************************************************
        _titlePosition = RectangleF.Empty;
        bool titleInBorder = false;

        if (BorderSkin.SkinStyle != BorderSkinStyle.None)
        {
            // Set border size
            this._chartBorderPosition = chartGraph.GetAbsoluteRectangle(chartAreasRectangle);

            // Get border interface
            IBorderType border3D = Common.BorderTypeRegistry.GetBorderType(BorderSkin.SkinStyle.ToString());
            if (border3D != null)
            {
                border3D.Resolution = chartGraph.Graphics.DpiX * Chart.DPIScale;
                // Check if title should be displayed in the border
                titleInBorder = border3D.GetTitlePositionInBorder() != RectangleF.Empty;
                _titlePosition = chartGraph.GetRelativeRectangle(border3D.GetTitlePositionInBorder());
                _titlePosition.Width = chartAreasRectangle.Width - _titlePosition.Width;

                // Adjust are position to the border size
                border3D.AdjustAreasPosition(chartGraph, ref chartAreasRectangle);
            }
        }

        //******************************************************
        //** Calculate position of all titles in the collection
        //******************************************************
        RectangleF frameTitlePosition = RectangleF.Empty;
        if (titleInBorder)
        {
            frameTitlePosition = new RectangleF(_titlePosition.Location, _titlePosition.Size);
        }

        foreach (Title title in this.Titles)
        {
            if (title.DockedToChartArea == Constants.NotSetValue &&
                title.Position.Auto &&
                title.Visible)
            {
                title.CalcTitlePosition(chartGraph, ref chartAreasRectangle, ref frameTitlePosition, elementSpacing);
            }
        }

        //******************************************************
        //** Calculate position of all legends in the collection
        //******************************************************
        this.Legends.CalcLegendPosition(chartGraph, ref chartAreasRectangle, elementSpacing);

        //******************************************************
        //** Calculate position of the chart area(s)
        //******************************************************
        chartAreasRectangle.Width -= elementSpacing;
        chartAreasRectangle.Height -= elementSpacing;
        RectangleF areaPosition = new RectangleF();

        // Get number of chart areas that requires automatic positioning
        int areaNumber = 0;
        foreach (ChartArea area in ChartAreas)
        {
            // Check if area is visible
            if (area.Visible)

            {
                if (area.Position.Auto)
                {
                    ++areaNumber;
                }
            }
        }

        // Calculate how many columns & rows of areas we going to have
        int areaColumns = (int)Math.Floor(Math.Sqrt(areaNumber));
        if (areaColumns < 1)
        {
            areaColumns = 1;
        }

        int areaRows = (int)Math.Ceiling(areaNumber / (float)areaColumns);

        // Set position for all areas
        int column = 0;
        int row = 0;
        foreach (ChartArea area in ChartAreas)
        {
            // Check if area is visible
            if (area.Visible)

            {
                if (area.Position.Auto)
                {
                    // Calculate area position
                    areaPosition.Width = chartAreasRectangle.Width / areaColumns - elementSpacing;
                    areaPosition.Height = chartAreasRectangle.Height / areaRows - elementSpacing;
                    areaPosition.X = chartAreasRectangle.X + column * (chartAreasRectangle.Width / areaColumns) + elementSpacing;
                    areaPosition.Y = chartAreasRectangle.Y + row * (chartAreasRectangle.Height / areaRows) + elementSpacing;

                    // Calculate position of all titles in the collection docked outside of the chart area
                    TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref areaPosition, elementSpacing);

                    // Calculate position of the legend if it's docked outside of the chart area
                    this.Legends.CalcOutsideLegendPosition(chartGraph, area, ref areaPosition, elementSpacing);

                    // Set area position without changing the Auto flag
                    area.Position.SetPositionNoAuto(areaPosition.X, areaPosition.Y, areaPosition.Width, areaPosition.Height);

                    // Go to next area
                    ++row;
                    if (row >= areaRows)
                    {
                        row = 0;
                        ++column;
                    }
                }
                else
                {
                    RectangleF rect = area.Position.ToRectangleF();

                    // Calculate position of all titles in the collection docked outside of the chart area
                    TitleCollection.CalcOutsideTitlePosition(this, chartGraph, area, ref rect, elementSpacing);

                    // Calculate position of the legend if it's docked outside of the chart area
                    this.Legends.CalcOutsideLegendPosition(chartGraph, area, ref rect, elementSpacing);
                }
            }
        }

        //******************************************************
        //** Align chart areas Position if required
        //******************************************************
        AlignChartAreasPosition();

        //********************************************************
        //** Check if only chart area position must be calculated.
        //********************************************************
        if (!calcAreaPositionOnly)
        {
            //******************************************************
            //** Call Resize function for each chart area.
            //******************************************************
            foreach (ChartArea area in ChartAreas)
            {
                // Check if area is visible
                if (area.Visible)

                {
                    area.Resize(chartGraph);
                }
            }

            //******************************************************
            //** Align chart areas InnerPlotPosition if required
            //******************************************************
            AlignChartAreas(AreaAlignmentStyles.PlotPosition);

            //******************************************************
            //** Calculate position of the legend if it's inside
            //** chart plotting area
            //******************************************************

            // Calculate position of all titles in the collection docked outside of the chart area
            TitleCollection.CalcInsideTitlePosition(this, chartGraph, elementSpacing);

            this.Legends.CalcInsideLegendPosition(chartGraph, elementSpacing);
        }
    }

    /// <summary>
    /// Minimum and maximum do not have to be calculated
    /// from data series every time. It is very time
    /// consuming. Minimum and maximum are buffered
    /// and only when this flags are set Minimum and
    /// Maximum are refreshed from data.
    /// </summary>
    internal void ResetMinMaxFromData()
    {
        if (ChartAreas != null)
        {
            // Call ResetMinMaxFromData function for each chart area.
            foreach (ChartArea area in ChartAreas)
            {
                // Check if area is visible
                if (area.Visible)
                {
                    area.ResetMinMaxFromData();
                }
            }
        }
    }

    /// <summary>
    /// RecalculateAxesScale the chart picture.
    /// </summary>
    public void Recalculate()
    {
        // Call ReCalc function for each chart area.
        foreach (ChartArea area in ChartAreas)
        {
            // Check if area is visible
            if (area.Visible)

            {
                area.ReCalcInternal();
            }
        }
    }

    #endregion Resizing methods

    #region Chart picture properties

    // VSTS 96787-Text Direction (RTL/LTR)
    /// <summary>
    /// Gets or sets the RightToLeft type.
    /// </summary>
    [
    DefaultValue(RightToLeft.No)
    ]
    public RightToLeft RightToLeft
    {
        get => this.Common.Chart.RightToLeft;
        set => this.Common.Chart.RightToLeft = value;
    }

    /// <summary>
    /// Indicates that non-critical chart exceptions will be suppressed.
    /// </summary>
    [
    SRCategory("CategoryAttributeMisc"),
    DefaultValue(false),
    SRDescription("DescriptionAttributeSuppressExceptions"),
    ]
    internal bool SuppressExceptions { set; get; }

    /// <summary>
    /// Chart border skin style.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(BorderSkinStyle.None),
    SRDescription("DescriptionAttributeBorderSkin"),
    ]
    public BorderSkin BorderSkin { get; set; }

    /// <summary>
    /// Reference to chart area collection
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeChartAreas"),
    Editor("ChartCollectionEditor", typeof(UITypeEditor))
    ]
    public ChartAreaCollection ChartAreas { get; }

    /// <summary>
    /// Chart legend collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeLegends"),
    Editor("LegendCellCollectionEditor", typeof(UITypeEditor))
    ]
    public LegendCollection Legends { get; }

    /// <summary>
    /// Chart title collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeCharttitle"),
    SRDescription("DescriptionAttributeTitles"),
    Editor("ChartCollectionEditor", typeof(UITypeEditor))
    ]
    public TitleCollection Titles { get; }

    /// <summary>
    /// Chart annotation collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeAnnotations3"),
    Editor("AnnotationCollectionEditor", (typeof(UITypeEditor)))
    ]
    public AnnotationCollection Annotations { get; }

    /// <summary>
    /// Background color for the Chart
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), "White"),
    SRDescription("DescriptionAttributeBackColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", (typeof(UITypeEditor)))
    ]
    public Color BackColor { get; set; } = Color.White;

    /// <summary>
    /// Border color for the Chart
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), "White"),
    SRDescription("DescriptionAttributeBorderColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", (typeof(UITypeEditor)))
    ]
    public Color BorderColor { get; set; } = Color.White;

    /// <summary>
    /// Chart width
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(300),
    SRDescription("DescriptionAttributeWidth"),
    ]
    public int Width
    {
        get => _width;
        set
        {
            this.InspectChartDimensions(value, this.Height);
            _width = value;
            Common.Width = _width;
        }
    }

    /// <summary>
    /// Series Data Manipulator
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    SRDescription("DescriptionAttributeDataManipulator"),
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibility(SerializationVisibility.Hidden)
    ]
    public DataManipulator DataManipulator { get; } = new DataManipulator();

    /// <summary>
    /// Chart height
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(300),
    SRDescription("DescriptionAttributeHeight3"),
    ]
    public int Height
    {
        get => _height;
        set
        {
            this.InspectChartDimensions(this.Width, value);
            _height = value;
            Common.Height = value;
        }
    }

    /// <summary>
    /// Back Hatch style
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartHatchStyle.None),
    SRDescription("DescriptionAttributeBackHatchStyle"),
    Editor("HatchStyleEditor", typeof(UITypeEditor))
    ]
    public ChartHatchStyle BackHatchStyle { get; set; } = ChartHatchStyle.None;

    /// <summary>
    /// Chart area background image
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(""),
    SRDescription("DescriptionAttributeBackImage"),
    Editor("ImageValueEditor", typeof(UITypeEditor)),
    NotifyParentProperty(true)
    ]
    public string BackImage { get; set; } = string.Empty;

    /// <summary>
    /// Chart area background image drawing mode.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartImageWrapMode.Tile),
    NotifyParentProperty(true),
    SRDescription("DescriptionAttributeImageWrapMode"),
    ]
    public ChartImageWrapMode BackImageWrapMode { get; set; } = ChartImageWrapMode.Tile;

    /// <summary>
    /// Background image transparent color.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), ""),
    NotifyParentProperty(true),
    SRDescription("DescriptionAttributeImageTransparentColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color BackImageTransparentColor { get; set; } = Color.Empty;

    /// <summary>
    /// Background image alignment used by ClampUnscale drawing mode.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartImageAlignmentStyle.TopLeft),
    NotifyParentProperty(true),
    SRDescription("DescriptionAttributeBackImageAlign"),
    ]
    public ChartImageAlignmentStyle BackImageAlignment { get; set; } = ChartImageAlignmentStyle.TopLeft;

    /// <summary>
    /// Indicates that smoothing is applied while drawing shadows.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(true),
    SRDescription("DescriptionAttributeSoftShadows3"),
    ]
    public bool IsSoftShadows { get; set; } = true;

    /// <summary>
    /// Specifies whether smoothing (AntiAliasing) is applied while drawing chart.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(typeof(AntiAliasingStyles), "All"),
    SRDescription("DescriptionAttributeAntiAlias"),
    ]
    public AntiAliasingStyles AntiAliasing { get; set; } = AntiAliasingStyles.All;

    /// <summary>
    /// Specifies the quality of text AntiAliasing.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(typeof(TextAntiAliasingQuality), "High"),
    SRDescription("DescriptionAttributeTextAntiAliasingQuality"),
    ]
    public TextAntiAliasingQuality TextAntiAliasingQuality { get; set; } = TextAntiAliasingQuality.High;

    /// <summary>
    /// A type for the background gradient
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(GradientStyle.None),
    SRDescription("DescriptionAttributeBackGradientStyle"),
    Editor("GradientEditor", typeof(UITypeEditor))
    ]
    public GradientStyle BackGradientStyle { get; set; } = GradientStyle.None;

    /// <summary>
    /// The second color which is used for a gradient
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), ""),
    SRDescription("DescriptionAttributeBackSecondaryColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color BackSecondaryColor { get; set; } = Color.Empty;

    /// <summary>
    /// The width of the border line
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(1),
    SRDescription("DescriptionAttributeChart_BorderlineWidth"),
    ]
    public int BorderWidth
    {
        get => _borderWidth;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionChartBorderIsNegative);
            }

            _borderWidth = value;
        }
    }

    /// <summary>
    /// The style of the border line
    /// </summary>
    [

    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartDashStyle.NotSet),
    SRDescription("DescriptionAttributeBorderDashStyle"),
    ]
    public ChartDashStyle BorderDashStyle { get; set; } = ChartDashStyle.NotSet;

    /// <summary>
    /// Gets the font cache.
    /// </summary>
    /// <value>The font cache.</value>
    internal FontCache FontCache { get; } = new();

    #endregion Chart picture properties

    #region Chart areas alignment methods

    /// <summary>
    /// Checks if any of the chart areas are aligned.
    /// Also checks if the chart ares name in AlignWithChartArea property is valid.
    /// </summary>
    /// <returns>True if at least one area requires alignment.</returns>
    private bool IsAreasAlignmentRequired()
    {
        bool alignmentRequired = false;

        // Loop through all chart areas
        foreach (ChartArea area in this.ChartAreas)
        {
            // Check if chart area is visible
            if (area.Visible)

            {
                // Check if area is aligned
                if (area.AlignWithChartArea != Constants.NotSetValue)
                {
                    alignmentRequired = true;

                    // Check the chart area used for alignment
                    if (this.ChartAreas.IndexOf(area.AlignWithChartArea) < 0)
                    {
                        throw new InvalidOperationException(SR.ExceptionChartAreaNameReferenceInvalid(area.Name, area.AlignWithChartArea));
                    }
                }
            }
        }

        return alignmentRequired;
    }

    /// <summary>
    /// Creates a list of the aligned chart areas.
    /// </summary>
    /// <param name="masterArea">Master chart area.</param>
    /// <param name="type">Alignment type.</param>
    /// <param name="orientation">Vertical or Horizontal orientation.</param>
    /// <returns>List of areas that area aligned to the master area.</returns>
    private ArrayList GetAlignedAreasGroup(ChartArea masterArea, AreaAlignmentStyles type, AreaAlignmentOrientations orientation)
    {
        ArrayList areaList = new ArrayList();

        // Loop through the chart areas and get the ones aligned with specified master area
        foreach (ChartArea area in this.ChartAreas)
        {
            // Check if chart area is visible
            if (area.Visible)

            {
                if (area.Name != masterArea.Name &&
                    area.AlignWithChartArea == masterArea.Name &&
                    (area.AlignmentStyle & type) == type &&
                    (area.AlignmentOrientation & orientation) == orientation)
                {
                    // Add client area into the list
                    areaList.Add(area);
                }
            }
        }

        // If list is not empty insert "master" area in the beginning
        if (areaList.Count > 0)
        {
            areaList.Insert(0, masterArea);
        }

        return areaList;
    }

    /// <summary>
    /// Performs specified type of alignment for the chart areas.
    /// </summary>
    /// <param name="type">Alignment type required.</param>
    internal void AlignChartAreas(AreaAlignmentStyles type)
    {
        // Check if alignment required
        if (IsAreasAlignmentRequired())
        {
            // Loop through all chart areas
            foreach (ChartArea area in this.ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible)

                {
                    // Get vertical areas alignment group using current area as a master
                    ArrayList alignGroup = GetAlignedAreasGroup(
                        area,
                        type,
                        AreaAlignmentOrientations.Vertical);

                    // Align each area in the group
                    if (alignGroup.Count > 0)
                    {
                        AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Vertical);
                    }

                    // Get horizontal areas alignment group using current area as a master
                    alignGroup = GetAlignedAreasGroup(
                        area,
                        type,
                        AreaAlignmentOrientations.Horizontal);

                    // Align each area in the group
                    if (alignGroup.Count > 0)
                    {
                        AlignChartAreasPlotPosition(alignGroup, AreaAlignmentOrientations.Horizontal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Align inner plot position of the chart areas in the group.
    /// </summary>
    /// <param name="areasGroup">List of areas in the group.</param>
    /// <param name="orientation">Group orientation.</param>
    private void AlignChartAreasPlotPosition(ArrayList areasGroup, AreaAlignmentOrientations orientation)
    {
        //****************************************************************
        //** Find the smallest size of the inner plot
        //****************************************************************
        RectangleF areaPlotPosition = ((ChartArea)areasGroup[0]).PlotAreaPosition.ToRectangleF();
        foreach (ChartArea area in areasGroup)
        {
            if (area.PlotAreaPosition.X > areaPlotPosition.X)
            {
                areaPlotPosition.X += area.PlotAreaPosition.X - areaPlotPosition.X;
                areaPlotPosition.Width -= area.PlotAreaPosition.X - areaPlotPosition.X;
            }

            if (area.PlotAreaPosition.Y > areaPlotPosition.Y)
            {
                areaPlotPosition.Y += area.PlotAreaPosition.Y - areaPlotPosition.Y;
                areaPlotPosition.Height -= area.PlotAreaPosition.Y - areaPlotPosition.Y;
            }

            if (area.PlotAreaPosition.Right < areaPlotPosition.Right)
            {
                areaPlotPosition.Width -= areaPlotPosition.Right - area.PlotAreaPosition.Right;
                if (areaPlotPosition.Width < 5)
                {
                    areaPlotPosition.Width = 5;
                }
            }

            if (area.PlotAreaPosition.Bottom < areaPlotPosition.Bottom)
            {
                areaPlotPosition.Height -= areaPlotPosition.Bottom - area.PlotAreaPosition.Bottom;
                if (areaPlotPosition.Height < 5)
                {
                    areaPlotPosition.Height = 5;
                }
            }
        }

        //****************************************************************
        //** Align inner plot position for all areas
        //****************************************************************
        foreach (ChartArea area in areasGroup)
        {
            // Get current plot position of the area
            RectangleF rect = area.PlotAreaPosition.ToRectangleF();

            // Adjust area position
            if ((orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
            {
                rect.X = areaPlotPosition.X;
                rect.Width = areaPlotPosition.Width;
            }

            if ((orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
            {
                rect.Y = areaPlotPosition.Y;
                rect.Height = areaPlotPosition.Height;
            }

            // Set new plot position in coordinates relative to chart picture
            area.PlotAreaPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);

            // Set new plot position in coordinates relative to chart area position
            rect.X = (rect.X - area.Position.X) / area.Position.Width * 100f;
            rect.Y = (rect.Y - area.Position.Y) / area.Position.Height * 100f;
            rect.Width = rect.Width / area.Position.Width * 100f;
            rect.Height = rect.Height / area.Position.Height * 100f;
            area.InnerPlotPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);

            if ((orientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
            {
                area.AxisX2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                area.AxisX.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
            }

            if ((orientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
            {
                area.AxisY2.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
                area.AxisY.AdjustLabelFontAtSecondPass(ChartGraph, area.InnerPlotPosition.Auto);
            }
        }
    }

    /// <summary>
    /// Aligns positions of the chart areas.
    /// </summary>
    private void AlignChartAreasPosition()
    {
        // Check if alignment required
        if (IsAreasAlignmentRequired())
        {
            // Loop through all chart areas
            foreach (ChartArea area in this.ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible)

                {
                    // Check if area is aligned by Position to any other area
                    if (area.AlignWithChartArea != Constants.NotSetValue && (area.AlignmentStyle & AreaAlignmentStyles.Position) == AreaAlignmentStyles.Position)
                    {
                        // Get current area position
                        RectangleF areaPosition = area.Position.ToRectangleF();

                        // Get master chart area
                        ChartArea masterArea = this.ChartAreas[area.AlignWithChartArea];

                        // Vertical alignment
                        if ((area.AlignmentOrientation & AreaAlignmentOrientations.Vertical) == AreaAlignmentOrientations.Vertical)
                        {
                            // Align area position
                            areaPosition.X = masterArea.Position.X;
                            areaPosition.Width = masterArea.Position.Width;
                        }

                        // Horizontal alignment
                        if ((area.AlignmentOrientation & AreaAlignmentOrientations.Horizontal) == AreaAlignmentOrientations.Horizontal)
                        {
                            // Align area position
                            areaPosition.Y = masterArea.Position.Y;
                            areaPosition.Height = masterArea.Position.Height;
                        }

                        // Set new position
                        area.Position.SetPositionNoAuto(areaPosition.X, areaPosition.Y, areaPosition.Width, areaPosition.Height);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Align chart areas cursor.
    /// </summary>
    /// <param name="changedArea">Changed chart area.</param>
    /// <param name="orientation">Orientation of the changed cursor.</param>
    /// <param name="selectionChanged">AxisName of change cursor or selection.</param>
    internal void AlignChartAreasCursor(ChartArea changedArea, AreaAlignmentOrientations orientation, bool selectionChanged)
    {
        // Check if alignment required
        if (IsAreasAlignmentRequired())
        {
            // Loop through all chart areas
            foreach (ChartArea area in this.ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible)

                {
                    // Get vertical areas alignment group using current area as a master
                    ArrayList alignGroup = GetAlignedAreasGroup(
                        area,
                        AreaAlignmentStyles.Cursor,
                        orientation);

                    // Align each area in the group if it contains changed area
                    if (alignGroup.Contains(changedArea))
                    {
                        // Set cursor position for all areas in the group
                        foreach (ChartArea groupArea in alignGroup)
                        {
                            groupArea.alignmentInProcess = true;

                            if (orientation == AreaAlignmentOrientations.Vertical)
                            {
                                if (selectionChanged)
                                {
                                    groupArea.CursorX.SelectionStart = changedArea.CursorX.SelectionStart;
                                    groupArea.CursorX.SelectionEnd = changedArea.CursorX.SelectionEnd;
                                }
                                else
                                {
                                    groupArea.CursorX.Position = changedArea.CursorX.Position;
                                }
                            }

                            if (orientation == AreaAlignmentOrientations.Horizontal)
                            {
                                if (selectionChanged)
                                {
                                    groupArea.CursorY.SelectionStart = changedArea.CursorY.SelectionStart;
                                    groupArea.CursorY.SelectionEnd = changedArea.CursorY.SelectionEnd;
                                }
                                else
                                {
                                    groupArea.CursorY.Position = changedArea.CursorY.Position;
                                }
                            }

                            groupArea.alignmentInProcess = false;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// One of the chart areas was zoomed by the user.
    /// </summary>
    /// <param name="changedArea">Changed chart area.</param>
    /// <param name="orientation">Orientation of the changed scaleView.</param>
    /// <param name="disposeBufferBitmap">Area double buffer image must be disposed.</param>
    internal void AlignChartAreasZoomed(ChartArea changedArea, AreaAlignmentOrientations orientation, bool disposeBufferBitmap)
    {
        // Check if alignment required
        if (IsAreasAlignmentRequired())
        {
            // Loop through all chart areas
            foreach (ChartArea area in this.ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible)

                {
                    // Get vertical areas alignment group using current area as a master
                    ArrayList alignGroup = GetAlignedAreasGroup(
                        area,
                        AreaAlignmentStyles.AxesView,
                        orientation);

                    // Align each area in the group if it contains changed area
                    if (alignGroup.Contains(changedArea))
                    {
                        // Set cursor position for all areas in the group
                        foreach (ChartArea groupArea in alignGroup)
                        {
                            // Clear image buffer
                            if (groupArea.areaBufferBitmap != null && disposeBufferBitmap)
                            {
                                groupArea.areaBufferBitmap.Dispose();
                                groupArea.areaBufferBitmap = null;
                            }

                            if (orientation == AreaAlignmentOrientations.Vertical)
                            {
                                groupArea.CursorX.SelectionStart = double.NaN;
                                groupArea.CursorX.SelectionEnd = double.NaN;
                            }

                            if (orientation == AreaAlignmentOrientations.Horizontal)
                            {
                                groupArea.CursorY.SelectionStart = double.NaN;
                                groupArea.CursorY.SelectionEnd = double.NaN;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Align chart areas axes views.
    /// </summary>
    /// <param name="changedArea">Changed chart area.</param>
    /// <param name="orientation">Orientation of the changed scaleView.</param>
    internal void AlignChartAreasAxesView(ChartArea changedArea, AreaAlignmentOrientations orientation)
    {
        // Check if alignment required
        if (IsAreasAlignmentRequired())
        {
            // Loop through all chart areas
            foreach (ChartArea area in this.ChartAreas)
            {
                // Check if chart area is visible
                if (area.Visible)

                {
                    // Get vertical areas alignment group using current area as a master
                    ArrayList alignGroup = GetAlignedAreasGroup(
                        area,
                        AreaAlignmentStyles.AxesView,
                        orientation);

                    // Align each area in the group if it contains changed area
                    if (alignGroup.Contains(changedArea))
                    {
                        // Set cursor position for all areas in the group
                        foreach (ChartArea groupArea in alignGroup)
                        {
                            groupArea.alignmentInProcess = true;

                            if (orientation == AreaAlignmentOrientations.Vertical)
                            {
                                groupArea.AxisX.ScaleView.Position = changedArea.AxisX.ScaleView.Position;
                                groupArea.AxisX.ScaleView.Size = changedArea.AxisX.ScaleView.Size;
                                groupArea.AxisX.ScaleView.SizeType = changedArea.AxisX.ScaleView.SizeType;

                                groupArea.AxisX2.ScaleView.Position = changedArea.AxisX2.ScaleView.Position;
                                groupArea.AxisX2.ScaleView.Size = changedArea.AxisX2.ScaleView.Size;
                                groupArea.AxisX2.ScaleView.SizeType = changedArea.AxisX2.ScaleView.SizeType;
                            }

                            if (orientation == AreaAlignmentOrientations.Horizontal)
                            {
                                groupArea.AxisY.ScaleView.Position = changedArea.AxisY.ScaleView.Position;
                                groupArea.AxisY.ScaleView.Size = changedArea.AxisY.ScaleView.Size;
                                groupArea.AxisY.ScaleView.SizeType = changedArea.AxisY.ScaleView.SizeType;

                                groupArea.AxisY2.ScaleView.Position = changedArea.AxisY2.ScaleView.Position;
                                groupArea.AxisY2.ScaleView.Size = changedArea.AxisY2.ScaleView.Size;
                                groupArea.AxisY2.ScaleView.SizeType = changedArea.AxisY2.ScaleView.SizeType;
                            }

                            groupArea.alignmentInProcess = false;
                        }
                    }
                }
            }
        }
    }

    #endregion Chart areas alignment methods

    #region Helper methods

    /// <summary>
    /// Inspects the chart dimensions.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    internal void InspectChartDimensions(int width, int height)
    {
        if (this.Chart.IsDesignMode() && (width * height > 100 * 1024 * 1024))
        {
            throw new ArgumentException(SR.ExceptionChartOutOfLimits);
        }

        if (width < 0)
        {
            throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan(nameof(Width), "0px"));
        }

        if (height < 0)
        {
            throw new ArgumentException(SR.ExceptionValueMustBeGreaterThan(nameof(Height), "0px"));
        }
    }

    /// <summary>
    /// Loads chart appearance template from file.
    /// </summary>
    /// <param name="name">Template file name to load from.</param>
    public void LoadTemplate(string name)
    {
        // Check arguments
        if (name == null)
            throw new ArgumentNullException(nameof(name));

        // Load template data into the stream
        Stream stream = new FileStream(name, FileMode.Open, FileAccess.Read);

        // Load template from stream
        LoadTemplate(stream);

        // Close template stream
        stream.Close();
    }

    /// <summary>
    /// Loads chart appearance template from stream.
    /// </summary>
    /// <param name="stream">Template stream to load from.</param>
    public void LoadTemplate(Stream stream)
    {
        // Check arguments
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        ChartSerializer serializer = (ChartSerializer)this.Common.container.GetService(typeof(ChartSerializer));
        if (serializer != null)
        {
            // Save previous serializer properties
            string oldSerializableContent = serializer.SerializableContent;
            string oldNonSerializableContent = serializer.NonSerializableContent;
            SerializationFormat oldFormat = serializer.Format;
            bool oldIgnoreUnknownXmlAttributes = serializer.IsUnknownAttributeIgnored;
            bool oldTemplateMode = serializer.IsTemplateMode;

            // Set serializer properties
            serializer.Content = SerializationContents.Appearance;
            serializer.SerializableContent += ",Chart.Titles,Chart.Annotations," +
                                              "Chart.Legends,Legend.CellColumns,Legend.CustomItems,LegendItem.Cells," +
                                              "Chart.Series,Series.*Style," +
                                              "Chart.ChartAreas,ChartArea.Axis*," +
                                              "Axis.*Grid,Axis.*TickMark, Axis.*Style," +
                                              "Axis.StripLines, Axis.CustomLabels";
            serializer.Format = SerializationFormat.Xml;
            serializer.IsUnknownAttributeIgnored = true;
            serializer.IsTemplateMode = true;

            try
            {
                // Load template
                serializer.Load(stream);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            finally
            {
                // Restore previous serializer properties
                serializer.SerializableContent = oldSerializableContent;
                serializer.NonSerializableContent = oldNonSerializableContent;
                serializer.Format = oldFormat;
                serializer.IsUnknownAttributeIgnored = oldIgnoreUnknownXmlAttributes;
                serializer.IsTemplateMode = oldTemplateMode;
            }
        }
    }

    /// <summary>
    /// Returns the default title from Titles collection.
    /// </summary>
    /// <param name="create">Create title if it doesn't exists.</param>
    /// <returns>Default title.</returns>
    internal Title GetDefaultTitle(bool create)
    {
        // Check if default title exists
        Title defaultTitle = null;
        foreach (Title title in this.Titles)
        {
            if (title.Name == "Default Title")
            {
                defaultTitle = title;
            }
        }

        // Create new default title
        if (defaultTitle == null && create)
        {
            defaultTitle = new Title
            {
                Name = "Default Title"
            };
            this.Titles.Insert(0, defaultTitle);
        }

        return defaultTitle;
    }

    /// <summary>
    /// Checks if tooltips are enabled
    /// </summary>
    /// <returns>true if tooltips enabled</returns>
    private bool IsToolTipsEnabled()
    {
        // Data series loop
        foreach (Series series in Common.DataManager.Series)
        {
            // Check series tooltips
            if (series.ToolTip.Length > 0)
            {
                // ToolTips enabled
                return true;
            }

            // Check series tooltips
            if (series.LegendToolTip.Length > 0 ||
                series.LabelToolTip.Length > 0)
            {
                // ToolTips enabled
                return true;
            }

            // Check point tooltips only for "non-Fast" chart types
            if (!series.IsFastChartType())
            {
                // Data point loop
                foreach (DataPoint point in series.Points)
                {
                    // ToolTip empty
                    if (point.ToolTip.Length > 0)
                    {
                        // ToolTips enabled
                        return true;
                    }
                    // ToolTip empty
                    if (point.LegendToolTip.Length > 0 ||
                        point.LabelToolTip.Length > 0)
                    {
                        // ToolTips enabled
                        return true;
                    }
                }
            }
        }

        // Legend items loop
        foreach (Legend legend in Legends)
        {
            if (!(legend.CustomItems?.Count > 0))
                continue;

            foreach (LegendItem legendItem in legend.CustomItems)
            {
                // ToolTip empty
                if (legendItem.ToolTip.Length > 0)
                {
                    return true;
                }
            }
        }

        // Title items loop
        foreach (Title title in Titles)
        {
            // ToolTip empty
            if (title.ToolTip.Length > 0)
            {
                return true;
            }
        }

        return false;
    }

    #endregion Helper methods

    #region IDisposable Members

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            ChartGraph.Dispose();
            ChartAreas.Dispose();
            hotRegionsList.Dispose();
            if (nonTopLevelChartBuffer is not null)
            {
                nonTopLevelChartBuffer.Dispose();
                nonTopLevelChartBuffer = null;
            }

            FontCache.Dispose();
        }

        _disposedValue = true;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Members
}

/// <summary>
/// Event arguments of Chart paint event.
/// </summary>
public class ChartPaintEventArgs : EventArgs
{
    #region Fields

    // Private fields
    private Chart _chart;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the chart element of the event.
    /// </summary>
    /// <value>The chart element.</value>
    public object ChartElement { get; }

    /// <summary>
    /// Gets the ChartGraphics object of the event.
    /// </summary>
    public ChartGraphics ChartGraphics { get; }

    /// <summary>
    /// Chart Common elements.
    /// </summary>
    internal CommonElements CommonElements { get; }

    /// <summary>
    /// Chart element position in relative coordinates of the event.
    /// </summary>
    public ElementPosition Position { get; }

    /// <summary>
    /// Chart object of the event.
    /// </summary>
    public Chart Chart
    {
        get
        {
            if (_chart == null && CommonElements != null)
            {
                _chart = CommonElements.Chart;
            }

            return _chart;
        }
    }

    #endregion Properties

    #region Methods

    /// <summary>
    /// Default constructor is not accessible
    /// </summary>
    private ChartPaintEventArgs()
    {
    }

    /// <summary>
    /// Paint event arguments constructor.
    /// </summary>
    /// <param name="chartElement">Chart element.</param>
    /// <param name="chartGraph">Chart graphics.</param>
    /// <param name="common">Common elements.</param>
    /// <param name="position">Position.</param>
    internal ChartPaintEventArgs(object chartElement, ChartGraphics chartGraph, CommonElements common, ElementPosition position)
    {
        this.ChartElement = chartElement;
        this.ChartGraphics = chartGraph;
        this.CommonElements = common;
        this.Position = position;
    }

    #endregion Methods
}

/// <summary>
/// Event arguments of localized numbers formatting event.
/// </summary>
public class FormatNumberEventArgs : EventArgs
{
    #region Properties

    /// <summary>
    /// Value to be formatted.
    /// </summary>
    public double Value { get; }

    /// <summary>
    /// Localized text.
    /// </summary>
    public string LocalizedValue { get; set; }

    /// <summary>
    /// Format string.
    /// </summary>
    public string Format { get; }

    /// <summary>
    /// Value type.
    /// </summary>
    public ChartValueType ValueType { get; } = ChartValueType.Auto;

    /// <summary>
    /// The sender object of the event.
    /// </summary>
    public object SenderTag { get; }

    /// <summary>
    /// Chart element type.
    /// </summary>
    public ChartElementType ElementType { get; } = ChartElementType.Nothing;

    #endregion Properties

    #region Methods

    /// <summary>
    /// Default constructor is not accessible
    /// </summary>
    private FormatNumberEventArgs()
    {
    }

    /// <summary>
    /// Object constructor.
    /// </summary>
    /// <param name="value">Value to be formatted.</param>
    /// <param name="format">Format string.</param>
    /// <param name="valueType">Value type..</param>
    /// <param name="localizedValue">Localized value.</param>
    /// <param name="senderTag">Chart element object tag.</param>
    /// <param name="elementType">Chart element type.</param>
    internal FormatNumberEventArgs(double value, string format, ChartValueType valueType, string localizedValue, object senderTag, ChartElementType elementType)
    {
        this.Value = value;
        this.Format = format;
        this.ValueType = valueType;
        this.LocalizedValue = localizedValue;
        this.SenderTag = senderTag;
        this.ElementType = elementType;
    }

    #endregion Methods
}

#region FontCache

/// <summary>
/// Font cache class helps ChartElements to reuse the Font instances
/// </summary>
internal sealed class FontCache : IDisposable
{
    #region Static

    // Default font family name
    private static string _defaultFamilyName;

    /// <summary>
    /// Gets the default font family name.
    /// </summary>
    /// <value>The default font family name.</value>
    public static string DefaultFamilyName
    {
        get
        {
            if (_defaultFamilyName is null)
            {
                // Find the "Microsoft Sans Serif" font
                foreach (FontFamily fontFamily in FontFamily.Families)
                {
                    if (fontFamily.Name == "Microsoft Sans Serif")
                    {
                        _defaultFamilyName = fontFamily.Name;
                        break;
                    }
                }
                // Not found - use the default Sans Serif font
                _defaultFamilyName ??= FontFamily.GenericSansSerif.Name;
            }

            return _defaultFamilyName;
        }
    }

    /// <summary>
    /// Gets the default font.
    /// </summary>
    /// <value>The default font.</value>
    public static Font DefaultFont => s_globalfontCache.GetFont(DefaultFamilyName, 8 * Chart.DPIScale);

    /// <summary>
    /// Gets the default bold font.
    /// </summary>
    /// <value>The default bold font.</value>
    public static Font DefaultBoldFont => s_globalfontCache.GetFont(DefaultFamilyName, 8 * Chart.DPIScale, FontStyle.Bold);

    /// <summary>
    /// Try get the default bold font.
    /// </summary>
    /// <value> The default bold font or <see langword="null" /> if not created yet.</value>
    public static Font TryGetDefaultBoldFont() => s_globalfontCache.TryGetFont(DefaultFamilyName, 8 * Chart.DPIScale, FontStyle.Bold);

    #endregion Static

    #region Fields

    // Cached fonts dictionary
    private readonly Dictionary<(string Name, FontStyle Style, GraphicsUnit Unit, float Size), Font> _fontCache = new();
    // Global font cash for default fonts.
    private static readonly FontCache s_globalfontCache = new();

    #endregion Fields

    #region Methods

    /// <summary>
    /// Gets the font.
    /// </summary>
    /// <param name="font">The template font.</param>
    /// <param name="size">The size.</param>
    /// <returns>
    /// Font instance
    /// </returns>
    public Font GetFont(Font font, float size)
    {
        return GetFont(font.FontFamily.Name, size, font.Style, font.Unit);
    }

    /// <summary>
    /// Gets the font.
    /// </summary>
    /// <param name="family">The family.</param>
    /// <param name="size">The size.</param>
    /// <param name="style">The style.</param>
    /// <param name="unit">The unit.</param>
    /// <returns>Font instance</returns>
    public Font GetFont(FontFamily family, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
    {
        return GetFont(family.Name, size, style, unit);
    }

    /// <summary>
    /// Gets the font.
    /// </summary>
    /// <param name="familyName">Name of the family.</param>
    /// <param name="size">The size.</param>
    /// <param name="style">The style.</param>
    /// <param name="unit">The unit.</param>
    /// <returns>
    /// Font instance
    /// </returns>
    public Font GetFont(string familyName, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
    {
        var key = (familyName, style, unit, size);
        if (!_fontCache.TryGetValue(key, out var font))
        {
            font = new Font(familyName, size, style, unit);
            _fontCache.Add(key, font);
        }

        return font;
    }

    /// <summary>
    /// Try get the font.
    /// </summary>
    /// <param name="familyName">Name of the family.</param>
    /// <param name="size">The size.</param>
    /// <param name="style">The style.</param>
    /// <param name="unit">The unit.</param>
    /// <returns>
    /// Font instance or <see langword="null" />.
    /// </returns>
    private Font TryGetFont(string familyName, float size, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
    {
        _fontCache.TryGetValue((familyName, style, unit, size), out var font);
        return font;
    }

    #endregion Methods

    #region IDisposable Members

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        foreach (Font font in _fontCache.Values)
        {
            font.Dispose();
        }

        _fontCache.Clear();
    }

    #endregion IDisposable Members
}

#endregion FontCache
