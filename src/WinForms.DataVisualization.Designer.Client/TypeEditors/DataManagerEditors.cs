// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converter classes for the 
//				Series and DataPoint properties.
//


using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

using Microsoft.DotNet.DesignTools.Client;
using Microsoft.DotNet.DesignTools.Client.Editors;
using Microsoft.DotNet.DesignTools.Client.Proxies;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Collection editor that supports property help in the property grid
    /// </summary>
    internal class ChartCollectionEditor : Microsoft.DotNet.DesignTools.Client.Editors.CollectionEditor
    {
        #region Editor methods and properties 

        // Collection editor form
        Form? _form;
        //object? _chart;
        //ITypeDescriptorContext? _context;

        // Help topic string
        string _helpTopic = string.Empty;
        /// <summary>
        /// Object constructor.
        /// </summary>
        /// <param name="type">AxisName.</param>
        public ChartCollectionEditor(Type type) : base(type)
        {
        }

        /// <summary>
        /// Edit object's value.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="provider">Service provider.</param>
        /// <param name="value">Value to edit.</param>
        /// <returns>The new value of the object.</returns>
        public override object? EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            //_context = context;
            // Save current control type descriptor context
            //_chart = context.Instance;

            if (provider is null || value is not ObjectProxy proxy || (proxy.TypeIdentity.Name != "ChartAreaCollection" && proxy.TypeIdentity.Name != "LegendCollection"))
                return base.EditValue(context, provider!, value);

            Endpoint<ChartCollectionEditorEditValueRequest, Response.Empty>.Sender? sender = null;
            try
            {
                var client = provider.GetRequiredService<IDesignToolsClient>();
                sender = client.Protocol.GetEndpoint<ChartCollectionEditorEditValueEndpoint>().GetSender(client);
                sender.Value.SendRequest(new ChartCollectionEditorEditValueRequest(value, true));
                return base.EditValue(context, provider, value);
            }
            finally
            {
                sender?.SendRequest(new ChartCollectionEditorEditValueRequest(value, false));
            }
        }

        ///// <summary>
        ///// Sets the specified array as the items of the collection.
        ///// </summary>
        ///// <param name="editValue">The collection to edit.</param>
        ///// <param name="value">An array of objects to set as the collection items.</param>
        ///// <returns>
        ///// The newly created collection object or, otherwise, the collection indicated by the <paramref name="editValue"/> parameter.
        ///// </returns>
        //protected override object SetItems(object editValue, object[] value)
        //{
        //    object result = base.SetItems(editValue, value);

        //    IComponentChangeService svc = _context.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
        //    INameController controller = editValue as INameController;
        //    if (controller != null && svc != null && (editValue is ChartAreaCollection || editValue is LegendCollection))
        //    {
        //        IList newList = (IList)result;
        //        bool elementsRemoved = false;
        //        foreach (ChartNamedElement element in controller.Snapshot)
        //        {
        //            if (newList.IndexOf(element) < 0)
        //            {
        //                elementsRemoved = true;
        //            }
        //        }

        //        if (elementsRemoved)
        //        {
        //            svc.OnComponentChanging(this._chart, null);
        //            ChartNamedElement defaultElement = (ChartNamedElement)(newList.Count > 0 ? newList[0] : null);
        //            foreach (ChartNamedElement element in controller.Snapshot)
        //            {
        //                if (newList.IndexOf(element) < 0)
        //                {
        //                    controller.OnNameReferenceChanged(new NameReferenceChangedEventArgs(element, defaultElement));
        //                }
        //            }

        //            svc.OnComponentChanged(this._chart, null, null, null);
        //        }
        //    }

        //    return result;
        //}


        /// <summary>
        /// Override the HelpTopic property to provide different topics,
        /// depending on selected property.
        /// </summary>
        protected override string? HelpTopic
        {
            get
            {
                return (_helpTopic.Length == 0) ? base.HelpTopic : _helpTopic;
            }
        }

        /// <summary>
        /// Displaying help for the currently selected item in the property grid
        /// </summary>
        protected override void ShowHelp()
        {
            // Init topic name
            _helpTopic = string.Empty;
            PropertyGrid? grid = Helpers.GetPropertyGrid(_form?.Controls);

            // Check currently selected grid item
            if (grid is not null)
            {
                GridItem item = grid.SelectedGridItem;
                if (item is not null && (item.GridItemType == GridItemType.Property || item.GridItemType == GridItemType.ArrayValue))
                {
#warning designer question
                    // Original code:
                    //_helpTopic = item.PropertyDescriptor.ComponentType.ToString() + "." + item.PropertyDescriptor.Name;

                    // We have a proxy object (Microsoft.DotNet.DesignTools.Client.Proxies.ProxyPropertyDescriptor) here as PropertyDescriptor.
                    // So to get real type we need get PropertyData (Microsoft.DotNet.DesignTools.Protocol.PropertyData) from PropertyDescriptor and then ComponentType from it.
                    // Because Microsoft.DotNet.DesignTools.Client.Proxies.ProxyPropertyDescriptor and Microsoft.DotNet.DesignTools.Protocol.PropertyData are internal we need to use reflection...
                    if (item.PropertyDescriptor.GetPropValue("PropertyData")?.GetPropValue("ComponentType") is Microsoft.DotNet.DesignTools.Protocol.Types.TypeIdentity typeIdentity)
                        _helpTopic = typeIdentity.TypeName + "." + item.PropertyDescriptor.Name;
                }
            }

            // Call base class
            base.ShowHelp();

            // Re-Init topic name
            _helpTopic = string.Empty;
        }

        /// <summary>
        /// Collect the collection editor form buttons into array. Added for VS2005 compatibility.
        /// </summary>
        /// <param name="buttons"></param>
        /// <param name="controls"></param>
        private void CollectButtons(ArrayList buttons, Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Button)
                {
                    buttons.Add(control);
                }

                if (control.Controls.Count > 0)
                {
                    CollectButtons(buttons, control.Controls);
                }
            }
        }

        /// <summary>
        /// Creates form for collection editing.
        /// </summary>
        /// <returns>Form object.</returns>
        protected override ICollectionForm CreateCollectionForm(ObjectProxy viewModel)
        {
            var vm = base.CreateCollectionForm(viewModel);
            _form = vm as Form;
            if (_form is not null)
            {
                // Changed Apr 29, DT,  for VS2005 compatibility
                PropertyGrid? grid = Helpers.GetPropertyGrid(_form.Controls);
                if (grid is not null)
                {
                    // Show properties help
                    grid.HelpVisible = true;
                    grid.CommandsVisibleIfAvailable = true;

                    // Hookup to the update events
                    grid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnPropertyChanged);
                    grid.ControlAdded += new ControlEventHandler(this.OnControlAddedRemoved);
                    grid.ControlRemoved += new ControlEventHandler(this.OnControlAddedRemoved);

                }

                // Changed Apr 29, DT, for VS2005 compatibility
                ArrayList buttons = new ArrayList();
                this.CollectButtons(buttons, _form.Controls);
                foreach (Button button in buttons)
                {
                    if (button.DialogResult == DialogResult.OK || button.DialogResult == DialogResult.Cancel)
                    {
                        button.Click += new EventHandler(this.OnOkClicked);
                    }
                }
            }

            return vm;
        }


        /// <summary>
        /// Update design-time HTML when OK button is clicked in the collection editor
        /// </summary>
        private void OnOkClicked(object sender, EventArgs e)
        {
            // Clear the help topic
            _helpTopic = string.Empty;
        }

        /// <summary>
        /// Update design-time HTML when property is added or removed
        /// </summary>
        private void OnControlAddedRemoved(object sender, ControlEventArgs e)
        {
        }

        /// <summary>
        /// Update design-time HTML when property is changed
        /// </summary>
        private void OnPropertyChanged(object sender, PropertyValueChangedEventArgs e)
        {
        }

        #endregion
    }


    /// <summary>
    /// Designer editor for the data points collection.
    /// </summary>
    internal class DataPointCollectionEditor : ChartCollectionEditor
	{
		#region Editor methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public DataPointCollectionEditor(Type type) : base(type)
        {
		}

#warning designer
        ///// <summary>
        ///// Do not allow to edit if multiple series selected.
        ///// </summary>
        ///// <param name="context">Descriptor context.</param>
        ///// <param name="provider">Service provider.</param>
        ///// <param name="value">Value to edit.</param>
        ///// <returns>The new value of the object.</returns>
        //public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) 
        //{
        //	if (context != null && context.Instance != null)
        //	{
        //		// Save current control type descriptor context
        //		if(!(context.Instance is Series))
        //		{
        //                  throw (new InvalidOperationException(SR.ExceptionEditorMultipleSeriesEditiingUnsupported));
        //		}
        //	}
        //	return base.EditValue(context, provider, value);
        //}

        //      /// <summary>
        //      /// Create instance of data point object
        //      /// </summary>
        //      /// <param name="itemType">Item type.</param>
        //      /// <returns>New item instance.</returns>
        //      protected override object CreateInstance(Type itemType)
        //{
        //	if (Context != null && Context.Instance != null)
        //	{
        //		if (Context.Instance is Series)
        //		{
        //			Series	series = (Series)Context.Instance;
        //			DataPoint	newDataPoint = new DataPoint(series);
        //			return newDataPoint;
        //		}
        //		else if(Context.Instance is Array)
        //		{
        //                  throw new InvalidOperationException(SR.ExceptionEditorMultipleSeriesEditiingUnsupported);
        //		}
        //	}

        //	return base.CreateInstance(itemType);
        //}

        #endregion
    }


	/// <summary>
	/// Designer editor for the data series collection.
	/// </summary>
	internal class SeriesCollectionEditor : ChartCollectionEditor
	{
		#region Editor methods

		/// <summary>
		/// Object constructor.
		/// </summary>
		public SeriesCollectionEditor(Type type) : base(type)
        {
		}

#warning dsigner
        //internal static Series CreateNewSeries(Chart control, string suggestedChartArea)
        //{
        //    int countSeries = control.Series.Count + 1;
        //    string seriesName = "Series" + countSeries.ToString(System.Globalization.CultureInfo.InvariantCulture);

        //    // Check if this name already in use
        //    bool seriesFound = true;
        //    while (seriesFound)
        //    {
        //        seriesFound = false;
        //        foreach (Series series in control.Series)
        //        {
        //            if (series.Name == seriesName)
        //            {
        //                seriesFound = true;
        //            }
        //        }

        //        if (seriesFound)
        //        {
        //            ++countSeries;
        //            seriesName = "Series" + countSeries.ToString(System.Globalization.CultureInfo.InvariantCulture);
        //        }
        //    }

        //    // Create new series
        //    Series newSeries = new Series(seriesName);

        //    // Check if default chart area name exists
        //    if (control.ChartAreas.Count > 0)
        //    {
        //        bool defaultFound = false;

        //        if (!string.IsNullOrEmpty(suggestedChartArea) &&
        //            control.ChartAreas.IndexOf(suggestedChartArea) != -1)
        //        {
        //            newSeries.ChartArea = suggestedChartArea;
        //            defaultFound = true;
        //        }
        //        else
        //        {
        //            foreach (ChartArea area in control.ChartAreas)
        //            {
        //                if (area.Name == newSeries.ChartArea)
        //                {
        //                    defaultFound = true;
        //                    break;
        //                }
        //            }
        //        }

        //        // If default chart area was not found - use name of the first area
        //        if (!defaultFound)
        //        {
        //            newSeries.ChartArea = control.ChartAreas[0].Name;
        //        }

        //        // Check if series area is circular
        //        if (control.ChartAreas[newSeries.ChartArea].chartAreaIsCurcular)
        //        {
        //            // Change default chart type
        //            newSeries.ChartTypeName = ChartTypeNames.Radar;

        //            // Check if it's a Polar chart type
        //            IChartType chartType = control.ChartAreas[newSeries.ChartArea].GetCircularChartType() as IChartType;
        //            if (chartType != null && string.Equals(chartType.Name, ChartTypeNames.Polar, StringComparison.OrdinalIgnoreCase))
        //            {
        //                newSeries.ChartTypeName = ChartTypeNames.Polar;
        //            }
        //        }
        //    }

        //    return newSeries;
        //}

        /// <summary>
        /// Create series instance in the editor 
        /// </summary>
        /// <param name="itemType">Item type.</param>
        /// <returns>Newly created item.</returns>
        protected override object? CreateInstance(Type itemType)
		{
			if (Context is not null && Context.Instance is not null)
			{
#warning dsigner
                //Chart	control = (Chart)Helpers.GetChartReference(Context.Instance);
                //            return SeriesCollectionEditor.CreateNewSeries(control, String.Empty);
            }

			return base.CreateInstance(itemType);
		}
	
		#endregion
	}
}