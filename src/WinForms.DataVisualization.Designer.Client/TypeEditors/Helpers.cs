using System;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace WinForms.DataVisualization.Designer.Client
{
    internal static class Helpers
    {
#warning designer
        /// <summary>
        /// Checks if the instance belongs to Chart type or contains the field of chart type.
        /// NOTE: Required for the Diagram product.
        /// </summary>
        /// <param name="instance">Instance to check.</param>
        /// <returns>
        /// Object of chart type.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        //internal static Chart GetChartReference(object instance)
        //{
        //    // Check instance type.
        //    if (instance is Chart)
        //    {
        //        return instance;
        //    }

        //    // Read chart reference from the "chart" field.
        //    IChartElement element = instance as IChartElement;
        //    if (element is not null)
        //        return element.Common.Chart;
        //    else
        //        throw new InvalidOperationException(SR.ExceptionEditorContectInstantsIsNotChartObject);
        //}

        /// <summary>
        /// Returns the collection form property grid.
        /// </summary>
        /// <param name="controls"></param>
        /// <returns></returns>
        internal static PropertyGrid? GetPropertyGrid(Control.ControlCollection? controls)
        {
            if (controls is null)
                return null;

            foreach (Control control in controls)
            {
                PropertyGrid? grid = control as PropertyGrid;
                if (grid is not null)
                {
                    return grid;
                }

                if (control.Controls.Count > 0)
                {
                    grid = GetPropertyGrid(control.Controls);
                    if (grid is not null)
                    {
                        return grid;
                    }
                }
            }

            return null;
        }
    }
}