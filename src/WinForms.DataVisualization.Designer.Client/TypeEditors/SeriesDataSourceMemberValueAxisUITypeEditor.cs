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
using System.Drawing.Design;
using System.Windows.Forms;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// UI type editor for the Y data source members of the series.
    /// </summary>
    internal class SeriesDataSourceMemberValueAxisUITypeEditor : UITypeEditor
    {
		#region Editor methods and properties

        internal virtual SeriesDataSourceMemberYCheckedListBox GetDropDownControl(/*Chart chart,*/ ITypeDescriptorContext context, object value, bool flag)
        {
            return new SeriesDataSourceMemberYCheckedListBox(/*chart,*/ value, flag);
        }

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
        //	if (context != null && context.Instance != null && provider != null) 
        //	{
        //		IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
        //		if(edSvc != null) 
        //		{
        //                  Chart chart = ConverterHelper.GetChartFromContext(context);

        //			if(chart != null)
        //			{

        //				// Create control for editing
        //				SeriesDataSourceMemberYCheckedListBox control = this.GetDropDownControl(chart, context, value, true);

        //				// Show drop down control
        //				edSvc.DropDownControl(control);

        //				// Get new enumeration value
        //				value = control.GetNewValue();
        //			}
        //		}
        //	}

        //	return value;
        //}

        ///// <summary>
        ///// Gets editing style.
        ///// </summary>
        ///// <param name="context">Editing context.</param>
        ///// <returns>Editor style.</returns>
        //public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) 
        //{
        //	if (context != null && context.Instance != null) 
        //	{
        //		// Check how many Y values in the series.
        //		int	yValuesNumber = 1;
        //		if(context.Instance is Series)
        //		{
        //			yValuesNumber = ((Series)context.Instance).YValuesPerPoint;
        //		}
        //		else if(context.Instance is Array)
        //		{
        //			Array	array = (Array)context.Instance;
        //			if(array.Length > 0 && array.GetValue(0) is Series)
        //			{
        //				yValuesNumber = Math.Max(yValuesNumber, ((Series)array.GetValue(0)).YValuesPerPoint);
        //			}
        //		}

        //		return (yValuesNumber == 1) ? UITypeEditorEditStyle.None : UITypeEditorEditStyle.DropDown;
        //	}
        //	return base.GetEditStyle(context);
        //}

        #endregion
    }

    /// <summary>
    /// Checked list box, which is used for the series Y data source member UI type editing.
    /// </summary>
    internal class SeriesDataSourceMemberYCheckedListBox : CheckedListBox
    {
#warning designer
        // Chart object 
        //private Chart _chart;

        // Object to edit
        protected object editValue;

        // Indicates that editor was used for the Y values members
        protected bool usedForYValue;

        #region Control constructor

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="chart">Chart control.</param>
        /// 
        /// <param name="editValue">Value to edit.</param>
        /// <param name="usedForYValue">Indicates that editor was used for the Y values members.</param>
        public SeriesDataSourceMemberYCheckedListBox(/*Chart chart, */object editValue, bool usedForYValue)
        {
            // Set editable value
            this.editValue = editValue;
            this.usedForYValue = usedForYValue;

            // Set control border style
            this.BorderStyle = System.Windows.Forms.BorderStyle.None;

            this.IntegralHeight = false;
            // Fill member items list
            //this.FillList();

#warning designer
            // Set Chart
            //_chart = chart;
        }

        #endregion

        #region Control methods

        protected override void OnCreateControl()
        {
            this.FillList();
        }

        internal virtual ArrayList GetMemberNames()
        {
#warning designer
            //object dataSource = null;
            //if (ChartWinDesigner.controlDesigner != null)
            //{
            //    dataSource = ChartWinDesigner.controlDesigner.GetControlDataSource(_chart);
            //}

            //// Get list of members
            //if (dataSource != null)
            //{
            //    return ChartImage.GetDataSourceMemberNames(dataSource, this.usedForYValue);
            //}

            return new ArrayList();
        }

        /// <summary>
        /// Fills checked list items
        /// </summary>
        private void FillList()
        {
            // Create array of current names
            string[]? currentNames = null;
            if (editValue != null && editValue is string)
            {
                string editedString = (string)editValue;
                currentNames = editedString.Split(',');
            }

            ArrayList memberNames = this.GetMemberNames();

            // Fill list with all possible values in the enumeration
            foreach (string name in memberNames)
            {
                // Test if item should be checked by default
                bool isChecked = false;
                if (currentNames != null)
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
            string result = String.Empty;
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