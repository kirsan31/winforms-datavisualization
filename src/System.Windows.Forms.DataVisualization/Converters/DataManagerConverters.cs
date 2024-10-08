// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Converter classes for the Series and DataPoint properties.
//


using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Data;

namespace System.Windows.Forms.DataVisualization.Charting;

/// <summary>
/// Chart area name converter. Displays list of available areas names
/// </summary>
internal sealed class SeriesAreaNameConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Non exclusive standard values.</returns>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return false;
    }

    /// <summary>
    /// Fill in the list of the chart areas for the series.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList values = [];

        Chart chart = ConverterHelper.GetChartFromContext(context);

        if (chart != null)
        {
            foreach (ChartArea area in chart.ChartAreas)
            {
                values.Add(area.Name);
            }
        }

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Chart data source design-time converter. Displays list of available data sources.
/// </summary>
internal sealed class ChartDataSourceConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Non exclusive standard values.</returns>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Fill in the list of chart type names.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList values = [];

        if (context != null && context.Container != null)
        {
            // Loop through all components in the container
            foreach (IComponent comonent in context.Container.Components)
            {
                // Check if component can be a data source
                if (ChartImage.IsValidDataSource(comonent))
                {
                    // Add component name
                    values.Add(comonent.Site.Name);
                }
            }
        }

        // Add "None" data source
        values.Add("(none)");

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Series data source members converter.
/// </summary>
internal sealed class SeriesDataSourceMemberConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Non exclusive standard values.</returns>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return false;
    }

    /// <summary>
    /// Fill in the list of the data source members.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList values = [];

        Chart chart = ConverterHelper.GetChartFromContext(context);
        object dataSource = null;

        if (chart != null)
        {
            if (chart != null && ChartImage.IsValidDataSource(chart.DataSource))
            {
                dataSource = chart.DataSource;
            }

            // Check if it's Y values member
            bool usedForYValues = false;
            if (context.PropertyDescriptor != null && context.PropertyDescriptor.Name == "YValueMembers")
            {
                usedForYValues = true;
            }

            // Populate list with all members names
            var memberNames = ChartImage.GetDataSourceMemberNames(dataSource, usedForYValues);
            foreach (string name in memberNames)
            {
                values.Add(name);
            }

            values.Add("(none)");
        }

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Chart legend name converter. Displays list of available legend names
/// </summary>
internal sealed class SeriesLegendNameConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Non exclusive standard values.</returns>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return false;
    }

    /// <summary>
    /// Fill in the list of the chart legend for the series.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList values = [];

        Chart chart = ConverterHelper.GetChartFromContext(context);

        if (chart != null)
        {
            foreach (Legend legend in chart.Legends)
            {
                values.Add(legend.Name);
            }
        }

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Chart type converter. Displays list of available chart type names
/// </summary>
internal sealed class ChartTypeConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Non exclusive standard values.</returns>
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Fill in the list of chart type names.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        Collections.Generic.List<string> values = [];
        Chart chart = ConverterHelper.GetChartFromContext(context);
        if (chart is not null)
        {
            // Get chart type registry service
            ChartTypeRegistry registry = (ChartTypeRegistry)chart.GetService(typeof(ChartTypeRegistry));
            if (registry is not null)
            {
                // Enumerate all chart types names
                foreach (string n in registry.registeredChartTypes.Keys)
                    values.Add(n);
            }
            else
            {
                throw new InvalidOperationException(SR.ExceptionEditorChartTypeRegistryServiceInaccessible);
            }
        }

        // Sort all values
        values.Sort();
        return new StandardValuesCollection(values);
    }

    #endregion
}


/// <summary>
/// Data series name converter. Displays list of available series names
/// </summary>
internal sealed class SeriesNameConverter : StringConverter
{
    #region Converter methods

    /// <summary>
    /// Standard values supported - return true
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values supported.</returns>
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Standard values are not exclusive - return false
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
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        DataManager dataManager = null;
        ArrayList values = [];

        if (context != null && context.Instance != null)
        {
            // Call GetService method using reflection
            MethodInfo methodInfo = context.Instance.GetType().GetMethod("GetService");
            if (methodInfo != null)
            {
                object[] parameters = new object[1];
                parameters[0] = typeof(DataManager);
                dataManager = (DataManager)methodInfo.Invoke(context.Instance, parameters);
            }

            // If data manager service was seccesfully retrived
            if (dataManager != null)
            {
                foreach (Series series in dataManager.Series)
                {
                    values.Add(series.Name);
                }
            }
            else
            {
                throw new InvalidOperationException(SR.ExceptionEditorChartTypeRegistryServiceInObjectInaccessible(context.Instance.GetType().ToString()));
            }
        }

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Data point properties converter
/// </summary>
internal class NoNameExpandableObjectConverter : ExpandableObjectConverter
{
    #region Converter methods

    /// <summary>
    /// Overrides the ConvertTo method of TypeConverter.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert.</param>
    /// <param name="destinationType">Conversion destination type.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (context != null && context.Instance != null)
        {
            if (destinationType == typeof(string))
            {
                return string.Empty;
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    #endregion
}

/// <summary>
/// Converter for the array of doubles
/// </summary>
internal sealed class DoubleArrayConverter : ArrayConverter
{
    #region Converter methods

    /// <summary>
    /// Overrides the CanConvertFrom method of TypeConverter.
    /// The ITypeDescriptorContext interface provides the context for the
    /// conversion. Typically this interface is used at design time to 
    /// provide information about the design-time container.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="sourceType">Conversion source type.</param>
    /// <returns>Indicates if conversion is possible.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Overrides the ConvertFrom method of TypeConverter.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert from.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        object result = null;
        bool convertFromDate = false;

        // Try to check if value type is date
        if (context != null && context.Instance != null)
        {
            DataPoint dataPoint = (DataPoint)context.Instance;
            if (dataPoint.series != null && dataPoint.series.IsYValueDateTime())
            {
                convertFromDate = true;
            }
        }

        // Can convert from string where each array element is separated by comma
        if (value is string stringValue)
        {
            string[] values = stringValue.Split(',');
            double[] array = new double[values.Length];
            for (int index = 0; index < values.Length; index++)
            {
                // Try to convert from date-time string format
                if (convertFromDate)
                {
                    if (DateTime.TryParse(values[index], CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime valueAsDate))
                    {
                        result = valueAsDate;
                    }
                    else if (DateTime.TryParse(values[index], CultureInfo.CurrentCulture, DateTimeStyles.None, out valueAsDate))
                    {
                        result = valueAsDate;
                    }
                    else
                    {
                        result = null;
                    }
                }

                // Save converted value in the array
                if (result != null)
                {
                    array[index] = (double)result;
                }
                else
                {
                    array[index] = CommonElements.ParseDouble(values[index]);
                }
            }

            return array;
        }

        // Call base class
        return base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Overrides the ConvertTo method of TypeConverter.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert.</param>
    /// <param name="destinationType">Conversion destination type.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        bool convertToDate = false;

        // Check if we should convert to date string format
        if (context != null && context.Instance != null)
        {
            DataPoint dataPoint = (DataPoint)context.Instance;
            if (dataPoint.series != null && dataPoint.series.IsYValueDateTime())
            {
                convertToDate = true;
            }
        }


        if (destinationType == typeof(string))
        {
            double[] array = (double[])value;
            string result = string.Empty;

            foreach (double d in array)
            {
                if (convertToDate)
                {
                    result += DateTime.FromOADate(d).ToString("g", CultureInfo.InvariantCulture) + ",";
                }
                else
                {
                    result += d.ToString(CultureInfo.InvariantCulture) + ",";
                }
            }

            return result.TrimEnd(',');
        }


        return base.ConvertTo(context, culture, value, destinationType);
    }

    #endregion
}

/// <summary>
/// Converts data point values to and from date string format
/// </summary>
internal sealed class DataPointValueConverter : DoubleConverter
{
    #region Converter methods

    /// <summary>
    /// Convert values to date string
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert.</param>
    /// <param name="destinationType">Conversion destination type.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (context != null && context.Instance != null)
        {
            DataPoint dataPoint = (DataPoint)context.Instance;

            if (destinationType == typeof(string) && dataPoint.series.IsXValueDateTime())
            {
                DateTime valueAsSate = DateTime.FromOADate((double)value);
                return valueAsSate.ToString("g", CultureInfo.CurrentCulture);
            }
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <summary>
    /// Convert values from date string.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert from.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        if (context != null && context.Instance != null)
        {
            if (value is string stringValue)
            {
                DataPoint dataPoint = (DataPoint)context.Instance;

                if (dataPoint.series.IsXValueDateTime())
                {
                    DateTime valueAsSate = DateTime.Parse(stringValue, CultureInfo.CurrentCulture);
                    return valueAsSate.ToOADate();
                }
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    #endregion
}

/// <summary>
/// Removes the String type for Y axes
/// </summary>
internal sealed class SeriesYValueTypeConverter : EnumConverter
{
    #region Converter methods

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="type">Enumeration type.</param>
    public SeriesYValueTypeConverter(Type type) : base(type)
    {
    }

    /// <summary>
    /// Fill in the list of data series names.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Standard values collection.</returns>
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        ArrayList values = [];

        // Call base class
        StandardValuesCollection val = base.GetStandardValues(context);

        // Remove string type
        foreach (object o in val)
        {
            if (o.ToString() != "String")
            {

                values.Add(o);
            }
        }

        return new StandardValuesCollection(values);
    }

    #endregion
}

/// <summary>
/// Data point properties converter
/// </summary>
internal sealed class ColorArrayConverter : TypeConverter
{
    #region Converter methods

    /// <summary>
    /// This method overrides CanConvertTo from TypeConverter. This is called when someone
    /// wants to convert an instance of object to another type.  Here,
    /// only conversion to an InstanceDescriptor is supported.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <returns>True if object can be converted.</returns>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return true;
        }

        // Always call the base to see if it can perform the conversion.
        return base.CanConvertTo(context, destinationType);
    }

    /// <summary>
    /// Overrides the CanConvertFrom method of TypeConverter.
    /// The ITypeDescriptorContext interface provides the context for the
    /// conversion. Typically this interface is used at design time to 
    /// provide information about the design-time container.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="sourceType">Conversion source type.</param>
    /// <returns>Indicates if conversion is possible.</returns>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    /// <summary>
    /// Overrides the ConvertTo method of TypeConverter.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert.</param>
    /// <param name="destinationType">Conversion destination type.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return ColorArrayToString(value as Color[]);
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }

    /// <summary>
    /// Overrides the ConvertFrom method of TypeConverter.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Value to convert from.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        // Can convert from string where each array element is separated by comma
        if (value is string stringValue)
        {
            return StringToColorArray(stringValue);
        }

        // Call base class
        return base.ConvertFrom(context, culture, value);
    }

    /// <summary>
    /// Converts array of colors into string.
    /// </summary>
    /// <param name="colors">Colors array.</param>
    /// <returns>Result string.</returns>
    public static string ColorArrayToString(Color[] colors)
    {
        if (colors != null && colors.GetLength(0) > 0)
        {
            ColorConverter colorConverter = new ColorConverter();
            string result = string.Empty;
            foreach (Color color in colors)
            {
                if (result.Length > 0)
                {
                    result += "; ";
                }

                result += colorConverter.ConvertToInvariantString(color);
            }

            return result;
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts string into array of colors.
    /// </summary>
    /// <param name="colorNames">String data.</param>
    /// <returns>Array of colors.</returns>
    public static Color[] StringToColorArray(string colorNames)
    {
        ColorConverter colorConverter = new ColorConverter();
        Color[] array = Array.Empty<Color>();
        if (colorNames.Length > 0)
        {
            string[] colorValues = colorNames.Split(';');
            array = new Color[colorValues.Length];
            int index = 0;
            foreach (string str in colorValues)
            {
                array[index++] = (Color)colorConverter.ConvertFromInvariantString(str);
            }
        }

        return array;
    }

    #endregion
}

/// <summary>
/// Provides a set of helper methods used by converters
/// </summary>
internal static class ConverterHelper
{

    #region Static
    /// <summary>
    /// Gets the chart from context.
    /// </summary>
    /// <param name="context">The context.</param>
    public static Chart GetChartFromContext(ITypeDescriptorContext context)
    {
        if (context is null)
            return null;

        return GetChartFromContextInstance(context.Instance);
    }

    /// <summary>
    /// Gets the chart from context.
    /// </summary>
    /// <param name="instance">The instance.</param>
    /// <returns></returns>
    public static Chart GetChartFromContextInstance(object instance)
    {
        if (instance is null)
            return null;


        if (instance is IChartElement element && element.Common is not null)
            return element.Common.Chart;

        if (instance is IList list && list.Count > 0)
        {
            element = list[0] as IChartElement;
            if (element.Common is not null)
                return element.Common.Chart;
        }

        if (instance is Chart chart)
            return chart;

        if (instance is IServiceProvider provider)
            return provider.GetService(typeof(Chart)) as Chart;

        return null;
    }
    #endregion
}


