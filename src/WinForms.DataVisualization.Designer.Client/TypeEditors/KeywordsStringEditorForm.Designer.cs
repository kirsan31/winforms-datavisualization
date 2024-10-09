// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editor for the strings that may contain
//				keywords. Form automatically retrieves the list of 
//				recognizable keywords from the chart keywords 
//				registry.
//


using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// String editor form that is used to edit properties that support keywords.
    /// </summary>
    internal partial class KeywordsStringEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container _components = null;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._richTextBox = new System.Windows.Forms.RichTextBox();
            this._groupBoxString = new System.Windows.Forms.GroupBox();
            this._buttonEdit = new System.Windows.Forms.Button();
            this._buttonInsert = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._labelDescription = new System.Windows.Forms.Label();
            this._panelOkCancelButtons = new System.Windows.Forms.Panel();
            this._panelTopContent = new System.Windows.Forms.Panel();
            this._panelInsertEditButtons = new System.Windows.Forms.Panel();
            this._groupBoxString.SuspendLayout();
            this._panelOkCancelButtons.SuspendLayout();
            this._panelTopContent.SuspendLayout();
            this._panelInsertEditButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this._richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._richTextBox.Location = new System.Drawing.Point(6, 19);
            this._richTextBox.Margin = new System.Windows.Forms.Padding(7);
            this._richTextBox.Name = "_richTextBox";
            this._richTextBox.Size = new System.Drawing.Size(488, 106);
            this._richTextBox.TabIndex = 0;
            this._richTextBox.WordWrap = false;
            this._richTextBox.SelectionChanged += new System.EventHandler(this.richTextBox_SelectionChanged);
            this._richTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox_KeyDown);
            this._richTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.richTextBox_KeyPress);
            this._richTextBox.TextChanged += new System.EventHandler(this.richTextBox_TextChanged);
            // 
            // groupBoxString
            // 
            this._groupBoxString.Controls.Add(this._panelInsertEditButtons);
            this._groupBoxString.Controls.Add(this._richTextBox);
            this._groupBoxString.Dock = System.Windows.Forms.DockStyle.Fill;
            this._groupBoxString.Location = new System.Drawing.Point(0, 56);
            this._groupBoxString.Name = "_groupBoxString";
            this._groupBoxString.Padding = new System.Windows.Forms.Padding(6);
            this._groupBoxString.Size = new System.Drawing.Size(500, 131);
            this._groupBoxString.TabIndex = 1;
            this._groupBoxString.TabStop = false;
            this._groupBoxString.Text = SR.LabelStringWithKeywords;
            // 
            // buttonEdit
            // 
            this._buttonEdit.Enabled = false;
            this._buttonEdit.Location = new System.Drawing.Point(30, 34);
            this._buttonEdit.Name = "_buttonEdit";
            this._buttonEdit.Size = new System.Drawing.Size(156, 27);
            this._buttonEdit.TabIndex = 2;
            this._buttonEdit.Text = SR.LabelEditKeyword;
            this._buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // buttonInsert
            // 
            this._buttonInsert.Location = new System.Drawing.Point(30, 2);
            this._buttonInsert.Name = "_buttonInsert";
            this._buttonInsert.Size = new System.Drawing.Size(156, 27);
            this._buttonInsert.TabIndex = 1;
            this._buttonInsert.Text = SR.LabelInsertNewKeyword;
            this._buttonInsert.Click += new System.EventHandler(this.buttonInsert_Click);
            // 
            // buttonOk
            // 
            this._buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Location = new System.Drawing.Point(305, 9);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(90, 27);
            this._buttonOk.TabIndex = 2;
            this._buttonOk.Text = SR.LabelButtonOk;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this._buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Location = new System.Drawing.Point(401, 9);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(90, 27);
            this._buttonCancel.TabIndex = 3;
            this._buttonCancel.Text = SR.LabelButtonCancel;
            // 
            // labelDescription
            // 
            this._labelDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this._labelDescription.Location = new System.Drawing.Point(0, 0);
            this._labelDescription.Name = "_labelDescription";
            this._labelDescription.Size = new System.Drawing.Size(500, 56);
            this._labelDescription.TabIndex = 0;
            this._labelDescription.Text = SR.DesciptionCustomLabelEditorTitle;
            // 
            // _panelOkCancelButtons
            // 
            this._panelOkCancelButtons.Controls.Add(this._buttonOk);
            this._panelOkCancelButtons.Controls.Add(this._buttonCancel);
            this._panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._panelOkCancelButtons.Location = new System.Drawing.Point(4, 191);
            this._panelOkCancelButtons.Name = "_panelOkCancelButtons";
            this._panelOkCancelButtons.Padding = new System.Windows.Forms.Padding(6);
            this._panelOkCancelButtons.Size = new System.Drawing.Size(500, 44);
            this._panelOkCancelButtons.TabIndex = 4;
            // 
            // _panelTopContent
            // 
            this._panelTopContent.Controls.Add(this._groupBoxString);
            this._panelTopContent.Controls.Add(this._labelDescription);
            this._panelTopContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panelTopContent.Location = new System.Drawing.Point(4, 4);
            this._panelTopContent.Name = "_panelTopContent";
            this._panelTopContent.Size = new System.Drawing.Size(500, 187);
            this._panelTopContent.TabIndex = 5;
            // 
            // _panelInsertEditButtons
            // 
            this._panelInsertEditButtons.Controls.Add(this._buttonInsert);
            this._panelInsertEditButtons.Controls.Add(this._buttonEdit);
            this._panelInsertEditButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this._panelInsertEditButtons.Location = new System.Drawing.Point(305, 19);
            this._panelInsertEditButtons.Name = "_panelInsertEditButtons";
            this._panelInsertEditButtons.Size = new System.Drawing.Size(189, 106);
            this._panelInsertEditButtons.TabIndex = 3;
            // 
            // KeywordsStringEditorForm
            // 
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(524, 275);
            this.Controls.Add(this._panelTopContent);
            this.Controls.Add(this._panelOkCancelButtons);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(524, 275);
            this.Padding = new System.Windows.Forms.Padding(4);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "KeywordsStringEditorForm";
            this.Text = SR.LabelStringKeywordsEditor;
            this.Load += new System.EventHandler(this.KeywordsStringEditorForm_Load);
            this._groupBoxString.ResumeLayout(false);
            this._panelOkCancelButtons.ResumeLayout(false);
            this._panelTopContent.ResumeLayout(false);
            this._panelInsertEditButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.RichTextBox _richTextBox;
        private System.Windows.Forms.GroupBox _groupBoxString;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonInsert;
        private System.Windows.Forms.Button _buttonEdit;
        private System.Windows.Forms.Label _labelDescription;
        private System.Windows.Forms.Panel _panelInsertEditButtons;
        private System.Windows.Forms.Panel _panelOkCancelButtons;
        private System.Windows.Forms.Panel _panelTopContent;
    }
}