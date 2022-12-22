// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converter classes for the 
//				Series and DataPoint properties.
//


using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Reflection;
using System.Resources;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Chart type editor. Paint chart type image in the property grid.
    /// </summary>
    internal class ChartTypeEditor : UITypeEditor
    {
        private ResourceManager? _resourceManager;

        #region Converter methods

        /// <summary>
        /// Override this function to support chart type drawing
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <returns>Can paint values.</returns>
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            // Always return true
            return true;
        }

        /// <summary>
        /// Override this function to support chart type drawing
        /// </summary>
        /// <param name="e">Paint value event arguments.</param>
        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Context?.Instance is null || e.Value is null)
                return;

            string chartTypeName = string.Empty;
            if (e.Value is string @string)
            {
                chartTypeName = @string;
            }
            else if (e.Value is Microsoft.DotNet.DesignTools.Client.Proxies.EnumProxy enumProxy)
            {
                chartTypeName = ChartTypeNames.GetChartTypeName(enumProxy.AsEnumValue<SeriesChartType>());
            }

            if (string.IsNullOrEmpty(chartTypeName))
                return;

            _resourceManager ??= new ResourceManager(typeof(ChartTypeEditor).Namespace, Assembly.GetExecutingAssembly());

            // Get image
            Image? chartImage = _resourceManager.GetObject(chartTypeName + "ChartType") as Image;
            // Draw image
            if (chartImage is not null)
                e.Graphics.DrawImage(chartImage, e.Bounds);
        }

        #endregion
    }
}