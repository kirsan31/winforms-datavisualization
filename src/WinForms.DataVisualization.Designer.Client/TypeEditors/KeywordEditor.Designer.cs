// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	A keyword editor form. Allows the end user to insert
//				new and edit existing keywords in the string.
//


using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Summary description for KeywordEditor.
    /// </summary>
    internal partial class KeywordEditor
    {
        private System.ComponentModel.IContainer _components;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._components = new System.ComponentModel.Container();
            this._groupBoxKeywords = new System.Windows.Forms.GroupBox();
            this._listBoxKeywords = new System.Windows.Forms.ListBox();
            this._groupBoxDescription = new System.Windows.Forms.GroupBox();
            this._labelDescription = new System.Windows.Forms.Label();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._groupBoxFormat = new System.Windows.Forms.GroupBox();
            this._textBoxPrecision = new System.Windows.Forms.TextBox();
            this._labelSample = new System.Windows.Forms.Label();
            this._textBoxSample = new System.Windows.Forms.TextBox();
            this._numericUpDownYValue = new System.Windows.Forms.NumericUpDown();
            this._labelYValue = new System.Windows.Forms.Label();
            this._comboBoxFormat = new System.Windows.Forms.ComboBox();
            this._labelPrecision = new System.Windows.Forms.Label();
            this._labelFormat = new System.Windows.Forms.Label();
            this._labelCustomFormat = new System.Windows.Forms.Label();
            this._textBoxCustomFormat = new System.Windows.Forms.TextBox();
            this._toolTip = new System.Windows.Forms.ToolTip(this._components);
            this._groupBoxKeywords.SuspendLayout();
            this._groupBoxDescription.SuspendLayout();
            this._groupBoxFormat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this._numericUpDownYValue).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxKeywords
            // 
            this._groupBoxKeywords.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                           this._listBoxKeywords});
            this._groupBoxKeywords.Location = new System.Drawing.Point(8, 16);
            this._groupBoxKeywords.Name = "groupBoxKeywords";
            this._groupBoxKeywords.Size = new System.Drawing.Size(216, 232);
            this._groupBoxKeywords.TabIndex = 0;
            this._groupBoxKeywords.TabStop = false;
            this._groupBoxKeywords.Text = SR.LabelKeyKeywords;
            // 
            // listBoxKeywords
            // 
            this._listBoxKeywords.Location = new System.Drawing.Point(8, 24);
            this._listBoxKeywords.Name = "listBoxKeywords";
            this._listBoxKeywords.Size = new System.Drawing.Size(200, 199);
            this._listBoxKeywords.TabIndex = 0;
            this._listBoxKeywords.DoubleClick += new System.EventHandler(this.listBoxKeywords_DoubleClick);
            this._listBoxKeywords.SelectedIndexChanged += new System.EventHandler(this.listBoxKeywords_SelectedIndexChanged);
            // 
            // groupBoxDescription
            // 
            this._groupBoxDescription.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                              this._labelDescription});
            this._groupBoxDescription.Location = new System.Drawing.Point(240, 16);
            this._groupBoxDescription.Name = "groupBoxDescription";
            this._groupBoxDescription.Size = new System.Drawing.Size(328, 88);
            this._groupBoxDescription.TabIndex = 1;
            this._groupBoxDescription.TabStop = false;
            this._groupBoxDescription.Text = SR.LabelDescription;
            // 
            // labelDescription
            // 
            this._labelDescription.Location = new System.Drawing.Point(16, 24);
            this._labelDescription.Name = "labelDescription";
            this._labelDescription.Size = new System.Drawing.Size(304, 56);
            this._labelDescription.TabIndex = 0;
            this._labelDescription.Text = "<replaced at runtime>";
            // 
            // buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(479, 256);
            this._buttonCancel.Name = "buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(90, 27);
            this._buttonCancel.TabIndex = 4;
            this._buttonCancel.Text = SR.LabelButtonCancel;
            // 
            // buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Location = new System.Drawing.Point(367, 256);
            this._buttonOk.Name = "buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(90, 27);
            this._buttonOk.TabIndex = 3;
            this._buttonOk.Text = SR.LabelButtonOk;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // groupBoxFormat
            // 
            this._groupBoxFormat.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                                         this._textBoxPrecision,
                                                                                         this._labelSample,
                                                                                         this._textBoxSample,
                                                                                         this._numericUpDownYValue,
                                                                                         this._labelYValue,
                                                                                         this._comboBoxFormat,
                                                                                         this._labelPrecision,
                                                                                         this._labelFormat,
                                                                                         this._labelCustomFormat,
                                                                                         this._textBoxCustomFormat});
            this._groupBoxFormat.Location = new System.Drawing.Point(240, 112);
            this._groupBoxFormat.Name = "groupBoxFormat";
            this._groupBoxFormat.Size = new System.Drawing.Size(328, 136);
            this._groupBoxFormat.TabIndex = 2;
            this._groupBoxFormat.TabStop = false;
            this._groupBoxFormat.Text = SR.LabelValueFormatting;
            // 
            // textBoxPrecision
            // 
            this._textBoxPrecision.Location = new System.Drawing.Point(112, 48);
            this._textBoxPrecision.Name = "textBoxPrecision";
            this._textBoxPrecision.Size = new System.Drawing.Size(64, 20);
            this._textBoxPrecision.TabIndex = 3;
            this._textBoxPrecision.Text = "";
            this._textBoxPrecision.TextChanged += new System.EventHandler(this.textBoxPrecision_TextChanged);
            // 
            // labelSample
            // 
            this._labelSample.Location = new System.Drawing.Point(8, 72);
            this._labelSample.Name = "labelSample";
            this._labelSample.Size = new System.Drawing.Size(96, 23);
            this._labelSample.TabIndex = 7;
            this._labelSample.Text = SR.LabelFormatKeySample;
            this._labelSample.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxSample
            // 
            this._textBoxSample.Location = new System.Drawing.Point(112, 72);
            this._textBoxSample.Name = "textBoxSample";
            this._textBoxSample.ReadOnly = true;
            this._textBoxSample.Size = new System.Drawing.Size(192, 20);
            this._textBoxSample.TabIndex = 8;
            this._textBoxSample.Text = "";
            // 
            // numericUpDownYValue
            // 
            this._numericUpDownYValue.CausesValidation = false;
            this._numericUpDownYValue.Location = new System.Drawing.Point(112, 104);
            this._numericUpDownYValue.Maximum = new System.Decimal(new int[] {
                                                                                9,
                                                                                0,
                                                                                0,
                                                                                0});
            this._numericUpDownYValue.Name = "numericUpDownYValue";
            this._numericUpDownYValue.Size = new System.Drawing.Size(64, 20);
            this._numericUpDownYValue.TabIndex = 10;
            this._numericUpDownYValue.ValueChanged += new System.EventHandler(this.numericUpDownYValue_ValueChanged);
            // 
            // labelYValue
            // 
            this._labelYValue.Location = new System.Drawing.Point(8, 104);
            this._labelYValue.Name = "labelYValue";
            this._labelYValue.Size = new System.Drawing.Size(96, 23);
            this._labelYValue.TabIndex = 9;
            this._labelYValue.Text = SR.LabelKeyYValueIndex;
            this._labelYValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboBoxFormat
            // 
            this._comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._comboBoxFormat.Items.AddRange(new object[] {
                                                                SR.DescriptionTypeNone,
                                                                SR.DescriptionNumberFormatTypeCurrency,
                                                                SR.DescriptionNumberFormatTypeDecimal,
                                                                SR.DescriptionNumberFormatTypeScientific,
                                                                SR.DescriptionNumberFormatTypeFixedPoint,
                                                                SR.DescriptionNumberFormatTypeGeneral,
                                                                SR.DescriptionNumberFormatTypeNumber,
                                                                SR.DescriptionNumberFormatTypePercent,
                                                                SR.DescriptionTypeCustom});

            this._comboBoxFormat.Location = new System.Drawing.Point(112, 24);
            this._comboBoxFormat.MaxDropDownItems = 10;
            this._comboBoxFormat.Name = "comboBoxFormat";
            this._comboBoxFormat.Size = new System.Drawing.Size(192, 21);
            this._comboBoxFormat.TabIndex = 1;
            this._comboBoxFormat.SelectedIndexChanged += new System.EventHandler(this.comboBoxFormat_SelectedIndexChanged);
            // 
            // labelPrecision
            // 
            this._labelPrecision.Location = new System.Drawing.Point(8, 48);
            this._labelPrecision.Name = "labelPrecision";
            this._labelPrecision.Size = new System.Drawing.Size(96, 23);
            this._labelPrecision.TabIndex = 2;
            this._labelPrecision.Text = SR.LabelKeyPrecision;
            this._labelPrecision.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelFormat
            // 
            this._labelFormat.Location = new System.Drawing.Point(8, 24);
            this._labelFormat.Name = "labelFormat";
            this._labelFormat.Size = new System.Drawing.Size(96, 23);
            this._labelFormat.TabIndex = 0;
            this._labelFormat.Text = SR.LabelKeyFormat;
            this._labelFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelCustomFormat
            // 
            this._labelCustomFormat.Location = new System.Drawing.Point(8, 48);
            this._labelCustomFormat.Name = "labelCustomFormat";
            this._labelCustomFormat.Size = new System.Drawing.Size(96, 23);
            this._labelCustomFormat.TabIndex = 4;
            this._labelCustomFormat.Text = SR.LabelKeyCustomFormat;
            this._labelCustomFormat.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this._labelCustomFormat.Visible = false;
            // 
            // textBoxCustomFormat
            // 
            this._textBoxCustomFormat.Location = new System.Drawing.Point(112, 48);
            this._textBoxCustomFormat.Name = "textBoxCustomFormat";
            this._textBoxCustomFormat.Size = new System.Drawing.Size(192, 20);
            this._textBoxCustomFormat.TabIndex = 5;
            this._textBoxCustomFormat.Text = "";
            this._textBoxCustomFormat.Visible = false;
            this._textBoxCustomFormat.TextChanged += new System.EventHandler(this.textBoxCustomFormat_TextChanged);
            // 
            // KeywordEditor
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(578, 295);
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                                                                          this._groupBoxFormat,
                                                                          this._buttonCancel,
                                                                          this._buttonOk,
                                                                          this._groupBoxDescription,
                                                                          this._groupBoxKeywords});
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeywordEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = SR.LabelKeywordEditor;
            this.Load += new System.EventHandler(this.KeywordEditor_Load);
            this._groupBoxKeywords.ResumeLayout(false);
            this._groupBoxDescription.ResumeLayout(false);
            this._groupBoxFormat.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this._numericUpDownYValue).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private GroupBox _groupBoxKeywords;
        private ListBox _listBoxKeywords;
        private GroupBox _groupBoxDescription;
        private Label _labelDescription;
        private Button _buttonCancel;
        private Button _buttonOk;
        private GroupBox _groupBoxFormat;
        private Label _labelFormat;
        private NumericUpDown _numericUpDownYValue;
        private Label _labelYValue;
        private ComboBox _comboBoxFormat;
        private Label _labelPrecision;
        private TextBox _textBoxCustomFormat;
        private Label _labelCustomFormat;
        private Label _labelSample;
        private TextBox _textBoxSample;
        private TextBox _textBoxPrecision;
        private ToolTip _toolTip;
    }
}