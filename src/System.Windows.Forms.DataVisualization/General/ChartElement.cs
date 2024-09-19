﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	The chart element is base class for the big number
//				of classes. It stores common methods and data.
//

using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms.DataVisualization.Charting;

#region Enumerations

/// <summary>
/// Axis Arrow orientation
/// </summary>
internal enum ArrowOrientation
{
    /// <summary>
    /// Arrow direction is Right - Left
    /// </summary>
    Left,

    /// <summary>
    /// Arrow direction is Left - Right
    /// </summary>
    Right,

    /// <summary>
    /// Arrow direction is Bottom - Top
    /// </summary>
    Top,

    /// <summary>
    /// Arrow direction is Top - Bottom
    /// </summary>
    Bottom
}

/// <summary>
/// An enumeration of image alignment.
/// </summary>
public enum ChartImageAlignmentStyle
{
    /// <summary>
    /// The image is aligned to the top left corner of the chart element.
    /// </summary>
    TopLeft,

    /// <summary>
    /// The image is aligned to the top boundary of the chart element.
    /// </summary>
    Top,

    /// <summary>
    /// The image is aligned to the top right corner of the chart element.
    /// </summary>
    TopRight,

    /// <summary>
    /// The image is aligned to the right boundary of the chart element.
    /// </summary>
    Right,

    /// <summary>
    /// The image is aligned to the bottom right corner of the chart element.
    /// </summary>
    BottomRight,

    /// <summary>
    /// The image is aligned to the bottom boundary of the chart element.
    /// </summary>
    Bottom,

    /// <summary>
    /// The image is aligned to the bottom left corner of the chart element.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// The image is aligned to the left boundary of the chart element.
    /// </summary>
    Left,

    /// <summary>
    /// The image is aligned in the center of the chart element.
    /// </summary>
    Center
};

/// <summary>
/// An enumeration that specifies a background image drawing mode.
/// </summary>
public enum ChartImageWrapMode
{
    /// <summary>
    /// Background image is scaled to fit the entire chart element.
    /// </summary>
    Scaled = WrapMode.Clamp,

    /// <summary>
    /// Background image is tiled to fit the entire chart element.
    /// </summary>
    Tile = WrapMode.Tile,

    /// <summary>
    /// Every other tiled image is reversed around the X-axis.
    /// </summary>
    TileFlipX = WrapMode.TileFlipX,

    /// <summary>
    /// Every other tiled image is reversed around the X-axis and Y-axis.
    /// </summary>
    TileFlipXY = WrapMode.TileFlipXY,

    /// <summary>
    /// Every other tiled image is reversed around the Y-axis.
    /// </summary>
    TileFlipY = WrapMode.TileFlipY,

    /// <summary>
    /// Background image is not scaled.
    /// </summary>
    Unscaled = 100
};

/// <summary>
/// An enumeration that specifies the state of an axis.
/// </summary>
public enum AxisEnabled
{
    /// <summary>
    /// The axis is only enabled if it used to plot a Series.
    /// </summary>
    Auto,

    /// <summary>
    /// The axis is always enabled.
    /// </summary>
    True,

    /// <summary>
    /// The axis is never enabled.
    /// </summary>
    False
};

/// <summary>
/// An enumeration of units of measurement of an interval.
/// </summary>
public enum DateTimeIntervalType
{
    /// <summary>
    /// Automatically determined by the Chart control.
    /// </summary>
    Auto,

    /// <summary>
    /// The interval is numerical.
    /// </summary>
    Number,

    /// <summary>
    /// The interval is years.
    /// </summary>
    Years,

    /// <summary>
    /// The interval is months.
    /// </summary>
    Months,

    /// <summary>
    /// The interval is weeks.
    /// </summary>
    Weeks,

    /// <summary>
    /// The interval is days.
    /// </summary>
    Days,

    /// <summary>
    /// The interval is hours.
    /// </summary>
    Hours,

    /// <summary>
    /// The interval is minutes.
    /// </summary>
    Minutes,

    /// <summary>
    /// The interval is seconds.
    /// </summary>
    Seconds,

    /// <summary>
    /// The interval is milliseconds.
    /// </summary>
    Milliseconds,

    /// <summary>
    /// The interval type is not defined.
    /// </summary>
    NotSet,
}

/// <summary>
/// An enumeration that specifies value types for various chart properties
/// </summary>
public enum ChartValueType
{
    /// <summary>
    /// Property type is set automatically by the Chart control.
    /// </summary>
    Auto,

    /// <summary>
    /// Double value.
    /// </summary>
    Double,

    /// <summary>
    /// Single value.
    /// </summary>
    Single,

    /// <summary>
    /// Int32 value.
    /// </summary>
    Int32,

    /// <summary>
    /// Int64 value.
    /// </summary>
    Int64,

    /// <summary>
    /// UInt32 value.
    /// </summary>
    UInt32,

    /// <summary>
    /// UInt64 value.
    /// </summary>
    UInt64,

    /// <summary>
    /// String value.
    /// </summary>
    String,

    /// <summary>
    /// DateTime value.
    /// </summary>
    DateTime,

    /// <summary>
    /// Date portion of the DateTime value.
    /// </summary>
    Date,

    /// <summary>
    /// Time portion of the DateTime value.
    /// </summary>
    Time,

    /// <summary>
    /// DateTime with offset
    /// </summary>
    DateTimeOffset
};


/// <summary>
/// An enumeration that specifies the level of anti-aliasing quality.
/// </summary>
public enum TextAntiAliasingQuality
{
    /// <summary>
    /// Normal anti-aliasing quality.
    /// </summary>
    Normal,

    /// <summary>
    /// High anti-aliasing quality.
    /// </summary>
    High,

    /// <summary>
    /// System default anti-aliasing quality.
    /// </summary>
    SystemDefault
}

#endregion Enumerations

#region ChartElement

/// <summary>
/// Common chart helper methods used across different chart elements.
/// </summary>
internal sealed class ChartHelper
{
    #region Fields

    /// <summary>
    /// Maximum number of grid lines per Axis
    /// </summary>
    internal const int MaxNumOfGridlines = 10000;

    #endregion Fields

    #region Constructor

    /// <summary>
    /// Private constructor to avoid instantiating the class
    /// </summary>
    private ChartHelper() { }

    #endregion Constructor

    #region Methods

    /// <summary>
    /// Adjust the beginning of the first interval depending on the type and size.
    /// </summary>
    /// <param name="start">Original start point.</param>
    /// <param name="intervalSize">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <returns>Adjusted interval start position as double.</returns>
    internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type)
    {
        return AlignIntervalStart(start, intervalSize, type, null);
    }

    /// <summary>
    /// Adjust the beginning of the first interval depending on the type and size.
    /// </summary>
    /// <param name="start">Original start point.</param>
    /// <param name="intervalSize">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <param name="series">First series connected to the axis.</param>
    /// <returns>Adjusted interval start position as double.</returns>
    internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series)
    {
        return AlignIntervalStart(start, intervalSize, type, series, true);
    }

    /// <summary>
    /// Adjust the beginning of the first interval depending on the type and size.
    /// </summary>
    /// <param name="start">Original start point.</param>
    /// <param name="intervalSize">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <param name="series">First series connected to the axis.</param>
    /// <param name="majorInterval">Interval is used for major gridlines or tickmarks.</param>
    /// <returns>Adjusted interval start position as double.</returns>
    internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series, bool majorInterval)
    {
        // Special case for indexed series
        if (series != null && series.IsXValueIndexed)
        {
            if (type == DateTimeIntervalType.Auto ||
                type == DateTimeIntervalType.Number)
            {
                if (majorInterval)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            return -(series.Points.Count + 1);
        }

        // Non indexed series
        else
        {
            // Do not adjust start position for these interval type
            if (type == DateTimeIntervalType.Auto ||
                type == DateTimeIntervalType.Number)
            {
                return start;
            }

            // Get the beginning of the interval depending on type
            DateTime newStartDate = DateTime.FromOADate(start);

            // Adjust the months interval depending on size
            if (intervalSize > 0.0 && intervalSize != 1.0)
            {
                if (type == DateTimeIntervalType.Months && intervalSize <= 12.0 && intervalSize > 1)
                {
                    // Make sure that the beginning is aligned correctly for cases
                    // like quarters and half years
                    DateTime resultDate = newStartDate;
                    DateTime sizeAdjustedDate = new DateTime(newStartDate.Year, 1, 1, 0, 0, 0);
                    while (sizeAdjustedDate < newStartDate)
                    {
                        resultDate = sizeAdjustedDate;
                        sizeAdjustedDate = sizeAdjustedDate.AddMonths((int)intervalSize);
                    }

                    newStartDate = resultDate;
                    return newStartDate.ToOADate();
                }
            }

            // Check interval type
            switch (type)
            {
                case DateTimeIntervalType.Years:
                    int year = (int)((int)(newStartDate.Year / intervalSize) * intervalSize);
                    if (year <= 0)
                    {
                        year = 1;
                    }

                    newStartDate = new DateTime(year,
                        1, 1, 0, 0, 0);
                    break;

                case DateTimeIntervalType.Months:
                    int month = (int)((int)(newStartDate.Month / intervalSize) * intervalSize);
                    if (month <= 0)
                    {
                        month = 1;
                    }

                    newStartDate = new DateTime(newStartDate.Year,
                        month, 1, 0, 0, 0);
                    break;

                case DateTimeIntervalType.Days:
                    int day = (int)((int)(newStartDate.Day / intervalSize) * intervalSize);
                    if (day <= 0)
                    {
                        day = 1;
                    }

                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month, day, 0, 0, 0);
                    break;

                case DateTimeIntervalType.Hours:
                    int hour = (int)((int)(newStartDate.Hour / intervalSize) * intervalSize);
                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month, newStartDate.Day, hour, 0, 0);
                    break;

                case DateTimeIntervalType.Minutes:
                    int minute = (int)((int)(newStartDate.Minute / intervalSize) * intervalSize);
                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month,
                        newStartDate.Day,
                        newStartDate.Hour,
                        minute,
                        0);
                    break;

                case DateTimeIntervalType.Seconds:
                    int second = (int)((int)(newStartDate.Second / intervalSize) * intervalSize);
                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month,
                        newStartDate.Day,
                        newStartDate.Hour,
                        newStartDate.Minute,
                        second,
                        0);
                    break;

                case DateTimeIntervalType.Milliseconds:
                    int milliseconds = (int)((int)(newStartDate.Millisecond / intervalSize) * intervalSize);
                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month,
                        newStartDate.Day,
                        newStartDate.Hour,
                        newStartDate.Minute,
                        newStartDate.Second,
                        milliseconds);
                    break;

                case DateTimeIntervalType.Weeks:

                    // NOTE: Code below was changed to fix issue #5962
                    // Elements that have interval set to weeks should be aligned to the
                    // nearest Monday no matter how many weeks is the interval.
                    //newStartDate = newStartDate.AddDays(-((int)newStartDate.DayOfWeek * intervalSize));
                    newStartDate = newStartDate.AddDays(-(int)newStartDate.DayOfWeek);
                    newStartDate = new DateTime(newStartDate.Year,
                        newStartDate.Month, newStartDate.Day, 0, 0, 0);
                    break;
            }

            return newStartDate.ToOADate();
        }
    }

    /// <summary>
    /// Gets interval size as double number.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="interval">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <returns>Interval size as double.</returns>
    internal static double GetIntervalSize(double current, double interval, DateTimeIntervalType type)
    {
        return GetIntervalSize(
            current,
            interval,
            type,
            null,
            0,
            DateTimeIntervalType.Number,
            true,
            true);
    }

    /// <summary>
    /// Gets interval size as double number.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="interval">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <param name="series">First series connected to the axis.</param>
    /// <param name="intervalOffset">Offset size.</param>
    /// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
    /// <param name="forceIntIndex">Force Integer indexed</param>
    /// <returns>Interval size as double.</returns>
    internal static double GetIntervalSize(
        double current,
        double interval,
        DateTimeIntervalType type,
        Series series,
        double intervalOffset,
        DateTimeIntervalType intervalOffsetType,
        bool forceIntIndex)
    {
        return GetIntervalSize(
            current,
            interval,
            type,
            series,
            intervalOffset,
            intervalOffsetType,
            forceIntIndex,
            true);
    }

    /// <summary>
    /// Gets interval size as double number.
    /// </summary>
    /// <param name="current">Current value.</param>
    /// <param name="interval">Interval size.</param>
    /// <param name="type">AxisName of the interval (Month, Year, ...).</param>
    /// <param name="series">First series connected to the axis.</param>
    /// <param name="intervalOffset">Offset size.</param>
    /// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
    /// <param name="forceIntIndex">Force Integer indexed</param>
    /// <param name="forceAbsInterval">Force Integer indexed</param>
    /// <returns>Interval size as double.</returns>
    internal static double GetIntervalSize(
        double current,
        double interval,
        DateTimeIntervalType type,
        Series series,
        double intervalOffset,
        DateTimeIntervalType intervalOffsetType,
        bool forceIntIndex,
        bool forceAbsInterval)
    {
        // AxisName is not date.
        if (type == DateTimeIntervalType.Number || type == DateTimeIntervalType.Auto)
        {
            return interval;
        }

        // Special case for indexed series
        if (series != null && series.IsXValueIndexed)
        {
            // Check point index
            int pointIndex = (int)Math.Ceiling(current - 1);
            if (pointIndex < 0)
            {
                pointIndex = 0;
            }

            if (pointIndex >= series.Points.Count || series.Points.Count <= 1)
            {
                return interval;
            }

            // Get starting and ending values of the closest interval
            double adjuster = 0;
            double xValue = series.Points[pointIndex].XValue;
            xValue = AlignIntervalStart(xValue, 1, type, null);
            double xEndValue = xValue + GetIntervalSize(xValue, interval, type);
            xEndValue += GetIntervalSize(xEndValue, intervalOffset, intervalOffsetType);
            xValue += GetIntervalSize(xValue, intervalOffset, intervalOffsetType);
            if (intervalOffset < 0)
            {
                xValue += GetIntervalSize(xValue, interval, type);
                xEndValue += GetIntervalSize(xEndValue, interval, type);
            }

            // The first point in the series
            if (pointIndex == 0 && current < 0)
            {
                // Round the first point value depending on the interval type
                DateTime dateValue = DateTime.FromOADate(series.Points[pointIndex].XValue);
                DateTime roundedDateValue = dateValue;
                switch (type)
                {
                    case DateTimeIntervalType.Years: // Ignore hours,...
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month, dateValue.Day, 0, 0, 0);
                        break;

                    case DateTimeIntervalType.Months: // Ignore hours,...
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month, dateValue.Day, 0, 0, 0);
                        break;

                    case DateTimeIntervalType.Days: // Ignore hours,...
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month, dateValue.Day, 0, 0, 0);
                        break;

                    case DateTimeIntervalType.Hours: //
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month, dateValue.Day, dateValue.Hour,
                            dateValue.Minute, 0);
                        break;

                    case DateTimeIntervalType.Minutes:
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month,
                            dateValue.Day,
                            dateValue.Hour,
                            dateValue.Minute,
                            dateValue.Second);
                        break;

                    case DateTimeIntervalType.Seconds:
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month,
                            dateValue.Day,
                            dateValue.Hour,
                            dateValue.Minute,
                            dateValue.Second,
                            0);
                        break;

                    case DateTimeIntervalType.Weeks:
                        roundedDateValue = new DateTime(dateValue.Year,
                            dateValue.Month, dateValue.Day, 0, 0, 0);
                        break;
                }

                // The first point value is exactly on the interval boundaries
                if (roundedDateValue.ToOADate() == xValue || roundedDateValue.ToOADate() == xEndValue)
                {
                    return -current + 1;
                }
            }

            // Adjuster of 0.5 means that position should be between points
            ++pointIndex;
            while (pointIndex < series.Points.Count)
            {
                if (series.Points[pointIndex].XValue >= xEndValue)
                {
                    if (series.Points[pointIndex].XValue > xEndValue && !forceIntIndex)
                    {
                        adjuster = -0.5;
                    }

                    break;
                }

                ++pointIndex;
            }

            // If last point outside of the max series index
            if (pointIndex == series.Points.Count)
            {
                pointIndex += series.Points.Count / 5 + 1;
            }

            double size = pointIndex + 1 - current + adjuster;

            return (size != 0) ? size : interval;
        }

        // Non indexed series
        else
        {
            DateTime date = DateTime.FromOADate(current);
            TimeSpan span = new TimeSpan(0);

            if (type == DateTimeIntervalType.Days)
            {
                span = TimeSpan.FromDays(interval);
            }
            else if (type == DateTimeIntervalType.Hours)
            {
                span = TimeSpan.FromHours(interval);
            }
            else if (type == DateTimeIntervalType.Milliseconds)
            {
                span = TimeSpan.FromMilliseconds(interval);
            }
            else if (type == DateTimeIntervalType.Seconds)
            {
                span = TimeSpan.FromSeconds(interval);
            }
            else if (type == DateTimeIntervalType.Minutes)
            {
                span = TimeSpan.FromMinutes(interval);
            }
            else if (type == DateTimeIntervalType.Weeks)
            {
                span = TimeSpan.FromDays(7.0 * interval);
            }
            else if (type == DateTimeIntervalType.Months)
            {
                // Special case handling when current date points
                // to the last day of the month
                bool lastMonthDay = false;
                if (date.Day == DateTime.DaysInMonth(date.Year, date.Month))
                {
                    lastMonthDay = true;
                }

                // Add specified amount of months
                date = date.AddMonths((int)Math.Floor(interval));
                span = TimeSpan.FromDays(30.0 * (interval - Math.Floor(interval)));

                // Check if last month of the day was used
                if (lastMonthDay && span.Ticks == 0)
                {
                    // Make sure the last day of the month is selected
                    int daysInMobth = DateTime.DaysInMonth(date.Year, date.Month);
                    date = date.AddDays(daysInMobth - date.Day);
                }
            }
            else if (type == DateTimeIntervalType.Years)
            {
                date = date.AddYears((int)Math.Floor(interval));
                span = TimeSpan.FromDays(365.0 * (interval - Math.Floor(interval)));
            }

            // Check if an absolute interval size must be returned
            double result = date.Add(span).ToOADate() - current;
            if (forceAbsInterval)
            {
                result = Math.Abs(result);
            }

            return result;
        }
    }

    /// <summary>
    /// Check if any series is indexed.
    /// </summary>
    /// <param name="common">Reference to common chart classes.</param>
    /// <param name="series">Data series names.</param>
    /// <returns>True if any series is indexed.</returns>
    internal static bool IndexedSeries(CommonElements common, List<string> series)
    {
        // Data series loop
        foreach (string ser in series)
        {
            Series localSeries = common.DataManager.Series[ser];
            // Check series indexed flag
            if (localSeries.IsXValueIndexed)
                return true; // If flag set in at least one series - all series are indexed
        }

        return false;
    }

    /// <summary>
    /// Check if all data points in the series have X value set to 0.
    /// </summary>
    /// <param name="series">Data series to check.</param>
    private static bool SeriesXValuesZeros(Series series)
    {
        // Check if X value zeros check was already done
        if (series.xValuesZeros is not null)
            return series.xValuesZeros.Value;

        // Data point loop
        foreach (DataPoint point in series.Points)
        {
            if (point.XValue != 0.0)
            {
                // If any data point has value different than 0 return false
                series.xValuesZeros = false;
                return false;
            }
        }

        series.xValuesZeros = true;
        return true;
    }

    /// <summary>
    /// Check if all data points in many series have X value set to 0.
    /// </summary>
    /// <param name="common">Reference to common chart classes.</param>
    /// <param name="series">Data series.</param>
    /// <returns>True if all data points have value 0.</returns>
    internal static bool SeriesXValuesZeros(CommonElements common, List<string> series)
    {
        // Data series loop
        foreach (string ser in series)
        {
            // Check one series X values
            if (!SeriesXValuesZeros(common.DataManager.Series[ser]))
                return false;
        }

        return true;
    }

    #endregion Methods
}

#endregion ChartElement
