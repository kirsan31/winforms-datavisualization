using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Forms.DataVisualization.Charting;

internal class DataPointCustomPropertiesConverter : TypeConverter
{

    /// <summary>
    /// Returns whether this object supports properties, using the specified context.
    /// </summary>
    /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
    /// <returns>
    /// true if <see cref="M:System.ComponentModel.TypeConverter.GetProperties(System.Object)"/> should be called to find the properties of this object; otherwise, false.
    /// </returns>
    public override bool GetPropertiesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Returns a collection of properties for the type of array specified by the value parameter, using the specified context and attributes.
    /// </summary>
    /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="value">An <see cref="T:System.Object"/> that specifies the type of array for which to get properties.</param>
    /// <param name="attributes">An array of type <see cref="T:System.Attribute"/> that is used as a filter.</param>
    /// <returns>
    /// A <see cref="T:System.ComponentModel.PropertyDescriptorCollection"/> with the properties that are exposed for this data type, or null if there are no properties.
    /// </returns>
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
    {
        // Fill collection with properties descriptors
        PropertyDescriptorCollection propDescriptors = TypeDescriptor.GetProperties(value, attributes, false);

        // Return original collection if not in design mode
        if (context != null && context.Instance is ChartElement &&
            (context.Instance as ChartElement).Chart is not null &&
            (context.Instance as ChartElement).Chart.IsDesignMode())
        {
            // Create new descriptors collection
            PropertyDescriptorCollection newPropDescriptors = new PropertyDescriptorCollection(null);

            // Loop through all original property descriptors
            foreach (PropertyDescriptor propertyDescriptor in propDescriptors)
            {
                // Change name of "CustomAttributesEx" property to "CustomProperties"
                if (propertyDescriptor.Name == "CustomAttributesEx")
                {
                    DynamicPropertyDescriptor dynPropDesc = new DynamicPropertyDescriptor(
                        propertyDescriptor,
                        "CustomProperties");
                    newPropDescriptors.Add(dynPropDesc);
                }
                else
                {
                    newPropDescriptors.Add(propertyDescriptor);
                }
            }

            return newPropDescriptors;
        }

        // Return original collection if not in design mode
        return propDescriptors;

    }

    /// <summary>
    /// Converts the given value object to the specified type, using the specified context and culture information.
    /// </summary>
    /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
    /// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
    /// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
    /// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
    /// <returns>
    /// An <see cref="T:System.Object"/> that represents the converted value.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception>
    /// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        if (context is not null)
        {
            if (destinationType == typeof(string))
            {
                return string.Empty;
            }
        }                // Always call base, even if you can't convert.

        return base.ConvertTo(context, culture, value, destinationType);
    }
}


/// <summary>
/// DataPoint Converter - helps windows form serializer to create inline datapoints.
/// </summary>
internal sealed class DataPointConverter : DataPointCustomPropertiesConverter
{

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
        if (destinationType == typeof(InstanceDescriptor))
        {
            return true;
        }

        // Always call the base to see if it can perform the conversion.
        return base.CanConvertTo(context, destinationType);
    }

    /// <summary>
    /// This methods performs the actual conversion from an object to an InstanceDescriptor.
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <param name="culture">Culture information.</param>
    /// <param name="value">Object value.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <returns>Converted object.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        DataPoint dataPoint = value as DataPoint;
        if (destinationType == typeof(InstanceDescriptor) && dataPoint != null)
        {
            if (dataPoint.YValues.Length > 1)
            {
                ConstructorInfo ci = typeof(DataPoint).GetConstructor(new Type[] { typeof(double), typeof(string) });
                string yValues = string.Empty;
                foreach (double y in dataPoint.YValues)
                {
                    yValues += y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
                }

                return new InstanceDescriptor(ci, new object[] { dataPoint.XValue, yValues.TrimEnd(',') }, false);
            }
            else
            {
                ConstructorInfo ci = typeof(DataPoint).GetConstructor(new Type[] { typeof(double), typeof(double) });
                return new InstanceDescriptor(ci, new object[] { dataPoint.XValue, dataPoint.YValues[0] }, false);
            }
        }

        // Always call base, even if you can't convert.
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
