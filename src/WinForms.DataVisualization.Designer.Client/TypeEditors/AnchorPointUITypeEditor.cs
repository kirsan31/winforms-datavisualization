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
    /// UI type editor for the annotation anchor point
    /// </summary>
    internal class AnchorPointUITypeEditor : System.Drawing.Design.UITypeEditor
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
        //            // Create control for editing
        //            AnchorPointNameTreeView control = new AnchorPointNameTreeView(
        //                edSvc,
        //                (Annotation)context.Instance,
        //                value as DataPoint);

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
    ///// Anchor data points name tree view, which is used for the UI type editing.
    ///// </summary>
    //internal class AnchorPointNameTreeView : TreeView
    //{
    //    #region Control fields

    //    // Annotation object to edit
    //    private Annotation _annotation;
    //    private DataPoint _dataPoint;
    //    IWindowsFormsEditorService _edSvc;

    //    #endregion

    //    #region Control constructor

    //    /// <summary>
    //    /// Public constructor.
    //    /// </summary>
    //    /// <param name="edSvc">Editor service.</param>
    //    /// <param name="annotation">Annotation to edit.</param>
    //    /// <param name="dataPoint">Annotation anchor data point to edit.</param>
    //    public AnchorPointNameTreeView(
    //        IWindowsFormsEditorService edSvc,
    //        Annotation annotation,
    //        DataPoint dataPoint)
    //    {
    //        // Set editable value
    //        this._annotation = annotation;
    //        this._dataPoint = dataPoint;
    //        this._edSvc = edSvc;

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

    //            // Loop through all series
    //            foreach (Series series in chart.Series)
    //            {
    //                TreeNode seriesNode = this.Nodes.Add(series.Name);
    //                seriesNode.Tag = series;

    //                // Indicate that there are no points in series
    //                if (series.Points.Count == 0)
    //                {
    //                    seriesNode.Nodes.Add("(empty)");
    //                }

    //                // Loop through all points
    //                int index = 1;
    //                foreach (DataPoint point in series.Points)
    //                {
    //                    TreeNode dataPointNode = seriesNode.Nodes.Add("DataPoint" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
    //                    dataPointNode.Tag = point;
    //                    ++index;

    //                    // Check if this node should be selected
    //                    if (point == _dataPoint)
    //                    {
    //                        seriesNode.Expand();
    //                        this.SelectedNode = dataPointNode;
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
    //    public DataPoint GetNewValue()
    //    {
    //        if (this.SelectedNode != null &&
    //            this.SelectedNode.Tag != null &&
    //            this.SelectedNode.Tag is DataPoint)
    //        {
    //            return (DataPoint)this.SelectedNode.Tag;
    //        }
    //        return null;
    //    }

    //    /// <summary>
    //    /// Mouse double clicked.
    //    /// </summary>
    //    protected override void OnDoubleClick(EventArgs e)
    //    {
    //        base.OnDoubleClick(e);
    //        if (this._edSvc != null)
    //        {
    //            if (GetNewValue() != null)
    //            {
    //                this._edSvc.CloseDropDown();
    //            }
    //            else if (this.SelectedNode != null &&
    //                this.SelectedNode.Text == "NotSet")
    //            {
    //                this._edSvc.CloseDropDown();
    //            }
    //        }
    //    }
    //    #endregion
    //}

}
