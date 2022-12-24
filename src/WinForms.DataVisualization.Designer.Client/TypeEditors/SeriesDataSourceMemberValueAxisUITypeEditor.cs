// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editors and converter classes for the 
//				Series and DataPoint properties.
//


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Microsoft.DotNet.DesignTools.Client;
using Microsoft.DotNet.DesignTools.Client.Proxies;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// UI type editor for the Y data source members of the series.
    /// </summary>
    internal class SeriesDataSourceMemberValueAxisUITypeEditor : UITypeEditor
    {
        #region Editor methods and properties
        private int _maxItemCheck;

        /// <summary>
        /// Display a drop down list with check boxes.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <param name="provider">Provider.</param>
        /// <param name="value">Value to edit.</param>
        /// <returns>Result</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context?.Instance is not null && provider is not null)
            {
                if (provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                    return value;

                var client = provider.GetRequiredService<IDesignToolsClient>();
                var sender = client.Protocol.GetEndpoint<SeriesDSMemberValueAxisEditorEditValueEndpoint>().GetSender(client);
                var response = sender.SendRequest(new SeriesDSMemberValueAxisEditorEditValueRequest(context.Instance));
                if (response?.DSMemberNames is null)
                    return value;

                // Create control for editing
                SeriesDataSourceMemberYCheckedListBox control = new SeriesDataSourceMemberYCheckedListBox(response.DSMemberNames.Select(m => m?.String).ToList().AsReadOnly(), _maxItemCheck, value);

                // Show drop down control
                edSvc.DropDownControl(control);

                // Get new enumeration value
                value = control.GetNewValue();
            }

            return value;
        }

        /// <summary>
        /// Gets editing style.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <returns>Editor style.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context?.Instance is null)
                return base.GetEditStyle(context);

            // Check how many Y values in the series.
            int yValuesNumber = 1;
            object instanse = context.Instance;

            if (instanse is Array array)
            {
                if (array.Length > 0)
                    instanse = array.GetValue(0);
            }

            if (instanse is ObjectProxy objectProxy)
                yValuesNumber = objectProxy.GetPropertyValue<int>("YValuesPerPoint");

            _maxItemCheck = yValuesNumber;
            return yValuesNumber < 2 ? UITypeEditorEditStyle.None : UITypeEditorEditStyle.DropDown;
        }

        #endregion
    }

    /// <summary>
    /// Checked list box, which is used for the series Y data source member UI type editing.
    /// </summary>
    internal class SeriesDataSourceMemberYCheckedListBox : CheckedListBox
    {
        private readonly object _editValue;
        private readonly IReadOnlyList<string?> _memberNames;
        private readonly int _maxItemCheck;

        #region Control constructor

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="memberNames">The member names.</param>
        /// <param name="maxIttemCheck">The maximum items to check.</param>
        /// <param name="editValue">Value to edit.</param>
        public SeriesDataSourceMemberYCheckedListBox(IReadOnlyList<string?> memberNames, int maxItemCheck, object editValue)
        {
            // Set editable value
            this._editValue = editValue;
            this._memberNames = memberNames ?? new List<string?>(0).AsReadOnly();
            // Set control border style
            this.BorderStyle = BorderStyle.None;
            this.IntegralHeight = false;
            _maxItemCheck = maxItemCheck;
        }

        #endregion

        #region Control methods

        protected override void OnCreateControl()
        {
            // Fill member items list
            this.FillList();
            base.OnCreateControl();
        }

        protected override void OnItemCheck(ItemCheckEventArgs ice)
        {
            if (ice.NewValue == CheckState.Checked && CheckedIndices.Count >= _maxItemCheck)
            {
                ice.NewValue = CheckState.Unchecked;
                return;
            }

            base.OnItemCheck(ice);
        }

        /// <summary>
        /// Fills checked list items
        /// </summary>
        private void FillList()
        {
            // Create array of current names
            string[]? currentNames = null;
            if (_editValue is string editedString)
            {
                currentNames = editedString.Split(',');
            }

            // Fill list with all possible values in the enumeration
            foreach (string? name in _memberNames)
            {
                if (name is null)
                    continue;

                // Test if item should be checked by default
                bool isChecked = false;
                if (currentNames is not null)
                {
                    foreach (string curName in currentNames)
                    {
                        if (name == curName.Trim())
                        {
                            isChecked = true;
                        }
                    }
                }

                // Add items into the list
                this.Items.Add(name, isChecked);
            }
        }

        /// <summary>
        /// Gets new enumeration value.
        /// </summary>
        /// <returns>New enum value.</returns>
        public string GetNewValue()
        {
            // Update enumeration flags
            string result = string.Empty;
            foreach (object checkedItem in this.CheckedItems)
            {
                if (result.Length > 0)
                {
                    result += ", ";
                }
                result += (string)checkedItem;
            }

            // Return value
            return result;
        }

        #endregion
    }
}