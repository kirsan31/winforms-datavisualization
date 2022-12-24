// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time UI editor for Annotations.
//


using System.ComponentModel;
using System.Drawing.Design;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// UI type editor for the annotation axes.
    /// </summary>
    internal class AnnotationAxisUITypeEditor : System.Drawing.Design.UITypeEditor
    {
        #region Editor methods and properties

#warning designer
        ///// <summary>
        ///// Display a drop down list with check boxes.
        ///// </summary>
        ///// <param name="context">Editing context.</param>
        ///// <param name="provider">Provider.</param>
        ///// <param name="value">Value to edit.</param>
        ///// <returns>Result</returns>
        //public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        //{
        //    if (context != null && context.Instance != null && provider != null)
        //    {
        //        IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
        //        if (edSvc != null &&
        //            context.Instance is Annotation)
        //        {
        //            // Check if we dealing with X or Y axis
        //            bool showXAxes = true;
        //            if (context.PropertyDescriptor != null &&
        //                context.PropertyDescriptor.Name == "AxisY")
        //            {
        //                showXAxes = false;
        //            }

        //            // Create control for editing
        //            AnnotationAxisNameTreeView control = new AnnotationAxisNameTreeView(
        //                edSvc,
        //                (Annotation)context.Instance,
        //                value as Axis,
        //                showXAxes);

        //            // Show drop down control
        //            edSvc.DropDownControl(control);

        //            // Get new enumeration value
        //            value = control.GetNewValue();

        //            // Dispose control
        //            control.Dispose();
        //        }
        //    }

        //    return value;
        //}


        /// <summary>
        /// Gets editing style.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <returns>Editor style.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.Instance != null)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            return base.GetEditStyle(context);
        }

        #endregion
    }

#warning designer
    ///// <summary>
    ///// Annotation axes names tree view, which is used for the UI type editing.
    ///// </summary>
    //internal class AnnotationAxisNameTreeView : TreeView
    //{
    //    #region Control fields

    //    // Annotation object to edit
    //    private Annotation _annotation;
    //    private Axis _axis;
    //    IWindowsFormsEditorService _edSvc;
    //    private bool _showXAxes = true;

    //    #endregion

    //    #region Control constructor

    //    /// <summary>
    //    /// Public constructor.
    //    /// </summary>
    //    /// <param name="edSvc">Editor service.</param>
    //    /// <param name="annotation">Annotation to edit.</param>
    //    /// <param name="axis">Axis object.</param>
    //    /// <param name="showXAxes">Indicates if X or Y axis should be shown.</param>
    //    public AnnotationAxisNameTreeView(
    //        IWindowsFormsEditorService edSvc,
    //        Annotation annotation,
    //        Axis axis,
    //        bool showXAxes)
    //    {
    //        // Set editable value
    //        this._annotation = annotation;
    //        this._axis = axis;
    //        this._edSvc = edSvc;
    //        this._showXAxes = showXAxes;

    //        // Set control border style
    //        this.BorderStyle = BorderStyle.None;

    //        // Fill tree with data point names
    //        this.FillTree();
    //    }

    //    #endregion

    //    #region Control methods

    //    /// <summary>
    //    /// Fills data points name tree.
    //    /// </summary>
    //    private void FillTree()
    //    {
    //        bool nodeSelected = false;
    //        this.BeginUpdate();

    //        // Add "None" option
    //        TreeNode noPoint = this.Nodes.Add("NotSet");

    //        // Get chart object
    //        if (this._annotation != null &&
    //            _annotation.AnnotationGroup == null &&
    //            this._annotation.Chart != null)
    //        {
    //            Chart chart = this._annotation.Chart;

    //            // Loop through all chart areas
    //            foreach (ChartArea chartArea in chart.ChartAreas)
    //            {
    //                TreeNode areaNode = this.Nodes.Add(chartArea.Name);
    //                areaNode.Tag = chartArea;

    //                // Loop through all axes
    //                foreach (Axis curAxis in chartArea.Axes)
    //                {
    //                    // Hide X or Y axes
    //                    if (curAxis.AxisName == AxisName.Y || curAxis.AxisName == AxisName.Y2)
    //                    {
    //                        if (_showXAxes)
    //                        {
    //                            continue;
    //                        }
    //                    }
    //                    if (curAxis.AxisName == AxisName.X || curAxis.AxisName == AxisName.X2)
    //                    {
    //                        if (!_showXAxes)
    //                        {
    //                            continue;
    //                        }
    //                    }

    //                    // Create child node
    //                    TreeNode axisNode = areaNode.Nodes.Add(curAxis.Name);
    //                    axisNode.Tag = curAxis;

    //                    // Check if this node should be selected
    //                    if (_axis == curAxis)
    //                    {
    //                        areaNode.Expand();
    //                        this.SelectedNode = axisNode;
    //                        nodeSelected = true;
    //                    }
    //                }
    //            }
    //        }

    //        // Select default node
    //        if (!nodeSelected)
    //        {
    //            this.SelectedNode = noPoint;
    //        }

    //        this.EndUpdate();
    //    }

    //    /// <summary>
    //    /// Gets new data point.
    //    /// </summary>
    //    /// <returns>New enum value.</returns>
    //    public Axis GetNewValue()
    //    {
    //        if (this.SelectedNode is not null && this.SelectedNode.Tag is not null && this.SelectedNode.Tag is Axis)
    //        {
    //            return (Axis)this.SelectedNode.Tag;
    //        }

    //        return null;
    //    }

    //    /// <summary>
    //    /// Mouse double clicked.
    //    /// </summary>
    //    protected override void OnDoubleClick(EventArgs e)
    //    {
    //        base.OnDoubleClick(e);
    //        if (this._edSvc is not null)
    //        {
    //            if (GetNewValue() is not null)
    //            {
    //                this._edSvc.CloseDropDown();
    //            }
    //            else if (this.SelectedNode is not null && this.SelectedNode.Text == "NotSet")
    //            {
    //                this._edSvc.CloseDropDown();
    //            }
    //        }
    //    }
    //    #endregion
    //}
}
