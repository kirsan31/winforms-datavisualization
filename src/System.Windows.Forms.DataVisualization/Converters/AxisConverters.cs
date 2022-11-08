﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Converters for the Axis object properties.
//

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Forms.DataVisualization.Charting
{
    /// <summary>
    /// Converts labels, grid and ticks start position to support dates format
    /// </summary>
    internal class AxisLabelDateValueConverter : DoubleConverter
    {
        #region Converter methods

        /// <summary>
        /// Convert Min and Max values to string if step type is set to one of the DateTime type
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context != null && context.Instance != null)
            {
                // Convert to string
                if (destinationType == typeof(string))
                {
                    DateTimeIntervalType intervalType = DateTimeIntervalType.Auto;
                    double interval = 0;

                    // Get IntervalType property using reflection
                    PropertyInfo propertyInfo = context.Instance.GetType().GetProperty("IntervalType");
                    if (propertyInfo != null)
                    {
                        intervalType = (DateTimeIntervalType)propertyInfo.GetValue(context.Instance, null);
                    }

                    // Get Interval property using reflection
                    propertyInfo = context.Instance.GetType().GetProperty("Interval");
                    if (propertyInfo != null)
                    {
                        interval = (double)propertyInfo.GetValue(context.Instance, null);
                    }

                    // Try to get interval information from the axis
                    if (intervalType == DateTimeIntervalType.Auto)
                    {
                        // Get object's axis
                        Axis axis = null;
                        if (context.Instance is Axis)
                        {
                            axis = (Axis)context.Instance;
                        }
                        else
                        {
                            MethodInfo methodInfo = context.Instance.GetType().GetMethod("GetAxis");
                            if (methodInfo != null)
                            {
                                // Get axis object
                                axis = (Axis)methodInfo.Invoke(context.Instance, null);
                            }
                        }

                        // Get axis value type
                        if (axis != null)
                        {
                            intervalType = axis.GetAxisIntervalType();
                        }
                    }

                    // Convert value to date/time string
                    if (context.Instance.GetType() != typeof(StripLine) || interval == 0)
                    {
                        if (intervalType != DateTimeIntervalType.Number && intervalType != DateTimeIntervalType.Auto)
                        {
                            // Covert value to date/time
                            if (intervalType < DateTimeIntervalType.Hours)
                            {
                                return DateTime.FromOADate((double)value).ToShortDateString();
                            }
                            return DateTime.FromOADate((double)value).ToString("g", System.Globalization.CultureInfo.CurrentCulture);
                        }
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Convert Min and Max values from string if step type is set to one of the DateTime type
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert from.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            bool convertFromDate = false;
            string stringValue = value as string;

            // If context interface provided check if we are dealing with DateTime values
            if (context != null && context.Instance != null)
            {
                DateTimeIntervalType intervalType = DateTimeIntervalType.Auto;

                // Get intervalType property using reflection
                PropertyInfo propertyInfo = context.Instance.GetType().GetProperty("intervalType");
                if (propertyInfo != null)
                {
                    intervalType = (DateTimeIntervalType)propertyInfo.GetValue(context.Instance, null);
                }

                // Try to get interval information from the axis
                if (intervalType == DateTimeIntervalType.Auto)
                {
                    // Get object's axis
                    Axis axis = null;
                    if (context.Instance is Axis)
                    {
                        axis = (Axis)context.Instance;
                    }
                    else
                    {
                        MethodInfo methodInfo = context.Instance.GetType().GetMethod("GetAxis");
                        if (methodInfo != null)
                        {
                            // Get axis object
                            axis = (Axis)methodInfo.Invoke(context.Instance, null);
                        }
                    }

                    // Get axis value type
                    if (axis != null)
                    {
                        intervalType = axis.GetAxisIntervalType();
                    }
                }

                if (stringValue != null && intervalType != DateTimeIntervalType.Number && intervalType != DateTimeIntervalType.Auto)
                {
                    convertFromDate = true;
                }
            }

            object result;
            // Try to convert from double string
            try
            {
                result = base.ConvertFrom(context, culture, value);
            }
            catch (ArgumentException)
            {
                result = null;
            }
            catch (NotSupportedException)
            {
                result = null;
            }

            // Try to convert from date/time string
            if (stringValue != null && (convertFromDate || result == null))
            {
                bool parseSucceed = DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime valueAsDate);

                if (parseSucceed)
                {
                    // Succeded converting from date format
                    return valueAsDate.ToOADate();
                }
            }

            // Call base converter
            return base.ConvertFrom(context, culture, value);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts crossing property of the axis.
   /// Possible values: double, date, "Auto", "Min", "Max"
    /// </summary>
    internal class AxisCrossingValueConverter : AxisMinMaxValueConverter
    {
        #region Converter methods

        /// <summary>
        /// Standart values supported - return true.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standard values supported.</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Standart values are not exclusive - return false
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Non exclusive standard values.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        /// Fill in the list of standart values.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standart values collection.</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            values.Add(Double.NaN);
            values.Add(Double.MinValue);
            values.Add(Double.MaxValue);

            return new StandardValuesCollection(values);
        }

        /// <summary>
        /// Convert crossing value to string.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            double doubleValue = (double)value;
            if (destinationType == typeof(string))
            {
                if (Double.IsNaN(doubleValue))
                {
                    return Constants.AutoValue;
                }
                else if (doubleValue == Double.MinValue)
                {
                    return Constants.MinValue;
                }
                else if (doubleValue == Double.MaxValue)
                {
                    return Constants.MaxValue;
                }
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Convert crossing values from string
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert from.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // If converting from string value
            string crossingValue = value as string;
            if (crossingValue != null)
            {
                if (string.Equals(crossingValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase))
                {
                    return Double.NaN;
                }
                else if (string.Equals(crossingValue, Constants.MinValue, StringComparison.OrdinalIgnoreCase))
                {
                    return Double.MinValue;
                }
                else if (string.Equals(crossingValue, Constants.MaxValue, StringComparison.OrdinalIgnoreCase))
                {
                    return Double.MaxValue;
                }
            }

            // Call base converter
            return base.ConvertFrom(context, culture, value);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts min and max properties of the axis depending on the values type
    /// </summary>
    internal class AxisMinMaxValueConverter : DoubleConverter
    {
        #region Converter methods

        /// <summary>
        /// Convert Min and Max values to string if step type is set to one of the DateTime type
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (context != null && context.Instance != null && context.Instance is Axis)
            {
                Axis axis = (Axis)context.Instance;
                if (destinationType == typeof(string))
                {
                    string strValue = DoubleDateNanValueConverter.ConvertDateTimeToString(
                        (double)value,
                        axis.GetAxisValuesType(),
                        axis.InternalIntervalType);

                    if (strValue != null)
                        return strValue;
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Convert Min and Max values from string if step type is set to one of the DateTime type
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert from.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            bool convertFromDate = false;
            string stringValue = value as string;

            // If context interface provided check if we are dealing with DateTime values
            if (context != null && context.Instance != null && context.Instance is Axis)
            {
                Axis axis = (Axis)context.Instance;

                if (stringValue != null)
                {
                    if (axis.InternalIntervalType == DateTimeIntervalType.Auto)
                    {
                        if (axis.GetAxisValuesType() == ChartValueType.DateTime ||
                            axis.GetAxisValuesType() == ChartValueType.Date ||
                            axis.GetAxisValuesType() == ChartValueType.Time ||
                            axis.GetAxisValuesType() == ChartValueType.DateTimeOffset)
                        {
                            convertFromDate = true;
                        }
                    }
                    else
                    {
                        if (axis.InternalIntervalType != DateTimeIntervalType.Number)
                        {
                            convertFromDate = true;
                        }
                    }
                }
            }

            object result;
            // Try to convert from double string
            try
            {
                result = base.ConvertFrom(context, culture, value);
            }
            catch (ArgumentException)
            {
                result = null;
            }
            catch (NotSupportedException)
            {
                result = null;
            }

            // Try to convert from date/time string
            if (stringValue != null && (convertFromDate || result == null))
            {
                bool parseSucceed = DateTime.TryParse(stringValue, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime valueAsDate);

                if (parseSucceed)
                {
                    return valueAsDate.ToOADate();
                }
            }

            // Call base converter
            return base.ConvertFrom(context, culture, value);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts maximum and minimum property of the axis.
   /// Possible values: double, date, "Auto",
    /// </summary>
    internal class AxisMinMaxAutoValueConverter : AxisMinMaxValueConverter
    {
        #region Converter methods

        /// <summary>
        /// Standart values supported - return true
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standard values supported.</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Standart values are not exclusive - return false
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Non exclusive standard values.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        /// Fill in the list of data series names.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            values.Add(Double.NaN);

            return new StandardValuesCollection(values);
        }

        /// <summary>
        /// Convert minimum or maximum value to string
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            double doubleValue = (double)value;
            if (destinationType == typeof(string))
            {
                if (Double.IsNaN(doubleValue))
                {
                    return Constants.AutoValue;
                }
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Convert minimum or maximum values from string
        /// </summary>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // If converting from string value
            string crossingValue = value as string;
            if (crossingValue != null)
            {
                if (string.Equals(crossingValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase))
                {
                    return Double.NaN;
                }
            }

            // Call base converter
            return base.ConvertFrom(context, culture, value);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts title angle property of the strip line
    /// Possible values: 0, 90, 180, 270
    /// </summary>
    internal class StripLineTitleAngleConverter : Int32Converter
    {
        #region Converter methods

        /// <summary>
        /// Standart values supported - return true
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standard values supported.</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Standart values are not exclusive - return false
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Non exclusive standard values.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Fill in the list of data series names.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            values.Add(0);
            values.Add(90);
            values.Add(180);
            values.Add(270);

            return new StandardValuesCollection(values);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts Interval and IntervalOffset properties of the axis
    /// </summary>
    internal class AxisIntervalValueConverter : DoubleConverter
    {
        #region Converter methods

        /// <summary>
        /// Inicates that "NotSet" option is available
        /// </summary>
        internal bool hideNotSet = true;

        /// <summary>
        /// Standart values supported - return true.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standard values supported.</returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Standart values are not exclusive - return false
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Non exclusive standard values.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        /// <summary>
        /// Fill in the list of standart values.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Standart values collection.</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList values = new ArrayList();
            if (!hideNotSet)
            {
                values.Add(Double.NaN);
            }
            values.Add(0.0);

            return new StandardValuesCollection(values);
        }

        /// <summary>
        /// Convert crossing value to string.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            double doubleValue = (double)value;
            if (destinationType == typeof(string))
            {
                if (Double.IsNaN(doubleValue))
                {
                    return Constants.NotSetValue;
                }
                else if (doubleValue == 0.0)
                {
                    return Constants.AutoValue;
                }
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Convert crossing values from string
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert from.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // If converting from string value
            string crossingValue = value as string;
            if (crossingValue != null)
            {
                if (string.Equals(crossingValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase))
                {
                    return 0.0;
                }
                else if (string.Equals(crossingValue, Constants.NotSetValue, StringComparison.OrdinalIgnoreCase))
                {
                    return Double.NaN;
                }
            }

            // Call base converter
            return base.ConvertFrom(context, culture, value);
        }

        #endregion Converter methods
    }

    /// <summary>
    /// Converts Interval and IntervalOffset properties of the label style, tick marks and grids
    /// </summary>
    internal class AxisElementIntervalValueConverter : AxisIntervalValueConverter
    {
        /// <summary>
        /// Show the NotSet option for interval
        /// </summary>
        public AxisElementIntervalValueConverter()
        {
            base.hideNotSet = false;
        }
    }
}
