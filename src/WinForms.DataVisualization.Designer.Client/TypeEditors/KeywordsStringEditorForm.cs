// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editor for the strings that may contain
//				keywords. Form automatically retrieves the list of 
//				recognizable keywords from the chart keywords 
//				registry.
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace WinForms.DataVisualization.Designer.Client;

/// <summary>
/// String editor form that is used to edit properties that support keywords.
/// </summary>
internal partial class KeywordsStringEditorForm : Form
{
    #region Fields

    /// <summary>
    /// Property name that is being edited.
    /// </summary>
    private readonly string _propertyName = string.Empty;

    /// <summary>
    /// Object/class name being edited.
    /// </summary>
    private readonly string _classTypeName = string.Empty;

    /// <summary>
    /// Initial string to be edited.
    /// </summary>
    private readonly string _initialString = string.Empty;

    /// <summary>
    /// Result String after editing.
    /// </summary>
    public string ResultString = string.Empty;

    /// <summary>
    /// Maximum Y value index that can be used
    /// </summary>
    private readonly int _maxYValueIndex = 9;

    /// <summary>
    /// List of applicable keywords
    /// </summary>
    internal ArrayList applicableKeywords = new ArrayList();

    /// <summary>
    /// Reference to the keywords registry
    /// </summary>
    internal IReadOnlyList<KeywordInfo>? registeredKeywords;

    /// <summary>
    /// Name of the last selected keyword name
    /// </summary>
    private string _selectedKeywordName = string.Empty;

    /// <summary>
    /// Start index of selected keyword.
    /// </summary>
    private int _selectedKeywordStart = -1;

    /// <summary>
    /// Length of selected keyword.
    /// </summary>
    private int _selectedKeywordLength = -1;

    /// <summary>
    /// Indicates that RTF control is updating its text.
    /// Used to prevent recursive calls.
    /// </summary>
    private bool _updating;

    // resolved VSTS by extending the dialog by 36x28 pixels.
    // 5027	 MultiLang: ChartAPI: Strings are truncated on the 'String Keywords Editor' dialog
    // 65162 Garbled characters in the String Keyword Editor on the designer
    // 16588 DEU and JPN: VCS/VB/VWD/VC: ChartAPI: Some string are truncated on the 'String Keywords Editor'
    // 3523  DEU and JPN: VCS/VB/VWD/VC: ChartAPI: Some string are truncated on the 'String Keywords Editor'

    private const int widthDialogExtend = 80;
    private const int heightDialogExtend = 38;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Default public constructor
    /// </summary>
    public KeywordsStringEditorForm()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        PrepareControlsLayout();
    }

    /// <summary>
    /// Object constructor.
    /// </summary>
    /// <param name="initialString">String to edit.</param>
    /// <param name="classTypeName">Class name that being edited.</param>
    /// <param name="propertyName">Property name that is being edited.</param>
    /// <param name="maxYValueIndex">Maximum number of supported Y values.</param>
    public KeywordsStringEditorForm(string initialString, string classTypeName, string propertyName, int maxYValueIndex) : this()
    {

        // Save input parameters
        this._classTypeName = classTypeName;
        this._propertyName = propertyName;
        this._maxYValueIndex = maxYValueIndex;
        this._initialString = initialString;
        this.ResultString = initialString;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">True if disposing.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion // Constructor

    #region Event Handlers

    /// <summary>
    /// Form loaded event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void KeywordsStringEditorForm_Load(object sender, EventArgs e)
    {
        // Insert new line characters in the text
        this._labelDescription.Text = this._labelDescription.Text.Replace("\\n", "\n");

        // Load list of keywords applicable for the specified object and property.
        this.applicableKeywords = this.GetApplicableKeywords();
        if (this.applicableKeywords.Count == 0)
        {
            this._buttonInsert.Enabled = false;
            this._buttonEdit.Enabled = false;
        }

        if (!string.IsNullOrEmpty(this._initialString))
        {
            // Set text to edit
            this._richTextBox.Rtf = this.GetRtfText(this._initialString);
        }
    }

    /// <summary>
    /// Insert keyword button clicked event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void buttonInsert_Click(object sender, EventArgs e)
    {
        // Show keyword editor form
        using KeywordEditor keywordEditor = new KeywordEditor(
            this.applicableKeywords,
            string.Empty,
            this._maxYValueIndex);
        if (keywordEditor.ShowDialog() == DialogResult.OK)
        {
            if (_selectedKeywordLength > 0)
            {
                // Insert keyword at the end of curently selected keyword 
                // and separate them with space
                this._richTextBox.SelectionStart += this._richTextBox.SelectionLength;
                this._richTextBox.SelectionLength = 0;
                this._richTextBox.SelectedText = " " + keywordEditor.Keyword;
            }
            else
            {
                // Insert new keyword at current location
                this._richTextBox.SelectionLength = Math.Max(0, this._selectedKeywordLength);
                this._richTextBox.SelectedText = keywordEditor.Keyword;
            }
        }

        // Set focus back to the editor
        this._richTextBox.Focus();
    }

    /// <summary>
    /// Edit keyword button clicked event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void buttonEdit_Click(object sender, EventArgs e)
    {
        // Get selected keyword
        string keyword = this._richTextBox.Text.Substring(this._selectedKeywordStart, this._selectedKeywordLength);

        // Show keyword editor form
        using KeywordEditor keywordEditor = new KeywordEditor(
            this.applicableKeywords,
            keyword,
            this._maxYValueIndex);
        if (keywordEditor.ShowDialog() == DialogResult.OK)
        {
            int start = this._selectedKeywordStart;
            int length = this._selectedKeywordLength;

            // Update currently selected keyword
            this._richTextBox.Text = this._richTextBox.Text.Substring(0, start) + keywordEditor.Keyword + this._richTextBox.Text.Substring(start + length);
            this._richTextBox.SelectionStart = start + keywordEditor.Keyword.Length;
        }

        // Set focus back to the editor
        this._richTextBox.Focus();
    }

    /// <summary>
    /// Rich text box text changed event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void richTextBox_TextChanged(object sender, EventArgs e)
    {
        if (!_updating)
        {
            _updating = true;

            // Save current selection
            int selectionStart = this._richTextBox.SelectionStart;
            int selectionLength = this._richTextBox.SelectionLength;

            // Update RTF tex
            _richTextBox.Rtf = this.GetRtfText(_richTextBox.Text);

            // Restore selection
            this._richTextBox.SelectionStart = selectionStart;
            this._richTextBox.SelectionLength = selectionLength;

            _updating = false;
        }
    }

    /// <summary>
    /// Rich text box selection changed event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void richTextBox_SelectionChanged(object sender, EventArgs e)
    {
        // No any processing in selection mode with the Shift key down
        if ((ModifierKeys & Keys.Shift) != Keys.Shift)
        {
            if (!_updating)
            {
                _updating = true;

                // Update RTF text only when selected (bolded) keyword is changed
                string selectedKeywordTemp = this._selectedKeywordName;
                string newRtf = this.GetRtfText(_richTextBox.Text);
                if (selectedKeywordTemp != this._selectedKeywordName)
                {
                    // Save current selection
                    int selectionStart = this._richTextBox.SelectionStart;

                    // Update RTF text
                    _richTextBox.Rtf = newRtf;

                    // Restore selection
                    this._richTextBox.SelectionStart = selectionStart;
                    this._richTextBox.SelectionLength = 0;
                }

                _updating = false;
            }
        }
    }

    /// <summary>
    /// Rich text box key pressed event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void richTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        // Make sure we enter a closing bracket when user starts 
        // entering the format string
        if (e.KeyChar == '{')
        {
            if (_richTextBox.SelectionColor == Color.Blue)
            {
                e.Handled = true;
                _richTextBox.SelectedText = "{}";
                _richTextBox.SelectionStart--;
            }
        }
    }

    /// <summary>
    /// Rich text box key down event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void richTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Delete keyword when user press 'Delete' key
        if (e.KeyCode == Keys.Delete &&
            this._selectedKeywordStart >= 0 &&
            this._selectedKeywordLength > 0)
        {
            // Remember selection start because it will be changed as soon
            // as we update editor text
            int newSelectionPosition = this._selectedKeywordStart;

            // Remove keyword
            string newText = _richTextBox.Text.Substring(0, this._selectedKeywordStart);
            newText += _richTextBox.Text.Substring(this._selectedKeywordStart + this._selectedKeywordLength);
            _richTextBox.Text = newText;

            // Restore cursor (selection) position
            _richTextBox.SelectionStart = newSelectionPosition;
            e.Handled = true;
        }
    }


    /// <summary>
    /// Ok button pressed event handler.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event arguments.</param>
    private void buttonOk_Click(object sender, EventArgs e)
    {
        // Get text from the editor
        this.ResultString = this._richTextBox.Text;

        // New line character should be presented as 2 characters "\n"
        this.ResultString = this.ResultString.Replace("\r\n", "\\n");
        this.ResultString = this.ResultString.Replace("\n", "\\n");
    }

    #endregion // Event Handlers

    #region Helper Methods

    /// <summary>
    /// Helper method that generates the RTF text based on the string.
    /// </summary>
    /// <param name="originalText">Input text.</param>
    /// <returns>Input text formatted as RTF.</returns>
    private string GetRtfText(string originalText)
    {

        // Initialize empty string
        // Start with RTF header and font table
        string resultRtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}\r\n";

        // Add color table
        resultRtf += @"{\colortbl ;\red0\green0\blue255;}\r\n";

        // Add text starting tags
        resultRtf += @"\viewkind4\uc1\pard\f0\fs17 ";

        // Add text
        resultRtf += GetUnicodeRtf(this.GetColorHilightedRtfText(originalText));

        // Finish RTF format
        resultRtf += @"\par\r\n}";

        return resultRtf;
    }

    // VSTS: 65162: The non ansi 1252 characters will be lost, we need conversion in \uXXXX? format.
    private static string GetUnicodeRtf(string orginalText)
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        foreach (char c in orginalText.ToCharArray())
        {
            int charInt = Convert.ToInt32(c);
            if (charInt < 0x00 || charInt > 0x7f)
                result.Append(@"\u" + charInt.ToString() + "?");
            else
                result.Append(Convert.ToString(c));
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets specified text in RTF format where keyword are color highlighted.
    /// </summary>
    /// <param name="originalText">Original text.</param>
    /// <returns>Color highlighted RTF text.</returns>
    private string GetColorHilightedRtfText(string originalText)
    {
        string resultText = originalText;
        string selectedKeyword = string.Empty;

        // Reset selected keyword position
        this._selectedKeywordStart = -1;
        this._selectedKeywordLength = 0;

        // Current selection position that will be adjusted when formatting 
        // characters are added infron of it.
        int selectionStart = this._richTextBox.SelectionStart;

        // Replace special new line character sequence "\n"
        resultText = resultText.Replace("\\n", "\r\n");

        // Replace special RTF Character '\'
        int slashCountre = 0;
        for (int index = 0; index < resultText.Length && index < selectionStart; index++)
        {
            if (resultText[index] == '\\')
            {
                ++slashCountre;
            }
        }

        selectionStart += slashCountre;
        resultText = resultText.Replace(@"\", @"\\");

        // Iterate through all keywords 
        foreach (KeywordInfo keywordInfo in this.applicableKeywords)
        {
            // Fill array of possible names for that keyword
            string[] keywordNames = keywordInfo.GetKeywords();

            // Iterate through all possible names
            foreach (string keywordNameWithSpaces in keywordNames)
            {
                int startIndex = 0;

                // Trim spaces
                string keywordName = keywordNameWithSpaces.Trim();

                // Skip empty strings
                if (keywordName.Length > 0)
                {
                    // Try finding the keyword in the string
                    while ((startIndex = resultText.IndexOf(keywordName, startIndex, StringComparison.Ordinal)) >= 0)
                    {
                        int keywordLength = keywordName.Length;

                        // Check if Y value index can be part of the keyword
                        if (keywordInfo.SupportsValueIndex)
                        {
                            if (resultText.Length > startIndex + keywordLength &&
                                resultText[startIndex + keywordLength] == 'Y')
                            {
                                ++keywordLength;
                                if (resultText.Length > startIndex + keywordLength &&
                                    char.IsDigit(resultText[startIndex + keywordLength]))
                                {
                                    ++keywordLength;
                                }
                            }
                        }

                        // Check if format string can be part of the keyword
                        if (keywordInfo.SupportsFormatting)
                        {
                            if (resultText.Length > startIndex + keywordLength &&
                                resultText[startIndex + keywordLength] == '{')
                            {
                                ++keywordLength;
                                int formatEndBracket = resultText.IndexOf("}", startIndex + keywordLength, StringComparison.Ordinal);
                                if (formatEndBracket >= 0)
                                {
                                    keywordLength += formatEndBracket - startIndex - keywordLength + 1;
                                }
                            }
                        }

                        // Check if cursor currently located inside the keyword
                        bool isKeywordSelected = selectionStart > startIndex &&
                            selectionStart <= startIndex + keywordLength;

                        // Show Keyword with different color
                        string tempText = resultText.Substring(0, startIndex);
                        string formattedKeyword = string.Empty;
                        formattedKeyword += @"\cf1";
                        if (isKeywordSelected)
                        {
                            // Remember selected keyword by name and position
                            selectedKeyword = keywordInfo.Name;
                            selectedKeyword += "__" + startIndex.ToString(CultureInfo.InvariantCulture);
                            this._selectedKeywordStart = startIndex;
                            this._selectedKeywordStart -= selectionStart - this._richTextBox.SelectionStart;
                            this._selectedKeywordLength = keywordLength;

                            formattedKeyword += @"\b";
                        }

                        formattedKeyword += @"\ul";
                        // Replace keyword start symbol '#' with "#_" to avoid duplicate processing
                        formattedKeyword += "#_";
                        formattedKeyword += resultText.Substring(startIndex + 1, keywordLength - 1);
                        formattedKeyword += @"\cf0";
                        if (isKeywordSelected)
                        {
                            formattedKeyword += @"\b0";
                        }

                        formattedKeyword += @"\ul0 ";
                        tempText += formattedKeyword;
                        tempText += resultText.Substring(startIndex + keywordLength);
                        resultText = tempText;

                        // Adjust selection position
                        if (startIndex < selectionStart)
                        {
                            selectionStart += formattedKeyword.Length - keywordLength;
                        }

                        // Increase search start index by the length of the keyword
                        startIndex += formattedKeyword.Length;
                    }
                }
            }
        }

        // Set currently selected keyword name
        this._selectedKeywordName = selectedKeyword;

        // Update Edit button
        if (this._selectedKeywordName.Length > 0)
        {
            // Enable Edit button and set it text
            this._buttonEdit.Enabled = true;
        }
        else
        {
            this._buttonEdit.Enabled = false;
        }

        // Replace all the "\n" strings with new line objectTag "\par"
        resultText = resultText.Replace("\r\n", @"\par ");
        resultText = resultText.Replace("\n", @"\par ");
        resultText = resultText.Replace("\\n", @"\par ");

        // Replace special RTF Characters '{' and '}'
        // Has to be done after all processing because this character is 
        // used in keywords formatting.
        resultText = resultText.Replace(@"{", @"\{");
        resultText = resultText.Replace(@"}", @"\}");

        // Replace the "#_" string with keyword start symbol.
        // This  was previously changed to avoid duplicate processing.
        return resultText.Replace("#_", "#");
    }

    /// <summary>
    /// Get list of keywords applicable to current object and property.
    /// </summary>
    /// <returns></returns>
    private ArrayList GetApplicableKeywords()
    {
        // Create new array
        ArrayList keywordList = new ArrayList();

        // Get access to the chart keywords registry
        if (registeredKeywords is not null && this._propertyName.Length > 0 && this._classTypeName.Length > 0)
        {
            // Iterate through all keywords in the registry
            foreach (var keywordInfo in registeredKeywords)
            {
                // Check if keyword is supported by specified type
                bool typeSupported = false;
                string[] typeNames = keywordInfo.AppliesToTypes.Split(',');
                foreach (string typeName in typeNames)
                {
                    if (this._classTypeName == typeName.Trim())
                    {
                        typeSupported = true;
                        break;
                    }
                }

                // If type supported check property name
                if (typeSupported)
                {
                    string[] propertyNames = keywordInfo.AppliesToProperties.Split(',');
                    foreach (string propertyName in propertyNames)
                    {
                        if (this._propertyName == propertyName.Trim())
                        {
                            // Add KeywordInfo into the list
                            keywordList.Add(keywordInfo);
                            break;
                        }
                    }
                }
            }
        }

        return keywordList;
    }

    /// <summary>
    /// VSTS: 787930 - Expand the dialog with widthDialogExtend, heightDialogExtend  to make room for localization.
    /// </summary>
    private void PrepareControlsLayout()
    {
        int buttonWidthAdd = 18;
        this.Width += widthDialogExtend;
        this._panelOkCancelButtons.Width += widthDialogExtend;
        this._panelInsertEditButtons.Width += widthDialogExtend;
        this._buttonInsert.Width += widthDialogExtend + buttonWidthAdd;
        this._buttonInsert.Left -= buttonWidthAdd;
        this._buttonEdit.Width += widthDialogExtend + buttonWidthAdd;
        this._buttonEdit.Left -= buttonWidthAdd;
        this._labelDescription.Width += widthDialogExtend;

        this.Height += heightDialogExtend;
        this._panelOkCancelButtons.Top += heightDialogExtend;
        this._labelDescription.Height += heightDialogExtend;
    }

    #endregion // Helper Methods
}