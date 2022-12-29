using System;
using System.Windows.Forms.DataVisualization.Charting;
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace WinForms.DataVisualization.Designer.Server;

internal static class Helpers
{
    /// <summary>
    /// Checks if the instance belongs to Chart type or contains the field of chart type.
    /// NOTE: Required for the Diagram product.
    /// </summary>
    /// <param name="instance">Instance to check.</param>
    /// <returns>
    /// Object of chart type.
    /// </returns>
    /// <exception cref="System.InvalidOperationException"></exception>
    internal static Chart GetChartReference(object instance)
    {
        // Check instance type.
        if (instance is Chart chart)
            return chart;

        // Read chart reference from the "chart" field.
        IChartElement? element = instance as IChartElement;
        if (element is not null)
            return element.Common.Chart;
        else
            throw new InvalidOperationException(SR.ExceptionEditorContectInstantsIsNotChartObject);
    }
}