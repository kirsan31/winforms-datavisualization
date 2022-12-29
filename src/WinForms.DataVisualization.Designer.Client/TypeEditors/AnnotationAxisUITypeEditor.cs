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
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Design;

using Microsoft.DotNet.DesignTools.Client;
using Microsoft.DotNet.DesignTools.Client.Proxies;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// UI type editor for the annotation axes.
    /// </summary>
    internal class AnnotationAxisUITypeEditor : UITypeEditor
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

            // Check if we dealing with X or Y axis
            bool showXAxes;
            if (context.PropertyDescriptor.Name == "AxisY")
                showXAxes = false;
            else
                showXAxes = true;

            var client = provider.GetRequiredService<IDesignToolsClient>();
            var sender = client.Protocol.GetEndpoint<AnnotationAxisUITypeEditorEditValueEndpoint>().GetSender(client);
            var response = sender.SendRequest(new AnnotationAxisUITypeEditorEditValueRequest(context.Instance));
            if (response?.AxesByChartAreas is null)
                return value;

            // Create control for editing
            using AnnotationAxisNameTreeView control = new AnnotationAxisNameTreeView(edSvc, response.AxesByChartAreas, value, showXAxes);

            // Show drop down control
            edSvc.DropDownControl(control);

            // Get new enumeration value
            value = control.GetNewValue()!;

            return value;
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
    /// Annotation axes names tree view, which is used for the UI type editing.
    /// </summary>
    internal class AnnotationAxisNameTreeView : TreeView
    {
        #region Control fields

        // Annotation object to edit
        IReadOnlyList<ChartAreasAxesDPO> _axesByChartAreas;
        private object _axis;
        IWindowsFormsEditorService _edSvc;
        private bool _showXAxes = true;

        #endregion

        #region Control constructor

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="edSvc">Editor service.</param>
        /// <param name="annotation">Annotation to edit.</param>
        /// <param name="axis">Axis object.</param>
        /// <param name="showXAxes">Indicates if X or Y axis should be shown.</param>
        public AnnotationAxisNameTreeView(IWindowsFormsEditorService edSvc, IReadOnlyList<ChartAreasAxesDPO> axesByChartAreas, object axis, bool showXAxes)
        {
            // Set editable value
            this._axesByChartAreas = axesByChartAreas;
            this._axis = axis;
            this._edSvc = edSvc;
            this._showXAxes = showXAxes;

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

            // Get chart object
            if (_axesByChartAreas is not null)
            {
                // Loop through all chart areas
                foreach (var axA in _axesByChartAreas)
                {
                    if (axA is null || axA.Axes is null || axA.ChartAreaName is null)
                        continue;

                    TreeNode areaNode = this.Nodes.Add(axA.ChartAreaName);

                    // Loop through all axes
                    foreach (var curAxis in axA.Axes)
                    {
                        if (curAxis is not ObjectProxy proxy)
                            continue;

                        var axisName = proxy.GetPropertyValue<AxisName>("AxisName");
                        // Hide X or Y axes
                        if (axisName == AxisName.Y || axisName == AxisName.Y2)
                        {
                            if (_showXAxes)
                            {
                                continue;
                            }
                        }

                        if (axisName == AxisName.X || axisName == AxisName.X2)
                        {
                            if (!_showXAxes)
                            {
                                continue;
                            }
                        }

                        var name = proxy.GetPropertyValue<string>("Name");
                        // Create child node
                        TreeNode axisNode = areaNode.Nodes.Add(name);
                        axisNode.Tag = curAxis;

                        // Check if this node should be selected
                        if (_axis == curAxis)
                        {
                            areaNode.Expand();
                            this.SelectedNode = axisNode;
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
