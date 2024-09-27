using System;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class AnnotationCollectionEditor : ChartCollectionEditor
{
    public AnnotationCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    /// <summary>
    /// Gets the data types that this collection editor can contain.
    /// </summary>
    /// <returns>An array of data types that this collection can contain.</returns>
    protected override Type[] CreateNewItemTypes()
    {
        return new Type[] {
            typeof(LineAnnotation),
            typeof(VerticalLineAnnotation),
            typeof(HorizontalLineAnnotation),
            typeof(TextAnnotation),
            typeof(RectangleAnnotation),
            typeof(EllipseAnnotation),
            typeof(ArrowAnnotation),
            typeof(Border3DAnnotation),
            typeof(CalloutAnnotation),
            typeof(PolylineAnnotation),
            typeof(PolygonAnnotation),
            typeof(ImageAnnotation),
            typeof(AnnotationGroup)
        };
    }

    /// <summary>
    /// Create annotation instance in the editor 
    /// </summary>
    /// <param name="itemType">Item type.</param>
    /// <returns>Newly created item.</returns>
    protected override object CreateInstance(Type itemType)
    {
        // Call base class
        Annotation annotation = (base.CreateInstance(itemType) as Annotation)!;

        // Generate unique name 
        if (Helpers.GetChartReference(Context.Instance!) is Chart chart)
            annotation.Name = NextUniqueName(chart, itemType);

        return annotation;
    }


    /// <summary>
    /// Finds the unique name for a new annotation being added to the collection
    /// </summary>
    /// <param name="control">Chart control reference.</param>
    /// <param name="type">Type of the annotation added.</param>
    /// <returns>Next unique chart annotation name</returns>
    private static string NextUniqueName(Chart control, Type type)
    {
        // Find unique name
        string result = string.Empty;
        string prefix = type.Name;
        for (int i = 1; i < int.MaxValue; i++)
        {
            result = prefix + i.ToString(CultureInfo.InvariantCulture);

            // Check whether the name is unique
            if (control.Annotations.IsUniqueName(result))
            {
                break;
            }
        }

        return result;
    }
}