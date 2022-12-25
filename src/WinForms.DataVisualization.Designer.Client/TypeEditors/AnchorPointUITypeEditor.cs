// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time UI editor for Annotations.
//


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Microsoft.DotNet.DesignTools.Client;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// UI type editor for the annotation anchor point
    /// </summary>
    internal class AnchorPointUITypeEditor : UITypeEditor
    {
        #region Editor methods and properties

        /// <summary>
        /// Display a drop down list with check boxes.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <param name="provider">Provider.</param>
        /// <param name="value">Value to edit.</param>
        /// <returns>Result</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context?.Instance is null || provider?.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var client = provider.GetRequiredService<IDesignToolsClient>();
            var sender = client.Protocol.GetEndpoint<AnchorPointUITypeEditorEditValueEndpoint>().GetSender(client);
            var response = sender.SendRequest(new AnchorPointUITypeEditorEditValueRequest(context.Instance));
            if (response?.DataPointsBySeries is null)
                return value;

            // Create control for editing
            using AnchorPointNameTreeView control = new AnchorPointNameTreeView(edSvc, response.DataPointsBySeries, value);

            // Show drop down control
            edSvc.DropDownControl(control);

            // Get new enumeration value
            value = control.GetNewValue()!;
            return value!;
        }


        /// <summary>
        /// Gets editing style.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <returns>Editor style.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context?.Instance is not null)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            return base.GetEditStyle(context);
        }

        #endregion
    }

    /// <summary>
    /// Anchor data points name tree view, which is used for the UI type editing.
    /// </summary>
    internal class AnchorPointNameTreeView : TreeView
    {
        #region Control fields

        // Annotation object to edit
        private object _dataPoint;
        IWindowsFormsEditorService _edSvc;
        IReadOnlyList<SeriesDataPointDPO> _dataPointsBySeries;

        #endregion

        #region Control constructor

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="edSvc">Editor service.</param>
        /// <param name="annotation">Annotation to edit.</param>
        /// <param name="dataPoint">Annotation anchor data point to edit.</param>
        public AnchorPointNameTreeView(IWindowsFormsEditorService edSvc, IReadOnlyList<SeriesDataPointDPO> dataPointsBySeries, object dataPoint)
        {
            // Set editable value
            this._dataPoint = dataPoint;
            this._edSvc = edSvc;
            this._dataPointsBySeries = dataPointsBySeries;

            // Set control border style
            this.BorderStyle = BorderStyle.None;

            // Fill tree with data point names
            this.FillTree();
        }

        #endregion

        #region Control methods

        /// <summary>
        /// Fills data points name tree.
        /// </summary>
        private void FillTree()
        {
            bool nodeSelected = false;
            this.BeginUpdate();

            // Add "None" option
            TreeNode noPoint = this.Nodes.Add("NotSet");

            if (_dataPointsBySeries is not null)
            {
                // Loop through all series
                foreach (var dpS in _dataPointsBySeries)
                {
                    if (dpS is null || dpS.DataPoints is null || dpS.SeriesName is null)
                        continue;

                    TreeNode seriesNode = this.Nodes.Add(dpS.SeriesName);

                    // Indicate that there are no points in series
                    if (dpS.DataPoints.Count == 0)
                    {
                        seriesNode.Nodes.Add("(empty)");
                    }

                    // Loop through all points
                    int index = 1;
                    foreach (var point in dpS.DataPoints)
                    {
                        TreeNode dataPointNode = seriesNode.Nodes.Add("DataPoint" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        dataPointNode.Tag = point;
                        ++index;

                        // Check if this node should be selected
                        if (point == _dataPoint)
                        {
                            seriesNode.Expand();
                            this.SelectedNode = dataPointNode;
                            nodeSelected = true;
                        }
                    }
                }
            }

            // Select default node
            if (!nodeSelected)
            {
                this.SelectedNode = noPoint;
            }

            this.EndUpdate();
        }

        /// <summary>
        /// Gets new data point.
        /// </summary>
        /// <returns>New enum value.</returns>
        public object? GetNewValue()
        {
            return this.SelectedNode?.Tag;
        }

        /// <summary>
        /// Mouse double clicked.
        /// </summary>
        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
            if (this._edSvc is not null)
            {
                if (GetNewValue() is not null)
                {
                    this._edSvc.CloseDropDown();
                }
                else if (this.SelectedNode?.Text == "NotSet")
                {
                    this._edSvc.CloseDropDown();
                }
            }
        }
        #endregion
    }
}
