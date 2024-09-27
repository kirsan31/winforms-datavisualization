// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converters for the Axes array.
//


using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Designer editor for the chart areas collection.
    /// </summary>
    internal class AxesArrayEditor : ArrayEditor
    {
        #region Fields and Constructor

        // Collection form
        private CollectionForm? _form;

        // Help topic string
        private string _helpTopic = string.Empty;

        public AxesArrayEditor(Type type) : base(type)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Items can not be removed.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>False if can't remove.</returns>
        protected override bool CanRemoveInstance(object value)
        {
            return false;
        }

        /// <summary>
        /// Override the HelpTopic property to provide different topics,
        /// depending on selected property.
        /// </summary>
        protected override string HelpTopic => (_helpTopic.Length == 0) ? base.HelpTopic : _helpTopic;

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
        /// Creates editor's form.
        /// </summary>
        /// <returns>Collection form.</returns>
        protected override CollectionForm CreateCollectionForm()
        {
            // Create collection form using the base class
            _form = base.CreateCollectionForm();
            // Changed Apr 29, DT,  for VS2005 compatibility
            PropertyGrid? grid = Helpers.GetPropertyGrid(_form.Controls);
            if (grid is not null)
            {
                // Show properties help
                grid.HelpVisible = true;
                grid.CommandsVisibleIfAvailable = true;
            }

            // Changed Apr 29, DT, for VS2005 compatibility
            ArrayList buttons = new ArrayList();
            this.CollectButtons(buttons, _form.Controls);
            foreach (Button button in buttons)
            {
                if (button.Name.StartsWith("add", StringComparison.OrdinalIgnoreCase) ||
                    button.Name.StartsWith("remove", StringComparison.OrdinalIgnoreCase) ||
                    button.Text.Length == 0)
                {
                    button.Enabled = false;
                    button.EnabledChanged += new EventHandler(Button_EnabledChanged);
                }
            }

            return _form;
        }

        /// <summary>
        /// Flag to prevent stack overflow.
        /// </summary>
        private bool _button_EnabledChanging;

        /// <summary>
        /// Handles the EnabledChanged event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Button_EnabledChanged(object sender, EventArgs e)
        {
            if (_button_EnabledChanging)
                return;

            _button_EnabledChanging = true;
            try
            {
                ((Button)sender).Enabled = false;
            }
            finally
            {
                _button_EnabledChanging = false;
            }
        }

        #endregion
    }
}
