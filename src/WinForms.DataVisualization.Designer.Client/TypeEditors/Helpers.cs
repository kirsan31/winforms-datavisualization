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