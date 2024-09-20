// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Classes related to the Data Points:
//				DataPointCollection - data points collection class
//				DataPoint - data point properties and methods
//				DataPointCustomProperties - data point & series properties
//				DataPointComparer - used for sorting data points in series
//


using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace System.Windows.Forms.DataVisualization.Charting;

#region CustomProperties enumeration

/// <summary>
/// Enumeration of common properties names.
/// </summary>
internal enum CommonCustomProperties
{
    PointName,
    Label,
    AxisLabel,
    LabelFormat,
    IsValueShownAsLabel,
    Color,
    BorderColor,
    BorderDashStyle,
    BorderWidth,
    BackImage,
    BackImageWrapMode,
    BackImageAlignment,
    BackImageTransparentColor,
    BackGradientStyle,
    BackSecondaryColor,
    BackHatchStyle,
    Font,
    LabelForeColor,
    LabelAngle,
    MarkerStyle,
    MarkerSize,
    MarkerImage,
    MarkerImageTransparentColor,
    MarkerColor,
    MarkerBorderColor,
    MarkerBorderWidth,
    MapAreaAttributes,
    PostBackValue,
    MapAreaType,
    LegendMapAreaType,
    LabelMapAreaType,
    Url,
    ToolTip,
    Tag,
    LegendUrl,
    LegendToolTip,
    LegendText,
    LegendMapAreaAttributes,
    LegendPostBackValue,
    IsVisibleInLegend,
    LabelUrl,
    LabelToolTip,
    LabelMapAreaAttributes,
    LabelPostBackValue,
    LabelBorderColor,
    LabelBorderDashStyle,
    LabelBorderWidth,
    LabelBackColor,
};

#endregion

/// <summary>
/// Data points comparer class
/// </summary>
[
SRDescription("DescriptionAttributeDataPointComparer_DataPointComparer")
]
public class DataPointComparer : IComparer<DataPoint>
{
    #region Fields

    // Sorting order
    private readonly PointSortOrder _sortingOrder = PointSortOrder.Ascending;

    // Sorting value index
    private readonly int _sortingValueIndex = 1;

    #endregion

    #region Constructors

    /// <summary>
    /// Private default constructor.
    /// </summary>
    private DataPointComparer()
    {
    }

    /// <summary>
    /// Data points comparer class constructor.
    /// </summary>
    /// <param name="series">Data series.</param>
    /// <param name="sortOrder">Sorting order.</param>
    /// <param name="sortBy">Value used for sorting ("X", "Y or Y1", "Y2", ...).</param>
    public DataPointComparer(Series series, PointSortOrder sortOrder, string sortBy)
    {
        // Check if sorting value is valid
        sortBy = sortBy.ToUpper(CultureInfo.InvariantCulture);
        if (string.Equals(sortBy, "X", StringComparison.Ordinal))
        {
            _sortingValueIndex = -1;
        }
        else if (string.Equals(sortBy, "Y", StringComparison.Ordinal))
        {
            _sortingValueIndex = 0;
        }
        else if (string.Equals(sortBy, "AXISLABEL", StringComparison.Ordinal))
        {
            _sortingValueIndex = -2;
        }
        else if (sortBy.Length == 2 &&
                sortBy.StartsWith("Y", StringComparison.Ordinal) &&
                char.IsDigit(sortBy[1]))
        {
            _sortingValueIndex = int.Parse(sortBy[1..], CultureInfo.InvariantCulture) - 1;
        }
        else
        {
            throw new ArgumentException(SR.ExceptionDataPointConverterInvalidSorting, nameof(sortBy));
        }

        // Check if data series support as many Y values as required
        if (_sortingValueIndex > 0 && _sortingValueIndex >= series.YValuesPerPoint)
        {
            throw new ArgumentException(SR.ExceptionDataPointConverterUnavailableSorting(sortBy, series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)), nameof(sortBy));
        }

        this._sortingOrder = sortOrder;
    }

    #endregion

    #region Comparing method

    /// <summary>
    /// Compares two data points.
    /// </summary>
    /// <param name="x">First data point.</param>
    /// <param name="y">Second data point.</param>
    /// <returns>If the two values are equal, it returns zero.  If point 1 is greater than point 2, 
    /// it returns a positive integer; otherwise, it returns a negative integer.
    /// </returns>
    public int Compare(DataPoint x, DataPoint y)
    {
        int result;

        // Compare X value
        if (_sortingValueIndex == -1)
        {
            result = x.XValue.CompareTo(y.XValue);
        }
        // Compare Axis Label value
        else if (_sortingValueIndex == -2)
        {
            result = string.Compare(x.AxisLabel, y.AxisLabel, StringComparison.CurrentCulture);
        }
        // Compare one of the Y value(s)
        else
        {
            result = x.YValues[_sortingValueIndex].CompareTo(y.YValues[_sortingValueIndex]);
        }

        // Invert result depending on the sorting order
        if (this._sortingOrder == PointSortOrder.Descending)
        {
            result = -result;
        }

        return result;
    }

    #endregion
}

/// <summary>
/// A collection of data points.
/// </summary>
[
    SRDescription("DescriptionAttributeDataPointCollection_DataPointCollection"),
]
public class DataPointCollection : ChartElementCollection<DataPoint>
{
    #region Fields

    // Reference to the sereies of data points
    internal Series series;

    #endregion

    #region Constructors and Initialization

    /// <summary>
    /// Data Point Collection object constructor.
    /// </summary>
    /// <param name="series">Series object, which the Data Point Collection belongs to.</param>
    internal DataPointCollection(Series series) : base(series)
    {
        this.series = series;
    }

    /// <summary>
    /// Initialize data point series and name.
    /// </summary>
    /// <param name="dataPoint">Reference to the data point object to initialize.</param>
    internal void DataPointInit(ref DataPoint dataPoint)
    {
        DataPointInit(this.series, ref dataPoint);
    }

    /// <summary>
    /// Initialize data point series and name.
    /// </summary>
    /// <param name="series">Series the data point belongs to.</param>
    /// <param name="dataPoint">Reference to the data point object to initialize.</param>
    internal static void DataPointInit(Series series, ref DataPoint dataPoint)
    {
        dataPoint.series = series;

        if (dataPoint.AxisLabel.Length > 0 && series != null)
        {
            series.noLabelsInPoints = false;
        }

        // Set flag that tooltips flags should be recalculated
        if (dataPoint.ToolTip.Length > 0 &&
            dataPoint.LegendToolTip.Length > 0 &&
            dataPoint.LabelToolTip.Length > 0 &&
            series != null && series.Chart != null && series.Chart.selection != null)
        {
            series.Chart.selection.enabledChecked = false;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the new DataPoint to a collection and sets its Y values.
    /// </summary>
    /// <param name="y">The y.</param>
    /// <returns></returns>
    public DataPoint Add(params double[] y)
    {
        DataPoint point = new DataPoint(0, y);
        this.Add(point);
        return point;
    }

    /// <summary>
    /// Parse the input parameter with other point attribute binding rule
    /// in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. 
    /// For example: "Tooltip=Price{C1},Url=WebSiteName".
    /// </summary>
    /// <param name="otherFields">Other fields parameter.</param>
    /// <param name="otherAttributeNames">Returns array of attribute names.</param>
    /// <param name="otherFieldNames">Returns array of field names.</param>
    /// <param name="otherValueFormat">Returns array of format strings.</param>
    internal static void ParsePointFieldsParameter(
    string otherFields,
    ref string[] otherAttributeNames,
    ref string[] otherFieldNames,
    ref string[] otherValueFormat)
    {
        if (otherFields != null && otherFields.Length > 0)
        {
            // Split string by comma
            otherAttributeNames = otherFields.Replace(",,", "\n").Split(',');
            otherFieldNames = new string[otherAttributeNames.Length];
            otherValueFormat = new string[otherAttributeNames.Length];

            // Loop through all strings
            for (int index = 0; index < otherAttributeNames.Length; index++)
            {
                // Split string by equal sign
                int equalSignIndex = otherAttributeNames[index].IndexOf('=');
                if (equalSignIndex > 0)
                {
                    otherFieldNames[index] = otherAttributeNames[index][(equalSignIndex + 1)..];
                    otherAttributeNames[index] = otherAttributeNames[index][..equalSignIndex];
                }
                else
                {
                    throw new ArgumentException(SR.ExceptionParameterFormatInvalid, nameof(otherFields));
                }

                // Check if format string was specified
                int bracketIndex = otherFieldNames[index].IndexOf('{');
                if (bracketIndex > 0 && otherFieldNames[index][^1] == '}')
                {
                    otherValueFormat[index] = otherFieldNames[index][(bracketIndex + 1)..];
                    otherValueFormat[index] = otherValueFormat[index].Trim('{', '}');
                    otherFieldNames[index] = otherFieldNames[index][..bracketIndex];
                }

                // Trim and replace new line character
                otherAttributeNames[index] = otherAttributeNames[index].Trim().Replace("\n", ",");
                otherFieldNames[index] = otherFieldNames[index].Trim().Replace("\n", ",");
                if (otherValueFormat[index] != null)
                    otherValueFormat[index] = otherValueFormat[index].Trim().Replace("\n", ",");
            }
        }
    }

    /// <summary>
    /// Data bind X, Y and other values (like Tooltip, LabelStyle,...) of the data points to the data source.
    /// Data source can be the DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="xField">Name of the field for X values.</param>
    /// <param name="yFields">Comma separated names of the fields for Y values.</param>
    /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
    public void DataBind(IEnumerable dataSource, string xField, string yFields, string otherFields)
    {
        // Check arguments
        if (dataSource == null)
            throw new ArgumentNullException(nameof(dataSource), SR.ExceptionDataPointInsertionNoDataSource);
        if (dataSource is string)
            throw new ArgumentException(SR.ExceptionDataBindSeriesToString, nameof(dataSource));
        if (yFields == null)
            throw new ArgumentNullException(nameof(yFields));

        // Convert comma separated Y values field names string to array of names
        string[] yFieldNames = yFields.Replace(",,", "\n").Split(',');
        for (int index = 0; index < yFieldNames.Length; index++)
        {
            yFieldNames[index] = yFieldNames[index].Replace("\n", ",");
        }

        if (yFieldNames.GetLength(0) > series.YValuesPerPoint)
            throw new ArgumentOutOfRangeException(nameof(yFields), SR.ExceptionDataPointYValuesCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));

        // Convert other fields/properties names to two arrays of names
        string[] otherAttributeNames = null;
        string[] otherFieldNames = null;
        string[] otherValueFormat = null;
        ParsePointFieldsParameter(
            otherFields,
            ref otherAttributeNames,
            ref otherFieldNames,
            ref otherValueFormat);

        // Remove all existing data points
        this.Clear();

        // Get and reset enumerator
        IEnumerator enumerator = GetDataSourceEnumerator(dataSource);
        if (enumerator.GetType() != typeof(DbEnumerator))
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

        // Add data points
        bool valueExsist = true;
        object[] yValuesObj = new object[yFieldNames.Length];
        object xValueObj = null;
        bool autoDetectType = true;

        this.SuspendUpdates();
        try
        {
            do
            {
                // Move to the next objects in the enumerations
                if (valueExsist)
                {
                    valueExsist = enumerator.MoveNext();
                }

                // Auto detect valu(s) type
                if (autoDetectType)
                {
                    autoDetectType = false;
                    AutoDetectValuesType(this.series, enumerator, xField, enumerator, yFieldNames[0]);
                }

                // Create and initialize data point
                if (valueExsist)
                {
                    DataPoint newDataPoint = new DataPoint(series);
                    bool emptyValues = false;

                    // Set X to the value provided
                    if (xField.Length > 0)
                    {
                        xValueObj = ConvertEnumerationItem(enumerator.Current, xField);
                        if (IsEmptyValue(xValueObj))
                        {
                            emptyValues = true;
                            xValueObj = 0.0;
                        }
                    }

                    // Set Y values
                    if (yFieldNames.Length == 0)
                    {
                        yValuesObj[0] = ConvertEnumerationItem(enumerator.Current, null);
                        if (IsEmptyValue(yValuesObj[0]))
                        {
                            emptyValues = true;
                            yValuesObj[0] = 0.0;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < yFieldNames.Length; i++)
                        {
                            yValuesObj[i] = ConvertEnumerationItem(enumerator.Current, yFieldNames[i]);
                            if (IsEmptyValue(yValuesObj[i]))
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
                            object obj = ConvertEnumerationItem(enumerator.Current, otherFieldNames[i]);
                            if (!IsEmptyValue(obj))
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

                        //DataPointInit(ref newDataPoint);  // not needed here - will be called during adding in Initialize
                        newDataPoint.IsEmpty = true;
                        this.Add(newDataPoint);
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

                        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
                        this.Add(newDataPoint);
                    }
                }

            } while (valueExsist);

        }
        finally
        {
            this.ResumeUpdates();
        }
    }

    /// <summary>
    /// Data bind Y values of the data points to the data source.
    /// Data source can be the Array, Collection, DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="yValue">One or more enumerable objects with Y values.</param>
    public void DataBindY(params IEnumerable[] yValue)
    {
        DataBindXY(null, yValue);
    }

    /// <summary>
    /// Data bind X and Y values of the data points to the data source.
    /// Data source can be the Array, Collection, DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="xValue">Enumerable objects with X values.</param>
    /// <param name="yValues">One or more enumerable objects with Y values.</param>
    public void DataBindXY(IEnumerable xValue, params IEnumerable[] yValues)
    {
        // Y value must be provided
        if (yValues is null || (yValues.Length == 1 && yValues[0] is null))
            throw new ArgumentNullException(nameof(yValues));

        if (yValues.GetLength(0) == 0)
            throw new ArgumentException(SR.ExceptionDataPointBindingYValueNotSpecified, nameof(yValues));

        // Double check that a string object is not provided for data binding
        for (int i = 0; i < yValues.Length; i++)
        {
            if (yValues[i] is string)
            {
                throw new ArgumentException(SR.ExceptionDataBindYValuesToString, nameof(yValues));
            }
        }

        // Check if number of Y values do not out of range
        if (yValues.GetLength(0) > series.YValuesPerPoint)
        {
            throw new ArgumentOutOfRangeException(nameof(yValues), SR.ExceptionDataPointYValuesBindingCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));
        }

        // Remove all existing data points
        this.Clear();

        // Reset X, Y enumerators
        IEnumerator xEnumerator = null;
        IEnumerator[] yEnumerator = new IEnumerator[yValues.GetLength(0)];
        if (xValue != null)
        {
            // Double check that a string object is not provided for data binding
            if (xValue is string)
            {
                throw new ArgumentException(SR.ExceptionDataBindXValuesToString, nameof(xValue));
            }

            // Get and reset Y values enumerators
            xEnumerator = GetDataSourceEnumerator(xValue);
            if (xEnumerator.GetType() != typeof(DbEnumerator))
            {
                xEnumerator.Reset();
            }
        }

        for (int i = 0; i < yValues.Length; i++)
        {
            // Get and reset Y values enumerators
            yEnumerator[i] = GetDataSourceEnumerator(yValues[i]);
            if (yEnumerator[i].GetType() != typeof(DbEnumerator))
            {
                yEnumerator[i].Reset();
            }
        }

        // Add data points
        bool xValueExsist = false;
        object[] yValuesObj = new object[series.YValuesPerPoint];
        object xValueObj = null;
        bool autoDetectType = true;

        SuspendUpdates();
        try
        {
            bool yValueExsist;
            do
            {
                // Move to the next objects in the enumerations
                yValueExsist = true;
                for (int i = 0; i < yValues.Length; i++)
                {
                    if (yValueExsist)
                    {
                        yValueExsist = yEnumerator[i].MoveNext();
                    }
                }

                if (xValue != null)
                {
                    xValueExsist = xEnumerator.MoveNext();
                    if (yValueExsist && !xValueExsist)
                    {
                        throw new ArgumentOutOfRangeException(nameof(xValue), SR.ExceptionDataPointInsertionXValuesQtyIsLessYValues);
                    }
                }

                // Auto detect value(s) type
                if (autoDetectType)
                {
                    autoDetectType = false;
                    AutoDetectValuesType(this.series, xEnumerator, null, yEnumerator[0], null);
                }

                // Create and initialize data point
                if (xValueExsist || yValueExsist)
                {
                    DataPoint newDataPoint = new DataPoint(series);
                    bool emptyValues = false;

                    // Set X to the value provided
                    if (xValueExsist)
                    {
                        xValueObj = ConvertEnumerationItem(xEnumerator.Current, null);
                        if (xValueObj is DBNull || xValueObj == null)
                        {
                            emptyValues = true;
                            xValueObj = 0.0;
                        }
                    }

                    // Set Y values
                    for (int i = 0; i < yValues.Length; i++)
                    {
                        yValuesObj[i] = ConvertEnumerationItem(yEnumerator[i].Current, null);
                        if (yValuesObj[i] is DBNull || yValuesObj[i] == null)
                        {
                            emptyValues = true;
                            yValuesObj[i] = 0.0;
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

                        //DataPointInit(ref newDataPoint); // // not needed here - will be called during adding in Initialize
                        newDataPoint.IsEmpty = true;
                        this.Add(newDataPoint);
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

                        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
                        this.Add(newDataPoint);
                    }

                }

            } while (xValueExsist || yValueExsist);

        }
        finally
        {
            this.ResumeUpdates();
        }
    }

    /// <summary>
    /// Data bind Y values of the data points to the data source.
    /// Data source can be the Array, Collection, DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="yValue">Enumerable objects with Y values.</param>
    /// <param name="yFields">Name of the fields for Y values.</param>
    public void DataBindY(IEnumerable yValue, string yFields)
    {
        DataBindXY(null, null, yValue, yFields);
    }

    /// <summary>
    /// Data bind X and Y values of the data points to the data source.
    /// Data source can be the Array, Collection, DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="xValue">Enumerable object with X values.</param>
    /// <param name="xField">Name of the field for X values.</param>
    /// <param name="yValue">Enumerable objects with Y values.</param>
    /// <param name="yFields">Comma separated names of the fields for Y values.</param>
    public void DataBindXY(IEnumerable xValue, string xField, IEnumerable yValue, string yFields)
    {
        // Check arguments
        if (xValue is string)
            throw new ArgumentException(SR.ExceptionDataBindXValuesToString, nameof(xValue));
        if (yValue == null)
            throw new ArgumentNullException(nameof(yValue), SR.ExceptionDataPointInsertionYValueNotSpecified);
        if (yValue is string)
            throw new ArgumentException(SR.ExceptionDataBindYValuesToString, nameof(yValue));
        if (yFields == null)
            throw new ArgumentOutOfRangeException(nameof(yFields), SR.ExceptionDataPointYValuesCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));

        // Convert comma separated field names string to array of names
        string[] yFieldNames = yFields.Replace(",,", "\n").Split(',');
        for (int index = 0; index < yFieldNames.Length; index++)
        {
            yFieldNames[index] = yFieldNames[index].Replace("\n", ",");
        }

        if (yFieldNames.GetLength(0) > series.YValuesPerPoint)
            throw new ArgumentOutOfRangeException(nameof(yFields), SR.ExceptionDataPointYValuesCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));

        // Remove all existing data points
        this.Clear();

        // Reset X, Y enumerators
        IEnumerator xEnumerator = null;
        IEnumerator yEnumerator = GetDataSourceEnumerator(yValue);

        if (yEnumerator.GetType() != typeof(DbEnumerator))
        {
            yEnumerator.Reset();
        }

        if (xValue != null)
        {
            if (xValue != yValue)
            {
                xEnumerator = GetDataSourceEnumerator(xValue);
                if (xEnumerator.GetType() != typeof(DbEnumerator))
                {
                    xEnumerator.Reset();
                }
            }
            else
            {
                xEnumerator = yEnumerator;
            }
        }

        // Add data points
        bool xValueExsist = false;
        bool yValueExsist = true;
        object[] yValuesObj = new object[yFieldNames.Length];
        object xValueObj = null;
        bool autoDetectType = true;

        this.SuspendUpdates();
        try
        {
            do
            {
                // Move to the next objects in the enumerations
                if (yValueExsist)
                {
                    yValueExsist = yEnumerator.MoveNext();
                }

                if (xValue != null)
                {
                    if (xValue != yValue)
                    {
                        xValueExsist = xEnumerator.MoveNext();
                        if (yValueExsist && !xValueExsist)
                        {
                            throw new ArgumentOutOfRangeException(nameof(xValue), SR.ExceptionDataPointInsertionXValuesQtyIsLessYValues);
                        }
                    }
                    else
                    {
                        xValueExsist = yValueExsist;
                    }
                }

                // Auto detect valu(s) type
                if (autoDetectType)
                {
                    autoDetectType = false;
                    AutoDetectValuesType(this.series, xEnumerator, xField, yEnumerator, yFieldNames[0]);
                }

                // Create and initialize data point
                if (xValueExsist || yValueExsist)
                {
                    DataPoint newDataPoint = new DataPoint(series);
                    bool emptyValues = false;

                    // Set X to the value provided or use sequence numbers starting with 1
                    if (xValueExsist)
                    {
                        xValueObj = ConvertEnumerationItem(xEnumerator.Current, xField);
                        if (IsEmptyValue(xValueObj))
                        {
                            emptyValues = true;
                            xValueObj = 0.0;
                        }

                    }

                    if (yFieldNames.Length == 0)
                    {
                        yValuesObj[0] = ConvertEnumerationItem(yEnumerator.Current, null);
                        if (IsEmptyValue(yValuesObj[0]))
                        {
                            emptyValues = true;
                            yValuesObj[0] = 0.0;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < yFieldNames.Length; i++)
                        {
                            yValuesObj[i] = ConvertEnumerationItem(yEnumerator.Current, yFieldNames[i]);
                            if (IsEmptyValue(yValuesObj[i]))
                            {
                                emptyValues = true;
                                yValuesObj[i] = 0.0;
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

                        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
                        newDataPoint.IsEmpty = true;
                        this.Add(newDataPoint);
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

                        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
                        this.Add(newDataPoint);
                    }
                }

            } while (xValueExsist || yValueExsist);

        }
        finally
        {
            this.ResumeUpdates();
        }
    }

    /// <summary>
    /// Returns true if objet represents an empty value.
    /// </summary>
    /// <param name="val">Value to test.</param>
    /// <returns>True if empty.</returns>
    internal static bool IsEmptyValue(object val)
    {
        if (val is null || val is DBNull)
        {
            return true;
        }

        if (val is double dbl && double.IsNaN(dbl))
        {
            return true;
        }

        if (val is float single && float.IsNaN(single))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds one data point with one Y value.
    /// </summary>
    /// <param name="yValue">Y value of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddY(double yValue)
    {
        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Adds one data point with one or more Y values.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddY(params double[] yValue)
    {
        //Check arguments
        if (yValue is null)
            throw new ArgumentNullException(nameof(yValue));

        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Adds one data point with one or more Y values.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddY(params object[] yValue)
    {
        //Check arguments
        if (yValue is null || (yValue.Length == 1 && yValue[0] is null))
            throw new ArgumentNullException(nameof(yValue));

        // Auto detect DateTime values type
        if (this.series.YValueType == ChartValueType.Auto && yValue.Length > 0 && yValue[0] is not null)
        {
            if (yValue[0] is DateTime)
            {
                this.series.YValueType = ChartValueType.DateTime;
                this.series.autoYValueType = true;
            }
            else if (yValue[0] is DateTimeOffset)
            {
                this.series.YValueType = ChartValueType.DateTimeOffset;
                this.series.autoYValueType = true;
            }
        }

        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Adds one data point with X value and one Y value.
    /// </summary>
    /// <param name="yValue">Y value of the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddXY(double xValue, double yValue)
    {
        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Adds one data point with X value and one or more Y values.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddXY(double xValue, params double[] yValue)
    {
        // Auto detect DateTime and String values type
        if (this.series.XValueType == ChartValueType.Auto)
            this.series.autoXValueType = true;

        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Adds one data point with X value and one or more Y values.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <returns>Index of newly added data point.</returns>
    public int AddXY(object xValue, params object[] yValue)
    {

        // Auto detect DateTime and String values type
        if (this.series.XValueType == ChartValueType.Auto)
        {
            if (xValue is DateTime)
            {
                this.series.XValueType = ChartValueType.DateTime;
            }

            if (xValue is DateTimeOffset)
            {
                this.series.XValueType = ChartValueType.DateTimeOffset;
            }

            if (xValue is string)
            {
                this.series.XValueType = ChartValueType.String;
            }

            this.series.autoXValueType = true;
        }

        if (this.series.YValueType == ChartValueType.Auto &&
            yValue.Length > 0 &&
            yValue[0] != null)
        {
            if (yValue[0] is DateTime)
            {
                this.series.YValueType = ChartValueType.DateTime;
                this.series.autoYValueType = true;
            }
            else if (yValue[0] is DateTimeOffset)
            {
                this.series.YValueType = ChartValueType.DateTimeOffset;
                this.series.autoYValueType = true;
            }
        }

        // Create new point object
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        Add(newDataPoint);
        return Count - 1;
    }

    /// <summary>
    /// Insert one data point with X value and one Y value.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">Y value of the data point.</param>
    public void InsertXY(int index, double xValue, double yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Insert one data point with X value and one or more Y values.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void InsertXY(int index, double xValue, params double[] yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Insert one data point with X value and one or more Y values.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void InsertXY(int index, object xValue, params object[] yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueXY(xValue, yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Insert one data point with one Y value.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="yValue">Y value of the data point.</param>
    public void InsertY(int index, double yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Insert one data point with one or more Y values.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void InsertY(int index, params double[] yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Insert one data point with one or more Y values.
    /// </summary>
    /// <param name="index">Index after which to insert the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void InsertY(int index, params object[] yValue)
    {
        DataPoint newDataPoint = new DataPoint(series);
        newDataPoint.SetValueY(yValue);
        //DataPointInit(ref newDataPoint); // not needed here - will be called during adding in Initialize
        this.Insert(index, newDataPoint);
    }

    /// <summary>
    /// Removes all elements after <paramref name="index"/> from the <see cref="DataPointCollection"/>.
    /// </summary>
    /// <param name="index">The index after witch to remove elements. To remove all elements pass -1 here (unlike <see cref="ClearItems()"/> will not recalculate axes scale if collection already empty).</param>
    public void ClearAfter(int index)
    {
        if (Count == 0 || index + 1 >= Count) // not need to refresh Minimum and Maximum if we will not remove anything.
            return;

        // Refresh Minimum and Maximum from data after recalc and set data
        Common?.ChartPicture?.ResetMinMaxFromData();
        ClearItemsAfter(index);
    }

    /// <summary>
    /// Get data source enumerator object helper function.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <returns>Returns data source enumerator.</returns>
    internal static IEnumerator GetDataSourceEnumerator(IEnumerable dataSource)
    {
        if (dataSource is DataView dataView)
        {
            return dataView.GetEnumerator();
        }

        if (dataSource is DataSet dataSet)
        {
            if (dataSet.Tables.Count > 0)
            {
                return dataSet.Tables[0].Rows.GetEnumerator();
            }
        }

        return dataSource.GetEnumerator();
    }

    /// <summary>
    /// Convert enumeration item object from DataRow and DataRowView 
    /// to the actual value of specified column in row
    /// </summary>
    /// <param name="item">Enumeration item.</param>
    /// <param name="fieldName">Converted item.</param>
    /// <returns></returns>
    internal static object ConvertEnumerationItem(object item, string fieldName)
    {
        object result = item;

        // If original object is DataRow
        if (item is DataRow dataRow)
        {
            if (fieldName?.Length > 0)
            {
                // Check if specified column exist
                bool failed;
                if (dataRow.Table.Columns.Contains(fieldName))
                {
                    result = dataRow[fieldName];
                    failed = false;
                }
                else
                {
                    // Try to treat field name as column index number
                    failed = !int.TryParse(fieldName, NumberStyles.Any, CultureInfo.InvariantCulture, out int columnIndex);

                    if (!failed && columnIndex < dataRow.Table.Columns.Count && columnIndex >= 0)
                    {
                        result = dataRow[columnIndex];
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(fieldName));
                }
            }
            else
            {
                // Get first column value if name not specified
                result = dataRow[0];
            }
        }

        // If original object is DataRowView

        if (item is DataRowView dataRowView)
        {
            if (fieldName?.Length > 0)
            {
                // Check if specified column exist
                bool failed;
                if (dataRowView.DataView.Table.Columns.Contains(fieldName))
                {
                    result = dataRowView[fieldName];
                    failed = false;
                }
                else
                {
                    // Try to treat field name as column index number
                    failed = !int.TryParse(fieldName, NumberStyles.Any, CultureInfo.InvariantCulture, out int columnIndex);
                    if (!failed && columnIndex < dataRowView.DataView.Table.Columns.Count && columnIndex >= 0)
                    {
                        result = dataRowView[columnIndex];
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(fieldName));
                }
            }
            else
            {
                // Get first column value if name not specified
                result = dataRowView[0];
            }
        }

        // If original object is DbDataRecord
        if (item is DbDataRecord dbDataRecord)
        {
            if (fieldName?.Length > 0)
            {
                // Check if specified column exist
                bool failed = true;
                if (!char.IsNumber(fieldName, 0))
                {
                    try
                    {
                        result = dbDataRecord[fieldName];
                        failed = false;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        failed = true;
                    }
                }

                if (failed)
                {
                    // Try to treat field name as column index number
                    try
                    {
                        bool parseSucceed = int.TryParse(fieldName, NumberStyles.Any, CultureInfo.InvariantCulture, out int columnIndex);

                        if (parseSucceed)
                        {
                            result = dbDataRecord[columnIndex];
                            failed = false;
                        }
                        else
                        {
                            failed = true;
                        }
                    }
                    catch (IndexOutOfRangeException)
                    {
                        failed = true;
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(fieldName));
                }

            }
            else
            {
                // Get first column value if name not specified
                result = dbDataRecord[0];
            }
        }
        else
        {
            if (fieldName?.Length > 0)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item).Find(fieldName, true);
                if (descriptor is not null)
                    return descriptor.GetValue(item);
            }
        }

        return result;
    }
    /// <summary>
    /// Auto detects the X and Y(s) values type
    /// </summary>
    /// <param name="series">Series the values type is detected for.</param>
    /// <param name="xEnumerator">X values enumerator.</param>
    /// <param name="xField">X value field.</param>
    /// <param name="yEnumerator">Y values enumerator.</param>
    /// <param name="yField">Y value field.</param>
    internal static void AutoDetectValuesType(
        Series series,
        IEnumerator xEnumerator,
        string xField,
        IEnumerator yEnumerator,
        string yField)
    {
        if (series.XValueType == ChartValueType.Auto)
        {
            series.XValueType = GetValueType(xEnumerator, xField);
            if (series.XValueType != ChartValueType.Auto)
            {
                series.autoXValueType = true;
            }
        }

        if (series.YValueType == ChartValueType.Auto)
        {
            series.YValueType = GetValueType(yEnumerator, yField);
            if (series.YValueType != ChartValueType.Auto)
            {
                series.autoYValueType = true;
            }
        }
    }

    /// <summary>
    /// Return value type.
    /// </summary>
    /// <param name="enumerator">Values enumerator.</param>
    /// <param name="field">Value field.</param>
    private static ChartValueType GetValueType(IEnumerator enumerator, string field)
    {
        ChartValueType type = ChartValueType.Auto;
        Type columnDataType = null;

        // Check parameters
        if (enumerator == null)
        {
            return type;
        }

        // Check if current enumeration element is available
        try
        {
            if (enumerator.Current == null)
            {
                return type;
            }
        }
        catch (InvalidOperationException)
        {
            return type;
        }


        // If original object is DataRow
        if (enumerator.Current is DataRow)
        {
            if (field != null && field.Length > 0)
            {
                // Check if specified column exist
                bool failed = true;
                if (((DataRow)enumerator.Current).Table.Columns.Contains(field))
                {
                    columnDataType = ((DataRow)enumerator.Current).Table.Columns[field].DataType;
                    failed = false;
                }

                // Try to treat field as column number
                if (failed)
                {
                    bool parseSucceed = int.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out int columnIndex);

                    if (parseSucceed)
                    {
                        columnDataType = ((DataRow)enumerator.Current).Table.Columns[columnIndex].DataType;
                        failed = false;
                    }
                    else
                    {
                        failed = true;
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(field));
                }

            }
            else if (((DataRow)enumerator.Current).Table.Columns.Count > 0)
            {
                columnDataType = ((DataRow)enumerator.Current).Table.Columns[0].DataType;
            }
        }

        // If original object is DataRowView
        else if (enumerator.Current is DataRowView dRView)
        {
            if (field != null && field.Length > 0)
            {
                // Check if specified column exist
                bool failed = true;
                if (dRView.DataView.Table.Columns.Contains(field))
                {
                    columnDataType = dRView.DataView.Table.Columns[field].DataType;
                    failed = false;
                }

                // Try to treat field as column number
                if (failed)
                {
                    bool parseSucceed = int.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out int columnIndex);
                    if (parseSucceed)
                    {
                        columnDataType = dRView.DataView.Table.Columns[columnIndex].DataType;
                        failed = false;
                    }
                    else
                    {
                        failed = true;
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(field));
                }

            }
            else if (dRView.DataView.Table.Columns.Count > 0)
            {
                columnDataType = dRView.DataView.Table.Columns[0].DataType;
            }
        }

        // If original object is DbDataRecord
        else if (enumerator.Current is DbDataRecord record)
        {
            if (field != null && field.Length > 0)
            {
                bool failed = true;
                int columnIndex;
                if (!char.IsNumber(field, 0))
                {
                    columnIndex = record.GetOrdinal(field);
                    columnDataType = record.GetFieldType(columnIndex);
                    failed = false;
                }

                // Try to treat field as column number
                if (failed)
                {
                    failed = !int.TryParse(field, NumberStyles.Any, CultureInfo.InvariantCulture, out columnIndex);

                    if (!failed)
                    {
                        columnDataType = record.GetFieldType(columnIndex);
                    }
                }

                if (failed)
                {
                    throw new ArgumentException(SR.ExceptionColumnNameNotFound(field));
                }

            }
            else if (record.FieldCount > 0)
            {
                columnDataType = record.GetFieldType(0);
            }
        }
        // Try detecting simple data types
        else
        {
            if (field?.Length > 0)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(enumerator.Current).Find(field, true);
                if (descriptor is not null)
                {
                    columnDataType = descriptor.PropertyType;
                }
            }

            if (columnDataType is null)
            {
                columnDataType = enumerator.Current.GetType();
            }
        }

        // Use data type
        if (columnDataType != null)
        {
            if (columnDataType == typeof(DateTime))
                type = ChartValueType.DateTime;
            else if (columnDataType == typeof(DateTimeOffset))
                type = ChartValueType.DateTimeOffset;
            else if (columnDataType == typeof(TimeSpan))
                type = ChartValueType.Time;
            else if (columnDataType == typeof(double))
                type = ChartValueType.Double;
            else if (columnDataType == typeof(int))
                type = ChartValueType.Int32;
            else if (columnDataType == typeof(long))
                type = ChartValueType.Int64;
            else if (columnDataType == typeof(float))
                type = ChartValueType.Single;
            else if (columnDataType == typeof(string))
                type = ChartValueType.String;
            else if (columnDataType == typeof(uint))
                type = ChartValueType.UInt32;
            else if (columnDataType == typeof(ulong))
                type = ChartValueType.UInt64;
        }

        return type;
    }

    #endregion Methods

    #region DataPoint finding functions

    /// <summary>
    /// Find all the points that equal to the specified value starting from the specified index.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <param name="startIndex">Index of the point to start looking from.</param>
    /// <returns>Enumerator of datapoints.</returns>
    public IEnumerable<DataPoint> FindAllByValue(double valueToFind, string useValue, int startIndex)
    {
        // Loop through all points from specified index
        for (int i = startIndex; i < this.Count; i++)
        {
            DataPoint point = this[i];
            if (point.GetValueByName(useValue) == valueToFind)
            {
                yield return point;
            }
        }
    }

    /// <summary>
    /// Find all the points that equal to the specified value.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <returns>Enumerator of datapoints.</returns>
    public IEnumerable<DataPoint> FindAllByValue(double valueToFind, string useValue)
    {
        // Loop through all points from specified index
        for (int i = 0; i < this.Count; i++)
        {
            DataPoint point = this[i];
            if (point.GetValueByName(useValue) == valueToFind)
            {
                yield return point;
            }
        }
    }

    /// <summary>
    /// Find all the points that equal to the specified value.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <returns>Enumerator of datapoints.</returns>
    public IEnumerable<DataPoint> FindAllByValue(double valueToFind)
    {
        return FindAllByValue(valueToFind, "Y");
    }

    /// <summary>
    /// Find the first point that equals to the specified value starting from the specified index.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <param name="startIndex">Index of the point to start looking from.</param>
    /// <returns>Datapoint which matches the value.  Null if there is no match.</returns>
    public DataPoint FindByValue(double valueToFind, string useValue, int startIndex)
    {
        //Check arguments
        if (useValue == null)
            throw new ArgumentNullException(nameof(useValue));
        if (startIndex < 0 || startIndex >= this.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        // Loop through all points from specified index
        for (int i = startIndex; i < this.Count; i++)
        {
            DataPoint point = this[i];
            if (point.GetValueByName(useValue) == valueToFind)
            {
                return point;
            }
        }

        // Nothing was found
        return null;
    }

    /// <summary>
    /// Find the first point that equals to the specified value.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <returns>Datapoint which matches the value.  Null if there is no match.</returns>
    public DataPoint FindByValue(double valueToFind, string useValue)
    {
        return FindByValue(valueToFind, useValue, 0);
    }

    /// <summary>
    /// Find the first point that equals to the specified value.
    /// </summary>
    /// <param name="valueToFind">Point value to find.</param>
    /// <returns>Datapoint which matches the value.  Null if there is no match.</returns>
    public DataPoint FindByValue(double valueToFind)
    {
        return FindByValue(valueToFind, "Y");
    }

    /// <summary>
    /// Find point with the maximum value starting from specified index.
    /// </summary>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <param name="startIndex">Index of the point to start looking from.</param>
    /// <returns>Datapoint with the maximum value.</returns>
    public DataPoint FindMaxByValue(string useValue, int startIndex)
    {
        //Check arguments
        if (useValue == null)
            throw new ArgumentNullException(nameof(useValue));
        if (startIndex < 0 || startIndex >= this.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        bool isYValue = useValue.StartsWith("Y", StringComparison.OrdinalIgnoreCase);
        double maxValue = double.MinValue;
        DataPoint maxPoint = null;

        for (int i = startIndex; i < this.Count; i++)
        {
            DataPoint point = this[i];

            // Skip empty points when searching for the Y values
            if (point.IsEmpty && isYValue)
                continue;

            double pointValue = point.GetValueByName(useValue);

            if (maxValue < pointValue)
            {
                maxValue = pointValue;
                maxPoint = point;
            }
        }

        return maxPoint;
    }

    /// <summary>
    /// Find point with the maximum value.
    /// </summary>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <returns>Datapoint with the maximum value.</returns>
    public DataPoint FindMaxByValue(string useValue)
    {
        return FindMaxByValue(useValue, 0);
    }

    /// <summary>
    /// Find data point with the maximum value.
    /// </summary>
    /// <returns>Datapoint with the maximum value.</returns>
    public DataPoint FindMaxByValue()
    {
        return FindMaxByValue("Y");
    }

    /// <summary>
    /// Find point with the Min value starting from specified index.
    /// </summary>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <param name="startIndex">Index of the point to start looking from.</param>
    /// <returns>Datapoint with the Min value.</returns>
    public DataPoint FindMinByValue(string useValue, int startIndex)
    {
        if (useValue == null)
            throw new ArgumentNullException(nameof(useValue));
        if (startIndex < 0 || startIndex >= this.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        bool isYValue = useValue.StartsWith("Y", StringComparison.OrdinalIgnoreCase);
        double minValue = double.MaxValue;
        DataPoint minPoint = null;

        for (int i = startIndex; i < this.Count; i++)
        {
            DataPoint point = this[i];

            // Skip empty points when searching for the Y values
            if (point.IsEmpty && isYValue)
                continue;

            double pointValue = point.GetValueByName(useValue);

            if (minValue > pointValue)
            {
                minValue = pointValue;
                minPoint = point;
            }
        }

        return minPoint;
    }

    /// <summary>
    /// Find point with the Min value.
    /// </summary>
    /// <param name="useValue">Which point value to use (X, Y1, Y2,...).</param>
    /// <returns>Datapoint with the Min value.</returns>
    public DataPoint FindMinByValue(string useValue)
    {
        return FindMinByValue(useValue, 0);
    }

    /// <summary>
    /// Find point with the Min value
    /// </summary>
    /// <returns>Datapoint with the Min value.</returns>
    public DataPoint FindMinByValue()
    {
        return FindMinByValue("Y");
    }

    #endregion

    #region Collection<T> overrides

    /// <summary>
    /// Initializes the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    internal override void Initialize(DataPoint item)
    {
        DataPointInit(ref item);
        base.Initialize(item);
    }

    /// <summary>
    /// Removes all elements from the <see cref="DataPointCollection"/>.
    /// </summary>
    protected override void ClearItems()
    {
        // Refresh Minimum and Maximum from data after recalc and set data
        Common?.ChartPicture?.ResetMinMaxFromData();
        base.ClearItems();
    }

    #endregion
}

/// <summary>
/// Stores values and properties of a DataPoint of a Series.
/// </summary>
[
SRDescription("DescriptionAttributeDataPoint_DataPoint"),
DefaultProperty("YValues"),
TypeConverter(typeof(DataPointConverter))
]
public class DataPoint : DataPointCustomProperties
{
    #region Fields

    // Point X value
    private double _xValue;

    // Point Y values
    private double[] _yValue = new double[1];

    // Pre calculated (during painting) relative position of data point
    internal PointF positionRel = PointF.Empty;

    // VSTS:199794 - Accessibility needs the last rendered label content to be exposed.
    // The current label content evaluation is scattered over different chart types and cannot be isolated without risk of regression.
    // This variable will cache the label content taken just before drawing.
    internal string _lastLabelText = string.Empty;

    #endregion

    #region Constructors

    /// <summary>
    /// DataPoint object constructor.
    /// </summary>
    public DataPoint() : base(null, true)
    {
        _yValue = new double[1];
    }

    /// <summary>
    /// DataPoint object constructor.
    /// </summary>
    /// <param name="series">series object, which the DataPoint belongs to.</param>
    public DataPoint(Series series) : base(series, true)
    {
        // Create Y value(s) array
        _yValue = new double[series.YValuesPerPoint];
        _xValue = 0;
    }

    /// <summary>
    /// DataPoint object constructor.
    /// </summary>
    /// <param name="xValue">X value.</param>
    /// <param name="yValue">Y value.</param>
    public DataPoint(double xValue, double yValue) : base(null, true)
    {
        // Set Y value
        this._yValue = new double[1];
        this._yValue[0] = yValue;

        // Set X value
        this._xValue = xValue;
    }

    /// <summary>
    /// DataPoint object constructor.
    /// </summary>
    /// <param name="xValue">X value.</param>
    /// <param name="yValues">Array of Y values.</param>
    public DataPoint(double xValue, double[] yValues) : base(null, true)
    {
        // Set Y value
        this._yValue = yValues;

        // Set X value
        this._xValue = xValue;
    }

    /// <summary>
    /// DataPoint object constructor.
    /// </summary>
    /// <remarks>
    /// This method is only used during the Windows Forms serialization of the chart.
    /// </remarks>
    /// <param name="xValue">X value.</param>
    /// <param name="yValues">String of comma separated Y values.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public DataPoint(double xValue, string yValues) : base(null, true)
    {
        string[] values = yValues.Split(',');

        // Create Y value(s) array
        _yValue = new double[values.Length];

        for (int index = 0; index < values.Length; index++)
        {
            _yValue[index] = CommonElements.ParseDouble(values[index], true);
        }

        // Set X value
        this._xValue = xValue;
    }

    #endregion

    #region Data point methods

    /// <summary>
    /// Sets the specified data point attribute to the specified value.
    /// </summary>
    /// <param name="obj">Attribute value.</param>
    /// <param name="propertyName">Attribute name.</param>
    /// <param name="format">Value format.</param>
    internal void SetPointCustomProperty(
        object obj,
        string propertyName,
        string format)
    {
        // Convert value to string
        if (obj is not string stringValue)
        {
            ChartValueType valueType = ChartValueType.Auto;
            double doubleObj;
            if (obj is DateTime dtm)
            {
                doubleObj = dtm.ToOADate();
                valueType = ChartValueType.Date;
            }
            else
            {
                doubleObj = this.ConvertValue(obj);
            }

            // Try converting to string
            if (!double.IsNaN(doubleObj))
            {
                try
                {
                    stringValue = ValueConverter.FormatValue(
                        this.Chart,
                        this,
                        this.Tag,
                        doubleObj,
                        format,
                        valueType,
                        ChartElementType.DataPoint);
                }
                catch (FormatException)
                {
                    // Use basic string converter
                    stringValue = obj.ToString();
                }
            }
            else
            {
                // Use basic string converter
                stringValue = obj.ToString();
            }
        }

        // Assign data point attribute by name
        if (stringValue.Length > 0)
        {
            if (string.Equals(propertyName, "AxisLabel", StringComparison.OrdinalIgnoreCase))
            {
                this.AxisLabel = stringValue;
            }
            else if (string.Equals(propertyName, "Tooltip", StringComparison.OrdinalIgnoreCase))
            {
                this.ToolTip = stringValue;
            }
            else if (string.Equals(propertyName, "Label", StringComparison.OrdinalIgnoreCase))
            {
                this.Label = stringValue;
            }
            else if (string.Equals(propertyName, "LegendTooltip", StringComparison.OrdinalIgnoreCase))
            {
                this.LegendToolTip = stringValue;
            }
            else if (string.Equals(propertyName, "LegendText", StringComparison.OrdinalIgnoreCase))
            {
                this.LegendText = stringValue;
            }
            else if (string.Equals(propertyName, "LabelToolTip", StringComparison.OrdinalIgnoreCase))
            {
                this.LabelToolTip = stringValue;
            }
            else
            {
                this[propertyName] = stringValue;
            }
        }
    }


    /// <summary>
    /// Converts object to double.
    /// </summary>
    /// <param name="value">Object to convert.</param>
    /// <returns>Double value.</returns>
    private double ConvertValue(object value)
    {
        if (value is null)
            return 0;

        if (value is double dbl)
            return dbl;

        if (value is float single)
            return (double)single;

        if (value is decimal dcml)
            return (double)dcml;

        if (value is int @int)
            return @int;

        if (value is uint @uint)
            return @uint;

        if (value is long @long)
            return @long;

        if (value is ulong @ulong)
            return @ulong;

        if (value is byte @byte)
            return @byte;

        if (value is sbyte @sbyte)
            return @sbyte;

        if (value is bool @bool)
            return @bool ? 1.0 : 0.0;

        return CommonElements.ParseDouble(value.ToString());
    }

    /// <summary>
    /// Set X and Y value of the data point. If <paramref name="yValue"/> is <see cref="double.NaN"/> will set <see cref="IsEmpty"/> and Y value to 0.
    /// </summary>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">Y value of the data point.</param>
    public void SetValueXY(double xValue, double yValue)
    {
        if (double.IsNaN(yValue))
        {
            // Set point empty flag and values to zero
            this.IsEmpty = true;
            this._yValue[0] = 0.0;
            // set X
            this._xValue = xValue;
            return;
        }

        // Set Y value first
        SetValueY(yValue);
        // set X
        this._xValue = xValue;
    }

    /// <summary>
    /// Set X value and one or more Y values of the data point. If one of the <paramref name="yValue"/> is <see cref="double.NaN"/> will set <see cref="IsEmpty"/> and all Y values to 0.
    /// </summary>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void SetValueXY(double xValue, params double[] yValue)
    {
        // Set Y value first
        SetValueY(yValue);
        // set X
        this._xValue = xValue;

        // Check if one of Y values are not available
        for (int i = 0; i < _yValue.Length; i++)
        {
            if (!double.IsNaN(this._yValue[i]))
                continue;

            // Set point empty flag and values to zero
            this.IsEmpty = true;
            for (int valueIndex = 0; valueIndex < this._yValue.Length; valueIndex++)
            {
                this._yValue[valueIndex] = 0.0;
            }

            break;
        }
    }

    /// <summary>
    /// Set X value and one or more Y values of the data point. If one of the <paramref name="yValue"/> is <see cref="double.NaN"/> will set <see cref="IsEmpty"/> and all Y values to 0.
    /// </summary>
    /// <param name="xValue">X value of the data point.</param>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void SetValueXY(object xValue, params object[] yValue)
    {
        // Check arguments
        if (xValue is null)
            throw new ArgumentNullException(nameof(xValue));

        // Set Y value first
        SetValueY(yValue);

        // Check if parameters type matches with series type
        Type paramType = xValue.GetType();
        series?.CheckSupportedTypes(paramType);

        // Save value in the array
        if (xValue is string str)
            AxisLabel = str;
        else if (xValue is DateTime dtm)
            this._xValue = dtm.ToOADate();
        else
            this._xValue = ConvertValue(xValue);

        // Get Date or Time if required
        if (series is not null && xValue is DateTime dttm)
        {
            if (series.XValueType == ChartValueType.Date)
            {
                this._xValue = dttm.Date.ToOADate();
            }
            else if (series.XValueType == ChartValueType.Time)
            {
                DateTime time = new DateTime(
                    1899,
                    12,
                    30,
                    dttm.Hour,
                    dttm.Minute,
                    dttm.Second,
                    dttm.Millisecond);
                this._xValue = time.ToOADate();
            }
        }

        // Check if one of Y values are not available
        for (int i = 0; i < _yValue.Length; i++)
        {
            if (!double.IsNaN(this._yValue[i]))
                continue;

            // Set point empty flag and values to zero
            this.IsEmpty = true;
            for (int valueIndex = 0; valueIndex < this._yValue.Length; valueIndex++)
            {
                this._yValue[valueIndex] = 0.0;
            }

            break;
        }
    }

    /// <summary>
    /// Set Y value of the data point.
    /// </summary>
    /// <param name="yValue">Y value of the data point.</param>
    public void SetValueY(double yValue)
    {
        _yValue[0] = yValue;

        if (series is null)
            return;

        // Get Date or Time if required
        if (series.YValueType == ChartValueType.Date)
        {
            if (yValue == 0.0)
                this._yValue[0] = Math.Floor(yValue);
            else
                this._yValue[0] = DateTime.FromOADate(yValue).Date.ToOADate();
        }
        else if (series.YValueType == ChartValueType.Time)
        {
            if (yValue == 0.0)
            {
                this._yValue[0] = this._xValue - Math.Floor(yValue);
            }
            else
            {
                DateTime yTime = DateTime.FromOADate(yValue);
                DateTime time = new DateTime(
                    1899,
                    12,
                    30,
                    yTime.Hour,
                    yTime.Minute,
                    yTime.Second,
                    yTime.Millisecond);

                this._yValue[0] = time.ToOADate();
            }
        }
    }

    /// <summary>
    /// Set one or more Y values of the data point.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void SetValueY(params double[] yValue)
    {
        // Check number of parameters. Should be more than 0 and 
        if (yValue.Length == 0 || (series is not null && yValue.Length > series.YValuesPerPoint))
            throw new ArgumentOutOfRangeException(nameof(yValue), SR.ExceptionDataPointYValuesSettingCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));

        // Make sure the Y values array is big enough
        if (this._yValue.Length < yValue.Length)
            this._yValue = new double[yValue.Length];

        // Save value in the array
        Array.Copy(yValue, _yValue, yValue.Length);

        if (series is null)
            return;

        // Get Date or Time if required
        if (series.YValueType == ChartValueType.Date)
        {
            for (int i = 0; i < yValue.Length; i++)
            {
                if (yValue[i] == 0.0)
                    this._yValue[i] = Math.Floor(this._yValue[i]);
                else
                    this._yValue[i] = DateTime.FromOADate(yValue[i]).Date.ToOADate();
            }
        }
        else if (series.YValueType == ChartValueType.Time)
        {
            for (int i = 0; i < yValue.Length; i++)
            {
                if (yValue[i] == 0.0)
                {
                    this._yValue[i] = this._xValue - Math.Floor(this._yValue[i]);
                }
                else
                {
                    DateTime yTime = DateTime.FromOADate(yValue[i]);
                    DateTime time = new DateTime(
                        1899,
                        12,
                        30,
                        yTime.Hour,
                        yTime.Minute,
                        yTime.Second,
                        yTime.Millisecond);

                    this._yValue[i] = time.ToOADate();
                }
            }
        }
    }

    /// <summary>
    /// Set one or more Y values of the data point. If first of the <paramref name="yValue"/> is <see langword="null" /> or <see cref="DBNull"/> will set <see cref="IsEmpty"/>.
    /// </summary>
    /// <param name="yValue">List of Y values of the data point.</param>
    public void SetValueY(params object[] yValue)
    {
        // Check arguments
        if (yValue is null)
            throw new ArgumentNullException(nameof(yValue));

        // Check number of parameters. Should be more than 0 and 
        if (yValue.Length == 0 || (series is not null && yValue.Length > series.YValuesPerPoint))
            throw new ArgumentOutOfRangeException(nameof(yValue), SR.ExceptionDataPointYValuesSettingCountMismatch(series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture)));

        // Check if there is a Null Y value
        for (int i = 0; i < yValue.Length; i++)
        {
            if (yValue[i] is null || yValue[i] is DBNull)
            {
                yValue[i] = 0.0;
                if (i == 0)
                    this.IsEmpty = true;
            }
        }

        // Check if parameters type matches with series type
        Type paramType = yValue[0].GetType();
        series?.CheckSupportedTypes(paramType);

        // Make sure the Y values array is big enough
        if (this._yValue.Length < yValue.Length)
            this._yValue = new double[yValue.Length];

        // Save value in the array
        if (paramType == typeof(string))
        {
            try
            {
                for (int i = 0; i < yValue.Length; i++)
                {
                    this._yValue[i] = CommonElements.ParseDouble((string)yValue[i]);
                }
            }
            catch
            {
                // Get reference to the chart object
                if (Common?.ChartPicture?.SuppressExceptions == true)
                {
                    this.IsEmpty = true;
                    for (int i = 0; i < yValue.Length; i++)
                    {
                        yValue[i] = 0.0;
                    }
                }
                else
                {
                    throw new ArgumentException(SR.ExceptionDataPointYValueStringFormat);
                }
            }

        }
        else if (paramType == typeof(DateTime))
        {
            for (int i = 0; i < yValue.Length; i++)
            {
                if (yValue[i] is null || (yValue[i] is double dbl && dbl == 0.0))
                {
                    this._yValue[i] = DateTime.Now.ToOADate();
                }
                else
                {
                    this._yValue[i] = ((DateTime)yValue[i]).ToOADate();
                }
            }
        }
        else
        {
            for (int i = 0; i < yValue.Length; i++)
            {
                this._yValue[i] = ConvertValue(yValue[i]);
            }
        }

        if (series is null)
            return;

        // Get Date or Time if required
        for (int i = 0; i < yValue.Length; i++)
        {
            if (yValue[i] is null || (yValue[i] is double dbl && dbl == 0.0))
            {
                if (series.YValueType == ChartValueType.Date)
                {
                    this._yValue[i] = Math.Floor(this._yValue[i]);
                }
                else if (series.YValueType == ChartValueType.Time)
                {
                    this._yValue[i] = this._xValue - Math.Floor(this._yValue[i]);
                }
            }
            else
            {
                if (series.YValueType == ChartValueType.Date)
                {
                    DateTime yDate;
                    if (yValue[i] is DateTime dt)
                        yDate = dt;
                    else if (yValue[i] is double dbl1)
                        yDate = DateTime.FromOADate(dbl1);
                    else
                        yDate = Convert.ToDateTime(yValue[i], CultureInfo.InvariantCulture); //This will throw an exception in case when the yValue type is not compatible with the DateTime

                    this._yValue[i] = yDate.Date.ToOADate();
                }
                else if (series.YValueType == ChartValueType.Time)
                {
                    DateTime yTime;
                    if (yValue[i] is DateTime tm)
                        yTime = tm;
                    if (yValue[i] is double dbl2)
                        yTime = DateTime.FromOADate(dbl2);
                    else
                        yTime = Convert.ToDateTime(yValue[i], CultureInfo.InvariantCulture); //This will throw an exception in case when the yValue type is not compatible with the DateTime

                    DateTime time = new DateTime(
                        1899,
                        12,
                        30,
                        yTime.Hour,
                        yTime.Minute,
                        yTime.Second,
                        yTime.Millisecond);

                    this._yValue[i] = time.ToOADate();
                }
            }
        }
    }

    /// <summary>
    /// Creates an exact copy of this DataPoint object.
    /// </summary>
    /// <returns>An exact copy of this DataPoint object.</returns>
    public DataPoint Clone()
    {
        // Create new data point
        DataPoint clonePoint = new DataPoint();

        // Reset series pointer
        clonePoint.series = null;
        clonePoint.pointCustomProperties = this.pointCustomProperties;

        // Copy values
        clonePoint._xValue = this.XValue;
        clonePoint._yValue = new double[this._yValue.Length];
        this._yValue.CopyTo(clonePoint._yValue, 0);
        clonePoint.tempColorIsSet = this.tempColorIsSet;
        clonePoint.isEmptyPoint = this.isEmptyPoint;

        // Copy properties
        foreach (object key in this.properties.Keys)
        {
            clonePoint.properties.Add(key, this.properties[key]);
        }

        return clonePoint;
    }

    /// <summary>
    /// Resize Y values array.
    /// </summary>
    /// <param name="newSize">New number of Y values in array.</param>
    internal void ResizeYValueArray(int newSize)
    {
        // Create new array
        double[] newArray = new double[newSize];

        // Copy elements
        if (_yValue != null)
        {
            for (int i = 0; i < ((_yValue.Length < newSize) ? _yValue.Length : newSize); i++)
            {
                newArray[i] = _yValue[i];
            }
        }

        _yValue = newArray;
    }

    /// <summary>
    /// Helper function, which returns point value by it's name.
    /// </summary>
    /// <param name="valueName">Point value names. X, Y, Y2,...</param>
    /// <returns>Point value.</returns>
    public double GetValueByName(string valueName)
    {
        // Check arguments
        if (valueName == null)
            throw new ArgumentNullException(nameof(valueName));

        valueName = valueName.ToUpper(CultureInfo.InvariantCulture);
        if (string.Equals(valueName, "X", StringComparison.Ordinal))
        {
            return this.XValue;
        }
        else if (valueName.StartsWith("Y", StringComparison.Ordinal))

        {
            if (valueName.Length == 1)
            {
                return this.YValues[0];
            }
            else
            {
                int yIndex;
                try
                {
                    yIndex = int.Parse(valueName[1..], CultureInfo.InvariantCulture) - 1;
                }
                catch (Exception)
                {
                    throw new ArgumentException(SR.ExceptionDataPointValueNameInvalid, nameof(valueName));
                }

                if (yIndex < 0)
                {
                    throw new ArgumentException(SR.ExceptionDataPointValueNameYIndexIsNotPositive, nameof(valueName));
                }

                if (yIndex >= this.YValues.Length)
                {
                    throw new ArgumentException(SR.ExceptionDataPointValueNameYIndexOutOfRange, nameof(valueName));
                }

                return this.YValues[yIndex];
            }
        }
        else
        {
            throw new ArgumentException(SR.ExceptionDataPointValueNameInvalid, nameof(valueName));
        }
    }

    /// <summary>
    /// Replaces predefined keyword inside the string with their values.
    /// </summary>
    /// <param name="strOriginal">Original string with keywords.</param>
    /// <returns>Modified string.</returns>
    internal override string ReplaceKeywords(string strOriginal)
    {
        // Nothing to process
        if (strOriginal is null || strOriginal.Length == 0)
            return strOriginal;

        // Replace all "\n" strings with '\n' character
        string result = strOriginal;
        result = result.Replace("\\n", "\n");

        // #LABEL - point label
        result = result.Replace(KeywordName.Label, this.Label);

        // #LEGENDTEXT - series name
        result = result.Replace(KeywordName.LegendText, this.LegendText);

        // #AXISLABEL - series name
        result = result.Replace(KeywordName.AxisLabel, this.AxisLabel);

        // #CUSTOMPROPERTY - one of the custom properties by name
        result = ReplaceCustomPropertyKeyword(result, this);

        if (this.series is not null)
        {
            // #INDEX - point index
            if (result.Contains(KeywordName.Index, StringComparison.Ordinal)) // on series with many points unnecessary Points.IndexOf hang up GUI :(
                result = result.Replace(KeywordName.Index, this.series.Points.IndexOf(this).ToString(CultureInfo.InvariantCulture));

            // Replace series keywords
            result = this.series.ReplaceKeywords(result);

            // #PERCENT - percentage of Y value from total
            result = this.series.ReplaceOneKeyword(
                this.Chart,
                this,
                this.Tag,
                ChartElementType.DataPoint,
                result,
                KeywordName.Percent,
                this.YValues[0] / this.series.GetTotalYValue(),
                ChartValueType.Double,
                "P");

            // #VAL[X] - point value X, Y, Y2, ...
            if (this.series.XValueType == ChartValueType.String)
            {
                result = result.Replace(KeywordName.ValX, this.AxisLabel);
            }
            else
            {
                result = this.series.ReplaceOneKeyword(
                    this.Chart,
                    this,
                    this.Tag,
                    ChartElementType.DataPoint,
                    result,
                    KeywordName.ValX,
                    this.XValue,
                    this.series.XValueType,
                    string.Empty);
            }

            if (result.Contains(KeywordName.ValY, StringComparison.Ordinal))
            {
                // remove keywords #VALY? for unexcited Y value indexes
                for (int index = this.YValues.Length; index <= 7; index++)
                {
                    result = RemoveOneKeyword(result, KeywordName.ValY + index + 1);
                }

                for (int index = 1; index <= this.YValues.Length; index++)
                {
                    result = this.series.ReplaceOneKeyword(
                        this.Chart,
                        this,
                        this.Tag,
                        ChartElementType.DataPoint,
                        result,
                        KeywordName.ValY + index,
                        this.YValues[index - 1],
                        this.series.YValueType,
                        string.Empty);
                }

                result = this.series.ReplaceOneKeyword(
                    Chart,
                    this,
                    this.Tag,
                    ChartElementType.DataPoint,
                    result,
                    KeywordName.ValY,
                    this.YValues[0],
                    this.series.YValueType,
                    string.Empty);
            }

            result = this.series.ReplaceOneKeyword(
                Chart,
                this,
                this.Tag,
                ChartElementType.DataPoint,
                result,
                KeywordName.Val,
                this.YValues[0],
                this.series.YValueType,
                string.Empty);
        }

        return result;
    }

    /// <summary>
    /// Removes one keyword from format string.
    /// </summary>
    /// <param name="strOriginal">Original format string</param>
    /// <param name="keyword">The keyword</param>
    /// <returns>Modified format string</returns>
    private static string RemoveOneKeyword(string strOriginal, string keyword)
    {
        string result = strOriginal;
        int keyIndex;
        while ((keyIndex = result.IndexOf(keyword, StringComparison.Ordinal)) != -1)
        {
            // Get optional format
            int keyEndIndex = keyIndex + keyword.Length;
            if (result.Length > keyEndIndex && result[keyEndIndex] == '{')
            {
                int formatEnd = result.IndexOf('}', keyEndIndex);
                if (formatEnd == -1)
                {
                    throw new InvalidOperationException(SR.ExceptionDataSeriesKeywordFormatInvalid(result));
                }

                keyEndIndex = formatEnd + 1;
            }
            // Remove keyword string (with optional format)
            result = result.Remove(keyIndex, keyEndIndex - keyIndex);
            var strToReplace = SR.FormatErrorString;
            if (!string.IsNullOrEmpty(strToReplace))
            {
                result = result.Insert(keyIndex, strToReplace);
            }
        }

        return result;
    }


    /// <summary>
    /// Replaces all "#CUSTOMPROPERTY(XXX)" (where XXX is the custom attribute name) 
    /// keywords in the string provided. 
    /// </summary>
    /// <param name="originalString">String where the keyword need to be replaced.</param>
    /// <param name="properties">DataPoint or Series properties class.</param>
    /// <returns>Converted string.</returns>
    internal static string ReplaceCustomPropertyKeyword(string originalString, DataPointCustomProperties properties)
    {
        string result = originalString;
        int keyStartIndex;
        while ((keyStartIndex = result.IndexOf(KeywordName.CustomProperty, StringComparison.Ordinal)) >= 0)
        {
            string attributeValue = string.Empty;

            // Forward to the end of the keyword
            int keyEndIndex = keyStartIndex + KeywordName.CustomProperty.Length;

            // An opening bracket '(' must follow
            if (result.Length > keyEndIndex && result[keyEndIndex] == '(')
            {
                ++keyEndIndex;
                int attributeNameStartIndex = keyEndIndex;

                // Search for the closing bracket
                int closingBracketIndex = result.IndexOf(')', keyEndIndex);
                if (closingBracketIndex >= keyEndIndex)
                {
                    keyEndIndex = closingBracketIndex + 1;
                    string attributeName = result.Substring(attributeNameStartIndex, keyEndIndex - attributeNameStartIndex - 1);

                    // Get attribute value
                    if (properties.IsCustomPropertySet(attributeName))
                    {
                        attributeValue = properties.GetCustomProperty(attributeName);
                    }
                    else
                    {
                        // In case of the DataPoint check if the attribute is set in the parent series
                        if (properties is DataPoint dataPoint && dataPoint.series is not null)
                        {
                            if (dataPoint.series.IsCustomPropertySet(attributeName))
                            {
                                attributeValue = dataPoint.series.GetCustomProperty(attributeName);
                            }
                        }
                    }
                }
            }

            // Remove keyword string with attribute name
            result = result.Remove(keyStartIndex, keyEndIndex - keyStartIndex);

            // Insert value of the custom attribute
            result = result.Insert(keyStartIndex, attributeValue);
        }

        return result;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    internal override string ToStringInternal()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat(CultureInfo.CurrentCulture, "{{X={0}, ", XValue);
        if (YValues.Length == 1)
        {
            sb.AppendFormat(CultureInfo.CurrentCulture, "Y={0}", YValues[0]);
        }
        else
        {
            sb.Append("Y={");
            for (int i = 0; i < YValues.Length; i++)
            {
                if (i == 0)
                    sb.AppendFormat(CultureInfo.CurrentCulture, "{0}", YValues[i]);
                else
                    sb.AppendFormat(CultureInfo.CurrentCulture, ", {0}", YValues[i]);
            }

            sb.Append('}');
        }

        sb.Append('}');
        return sb.ToString();
    }
    #endregion

    #region	DataPoint Properties


    /// <summary>
    /// X value of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    Bindable(true),
    SRDescription("DescriptionAttributeDataPoint_XValue"),
    TypeConverter(typeof(DataPointValueConverter)),
    DefaultValue(typeof(double), "0.0"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    ]
    public double XValue
    {
        get => _xValue;
        set
        {
            _xValue = value;
            this.Invalidate(false);
        }
    }

    /// <summary>
    /// List of Y values of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    SRDescription("DescriptionAttributeDataPoint_YValues"),
    Bindable(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    TypeConverter(typeof(DoubleArrayConverter)),
    Editor(typeof(UITypeEditor), typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All),
    SerializationVisibility(SerializationVisibility.Attribute)
    ]
    public double[] YValues
    {
        get => _yValue;
        set
        {
            if (value == null)
            {
                // Clear array data
                for (int i = 0; i < _yValue.Length; i++)
                {
                    _yValue[i] = 0;
                }
            }
            else
            {
                _yValue = value;
            }

            this.Invalidate(false);
        }
    }

    /// <summary>
    /// A flag which indicates whether the data point is empty.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),

    Bindable(true),
    SRDescription("DescriptionAttributeDataPoint_Empty"),
    DefaultValue(false)
    ]
    public bool IsEmpty
    {
        get => isEmptyPoint;
        set
        {
            isEmptyPoint = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Name of the data point. This field is reserved for internal use only.
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    Bindable(true),
    Browsable(false),
    SRDescription("DescriptionAttributeDataPoint_Name"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibility(SerializationVisibility.Hidden)
    ]
    public override string Name
    {
        get => "DataPoint";
        set
        {
            //Dont call the base method - the names don't need to be unique
        }
    }

    #endregion
}

/// <summary>
/// Stores properties of one Data Point and Data series.
/// </summary>
[
SRDescription("DescriptionAttributeDataPointCustomProperties_DataPointCustomProperties"),
DefaultProperty("LabelStyle"),
TypeConverter(typeof(DataPointCustomPropertiesConverter))
]
public class DataPointCustomProperties : ChartNamedElement
{
    #region Fields and enumerations

    // True indicates data point properties. Otherwise - series.
    internal bool pointCustomProperties = true;

    // Reference to the data point series
    internal Series series;

    // Storage for the custom properties names/values
    internal Hashtable properties = new Hashtable();

    // Flag indicating that temp. color was set
    internal bool tempColorIsSet;

    // Design time custom properties data
    internal CustomProperties customProperties;

    // IsEmpty point indicator
    internal bool isEmptyPoint;

    #endregion

    #region Constructors

    /// <summary>
    /// DataPointCustomProperties constructor.
    /// </summary>
    public DataPointCustomProperties()
    {
        // Initialize the data series
        this.series = null;
        this.customProperties = new CustomProperties(this);
    }

    /// <summary>
    /// DataPointCustomProperties constructor.
    /// </summary>
    /// <param name="series">The series which the data point belongs to.</param>
    /// <param name="pointProperties">Indicates whether this is a data point custom properties.</param>
    public DataPointCustomProperties(Series series, bool pointProperties) : base(series, string.Empty)
    {
        // Initialize the data series
        this.series = series;
        this.pointCustomProperties = pointProperties;
        this.customProperties = new CustomProperties(this);
    }

    #endregion

    #region Custom Properties methods

    /// <summary>
    /// Checks if custom property with specified name was set.
    /// </summary>
    /// <param name="name">Name of the custom property to check.</param>
    /// <returns>True if custom property was set.</returns>
    public virtual bool IsCustomPropertySet(string name)
    {
        return properties.ContainsKey(name);
    }

    /// <summary>
    /// Checks if the custom property with specified name was set.
    /// </summary>
    /// <param name="property">The CommonCustomProperties object to check for.</param>
    /// <returns>True if attribute was set.</returns>
    internal bool IsCustomPropertySet(CommonCustomProperties property)
    {
        return properties.ContainsKey((int)property);
    }

    /// <summary>
    /// Delete the data point custom property with the specified name.
    /// </summary>
    /// <param name="name">Name of the property to delete.</param>
    public virtual void DeleteCustomProperty(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(SR.ExceptionAttributeNameIsEmpty);
        }

        // Check if trying to delete the common attribute
        string[] AttributesNames = Enum.GetNames(typeof(CommonCustomProperties));
        foreach (string commonName in AttributesNames)
        {
            if (name == commonName)
            {
                DeleteCustomProperty((CommonCustomProperties)Enum.Parse(typeof(CommonCustomProperties), commonName));
            }
        }

        // Remove attribute
        properties.Remove(name);
    }

    /// <summary>
    /// Delete Data Point attribute with specified name.
    /// </summary>
    /// <param name="property">ID of the attribute to delete.</param>
    internal void DeleteCustomProperty(CommonCustomProperties property)
    {
        // Check if trying to delete the common attribute from the series
        if (!this.pointCustomProperties)
        {
            throw new ArgumentException(SR.ExceptionAttributeUnableToDelete);
        }

        // Remove attribute
        properties.Remove((int)property);
    }

    /// <summary>
    /// Gets the data point custom property with the specified name.
    /// </summary>
    /// <param name="name">Name of the property to get.</param>
    /// <returns>Returns the data point custom property with the specified name.  If the requested one is not set, 
    /// the default custom property of the data series will be returned.</returns>
    public virtual string GetCustomProperty(string name)
    {
        if (!IsCustomPropertySet(name) && this.pointCustomProperties)
        {
            // Check if we are in serialization mode
            bool serializing = false;

            if (Chart != null && Chart.serializing)
            {
                serializing = true;
            }

            if (!serializing)
            {

                if (this.isEmptyPoint)
                {
                    // Return empty point properties from series
                    return (string)series.EmptyPointStyle.properties[name];
                }

                // Return properties from series
                return (string)series.properties[name];
            }
            else
            {
                // Return default properties
                return Series.defaultCustomProperties[name];
            }
        }

        return (string)properties[name];
    }


    /// <summary>
    /// Checks if data is currently serialized.
    /// </summary>
    /// <returns>True if serialized.</returns>
    internal bool IsSerializing()
    {
        // Check if series object is provided
        if (series == null)
        {
            return true;
        }

        // Check if we are in serialization mode
        if (Chart != null)
        {
            return Chart.serializing;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns an attribute object of the Data Point. If required attribute is not set
    /// in the Data Point the default attribute of the Data series is returned.
    /// </summary>
    /// <param name="attrib">Attribute name ID.</param>
    /// <returns>Attribute value.</returns>
    internal object GetAttributeObject(CommonCustomProperties attrib)
    {
        // Get series properties
        if (!this.pointCustomProperties || series == null)
        {
            return properties[(int)attrib];
        }

        // Get data point properties
        if (properties.Count == 0 || !IsCustomPropertySet(attrib))
        {
            // Check if we are in serialization mode
            bool serializing = false;
            if (Chart != null)
            {
                serializing = Chart.serializing;
            }

            if (!serializing)
            {
                if (this.isEmptyPoint)
                {
                    // Return empty point properties from series
                    return series.EmptyPointStyle.properties[(int)attrib];
                }

                // Return properties from series
                return series.properties[(int)attrib];
            }
            else
            {
                // Return default properties
                return Series.defaultCustomProperties.properties[(int)attrib];
            }
        }

        return properties[(int)attrib];
    }

    /// <summary>
    /// Sets a custom property of the data point. 
    /// </summary>
    /// <param name="name">Property name.</param>
    /// <param name="propertyValue">Property value.</param>
    public virtual void SetCustomProperty(string name, string propertyValue)
    {
        properties[name] = propertyValue;
    }

    /// <summary>
    /// Sets an attribute of the Data Point as an object. 
    /// </summary>
    /// <param name="attrib">Attribute name ID.</param>
    /// <param name="attributeValue">Attribute new value.</param>
    internal void SetAttributeObject(CommonCustomProperties attrib, object attributeValue)
    {
        properties[(int)attrib] = attributeValue;
    }

    /// <summary>
    /// Set the default properties of the data point.
    /// <param name="clearAll">Indicates that previous properties must be cleared.</param>
    /// </summary>
    public virtual void SetDefault(bool clearAll)
    {
        // If setting defaults for the data series - clear all properties and initialize common one
        if (!this.pointCustomProperties)
        {
            if (clearAll)
            {
                properties.Clear();
            }

            // !!! IMPORTANT !!!
            // After changing the default value of the common attribute you must also
            // change the DefaultAttribute of the property representing this attribute.
            if (!IsCustomPropertySet(CommonCustomProperties.ToolTip))
                SetAttributeObject(CommonCustomProperties.ToolTip, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LegendToolTip))
                SetAttributeObject(CommonCustomProperties.LegendToolTip, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.Color))
                SetAttributeObject(CommonCustomProperties.Color, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.IsValueShownAsLabel))
                SetAttributeObject(CommonCustomProperties.IsValueShownAsLabel, false);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerStyle))
                SetAttributeObject(CommonCustomProperties.MarkerStyle, MarkerStyle.None);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerSize))
                SetAttributeObject(CommonCustomProperties.MarkerSize, 5);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerImage))
                SetAttributeObject(CommonCustomProperties.MarkerImage, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.Label))
                SetAttributeObject(CommonCustomProperties.Label, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BorderWidth))
                SetAttributeObject(CommonCustomProperties.BorderWidth, 1);
            if (!IsCustomPropertySet(CommonCustomProperties.BorderDashStyle))
                SetAttributeObject(CommonCustomProperties.BorderDashStyle, ChartDashStyle.Solid);


            if (!IsCustomPropertySet(CommonCustomProperties.AxisLabel))
                SetAttributeObject(CommonCustomProperties.AxisLabel, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelFormat))
                SetAttributeObject(CommonCustomProperties.LabelFormat, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BorderColor))
                SetAttributeObject(CommonCustomProperties.BorderColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BackImage))
                SetAttributeObject(CommonCustomProperties.BackImage, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BackImageWrapMode))
                SetAttributeObject(CommonCustomProperties.BackImageWrapMode, ChartImageWrapMode.Tile);
            if (!IsCustomPropertySet(CommonCustomProperties.BackImageAlignment))
                SetAttributeObject(CommonCustomProperties.BackImageAlignment, ChartImageAlignmentStyle.TopLeft);
            if (!IsCustomPropertySet(CommonCustomProperties.BackImageTransparentColor))
                SetAttributeObject(CommonCustomProperties.BackImageTransparentColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BackGradientStyle))
                SetAttributeObject(CommonCustomProperties.BackGradientStyle, GradientStyle.None);
            if (!IsCustomPropertySet(CommonCustomProperties.BackSecondaryColor))
                SetAttributeObject(CommonCustomProperties.BackSecondaryColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.BackHatchStyle))
                SetAttributeObject(CommonCustomProperties.BackHatchStyle, ChartHatchStyle.None);
            if (!IsCustomPropertySet(CommonCustomProperties.Font))
                SetAttributeObject(CommonCustomProperties.Font, null);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerImageTransparentColor))
                SetAttributeObject(CommonCustomProperties.MarkerImageTransparentColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerColor))
                SetAttributeObject(CommonCustomProperties.MarkerColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerBorderColor))
                SetAttributeObject(CommonCustomProperties.MarkerBorderColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.MarkerBorderWidth))
                SetAttributeObject(CommonCustomProperties.MarkerBorderWidth, 1);
            if (!IsCustomPropertySet(CommonCustomProperties.MapAreaAttributes))
                SetAttributeObject(CommonCustomProperties.MapAreaAttributes, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.PostBackValue))
                SetAttributeObject(CommonCustomProperties.PostBackValue, string.Empty);

            if (!IsCustomPropertySet(CommonCustomProperties.LabelForeColor))
                SetAttributeObject(CommonCustomProperties.LabelForeColor, Color.Black);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelAngle))
                SetAttributeObject(CommonCustomProperties.LabelAngle, 0);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelToolTip))
                SetAttributeObject(CommonCustomProperties.LabelToolTip, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelUrl))
                SetAttributeObject(CommonCustomProperties.LabelUrl, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelPostBackValue))
                SetAttributeObject(CommonCustomProperties.LabelPostBackValue, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelMapAreaAttributes))
                SetAttributeObject(CommonCustomProperties.LabelMapAreaAttributes, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelBackColor))
                SetAttributeObject(CommonCustomProperties.LabelBackColor, Color.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelBorderWidth))
                SetAttributeObject(CommonCustomProperties.LabelBorderWidth, 1);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelBorderDashStyle))
                SetAttributeObject(CommonCustomProperties.LabelBorderDashStyle, ChartDashStyle.Solid);
            if (!IsCustomPropertySet(CommonCustomProperties.LabelBorderColor))
                SetAttributeObject(CommonCustomProperties.LabelBorderColor, Color.Empty);

            if (!IsCustomPropertySet(CommonCustomProperties.Url))
                SetAttributeObject(CommonCustomProperties.Url, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LegendUrl))
                SetAttributeObject(CommonCustomProperties.LegendUrl, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LegendPostBackValue))
                SetAttributeObject(CommonCustomProperties.LegendPostBackValue, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LegendText))
                SetAttributeObject(CommonCustomProperties.LegendText, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.LegendMapAreaAttributes))
                SetAttributeObject(CommonCustomProperties.LegendMapAreaAttributes, string.Empty);
            if (!IsCustomPropertySet(CommonCustomProperties.IsVisibleInLegend))
                SetAttributeObject(CommonCustomProperties.IsVisibleInLegend, true);
        }

        // If setting defaults for the data point - clear all properties
        else
        {
            properties.Clear();
        }
    }

    #endregion

    #region	DataPointCustomProperties Properties

    /// <summary>
    /// Indexer of the custom properties. Returns the DataPointCustomProperties object by index.
    /// </summary>
    /// <param name="index">Index of the custom property.</param>
    public string this[int index]
    {
        get
        {
            int currentIndex = 0;
            foreach (object key in properties.Keys)
            {
                if (currentIndex == index)
                {
                    if (key is string keyStr)
                    {
                        return keyStr;
                    }
                    else if (key is int)
                    {
                        return Enum.GetName(typeof(CommonCustomProperties), key);
                    }

                    return key.ToString();
                }

                ++currentIndex;
            }
            // we can't throw IndexOutOfRangeException here, it is reserved
            // by the CLR.
            throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// Indexer of the custom properties. Returns the DataPointCustomProperties object by name.
    /// </summary>
    /// <param name="name">Name of the custom property.</param>
    public string this[string name]
    {
        get
        {
            // If attribute is not set in data point - try getting it from the series
            if (!IsCustomPropertySet(name) && this.pointCustomProperties)
            {
                if (this.isEmptyPoint)
                {
                    return (string)series.EmptyPointStyle.properties[name];
                }

                return (string)series.properties[name];
            }

            return (string)properties[name];
        }
        set
        {
            properties[name] = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// The text of the data point label.
    /// </summary>
    [
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    SRCategory("CategoryAttributeLabel"),
    Bindable(true),
    SRDescription("DescriptionAttributeLabel"),
    ]
    public virtual string Label
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.Label))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.Label);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return string.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.Label);
                    }

                    return series.label;
                }
            }
            else
            {
                return series.label;
            }
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.Label, value);
            else
                series.label = value;

            this.Invalidate(true);
        }
    }

    /// <summary>
    /// The text of X axis label for the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeMisc"),
    Bindable(true),
    SRDescription("DescriptionAttributeAxisLabel"),
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    ]
    public virtual string AxisLabel
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.AxisLabel))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.AxisLabel);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return string.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.AxisLabel);
                    }

                    return series.axisLabel;

                }
            }
            else
            {
                return series.axisLabel;
            }
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.AxisLabel, value);
            else
                series.axisLabel = value;

            // Set flag that there are non-empy axis labels in series or points
            if (value.Length > 0 && series != null)
            {
                series.noLabelsInPoints = false;
            }

            this.Invalidate(false);
        }
    }

    /// <summary>
    /// Format string of the data point label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabel"),
    Bindable(true),
    SRDescription("DescriptionAttributeLabelFormat")
    ]
    public string LabelFormat
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelFormat))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.LabelFormat);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return string.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelFormat);
                    }

                    return series.labelFormat;
                }
            }
            else
            {
                return series.labelFormat;
            }
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelFormat, value);
            else
                series.labelFormat = value;
            this.Invalidate(false);
        }
    }

    /// <summary>
    /// A flag which indicates whether to show the data point's value on the label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabel"),
    Bindable(true),
    SRDescription("DescriptionAttributeShowLabelAsValue")
    ]
    public bool IsValueShownAsLabel
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.IsValueShownAsLabel))
                {
                    return (bool)GetAttributeObject(CommonCustomProperties.IsValueShownAsLabel);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return false;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (bool)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.IsValueShownAsLabel);
                    }

                    return series.showLabelAsValue;

                }
            }
            else
            {
                return series.showLabelAsValue;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.IsValueShownAsLabel, value);
            else
                series.showLabelAsValue = value;
            this.Invalidate(false);
        }
    }

    /// <summary>
    /// Color of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeColor4"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color Color
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.Color))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.Color);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return Color.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.Color);
                    }

                    return series.color;
                }
            }
            else
            {
                return series.color;
            }
        }
        set
        {
            // Remove the temp color flag
            this.tempColorIsSet = false;

            if (value == Color.Empty && this.pointCustomProperties)
            {
                DeleteCustomProperty(CommonCustomProperties.Color);
            }
            else
            {
                if (this.pointCustomProperties)
                    SetAttributeObject(CommonCustomProperties.Color, value);
                else
                    series.color = value;
                this.Invalidate(true);
            }
        }
    }

    /// <summary>
    /// Border color of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBorderColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color BorderColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BorderColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.BorderColor);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return Color.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BorderColor);
                    }

                    return series.borderColor;
                }
            }
            else
            {
                return series.borderColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BorderColor, value);
            else
                series.borderColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Border style of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBorderDashStyle")
    ]
    public ChartDashStyle BorderDashStyle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BorderDashStyle))
                {
                    return (ChartDashStyle)GetAttributeObject(CommonCustomProperties.BorderDashStyle);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return ChartDashStyle.Solid;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (ChartDashStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BorderDashStyle);
                    }

                    return series.borderDashStyle;

                }
            }
            else
            {
                return series.borderDashStyle;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BorderDashStyle, value);
            else
                series.borderDashStyle = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Border width of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBorderWidth"),
    ]
    public int BorderWidth
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BorderWidth))
                {
                    return (int)GetAttributeObject(CommonCustomProperties.BorderWidth);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return 1;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (int)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BorderWidth);
                    }

                    return series.borderWidth;

                }
            }
            else
            {
                return series.borderWidth;
            }
        }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionBorderWidthIsNotPositive);
            }

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BorderWidth, value);
            else
                series.borderWidth = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Background image of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBackImage"),
    Editor("ImageValueEditor", typeof(UITypeEditor)),
    ]
    public string BackImage
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackImage))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.BackImage);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return string.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackImage);
                    }

                    return series.backImage;

                }
            }
            else
            {
                return series.backImage;
            }
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackImage, value);
            else
                series.backImage = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the drawing mode of the background image.
    /// </summary>
    /// <value>
    /// A <see cref="ChartImageWrapMode"/> value that defines the drawing mode of the image. 
    /// </value>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeImageWrapMode")
    ]
    public ChartImageWrapMode BackImageWrapMode
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackImageWrapMode))
                {
                    return (ChartImageWrapMode)GetAttributeObject(CommonCustomProperties.BackImageWrapMode);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return ChartImageWrapMode.Tile;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (ChartImageWrapMode)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackImageWrapMode);
                    }

                    return series.backImageWrapMode;

                }
            }
            else
            {
                return series.backImageWrapMode;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackImageWrapMode, value);
            else
                series.backImageWrapMode = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets a color which will be replaced with a transparent color while drawing the background image.
    /// </summary>
    /// <value>
    /// A <see cref="Color"/> value which will be replaced with a transparent color while drawing the image.
    /// </value>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    NotifyParentProperty(true),
    SRDescription("DescriptionAttributeImageTransparentColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color BackImageTransparentColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackImageTransparentColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.BackImageTransparentColor);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return Color.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackImageTransparentColor);
                    }

                    return series.backImageTransparentColor;

                }
            }
            else
            {
                return series.backImageTransparentColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackImageTransparentColor, value);
            else
                series.backImageTransparentColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the alignment of the background image which is used by ClampUnscale drawing mode.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    NotifyParentProperty(true),
    SRDescription("DescriptionAttributeBackImageAlign")
    ]
    public ChartImageAlignmentStyle BackImageAlignment
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackImageAlignment))
                {
                    return (ChartImageAlignmentStyle)GetAttributeObject(CommonCustomProperties.BackImageAlignment);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return ChartImageAlignmentStyle.TopLeft;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (ChartImageAlignmentStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackImageAlignment);
                    }

                    return series.backImageAlignment;

                }
            }
            else
            {
                return series.backImageAlignment;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackImageAlignment, value);
            else
                series.backImageAlignment = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the background gradient style.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBackGradientStyle"),
    Editor("GradientEditor", typeof(UITypeEditor))
    ]
    public GradientStyle BackGradientStyle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackGradientStyle))
                {
                    return (GradientStyle)GetAttributeObject(CommonCustomProperties.BackGradientStyle);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return GradientStyle.None;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (GradientStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackGradientStyle);
                    }

                    return series.backGradientStyle;

                }
            }
            else
            {
                return series.backGradientStyle;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackGradientStyle, value);
            else
                series.backGradientStyle = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the secondary background color.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBackSecondaryColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color BackSecondaryColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackSecondaryColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.BackSecondaryColor);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return Color.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackSecondaryColor);
                    }

                    return series.backSecondaryColor;

                }
            }
            else
            {
                return series.backSecondaryColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackSecondaryColor, value);
            else
                series.backSecondaryColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the background hatch style.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBackHatchStyle"),
    Editor("HatchStyleEditor", typeof(UITypeEditor))
    ]
    public ChartHatchStyle BackHatchStyle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.BackHatchStyle))
                {
                    return (ChartHatchStyle)GetAttributeObject(CommonCustomProperties.BackHatchStyle);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return ChartHatchStyle.None;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (ChartHatchStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.BackHatchStyle);
                    }

                    return series.backHatchStyle;

                }
            }
            else
            {
                return series.backHatchStyle;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.BackHatchStyle, value);
            else
                series.backHatchStyle = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the font of the data point.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeFont")
    ]
    public Font Font
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.Font))
                {
                    if (GetAttributeObject(CommonCustomProperties.Font) is Font font)
                        return font;
                }

                if (IsSerializing())
                {
                    return FontCache.DefaultFont;
                }

                if (this.isEmptyPoint)
                {
                    return series.EmptyPointStyle.Font;
                }

                return series.font;
            }
            else
            {
                return series.font;
            }
        }

        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.Font, value);
            else
                series.font = value;

            this.Invalidate(false);
        }
    }

    /// <summary>
    /// Gets or sets the label color.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeFontColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor))
    ]
    public Color LabelForeColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelForeColor))
                {
                    Color color = (Color)GetAttributeObject(CommonCustomProperties.LabelForeColor);
                    return color;
                }
                else
                {
                    if (IsSerializing())
                    {
                        return Color.Black;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelForeColor);
                    }

                    if (SystemInformation.HighContrast)
                    {
                        return SystemColors.WindowText;
                    }

                    return series.fontColor;

                }
            }
            else
            {
                return series.fontColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelForeColor, value);
            else
                series.fontColor = value;
            this.Invalidate(false);
        }
    }

    /// <summary>
    /// Gets or sets the angle of the label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription(SR.Keys.DescriptionAttributeLabel_FontAngle)
    ]
    public int LabelAngle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelAngle))
                {
                    return (int)GetAttributeObject(CommonCustomProperties.LabelAngle);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return 0;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (int)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelAngle);
                    }

                    return series.fontAngle;

                }
            }
            else
            {
                return series.fontAngle;
            }
        }
        set
        {
            if (value < -90 || value > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionAngleRangeInvalid);
            }

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelAngle, value);
            else
                series.fontAngle = value;
            this.Invalidate(false);
        }
    }

    /// <summary>
    /// Gets or sets the marker style.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerStyle4"),
    Editor("MarkerStyleEditor", typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All)
    ]
    public MarkerStyle MarkerStyle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerStyle))
                {
                    return (MarkerStyle)GetAttributeObject(CommonCustomProperties.MarkerStyle);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return MarkerStyle.None;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (MarkerStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerStyle);
                    }

                    return series.markerStyle;

                }
            }
            else
            {
                return series.markerStyle;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerStyle, value);
            else
                series.markerStyle = value;

            if (this is Series thisSeries)

                thisSeries.tempMarkerStyleIsSet = false;

            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the size of the marker.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerSize"),
    RefreshProperties(RefreshProperties.All)
    ]
    public int MarkerSize
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerSize))
                {
                    return (int)GetAttributeObject(CommonCustomProperties.MarkerSize);
                }
                else
                {
                    if (IsSerializing())
                        return 5;

                    if (this.isEmptyPoint)
                        return (int)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerSize);

                    return series.markerSize;

                }
            }
            else
            {
                return series.markerSize;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerSize, value);
            else
                series.markerSize = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the marker image.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerImage"),
    Editor("ImageValueEditor", typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All)
    ]
    public string MarkerImage
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerImage))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.MarkerImage);
                }
                else
                {
                    if (IsSerializing())
                    {
                        return string.Empty;
                    }

                    if (this.isEmptyPoint)
                    {
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerImage);
                    }

                    return series.markerImage;

                }
            }
            else
            {
                return series.markerImage;
            }
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerImage, value);
            else
                series.markerImage = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the color which will be replaced with a transparent color while drawing the marker image.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeImageTransparentColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All)
    ]
    public Color MarkerImageTransparentColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerImageTransparentColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.MarkerImageTransparentColor);
                }
                else
                {
                    if (IsSerializing())
                        return Color.Empty;

                    if (this.isEmptyPoint)
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerImageTransparentColor);

                    return series.markerImageTransparentColor;

                }
            }
            else
            {
                return series.markerImageTransparentColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerImageTransparentColor, value);
            else
                series.markerImageTransparentColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the marker color.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerColor3"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All)
    ]
    public Color MarkerColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.MarkerColor);
                }
                else
                {
                    if (IsSerializing())
                        return Color.Empty;

                    if (this.isEmptyPoint)
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerColor);

                    return series.markerColor;

                }
            }
            else
            {
                return series.markerColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerColor, value);
            else
                series.markerColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Gets or sets the border color of the marker.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerBorderColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    RefreshProperties(RefreshProperties.All)
    ]
    public Color MarkerBorderColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerBorderColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.MarkerBorderColor);
                }
                else
                {
                    if (IsSerializing())
                        return Color.Empty;

                    if (this.isEmptyPoint)
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerBorderColor);

                    return series.markerBorderColor;

                }
            }
            else
            {
                return series.markerBorderColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerBorderColor, value);
            else
                series.markerBorderColor = value;
            this.Invalidate(true);
        }
    }



    /// <summary>
    /// Gets or sets the border width of the marker.
    /// </summary>
    [
    SRCategory("CategoryAttributeMarker"),
    Bindable(true),
    SRDescription("DescriptionAttributeMarkerBorderWidth")
    ]
    public int MarkerBorderWidth
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.MarkerBorderWidth))
                {
                    return (int)GetAttributeObject(CommonCustomProperties.MarkerBorderWidth);
                }
                else
                {
                    if (IsSerializing())
                        return 1;

                    if (this.isEmptyPoint)
                        return (int)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.MarkerBorderWidth);

                    return series.markerBorderWidth;

                }
            }
            else
            {
                return series.markerBorderWidth;
            }
        }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionBorderWidthIsNotPositive);

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.MarkerBorderWidth, value);
            else
                series.markerBorderWidth = value;

            this.Invalidate(true);
        }
    }



    /// <summary>
    /// Gets or sets the extended custom properties of the data point.
    /// Extended custom properties can be specified in the following format: 
    /// AttrName1=Value1, AttrName2=Value2, ...  
    /// </summary>
    [
    SRCategory("CategoryAttributeMisc"),
    Bindable(false),
    SRDescription("DescriptionAttributeCustomAttributesExtended"),
    DefaultValue(null),
    RefreshProperties(RefreshProperties.All),
    NotifyParentProperty(true),
    DesignOnly(true),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibility(SerializationVisibility.Hidden),
    EditorBrowsable(EditorBrowsableState.Never),
    DisplayName("CustomProperties")
    ]
    public CustomProperties CustomPropertiesExtended
    {
        set => customProperties = value;
        get => customProperties;
    }

    /// <summary>
    /// Gets or sets the custom properties of the data point.
    /// Custom properties can be specified in the following format: 
    /// AttrName1=Value1, AttrName2=Value2, ...  
    /// </summary>
    [
    SRCategory("CategoryAttributeMisc"),
    Bindable(true),
    Browsable(false),
    SRDescription("DescriptionAttributeCustomAttributesExtended"),
    DefaultValue("")
    ]
    public string CustomProperties
    {
        get
        {
            // Save all custom properties in a string
            string result = string.Empty;
            string[] attributesNames = Enum.GetNames(typeof(CommonCustomProperties));
            for (int i = properties.Count - 1; i >= 0; i--)
            {
                if (this[i] != null)
                {
                    string attributeName = this[i];

                    // Check if attribute is custom
                    bool customAttribute = true;
                    foreach (string name in attributesNames)
                    {
                        if (string.Equals(attributeName, name, StringComparison.OrdinalIgnoreCase))
                        {
                            customAttribute = false;
                            break;
                        }
                    }

                    // Add custom attribute to the string
                    if (customAttribute && properties[attributeName] != null)
                    {
                        if (result.Length > 0)
                            result += ", ";

                        string attributeValue = properties[attributeName].ToString().Replace(",", "\\,");
                        attributeValue = attributeValue.Replace("=", "\\=");

                        result += attributeName + "=" + attributeValue;
                    }
                }
            }

            return result;
        }
        set
        {
            // Replace NULL with empty string
            value ??= string.Empty;

            // Copy all common properties to the new collection
            Hashtable newAttributes = new Hashtable();
            Array enumValues = Enum.GetValues(typeof(CommonCustomProperties));
            foreach (object val in enumValues)
            {
                if (IsCustomPropertySet((CommonCustomProperties)val))
                {
                    newAttributes[(int)val] = properties[(int)val];
                }
            }

            if (value.Length > 0)
            {
                // Replace commas in value string
                value = value.Replace("\\,", "\\x45");
                value = value.Replace("\\=", "\\x46");

                // Add new custom properties
                string[] nameValueStrings = value.Split(',');
                foreach (string nameValue in nameValueStrings)
                {
                    string[] values = nameValue.Split('=');

                    // Check format
                    if (values.Length != 2)
                    {
                        throw new FormatException(SR.ExceptionAttributeInvalidFormat);
                    }

                    // Check for empty name or value
                    values[0] = values[0].Trim();
                    values[1] = values[1].Trim();
                    if (values[0].Length == 0)
                    {
                        throw new FormatException(SR.ExceptionAttributeInvalidFormat);
                    }

                    // Check if value already defined
                    foreach (object existingAttributeName in newAttributes.Keys)
                    {
                        if (existingAttributeName is string existingAttributeNameStr)
                        {
                            if (string.Equals(existingAttributeNameStr, values[0], StringComparison.OrdinalIgnoreCase))
                            {
                                throw new FormatException(SR.ExceptionAttributeNameIsNotUnique(values[0]));
                            }
                        }
                    }

                    string newValue = values[1].Replace("\\x45", ",");
                    newAttributes[values[0]] = newValue.Replace("\\x46", "=");

                }
            }

            properties = newAttributes;
            this.Invalidate(true);
        }
    }

    #endregion

    #region	IMapAreaAttributesutes Properties implementation

    /// <summary>
    /// Tooltip.
    /// </summary>
    [
    SRCategory("CategoryAttributeMapArea"),
    Bindable(true),
    SRDescription("DescriptionAttributeToolTip"),
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    ]
    public string ToolTip
    {
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.ToolTip, value);
            else
                series.toolTip = value;

            if (Chart != null && Chart.selection != null)
            {
                Chart.selection.enabledChecked = false;
            }
        }
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.ToolTip))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.ToolTip);
                }
                else
                {
                    if (IsSerializing())
                        return string.Empty;

                    if (this.isEmptyPoint)
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.ToolTip);

                    return series.toolTip;

                }
            }
            else
            {
                return series.toolTip;
            }
        }
    }

    /// <summary>
    /// Replaces predefined keyword inside the string with their values.
    /// </summary>
    /// <param name="strOriginal">Original string with keywords.</param>
    /// <returns>Modified string.</returns>
    internal virtual string ReplaceKeywords(string strOriginal)
    {
        return strOriginal;
    }

    #endregion

    #region	Legend properties

    /// <summary>
    /// Indicates whether the item is shown in the legend.
    /// </summary>
    [
    SRCategory("CategoryAttributeLegend"),
    Bindable(true),
    SRDescription("DescriptionAttributeShowInLegend")
    ]
    public bool IsVisibleInLegend
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.IsVisibleInLegend))
                {
                    return (bool)GetAttributeObject(CommonCustomProperties.IsVisibleInLegend);
                }
                else
                {
                    if (IsSerializing())
                        return true;

                    if (this.isEmptyPoint)
                        return (bool)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.IsVisibleInLegend);

                    return series.showInLegend;
                }
            }
            else
            {
                return series.showInLegend;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.IsVisibleInLegend, value);
            else
                series.showInLegend = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Text of the item in the legend
    /// </summary>
    [
    SRCategory("CategoryAttributeLegend"),
    Bindable(true),
    SRDescription("DescriptionAttributeLegendText"),
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    ]
    public string LegendText
    {
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LegendText, value);
            else
                series.legendText = value;
            this.Invalidate(true);
        }
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LegendText))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.LegendText);
                }
                else
                {
                    if (IsSerializing())
                        return string.Empty;

                    if (this.isEmptyPoint)
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LegendText);

                    return series.legendText;
                }
            }
            else
            {
                return series.legendText;
            }
        }
    }

    /// <summary>
    /// Tooltip of the item in the legend
    /// </summary>
    [
    SRCategory("CategoryAttributeLegend"),
    Bindable(true),
    SRDescription("DescriptionAttributeLegendToolTip"),
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    ]
    public string LegendToolTip
    {
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LegendToolTip, value);
            else
                series.legendToolTip = value;

            if (Chart != null && Chart.selection != null)
            {
                Chart.selection.enabledChecked = false;
            }
        }
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LegendToolTip))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.LegendToolTip);
                }
                else
                {
                    if (IsSerializing())
                        return string.Empty;

                    if (this.isEmptyPoint)
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LegendToolTip);

                    return series.legendToolTip;

                }
            }
            else
            {
                return series.legendToolTip;
            }
        }
    }



    /// <summary>
    /// Background color of the data point label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeLabelBackColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    DefaultValue(typeof(Color), ""),
    ]
    public Color LabelBackColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelBackColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.LabelBackColor);
                }
                else
                {
                    if (IsSerializing())
                        return Color.Empty;

                    if (this.isEmptyPoint)
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelBackColor);

                    if (SystemInformation.HighContrast)
                        return SystemColors.Window;

                    return series.labelBackColor;
                }
            }
            else
            {
                return series.labelBackColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelBackColor, value);
            else
                series.labelBackColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Border color of the data point label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBorderColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    DefaultValue(typeof(Color), ""),
    ]
    public Color LabelBorderColor
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelBorderColor))
                {
                    return (Color)GetAttributeObject(CommonCustomProperties.LabelBorderColor);
                }
                else
                {
                    if (IsSerializing())
                        return Color.Empty;

                    if (this.isEmptyPoint)
                        return (Color)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelBorderColor);

                    if (SystemInformation.HighContrast)
                        return SystemColors.ActiveBorder;

                    return series.labelBorderColor;
                }
            }
            else
            {
                return series.labelBorderColor;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelBorderColor, value);
            else
                series.labelBorderColor = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Border style of the label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeLabelBorderDashStyle")
    ]
    public ChartDashStyle LabelBorderDashStyle
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelBorderDashStyle))
                {
                    return (ChartDashStyle)GetAttributeObject(CommonCustomProperties.LabelBorderDashStyle);
                }
                else
                {
                    if (IsSerializing())
                        return ChartDashStyle.Solid;

                    if (this.isEmptyPoint)
                        return (ChartDashStyle)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelBorderDashStyle);

                    return series.labelBorderDashStyle;

                }
            }
            else
            {
                return series.labelBorderDashStyle;
            }
        }
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelBorderDashStyle, value);
            else
                series.labelBorderDashStyle = value;
            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Border width of the label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabelAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributeBorderWidth")
    ]
    public int LabelBorderWidth
    {
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelBorderWidth))
                {
                    return (int)GetAttributeObject(CommonCustomProperties.LabelBorderWidth);
                }
                else
                {
                    if (IsSerializing())
                        return 1;

                    if (this.isEmptyPoint)
                        return (int)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelBorderWidth);

                    return series.labelBorderWidth;

                }
            }
            else
            {
                return series.labelBorderWidth;
            }
        }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), SR.ExceptionLabelBorderIsNotPositive);

            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelBorderWidth, value);
            else
                series.labelBorderWidth = value;

            this.Invalidate(true);
        }
    }

    /// <summary>
    /// Tooltip of the data point label.
    /// </summary>
    [
    SRCategory("CategoryAttributeLabel"),
    Bindable(true),
    SRDescription("DescriptionAttributeLabelToolTip"),
    Editor("KeywordsStringEditor", typeof(UITypeEditor)),
    ]
    public string LabelToolTip
    {
        set
        {
            if (this.pointCustomProperties)
                SetAttributeObject(CommonCustomProperties.LabelToolTip, value);
            else
                series.labelToolTip = value;

            if (Chart?.selection is not null)
                Chart.selection.enabledChecked = false;
        }
        get
        {
            if (this.pointCustomProperties)
            {
                if (properties.Count != 0 && IsCustomPropertySet(CommonCustomProperties.LabelToolTip))
                {
                    return (string)GetAttributeObject(CommonCustomProperties.LabelToolTip);
                }
                else
                {
                    if (IsSerializing())
                        return string.Empty;

                    if (this.isEmptyPoint)
                        return (string)series.EmptyPointStyle.GetAttributeObject(CommonCustomProperties.LabelToolTip);

                    return series.labelToolTip;

                }
            }
            else
            {
                return series.labelToolTip;
            }
        }
    }

    #endregion

    #region Serialization control



    private bool CheckIfSerializationRequired(CommonCustomProperties attribute)
    {
        if (this is DataPoint)
        {
            return IsCustomPropertySet(attribute);
        }
        else
        {
            object attr1 = this.GetAttributeObject(attribute);
            object attr2 = Series.defaultCustomProperties.GetAttributeObject(attribute);
            if (attr1 is null || attr2 is null)
                return false;

            return !attr1.Equals(attr2);
        }
    }

    private void ResetProperty(CommonCustomProperties attribute)
    {
        if (this is DataPoint)
        {
            DeleteCustomProperty(attribute);
        }
        else
        {
            this.SetAttributeObject(attribute, Series.defaultCustomProperties.GetAttributeObject(attribute));
        }
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabel()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.Label);
        else
            return !string.IsNullOrEmpty(series.label);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeAxisLabel()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.AxisLabel);
        else
            return !string.IsNullOrEmpty(series.axisLabel);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelFormat()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelFormat);
        else
            return !string.IsNullOrEmpty(series.labelFormat);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeIsValueShownAsLabel()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.IsValueShownAsLabel);
        else
            return series.showLabelAsValue != false;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.Color);
        else
            return series.color != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBorderColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BorderColor);
        else
            return series.borderColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBorderDashStyle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BorderDashStyle);
        else
            return series.borderDashStyle != ChartDashStyle.Solid;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBorderWidth()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BorderWidth);
        else
            return series.borderWidth != 1;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerBorderWidth()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerBorderWidth);
        else
            return series.markerBorderWidth != 1;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackImage()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackImage);
        else
            return !string.IsNullOrEmpty(series.backImage);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackImageWrapMode()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackImageWrapMode);
        else
            return series.backImageWrapMode != ChartImageWrapMode.Tile;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackImageTransparentColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackImageTransparentColor);
        else
            return series.backImageTransparentColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackImageAlignment()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackImageAlignment);
        else
            return series.backImageAlignment != ChartImageAlignmentStyle.TopLeft;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackGradientStyle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackGradientStyle);
        else
            return series.backGradientStyle != GradientStyle.None;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackSecondaryColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackSecondaryColor);
        else
            return series.backSecondaryColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeBackHatchStyle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.BackHatchStyle);
        else
            return series.backHatchStyle != ChartHatchStyle.None;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeFont()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.Font);
        else
            return series?.font?.Equals(FontCache.DefaultFont) == false;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>
    internal bool ShouldSerializeLabelForeColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelForeColor);
        else
            return series.fontColor != Color.Black;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelAngle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelAngle);
        else
            return series.fontAngle != 0f;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerStyle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerStyle);
        else
            return series.markerStyle != MarkerStyle.None;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerSize()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerSize);
        else
            return series.markerSize != 5;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerImage()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerImage);
        else
            return !string.IsNullOrEmpty(series.markerImage);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerImageTransparentColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerImageTransparentColor);
        else
            return series.markerImageTransparentColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerColor);
        else
            return series.markerColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeMarkerBorderColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.MarkerBorderColor);
        else
            return series.markerBorderColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeToolTip()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.ToolTip);
        else
            return !string.IsNullOrEmpty(series.toolTip);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>
    internal bool ShouldSerializeIsVisibleInLegend()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.IsVisibleInLegend);
        else
            return series.showInLegend != true;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLegendText()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LegendText);
        else
            return !string.IsNullOrEmpty(series.legendText);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLegendToolTip()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LegendToolTip);
        else
            return !string.IsNullOrEmpty(series.legendToolTip);
    }



    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelToolTip()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelToolTip);
        else
            return !string.IsNullOrEmpty(series.labelToolTip);
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelBackColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelBackColor);
        else
            return series.labelBackColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelBorderColor()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelBorderColor);
        else
            return series.labelBorderColor != Color.Empty;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelBorderDashStyle()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelBorderDashStyle);
        else
            return series.labelBorderDashStyle != ChartDashStyle.Solid;
    }

    /// <summary>
    /// Returns true if property should be serialized.
    /// </summary>

    internal bool ShouldSerializeLabelBorderWidth()
    {
        if (this.pointCustomProperties)
            return CheckIfSerializationRequired(CommonCustomProperties.LabelBorderWidth);
        else
            return series.labelBorderWidth != 1;
    }


    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabel()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.Label);
        else
            series.label = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetAxisLabel()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.AxisLabel);
        else
            series.axisLabel = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelFormat()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelFormat);
        else
            series.labelFormat = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    public void ResetIsValueShownAsLabel()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.IsValueShownAsLabel);
        else
            series.IsValueShownAsLabel = false;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.Color);
        else
            series.color = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBorderColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BorderColor);
        else
            series.borderColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBorderDashStyle()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BorderDashStyle);
        else
            series.borderDashStyle = ChartDashStyle.Solid;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBorderWidth()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BorderWidth);
        else
            series.borderWidth = 1;
    }



    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerBorderWidth()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerBorderWidth);
        else
            series.markerBorderWidth = 1;
    }



    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBackImage()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BackImage);
        else
            series.backImage = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBackImageWrapMode()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BackImageWrapMode);
        else
            series.backImageWrapMode = ChartImageWrapMode.Tile;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBackImageTransparentColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BackImageTransparentColor);
        else
            series.backImageTransparentColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBackSecondaryColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BackSecondaryColor);
        else
            series.backSecondaryColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetBackHatchStyle()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.BackHatchStyle);
        else
            series.backHatchStyle = ChartHatchStyle.None;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetFont()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.Font);
        else
            series.font = FontCache.DefaultFont;
    }


    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelAngle()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelAngle);
        else
            series.fontAngle = 0;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerStyle()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerStyle);
        else
            series.markerStyle = MarkerStyle.None;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerSize()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerSize);
        else
            series.markerSize = 5;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerImage()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerImage);
        else
            series.markerImage = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerImageTransparentColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerImageTransparentColor);
        else
            series.markerImageTransparentColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerColor);
        else
            series.markerColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetMarkerBorderColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.MarkerBorderColor);
        else
            series.markerBorderColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetToolTip()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.ToolTip);
        else
            series.toolTip = string.Empty;

        if (Chart != null && Chart.selection != null)
        {
            Chart.selection.enabledChecked = false;
        }
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>
    public void ResetIsVisibleInLegend()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.IsVisibleInLegend);
        else
            series.showInLegend = true;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLegendText()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LegendText);
        else
            series.legendText = string.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLegendToolTip()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LegendToolTip);
        else
            series.legendToolTip = string.Empty;

        if (Chart != null && Chart.selection != null)
        {
            Chart.selection.enabledChecked = false;
        }
    }



    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelBackColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelBackColor);
        else
            series.labelBackColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelBorderColor()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelBorderColor);
        else
            series.labelBorderColor = Color.Empty;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelBorderDashStyle()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelBorderDashStyle);
        else
            series.labelBorderDashStyle = ChartDashStyle.Solid;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelBorderWidth()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelBorderWidth);
        else
            series.labelBorderWidth = 1;
    }

    /// <summary>
    /// Resets property to its default value.
    /// </summary>

    internal void ResetLabelToolTip()
    {
        if (this.pointCustomProperties)
            ResetProperty(CommonCustomProperties.LabelToolTip);
        else
            series.labelToolTip = string.Empty;

        if (Chart != null && Chart.selection != null)
        {
            Chart.selection.enabledChecked = false;
        }
    }



    #endregion

    #region Invalidating method

    /// <summary>
    /// Invalidate chart area.
    /// </summary>
    /// <param name="invalidateLegend">Invalidate legend area only.</param>
    internal void Invalidate(bool invalidateLegend)
    {
        if (this.series is not null)
            series.Invalidate(true, invalidateLegend);
        else
            (this as Series)?.Invalidate(true, invalidateLegend);
    }

    #endregion
}

/// <summary>
/// Class stores additional information about the data point in 3D space.
/// </summary>
internal sealed class DataPoint3D
{
    #region Fields

    /// <summary>
    /// Reference to the 2D data point object
    /// </summary>
    internal DataPoint dataPoint;

    /// <summary>
    /// Data point index.
    /// </summary>
    internal int index;

    /// <summary>
    /// Point X position in relative coordinates.
    /// </summary>
    internal double xPosition;

    /// <summary>
    /// Point Y position in relative coordinates.
    /// </summary>
    internal double yPosition;

    /// <summary>
    /// Point X center position in relative coordinates. Used for side-by-side charts.
    /// </summary>
    internal double xCenterVal;

    /// <summary>
    /// Point Z position in relative coordinates.
    /// </summary>
    internal float zPosition;

    /// <summary>
    /// Point width.
    /// </summary>
    internal double width;

    /// <summary>
    /// Point height.
    /// </summary>
    internal double height;

    /// <summary>
    /// Point depth.
    /// </summary>
    internal float depth;

    /// <summary>
    /// Indicates that point belongs to indexed series.
    /// </summary>
    internal bool indexedSeries;

    #endregion
}

/// <summary>
/// Design-time representation of the CustomProperties.
/// This class is used instead of the string "CustomProperties"
/// property at design time and supports expandable list
/// of custom properties.
/// </summary>
[TypeConverter(typeof(CustomPropertiesTypeConverter))]
[EditorBrowsable(EditorBrowsableState.Never)]
public class CustomProperties
{
    #region Fields

    // Reference to the properties class
    internal DataPointCustomProperties m_DataPointCustomProperties;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="properties">Attributes object.</param>
    internal CustomProperties(DataPointCustomProperties properties)
    {
        this.m_DataPointCustomProperties = properties;
    }

    #endregion // Constructor

    #region Properties

    internal virtual DataPointCustomProperties DataPointCustomProperties
    {
        get => this.m_DataPointCustomProperties;
        set => this.m_DataPointCustomProperties = value;

    }

    #endregion //Properties

    #region Methods

    /// <summary>
    /// Gets a comma separated string of user defined custom properties.
    /// </summary>
    /// <returns>Comma separated string of user defined custom properties.</returns>
    internal virtual string GetUserDefinedCustomProperties()
    {
        return GetUserDefinedCustomProperties(true);
    }

    /// <summary>
    /// Gets a comma separated string of user defined or non-user defined custom properties.
    /// </summary>
    /// <param name="userDefined">True if user defined properties must be returned.</param>
    /// <returns>Comma separated string of user defined custom properties.</returns>
    internal virtual string GetUserDefinedCustomProperties(bool userDefined)
    {
        // Get comma separated string of custom properties
        string customAttribute = this.DataPointCustomProperties.CustomProperties;
        string userDefinedCustomAttribute = string.Empty;

        // Get custom attribute registry
        CustomPropertyRegistry registry = (CustomPropertyRegistry)this.DataPointCustomProperties.Common.container.GetService(typeof(CustomPropertyRegistry));

        // Replace commas in value string
        customAttribute = customAttribute.Replace("\\,", "\\x45");
        customAttribute = customAttribute.Replace("\\=", "\\x46");

        // Split custom properties by commas into individual properties
        if (customAttribute.Length > 0)
        {
            string[] nameValueStrings = customAttribute.Split(',');
            foreach (string nameValue in nameValueStrings)
            {
                string[] values = nameValue.Split('=');

                // Check format
                if (values.Length != 2)
                {
                    throw new FormatException(SR.ExceptionAttributeInvalidFormat);
                }

                // Check for empty name or value
                values[0] = values[0].Trim();
                values[1] = values[1].Trim();
                if (values[0].Length == 0)
                {
                    throw new FormatException(SR.ExceptionAttributeInvalidFormat);
                }

                // Check if attribute is registered or user defined
                bool userDefinedAttribute = true;
                foreach (CustomPropertyInfo info in registry.registeredCustomProperties)
                {
                    if (string.Equals(info.Name, values[0], StringComparison.OrdinalIgnoreCase))
                    {
                        userDefinedAttribute = false;
                    }
                }

                // Copy attribute into the output string
                if (userDefinedAttribute == userDefined)
                {
                    if (userDefinedCustomAttribute.Length > 0)
                    {
                        userDefinedCustomAttribute += ", ";
                    }

                    string val = values[1].Replace("\\x45", ",");
                    val = val.Replace("\\x46", "=");
                    userDefinedCustomAttribute += values[0] + "=" + val;
                }
            }
        }

        return userDefinedCustomAttribute;
    }

    /// <summary>
    /// Sets user defined custom properties without cleaning registered properties.
    /// </summary>
    /// <param name="val">New user defined properties.</param>
    internal virtual void SetUserDefinedAttributes(string val)
    {
        // Get non-user defined custom properties
        string properties = GetUserDefinedCustomProperties(false);

        // Check if new string is empty
        if (val.Length > 0)
        {
            // Add comma at the end
            if (properties.Length > 0)
            {
                properties += ", ";
            }

            // Add new user defined properties
            properties += val;
        }

        // Set new custom attribute string
        this.DataPointCustomProperties.CustomProperties = properties;
    }


    #endregion // Methods
}