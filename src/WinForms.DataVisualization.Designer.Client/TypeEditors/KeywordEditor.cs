// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	A keyword editor form. Allows the end user to insert
//				new and edit existing keywords in the string.
//


using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Summary description for KeywordEditor.
    /// </summary>
    internal partial class KeywordEditor : Form
    {
        #region Fields

        /// <summary>
        /// List of keywords that are applicable to the edited property
        /// </summary>
        private readonly ArrayList _applicableKeywords = new ArrayList();

        /// <summary>
        /// Keyword beign edited or empty if inserting a new one.
        /// </summary>
        internal string Keyword = string.Empty;

        /// <summary>
        /// Maximum number of supported Y values.
        /// </summary>
        private readonly int _maxYValueIndex = 9;

        // resolved VSTS by extending the dialog by 36x28 pixels.
        // 5767	FRA: ChartAPI: String "Format Sample:" is  truncated on the "Keywords Editor'	
        // 4383	DEU: VC/VB/VCS/VWD: ChartAPI: The string "If a chart type supports..." is truncated on the 'Keyword Editor' dialog.
        // 3524	DEU: VC/VB/VCS/VWD: ChartAPI: The string "If a chart type supports..." is truncated on the 'Keyword Editor' dialog.

        private const int widthDialogExtend = 80;
        private const int heightDialogExtend = 38;

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Default public constructor.
        /// </summary>
        public KeywordEditor()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            PrepareControlsLayout();
        }

        /// <summary>
        /// Form constructor.
        /// </summary>
        /// <param name="applicableKeywords">List of keywords that can be inserted.</param>
        /// <param name="keyword">Keyword that should be edited.</param>
        /// <param name="maxYValueIndex">Maximum number of Y Values supported.</param>
        public KeywordEditor(ArrayList applicableKeywords, string keyword, int maxYValueIndex) : this()
        {
            // Save input data
            this._applicableKeywords = applicableKeywords;
            this.Keyword = keyword;
            this._maxYValueIndex = maxYValueIndex;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion // Constructors

        #region Event Handlers

        /// <summary>
        /// Form loaded event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void KeywordEditor_Load(object sender, EventArgs e)
        {
            // Set restriction on the Y Value index editor
            if (this._maxYValueIndex >= 0 && this._maxYValueIndex < 10)
            {
                this._numericUpDownYValue.Maximum = this._maxYValueIndex;
            }

            this._numericUpDownYValue.Enabled = this._maxYValueIndex > 0;
            this._labelYValue.Enabled = this._maxYValueIndex > 0;

            // Set tooltip for custom format
            this._toolTip.SetToolTip(this._textBoxCustomFormat, SR.DescriptionToolTipCustomFormatCharacters);

            // Select format None
            this._comboBoxFormat.SelectedIndex = 0;

            // Fill list of applicable keywords
            if (this._applicableKeywords is not null)
            {
                foreach (KeywordInfo keywordInfo in this._applicableKeywords)
                {
                    this._listBoxKeywords.Items.Add(keywordInfo);
                }
            }

            // Check if keyword for editing was specified
            if (this.Keyword.Length == 0 || this._applicableKeywords is null)
            {
                this._listBoxKeywords.SelectedIndex = 0;
                this._comboBoxFormat.SelectedIndex = 0;
            }
            else
            {
                // Iterate through all keywords and find a match
                bool itemFound = false;
                foreach (KeywordInfo keywordInfo in this._applicableKeywords)
                {
                    // Iterate through all possible keyword names
                    string[] keywordNames = keywordInfo.GetKeywords();
                    foreach (string keywordName in keywordNames)
                    {
                        if (this.Keyword.StartsWith(keywordName, StringComparison.Ordinal))
                        {
                            // Select keyword in the list
                            this._listBoxKeywords.SelectedItem = keywordInfo;
                            int keywordLength = keywordName.Length;

                            // Check if keyword support multiple Y values
                            if (keywordInfo.SupportsValueIndex)
                            {
                                if (this.Keyword.Length > keywordLength &&
                                    this.Keyword[keywordLength] == 'Y')
                                {
                                    ++keywordLength;
                                    if (this.Keyword.Length > keywordLength &&
                                        char.IsDigit(this.Keyword[keywordLength]))
                                    {
                                        int yValueIndex = int.Parse(this.Keyword.Substring(keywordLength, 1), CultureInfo.InvariantCulture);
                                        if (yValueIndex < 0 || yValueIndex > this._maxYValueIndex)
                                        {
                                            yValueIndex = 0;
                                        }

                                        _numericUpDownYValue.Value = yValueIndex;
                                        ++keywordLength;
                                    }
                                }
                            }

                            // Check if keyword support format string
                            if (keywordInfo.SupportsFormatting)
                            {
                                if (this.Keyword.Length > keywordLength &&
                                    this.Keyword[keywordLength] == '{' &&
                                    this.Keyword.EndsWith("}", StringComparison.Ordinal))
                                {
                                    // Get format string
                                    string format = this.Keyword.Substring(keywordLength + 1, this.Keyword.Length - keywordLength - 2);

                                    if (format.Length == 0)
                                    {
                                        // Select format None
                                        this._comboBoxFormat.SelectedIndex = 0;
                                    }
                                    else
                                    {
                                        // Check if format string is custom
                                        if (format.Length == 1 ||
                                            (format.Length == 2 && char.IsDigit(format[1])) ||
                                            (format.Length == 3 && char.IsDigit(format[2])))
                                        {
                                            if (format[0] == 'C')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 1;
                                            }
                                            else if (format[0] == 'D')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 2;
                                            }
                                            else if (format[0] == 'E')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 3;
                                            }
                                            else if (format[0] == 'F')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 4;
                                            }
                                            else if (format[0] == 'G')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 5;
                                            }
                                            else if (format[0] == 'N')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 6;
                                            }
                                            else if (format[0] == 'P')
                                            {
                                                this._comboBoxFormat.SelectedIndex = 7;
                                            }
                                            else
                                            {
                                                // Custom format
                                                this._comboBoxFormat.SelectedIndex = 8;
                                                this._textBoxCustomFormat.Text = format;
                                            }

                                            // Get precision
                                            if (this._comboBoxFormat.SelectedIndex != 8 && format.Length > 0)
                                            {
                                                this._textBoxPrecision.Text = format.Substring(1);
                                            }
                                        }
                                        else
                                        {
                                            // Custom format
                                            this._comboBoxFormat.SelectedIndex = 8;
                                            this._textBoxCustomFormat.Text = format;
                                        }
                                    }
                                }
                            }

                            // Stop iteration
                            itemFound = true;
                            break;
                        }
                    }

                    // Break from the keywords loop
                    if (itemFound)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Selected format changed event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void comboBoxFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Format disabled
            _labelCustomFormat.Enabled = this._comboBoxFormat.SelectedIndex > 0;
            _textBoxCustomFormat.Enabled = this._comboBoxFormat.SelectedIndex > 0;
            _labelPrecision.Enabled = this._comboBoxFormat.SelectedIndex > 0;
            _textBoxPrecision.Enabled = this._comboBoxFormat.SelectedIndex > 0;
            _labelSample.Enabled = this._comboBoxFormat.SelectedIndex > 0;
            _textBoxSample.Enabled = this._comboBoxFormat.SelectedIndex > 0;

            // Hide show form control depending on the format selection
            bool customFormat = (string)_comboBoxFormat.SelectedItem == "Custom";
            _labelCustomFormat.Visible = customFormat;
            _textBoxCustomFormat.Visible = customFormat;
            _labelPrecision.Visible = !customFormat;
            _textBoxPrecision.Visible = !customFormat;

            // Update format sample
            this.UpdateNumericSample();
        }


        /// <summary>
        /// Selected keyword changed event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void listBoxKeywords_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get selected keyword
            KeywordInfo? keywordInfo = _listBoxKeywords.SelectedItem as KeywordInfo;
            if (keywordInfo is not null)
            {
                // Show description of the selected keyword
                this._labelDescription.Text = keywordInfo.Description.Replace("\\n", "\n");

                // Check if keyword support value formatting
                _groupBoxFormat.Enabled = keywordInfo.SupportsFormatting;

                // Check if keyword support Y value index
                _labelYValue.Enabled = keywordInfo.SupportsValueIndex;
                _numericUpDownYValue.Enabled = keywordInfo.SupportsValueIndex && this._maxYValueIndex > 0;
                this._labelYValue.Enabled = keywordInfo.SupportsValueIndex && this._maxYValueIndex > 0;
            }

        }

        /// <summary>
        /// Keyword double click event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void listBoxKeywords_DoubleClick(object sender, EventArgs e)
        {
            // Simulate accept button click when user double clicks in the list
            this.AcceptButton.PerformClick();
        }

        /// <summary>
        /// Precision text changed event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void textBoxPrecision_TextChanged(object sender, EventArgs e)
        {
            MessageBoxOptions messageBoxOptions = 0;
            if (RightToLeft == RightToLeft.Yes)
            {
                messageBoxOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
            }

            if (this._textBoxPrecision.Text.Length >= 1 && !char.IsDigit(this._textBoxPrecision.Text[0]))
            {
                MessageBox.Show(this, SR.MessagePrecisionInvalid, SR.MessageChartTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, messageBoxOptions);
                this._textBoxPrecision.Text = string.Empty;
            }
            else if (this._textBoxPrecision.Text.Length >= 2 && (!char.IsDigit(this._textBoxPrecision.Text[0]) || !char.IsDigit(this._textBoxPrecision.Text[1])))
            {
                MessageBox.Show(this, SR.MessagePrecisionInvalid, SR.MessageChartTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, messageBoxOptions);
                this._textBoxPrecision.Text = string.Empty;
            }

            this.UpdateNumericSample();
        }

        /// <summary>
        /// Custom format text changed event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void textBoxCustomFormat_TextChanged(object sender, EventArgs e)
        {
            this.UpdateNumericSample();
        }

        /// <summary>
        /// Ok button click event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Generate new keyword
            this.Keyword = string.Empty;

            // Get selected keyword
            if (this._listBoxKeywords.SelectedItem is KeywordInfo keywordInfo)
            {
                this.Keyword = keywordInfo.Keyword;

                if (keywordInfo.SupportsValueIndex &&
                    (int)_numericUpDownYValue.Value > 0)
                {
                    this.Keyword += "Y" + ((int)_numericUpDownYValue.Value).ToString(CultureInfo.InvariantCulture);
                }

                if (keywordInfo.SupportsFormatting &&
                    _comboBoxFormat.SelectedIndex > 0 &&
                    this.GetFormatString().Length > 0)
                {
                    this.Keyword += "{" + this.GetFormatString() + "}";
                }
            }
        }

        /// <summary>
        /// Y Value index changed event handler.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void numericUpDownYValue_ValueChanged(object sender, EventArgs e)
        {
            if (_numericUpDownYValue.Value > this._maxYValueIndex && _numericUpDownYValue.Value < 0)
            {
                MessageBoxOptions messageBoxOptions = 0;
                if (RightToLeft == RightToLeft.Yes)
                {
                    messageBoxOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
                }

                MessageBox.Show(this, SR.MessageYValueIndexInvalid(this._maxYValueIndex.ToString(CultureInfo.CurrentCulture)), SR.MessageChartTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, messageBoxOptions);

                _numericUpDownYValue.Value = 0;
            }
        }

        #endregion // Event Handlers

        #region Helper Methods

        /// <summary>
        /// Gets current format string
        /// </summary>
        /// <returns></returns>
        private string GetFormatString()
        {
            string formatString = string.Empty;
            if (this._comboBoxFormat.Enabled &&
                this._comboBoxFormat.SelectedIndex == 1)
            {
                formatString = "C" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 2)
            {
                formatString = "D" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 3)
            {
                formatString = "E" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 4)
            {
                formatString = "G" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 5)
            {
                formatString = "G" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 6)
            {
                formatString = "N" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 7)
            {
                formatString = "P" + _textBoxPrecision.Text;
            }
            else if (this._comboBoxFormat.SelectedIndex == 8)
            {
                formatString = this._textBoxCustomFormat.Text;
            }

            return formatString;
        }

        /// <summary>
        /// Updates numeric sample on the form.
        /// </summary>
        private void UpdateNumericSample()
        {
            string formatString = this.GetFormatString();
            if (this._comboBoxFormat.SelectedIndex == 0)
            {
                // No format
                _textBoxSample.Text = string.Empty;
            }
            else if (this._comboBoxFormat.SelectedIndex == 1)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.6789);
            }
            else if (this._comboBoxFormat.SelectedIndex == 2)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345);
            }
            else if (this._comboBoxFormat.SelectedIndex == 3)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.6789);
            }
            else if (this._comboBoxFormat.SelectedIndex == 4)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.6789);
            }
            else if (this._comboBoxFormat.SelectedIndex == 5)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.6789);
            }
            else if (this._comboBoxFormat.SelectedIndex == 6)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.6789);
            }
            else if (this._comboBoxFormat.SelectedIndex == 7)
            {
                _textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 0.126);
            }
            else if (this._comboBoxFormat.SelectedIndex == 8)
            {
                // Custom format
                bool success = false;
                try
                {
                    this._textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345.67890);
                    success = true;
                }
                catch (FormatException)
                {
                }

                if (!success)
                {
                    try
                    {
                        this._textBoxSample.Text = string.Format(CultureInfo.CurrentCulture, "{0:" + formatString + "}", 12345);
                        success = true;
                    }
                    catch (FormatException)
                    {
                    }
                }

                if (!success)
                {
                    this._textBoxSample.Text = SR.DesciptionCustomLabelFormatInvalid;
                }
            }
        }
        /// <summary>
        /// VSTS: 787936, 787937 - Expand the dialog with widthDialogExtend, heightDialogExtend  to make room for localization.
        /// </summary>
        private void PrepareControlsLayout()
        {

            this.Width += widthDialogExtend;
            this._buttonOk.Left += widthDialogExtend;
            this._buttonCancel.Left += widthDialogExtend;
            this._groupBoxDescription.Width += widthDialogExtend;
            this._groupBoxFormat.Width += widthDialogExtend;
            this._labelDescription.Width += widthDialogExtend;
            foreach (Control ctrl in this._groupBoxFormat.Controls)
            {
                if (ctrl is Label)
                    ctrl.Width += widthDialogExtend;
                else
                    ctrl.Left += widthDialogExtend;
            }

            this.Height += heightDialogExtend;
            this._buttonOk.Top += heightDialogExtend;
            this._buttonCancel.Top += heightDialogExtend;
            this._groupBoxKeywords.Height += heightDialogExtend;
            this._listBoxKeywords.IntegralHeight = false;
            this._listBoxKeywords.Height += heightDialogExtend;
            this._groupBoxDescription.Height += heightDialogExtend;
            this._labelDescription.Height += heightDialogExtend;
            this._groupBoxFormat.Top += heightDialogExtend;
        }

        #endregion // Helper Methods
    }
}