// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Annotation Converters.
//


using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Forms.DataVisualization.Charting
{
    /// <summary>
	/// Converts anchor data point to string name.
	/// </summary>
	internal sealed class AnchorPointValueConverter : TypeConverter
    {
        #region Converter methods

        /// <summary>
        /// Converts anchor data point to string name.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertation destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is null)
                    return Constants.NotSetValue;

                if (value is DataPoint dataPoint && dataPoint.series is not null)
                {
                    int pointIndex = dataPoint.series.Points.IndexOf(dataPoint) + 1;
                    return dataPoint.series.Name + " - " + SR.DescriptionTypePoint + pointIndex.ToString(CultureInfo.InvariantCulture);
                }
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion
    }

    /// <summary>
    /// Converts anchor data point to string name.
    /// </summary>
    internal sealed class AnnotationAxisValueConverter : TypeConverter
    {
        #region Converter methods

        /// <summary>
        /// Converts axis associated with annotation to string.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertation destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is null)
                    return Constants.NotSetValue;

                if (value is Axis axis && axis.ChartArea is not null)
                        return axis.ChartArea.Name + " - " + axis.Name;
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }
        #endregion
    }
}

