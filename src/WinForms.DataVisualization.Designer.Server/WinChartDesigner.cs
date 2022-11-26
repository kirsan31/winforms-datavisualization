// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Windows forms chart control designer class.
//


using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Designers;
using Microsoft.Win32;

namespace WinForms.DataVisualization.Designer.Server;


/// <summary>
/// Chart windows forms control designer
/// </summary>
internal class ChartWinDesigner : ControlDesigner
{
    #region Fields

    // Reference to the chart designer
    internal static ChartWinDesigner? controlDesigner;

    #endregion

    #region Methods

    /// <summary>
    /// Initialize designer.
    /// </summary>
    /// <param name="component">Component.</param>
    public override void Initialize(IComponent component)
    {
        base.Initialize(component);

        // Set reference to the designer
        ChartWinDesigner.controlDesigner = this;
        SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
    }

    protected override void OnMouseDragBegin(int x, int y, Keys modifierKeys)
    {
        base.OnMouseDragBegin(x, y, modifierKeys);
        ChartWinDesigner.controlDesigner = this;
    }



    /// <summary>
    /// Set default values for properties of the component.
    /// NOTE: Replaces obsolete method: OnSetComponentDefaults()
    /// </summary>
    /// <param name="defaultValues">Default values property bags.</param>
    public override void InitializeNewComponent(IDictionary? defaultValues)
    {
        if (Control is Chart chart)
        {
            // If control is not initialized
            if (chart.ChartAreas.Count == 0 &&
                chart.Series.Count == 0)
            {
                // Add Default chart area
                chart.ChartAreas.Add(new ChartArea());

                // Add Default series
                chart.Series.Add(new Series());

                chart.Legends.Add(new Legend());
            }
        }

        base.InitializeNewComponent(defaultValues);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            //Free managed resources
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// User changed Windows preferences
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">Event arguments.</param>
    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        // If user changed system colors, make Chart repaint itself.
        if (e.Category == UserPreferenceCategory.Color)
            Control.Invalidate();
    }


    #endregion

    #region Data Binding

    /// <summary>
    /// Data source was changed.
    /// </summary>
    /// <param name="chartControl">Reference to the chart control.</param>
    internal static void OnDataSourceChanged(Chart chartControl)
    {
        if (chartControl != null)
        {
            // Clear all value members properties in the series
            foreach (Series series in chartControl.Series)
            {
                series.XValueMember = string.Empty;
                series.YValueMembers = string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets selected data source object.
    /// </summary>
    public object? GetControlDataSource()
    {
        object? selectedDataSource = null;
        if (this.Control is Chart chart)
        {
            selectedDataSource = this.GetControlDataSource(chart);
        }

        return selectedDataSource;
    }

    /// <summary>
    /// Gets selected data source object.
    /// </summary>
    /// <param name="chart">Chart control.</param>
    /// <returns>Data source.</returns>
    internal object? GetControlDataSource(Chart chart)
    {
        object? selectedDataSource = null;
        if (chart != null)
        {
            if (chart.DataSource != null)
            {
                object? dataSourceObject = chart.DataSource;
                if (dataSourceObject is string fieldName && this.Component is not null)
                {
                    dataSourceObject = null;
                    ISite? componentSite = this.Component.Site;
                    if (componentSite is not null)
                    {
                        IContainer? container = (IContainer?)componentSite.GetService(typeof(IContainer));
                        if (container is not null)
                        {
                            dataSourceObject = container.Components[fieldName];
                        }
                    }
                }

                if (dataSourceObject is not null && ChartImage.IsValidDataSource(dataSourceObject))
                {
                    selectedDataSource = dataSourceObject;
                }
            }
        }

        return selectedDataSource;
    }

    #endregion //DataBinding

}

