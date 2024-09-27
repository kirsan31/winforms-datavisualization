// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converter classes for the 
//				Series and DataPoint properties.
//


using System;
using System.Collections;
using System.Windows.Forms;

using Microsoft.DotNet.DesignTools.Client.Proxies;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Collection editor that supports property help in the property grid
    /// </summary>
    internal class ChartCollectionEditor : Microsoft.DotNet.DesignTools.Client.Editors.CollectionEditor
    {
        #region Editor methods and properties 

        // Collection editor form
        private Form? _form;

        // Help topic string
        private string _helpTopic = string.Empty;


        /// <summary>
        /// Object constructor.
        /// </summary>
        /// <param name="type">AxisName.</param>
        public ChartCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.ChartCollectionEditor;


        /// <summary>
        /// Override the HelpTopic property to provide different topics,
        /// depending on selected property.
        /// </summary>
        protected override string? HelpTopic => (_helpTopic.Length == 0) ? base.HelpTopic : _helpTopic;

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
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataPointCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.DataPointCollectionEditor;
    }


    /// <summary>
    /// Designer editor for the data series collection.
    /// </summary>
    internal class SeriesCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public SeriesCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.SeriesCollectionEditor;
    }
}