// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converter classes for the 
//				Series and DataPoint properties.
//


using System.ComponentModel;
using System.Drawing.Design;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Chart type editor. Paint chart type image in the property grid.
    /// </summary>
    internal class ChartTypeEditor : UITypeEditor
	{
        #region Converter methods

#warning designer
        // Reference to the chart type registry
        //private ChartTypeRegistry	_chartTypeRegistry;

        /// <summary>
        /// Override this function to support chart type drawing
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Can paint values.</returns>
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
		{
   //         // Initialize the chartTypeRegistry using context
			//if (context != null && context.Instance != null)
			//{
   //             IChartElement chartElement = context.Instance as IChartElement;
   //             if (chartElement != null)
   //             {
   //                 this._chartTypeRegistry = chartElement.Common.ChartTypeRegistry;
   //             }
			//}

            // Always return true
			return true;
		}

		///// <summary>
		///// Override this function to support chart type drawing
		///// </summary>
		///// <param name="e">Paint value event arguments.</param>
		//public override void PaintValue(PaintValueEventArgs e)
		//{
		//	string	chartTypeName = String.Empty;
		//	if(_chartTypeRegistry != null && e != null)
		//	{
		//		if(e.Value is string)
		//		{
		//			chartTypeName = (string)e.Value;
		//		}
		//		else if(e.Value is SeriesChartType)
		//		{
		//			chartTypeName = Series.GetChartTypeName((SeriesChartType)e.Value);
		//		}


		//		if(!string.IsNullOrEmpty(chartTypeName))
		//		{
		//			IChartType chartType = _chartTypeRegistry.GetChartType(chartTypeName);

		//			// Get imahe from the chart type
		//			System.Drawing.Image	chartImage = null;
		//			if(chartType != null)
		//			{
		//				chartImage = chartType.GetImage(_chartTypeRegistry);
		//			}

		//			// Draw image
		//			if(chartImage != null)
		//			{
		//				e.Graphics.DrawImage(chartImage, e.Bounds);
		//			}
		//		}
		//	}
		//}
		
        #endregion
	}
}