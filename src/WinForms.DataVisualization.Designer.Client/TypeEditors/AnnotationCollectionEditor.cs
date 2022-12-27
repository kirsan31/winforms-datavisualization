// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time UI editor for Annotations.
//


using System;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Design;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Designer editor for the Annotation Collection.
    /// </summary>
    internal class AnnotationCollectionEditor : ChartCollectionEditor
    {
        #region Methods

        /// <summary>
        /// Object constructor.
        /// </summary>
        public AnnotationCollectionEditor(Type type) : base(type)
        {
        }

#warning designer
        //     /// <summary>
        //     /// Gets the data types that this collection editor can contain.
        //     /// </summary>
        //     /// <returns>An array of data types that this collection can contain.</returns>
        //     protected override Type[] CreateNewItemTypes()
        //     {
        //         return new Type[] { 
        //	typeof(LineAnnotation), 
        //	typeof(VerticalLineAnnotation),
        //	typeof(HorizontalLineAnnotation),
        //	typeof(TextAnnotation), 
        //	typeof(RectangleAnnotation), 
        //    typeof(EllipseAnnotation), 
        //    typeof(ArrowAnnotation),
        //	typeof(Border3DAnnotation),
        //    typeof(CalloutAnnotation),
        //    typeof(PolylineAnnotation), 
        //    typeof(PolygonAnnotation), 
        //    typeof(ImageAnnotation), 
        //	typeof(AnnotationGroup) 
        //};
        //     }

        //     /// <summary>
        //     /// Create annotation instance in the editor 
        //     /// </summary>
        //     /// <param name="itemType">Item type.</param>
        //     /// <returns>Newly created item.</returns>
        //     protected override object CreateInstance(Type itemType)
        //     {
        //         Chart control = (Chart)Helpers.GetChartReference(Context.Instance);

        //         // Call base class
        //         Annotation annotation = base.CreateInstance(itemType) as Annotation;

        //         // Generate unique name 
        //         if (control != null)
        //         {
        //             annotation.Name = NextUniqueName(control, itemType);
        //         }

        //         return annotation;
        //     }


        //     /// <summary>
        //     /// Finds the unique name for a new annotation being added to the collection
        //     /// </summary>
        //     /// <param name="control">Chart control reference.</param>
        //     /// <param name="type">Type of the annotation added.</param>
        //     /// <returns>Next unique chart annotation name</returns>
        //     private static string NextUniqueName(Chart control, Type type)
        //     {
        //         // Find unique name
        //         string result = string.Empty;
        //         string prefix = type.Name;
        //         for (int i = 1; i < System.Int32.MaxValue; i++)
        //         {
        //             result = prefix + i.ToString(CultureInfo.InvariantCulture);

        //             // Check whether the name is unique
        //             if (control.Annotations.IsUniqueName(result))
        //             {
        //                 break;
        //             }
        //         }
        //         return result;
        //     }


        #endregion // Methods
    }
}
