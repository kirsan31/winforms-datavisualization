// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Main windows forms chart control class.
//


using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.Formulas;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

using Size = System.Drawing.Size;


namespace System.Windows.Forms.DataVisualization.Charting;

#region Enumerations

/// <summary>
/// Specifies the format of the image
/// </summary>
public enum ChartImageFormat
{
    /// <summary>
    /// Gets the Joint Photographic Experts Group (JPEG) image format.
    /// </summary>
    Jpeg,

    /// <summary>
    /// Gets the W3C Portable Network Graphics (PNG) image format.
    /// </summary>
    Png,

    /// <summary>
    /// Gets the bitmap image format (BMP).
    /// </summary>
    Bmp,

    /// <summary>
    /// Gets the Tag Image File Format (TIFF) image format.
    /// </summary>
    Tiff,

    /// <summary>
    /// Gets the Graphics Interchange Format (GIF) image format.
    /// </summary>
    Gif,

    /// <summary>
    /// Gets the Enhanced Meta File (Emf) image format.
    /// </summary>
    Emf,

    /// <summary>
    /// Enhanced Meta File (EmfDual) image format.
    /// </summary>
    EmfDual,

    /// <summary>
    /// Enhanced Meta File (Emf+) image format.
    /// </summary>
    EmfPlus,
}

#endregion

/// <summary>
/// Chart windows forms control
/// </summary>
[ToolboxBitmap(typeof(Chart), "ChartControl.bmp")]
[SRDescription("DescriptionAttributeChart_Chart")]
[Designer("WinForms.DataVisualization.Designer.Server.ChartWinDesigner")]
[DesignerSerializer("WinForms.DataVisualization.Designer.Server.ChartWinDesignerSerializer", typeof(CodeDomSerializer))]
[DisplayName("Chart")]
public class Chart : System.Windows.Forms.Control, ISupportInitialize
{
    #region Control fields

    private bool _disposedValue;
    /// <summary>
    /// Determines whether or not to show debug markings in debug mode. For internal use.
    /// </summary>
    internal bool ShowDebugMarkings;

    // Chart services components
    private readonly ChartTypeRegistry _chartTypeRegistry;
    private readonly BorderTypeRegistry _borderTypeRegistry;
    private readonly CustomPropertyRegistry _customAttributeRegistry;
    private readonly DataManager _dataManager;
    internal readonly ChartImage chartPicture;
    private readonly ImageLoader _imageLoader;
    internal readonly ServiceContainer serviceContainer;
    private readonly PrintingManager _printingManager;

    // Selection class
    internal readonly Selection selection;

    // Formula registry service component
    private readonly FormulaRegistry _formulaRegistry;

    // Indicates that control invalidation is temporary disabled
    internal bool disableInvalidates;

    // Indicates that chart is serializing the data
    internal bool serializing;

    // Detailed serialization status which allows not only to determine if serialization
    // is curently in process but also check if we are saving, loading or resetting the chart.
    internal SerializationStatus serializationStatus = SerializationStatus.None;

    // Bitmap used for double buffering chart painting
    internal Bitmap paintBufferBitmap;

    // Graphics of the double buffered bitmap
    internal Graphics paintBufferBitmapGraphics;

    // Indicates that only chart area cursor/selection must be drawn during the next paint event
    internal bool paintTopLevelElementOnly;

    // Indicates that some chart properties where changed (used for painting)
    internal bool dirtyFlag = true;


    // Chart default cursor
#pragma warning disable CA2213 // Disposable fields should be disposed
    internal System.Windows.Forms.Cursor defaultCursor = Cursors.Default;
#pragma warning restore CA2213 // Disposable fields should be disposed

    // Keywords registry
    private readonly KeywordsRegistry _keywordsRegistry;

    #endregion

    #region Component Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
    }
    #endregion

    #region Control constructors

    /// <summary>
    /// Chart control constructor.
    /// </summary>
    public Chart()
    {

        //*******************************************************
        //** Check control license
        //*******************************************************

        //*********************************************************
        //** Set control styles
        //*********************************************************
        this.SetStyle(ControlStyles.ResizeRedraw, true);
        //this.SetStyle(ControlStyles.Opaque, true);
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        this.SetStyle(ControlStyles.Selectable, true);

        // NOTE: Fixes issue #4475
        this.SetStyle(ControlStyles.DoubleBuffer, true);

        // This is necessary to raise focus event on chart mouse click.
        this.SetStyle(ControlStyles.UserMouse, true);

        //*********************************************************
        //** Create services
        //*********************************************************
        serviceContainer = new ServiceContainer();
        _chartTypeRegistry = new ChartTypeRegistry();
        _borderTypeRegistry = new BorderTypeRegistry();
        _customAttributeRegistry = new CustomPropertyRegistry();

        _keywordsRegistry = new KeywordsRegistry();

        _dataManager = new DataManager(serviceContainer);
        _imageLoader = new ImageLoader(serviceContainer);

        chartPicture = new ChartImage(serviceContainer);
        Serializer = new ChartSerializer(serviceContainer);
        _printingManager = new PrintingManager(serviceContainer);
        _formulaRegistry = new FormulaRegistry();

        // Add services to the service container
        serviceContainer.AddService(typeof(Chart), this);                           // Chart Control
        serviceContainer.AddService(_chartTypeRegistry.GetType(), _chartTypeRegistry);// Chart types registry
        serviceContainer.AddService(_borderTypeRegistry.GetType(), _borderTypeRegistry);// Border types registry
        serviceContainer.AddService(_customAttributeRegistry.GetType(), _customAttributeRegistry);// Custom attribute registry
        serviceContainer.AddService(_dataManager.GetType(), _dataManager);          // Data Manager service
        serviceContainer.AddService(_imageLoader.GetType(), _imageLoader);          // Image Loader service
        serviceContainer.AddService(chartPicture.GetType(), chartPicture);          // Chart image service
        serviceContainer.AddService(Serializer.GetType(), Serializer);  // Chart serializer service
        serviceContainer.AddService(_printingManager.GetType(), _printingManager);  // Printing manager service
        serviceContainer.AddService(_formulaRegistry.GetType(), _formulaRegistry);  // Formula modules service
        serviceContainer.AddService(_keywordsRegistry.GetType(), _keywordsRegistry);    // Keywords registry

        // Initialize objects
        _dataManager.Initialize();


        // Register known chart types
        _chartTypeRegistry.Register(ChartTypeNames.Bar, typeof(BarChart));
        _chartTypeRegistry.Register(ChartTypeNames.Column, typeof(ColumnChart));
        _chartTypeRegistry.Register(ChartTypeNames.Point, typeof(PointChart));
        _chartTypeRegistry.Register(ChartTypeNames.Bubble, typeof(BubbleChart));
        _chartTypeRegistry.Register(ChartTypeNames.Line, typeof(LineChart));
        _chartTypeRegistry.Register(ChartTypeNames.Spline, typeof(SplineChart));
        _chartTypeRegistry.Register(ChartTypeNames.StepLine, typeof(StepLineChart));
        _chartTypeRegistry.Register(ChartTypeNames.Area, typeof(AreaChart));
        _chartTypeRegistry.Register(ChartTypeNames.SplineArea, typeof(SplineAreaChart));
        _chartTypeRegistry.Register(ChartTypeNames.StackedArea, typeof(StackedAreaChart));
        _chartTypeRegistry.Register(ChartTypeNames.Pie, typeof(PieChart));
        _chartTypeRegistry.Register(ChartTypeNames.Stock, typeof(StockChart));
        _chartTypeRegistry.Register(ChartTypeNames.Candlestick, typeof(CandleStickChart));
        _chartTypeRegistry.Register(ChartTypeNames.Doughnut, typeof(DoughnutChart));
        _chartTypeRegistry.Register(ChartTypeNames.StackedBar, typeof(StackedBarChart));
        _chartTypeRegistry.Register(ChartTypeNames.StackedColumn, typeof(StackedColumnChart));
        _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedColumn, typeof(HundredPercentStackedColumnChart));
        _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedBar, typeof(HundredPercentStackedBarChart));
        _chartTypeRegistry.Register(ChartTypeNames.OneHundredPercentStackedArea, typeof(HundredPercentStackedAreaChart));



        _chartTypeRegistry.Register(ChartTypeNames.Range, typeof(RangeChart));
        _chartTypeRegistry.Register(ChartTypeNames.SplineRange, typeof(SplineRangeChart));
        _chartTypeRegistry.Register(ChartTypeNames.RangeBar, typeof(RangeBarChart));
        _chartTypeRegistry.Register(ChartTypeNames.Radar, typeof(RadarChart));
        _chartTypeRegistry.Register(ChartTypeNames.RangeColumn, typeof(RangeColumnChart));
        _chartTypeRegistry.Register(ChartTypeNames.ErrorBar, typeof(ErrorBarChart));
        _chartTypeRegistry.Register(ChartTypeNames.BoxPlot, typeof(BoxPlotChart));



        _chartTypeRegistry.Register(ChartTypeNames.Renko, typeof(RenkoChart));
        _chartTypeRegistry.Register(ChartTypeNames.ThreeLineBreak, typeof(ThreeLineBreakChart));
        _chartTypeRegistry.Register(ChartTypeNames.Kagi, typeof(KagiChart));
        _chartTypeRegistry.Register(ChartTypeNames.PointAndFigure, typeof(PointAndFigureChart));





        _chartTypeRegistry.Register(ChartTypeNames.Polar, typeof(PolarChart));
        _chartTypeRegistry.Register(ChartTypeNames.FastLine, typeof(FastLineChart));
        _chartTypeRegistry.Register(ChartTypeNames.Funnel, typeof(FunnelChart));
        _chartTypeRegistry.Register(ChartTypeNames.Pyramid, typeof(PyramidChart));





        _chartTypeRegistry.Register(ChartTypeNames.FastPoint, typeof(FastPointChart));



        // Register known formula modules
        _formulaRegistry.Register(SR.FormulaNamePriceIndicators, typeof(PriceIndicators));
        _formulaRegistry.Register(SR.FormulaNameGeneralTechnicalIndicators, typeof(GeneralTechnicalIndicators));
        _formulaRegistry.Register(SR.FormulaNameTechnicalVolumeIndicators, typeof(VolumeIndicators));
        _formulaRegistry.Register(SR.FormulaNameOscillator, typeof(Oscillators));
        _formulaRegistry.Register(SR.FormulaNameGeneralFormulas, typeof(GeneralFormulas));
        _formulaRegistry.Register(SR.FormulaNameTimeSeriesAndForecasting, typeof(TimeSeriesAndForecasting));
        _formulaRegistry.Register(SR.FormulaNameStatisticalAnalysis, typeof(StatisticalAnalysis));



        // Register known 3D border types
        _borderTypeRegistry.Register("Emboss", typeof(EmbossBorder));
        _borderTypeRegistry.Register("Raised", typeof(RaisedBorder));
        _borderTypeRegistry.Register("Sunken", typeof(SunkenBorder));
        _borderTypeRegistry.Register("FrameThin1", typeof(FrameThin1Border));
        _borderTypeRegistry.Register("FrameThin2", typeof(FrameThin2Border));
        _borderTypeRegistry.Register("FrameThin3", typeof(FrameThin3Border));
        _borderTypeRegistry.Register("FrameThin4", typeof(FrameThin4Border));
        _borderTypeRegistry.Register("FrameThin5", typeof(FrameThin5Border));
        _borderTypeRegistry.Register("FrameThin6", typeof(FrameThin6Border));
        _borderTypeRegistry.Register("FrameTitle1", typeof(FrameTitle1Border));
        _borderTypeRegistry.Register("FrameTitle2", typeof(FrameTitle2Border));
        _borderTypeRegistry.Register("FrameTitle3", typeof(FrameTitle3Border));
        _borderTypeRegistry.Register("FrameTitle4", typeof(FrameTitle4Border));
        _borderTypeRegistry.Register("FrameTitle5", typeof(FrameTitle5Border));
        _borderTypeRegistry.Register("FrameTitle6", typeof(FrameTitle6Border));
        _borderTypeRegistry.Register("FrameTitle7", typeof(FrameTitle7Border));
        _borderTypeRegistry.Register("FrameTitle8", typeof(FrameTitle8Border));

        // Enable chart invalidating
        this.disableInvalidates = false;

        // Create selection object
        selection = new Selection(serviceContainer);

        // Create named images collection
        Images = new NamedImagesCollection();

        // Hook up event handlers
        ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.ChartAreaNameReferenceChanged);
        ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Legends.ChartAreaNameReferenceChanged);
        ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Titles.ChartAreaNameReferenceChanged);
        ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Annotations.ChartAreaNameReferenceChanged);
        ChartAreas.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(ChartAreas.ChartAreaNameReferenceChanged);
        Legends.NameReferenceChanged += new EventHandler<NameReferenceChangedEventArgs>(Series.LegendNameReferenceChanged);
    }



    #endregion

    #region Control painting methods

    /// <summary>
    /// Paint chart control.
    /// </summary>
    /// <param name="e">Paint event arguments.</param>
    protected override void OnPaint(PaintEventArgs e)
    {

        //*******************************************************
        //** Check control license

        // Disable invalidates
        this.disableInvalidates = true;

        //*******************************************************
        //** If chart background is transparent - draw without
        //** double buffering.
        //*******************************************************
        if (this.IsBorderTransparent() ||
            (!this.BackColor.IsEmpty && (this.BackColor == Color.Transparent || this.BackColor.A != 255)))
        {

            // Draw chart directly on the graphics
            try
            {
                if (this.paintTopLevelElementOnly)
                {
                    chartPicture.Paint(e.Graphics, false);
                }

                chartPicture.Paint(e.Graphics, this.paintTopLevelElementOnly);
            }
            catch (Exception)
            {
                // Draw exception method
                DrawException(e.Graphics);


                // Rethrow exception if not in design-time mode
                if (!this.DesignMode)
                {
                    throw;
                }
            }
        }
        else
        {

            //*******************************************************
            //** If nothing was changed in the chart and last image is stored in the buffer
            //** there is no need to repaint the chart.
            //*******************************************************
            if (this.dirtyFlag || paintBufferBitmap == null)
            {

                // Get scaling component from the drawing graphics
                using var mt = e.Graphics.Transform;
                float scaleX = mt.Elements[0];
                float scaleY = mt.Elements[3];

                // Create offscreen buffer bitmap
                if (paintBufferBitmap == null ||
                    paintBufferBitmap.Width < scaleX * ClientRectangle.Width ||
                    paintBufferBitmap.Height < scaleY * ClientRectangle.Height)
                {
                    if (paintBufferBitmap != null)
                    {
                        paintBufferBitmap.Dispose();
                        paintBufferBitmapGraphics.Dispose();
                    }

                    // Create offscreen bitmap taking in consideration graphics scaling
                    paintBufferBitmap = new Bitmap((int)(ClientRectangle.Width * scaleX), (int)(ClientRectangle.Height * scaleY), e.Graphics);
                    paintBufferBitmapGraphics = Graphics.FromImage(paintBufferBitmap);
                    paintBufferBitmapGraphics.ScaleTransform(scaleX, scaleY);
                }

                //*******************************************************
                //** Draw chart in bitmap buffer
                //*******************************************************
                try
                {
                    chartPicture.Paint(paintBufferBitmapGraphics, this.paintTopLevelElementOnly);
                }
                catch (Exception)
                {
                    // Draw exception method
                    DrawException(paintBufferBitmapGraphics);

                    // Rethrow exception if not in design-time mode
                    if (!this.DesignMode)
                    {
                        throw;
                    }
                }
            }

            //*******************************************************
            //** Push bitmap buffer forward into the screen
            //*******************************************************
            // Set drawing scale 1:1. Only persist the transformation from current matrix
            using System.Drawing.Drawing2D.Matrix drawingMatrix = new System.Drawing.Drawing2D.Matrix();
            using System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform;
            drawingMatrix.Translate(oldMatrix.OffsetX, oldMatrix.OffsetY);
            e.Graphics.Transform = drawingMatrix;

            // Draw image
            e.Graphics.DrawImage(paintBufferBitmap, 0, 0);
            e.Graphics.Transform = oldMatrix;
        }

        // Clears control dirty flag
        this.dirtyFlag = false;
        this.disableInvalidates = false;

        // Call base class
        base.OnPaint(e);

        //*******************************************************
        //** Check if smart client data must be loaded
        //*******************************************************
    }

    /// <summary>
    /// Paints control background.
    /// </summary>
    /// <param name="pevent">Event arguments.</param>
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        this.disableInvalidates = true;

        //*********************************************************
        //** Check if chart back ground has a transparent color
        //*********************************************************
        bool transpBack = false;
        if (chartPicture.BackColor.A != 255 && chartPicture.BackColor != Color.Empty)
        {
            transpBack = true;
        }
        else if (chartPicture.BackImageTransparentColor.A != 255 &&
            chartPicture.BackImageTransparentColor != Color.Empty &&
            !string.IsNullOrEmpty(chartPicture.BackImage))
        {
            transpBack = true;
        }

        //*********************************************************
        //** If chart or chart border page colr has transparent color
        //*********************************************************
        bool transpBorder = this.IsBorderTransparent();
        if (transpBorder || transpBack)
        {
            Color oldBackColor = chartPicture.BackColor;
            if (transpBorder)

            {
                chartPicture.BackColor = Color.Transparent;
            }

            // Call base class 
            base.OnPaintBackground(pevent);

            chartPicture.BackColor = oldBackColor;
        }

        this.disableInvalidates = false;
    }

    /// <summary>
    /// When user changes system color, the Chart redraws itself.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnSystemColorsChanged(EventArgs e)
    {
        base.OnSystemColorsChanged(e);

        this.Invalidate();
    }

    /// <summary>
    /// Checks if border skins is enabled in the chart and it uses transparency in the page color
    /// </summary>
    /// <returns>True if transparency is used in the border.</returns>
    private bool IsBorderTransparent()
    {
        bool transpBorder = false;
        if (chartPicture.BorderSkin.SkinStyle != BorderSkinStyle.None)
        {
            if (chartPicture.BorderSkin.PageColor.A != 255 &&
                chartPicture.BorderSkin.PageColor != Color.Empty)
            {
                transpBorder = true;
            }

            if (chartPicture.BorderSkin.BackColor.A != 255 && chartPicture.BorderSkin.BackColor != Color.Empty)
            {
                transpBorder = true;
            }
            else if (chartPicture.BorderSkin.BackImageTransparentColor.A != 255 &&
                chartPicture.BorderSkin.BackImageTransparentColor != Color.Empty &&
                !string.IsNullOrEmpty(chartPicture.BorderSkin.BackImage))
            {
                transpBorder = true;
            }
        }

        return transpBorder;
    }

    /// <summary>
    /// Draws exception information at design-time.
    /// </summary>
    /// <param name="graphics">Chart graphics to use.</param>
    private void DrawException(Graphics graphics)
    {
        // Fill background
        graphics.FillRectangle(Brushes.White, 0, 0, this.Width, this.Height);

        string addMessage = SR.ExceptionChartPreviewNotAvailable;
        // Get text rectangle
        RectangleF rect = new RectangleF(3, 3, this.Width - 6, this.Height - 6);

        // Draw exception text
        using StringFormat format = new StringFormat();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        using Font font = new Font(FontCache.DefaultFamilyName, 8 * DPIScale);
        graphics.DrawString(addMessage, font, Brushes.Black, rect, format);
    }

    /// <summary>
    /// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
    /// </summary>
    [
    EditorBrowsable(EditorBrowsableState.Never)
    ]
    public override void Refresh()
    {
        // Clear bitmap used to improve the performance of elements
        // like cursors and annotations
        // NOTE: Fixes issue #4157
        if (this.chartPicture.nonTopLevelChartBuffer != null)
        {
            this.chartPicture.nonTopLevelChartBuffer.Dispose();
            this.chartPicture.nonTopLevelChartBuffer = null;
        }

        this.dirtyFlag = true;
        this.ResetAccessibilityObject();
        base.Refresh();
    }

    /// <summary>
    /// Invalidates a specific region of the control and causes a paint message to be sent to the control.
    /// </summary>
    public new void Invalidate()
    {
        this.dirtyFlag = true;
        this.ResetAccessibilityObject();
        if (!this.disableInvalidates)
        {
            base.Invalidate(true);

        }

        // NOTE: Code below required for the Diagram integration. -AG
        if (!this.chartPicture.isSavingAsImage)
        {
            InvalidateEventArgs e = new InvalidateEventArgs(Rectangle.Empty);
            this.OnInvalidated(e);
        }
    }

    /// <summary>
    /// Invalidates a specific region of the control and causes a paint message to be sent to the control.
    /// </summary>
    public new void Invalidate(Rectangle rectangle)
    {
        this.dirtyFlag = true;
        this.ResetAccessibilityObject();
        if (!this.disableInvalidates)
        {
            base.Invalidate(rectangle);

        }

        // NOTE: Code below required for the Diagram integration. -AG
        if (!this.chartPicture.isSavingAsImage)
        {
            InvalidateEventArgs e = new InvalidateEventArgs(Rectangle.Empty);
            this.OnInvalidated(e);
        }
    }




    /// <summary>
    /// Updates chart cursor and range selection only.
    /// </summary>
    public void UpdateCursor()
    {
        // Set flag to redraw cursor/selection only
        this.paintTopLevelElementOnly = true;

        // Update chart cursor and range selection
        base.Update();

        // Clear flag to redraw cursor/selection only
        this.paintTopLevelElementOnly = false;
    }



    /// <summary>
    /// Updates chart annotations only.
    /// </summary>
    public void UpdateAnnotations()
    {
        // Set flag to redraw cursor/selection only
        this.paintTopLevelElementOnly = true;

        // Update chart cursor and range selection
        base.Update();

        // Clear flag to redraw cursor/selection only
        this.paintTopLevelElementOnly = false;
    }



    #endregion

    #region Control size and location properties/methods

    /// <summary>
    /// Returns default control size.
    /// </summary>
    protected override System.Drawing.Size DefaultSize => new System.Drawing.Size(300, 300);

    /// <summary>
    /// Control location changed.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnLocationChanged(EventArgs e)
    {
        // If chart or chart border page color has transparent color
        if ((chartPicture.BackColor.A != 255 && chartPicture.BackColor != Color.Empty) ||
            (chartPicture.BorderSkin.SkinStyle != BorderSkinStyle.None &&
            chartPicture.BorderSkin.PageColor.A != 255 &&
            chartPicture.BorderSkin.PageColor != Color.Empty))
        {
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }

        base.OnLocationChanged(e);
    }

    /// <summary>
    /// Control resized.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnResize(EventArgs e)
    {
        chartPicture.Width = this.Size.Width;
        chartPicture.Height = this.Size.Height;
        this.dirtyFlag = true;
        this.ResetAccessibilityObject();
        base.OnResize(e);
    }
    /// <summary>
    /// Fires RightToLeftChanged event.
    /// </summary>
    /// <param name="e">Event Arguments</param>
    protected override void OnRightToLeftChanged(EventArgs e)
    {
        base.OnRightToLeftChanged(e);
        this.Invalidate();
    }

    #endregion

    #region Chart image saving methods

    /// <summary>
    /// Saves chart image into the file.
    /// </summary>
    /// <param name="imageFileName">Image file name</param>
    /// <param name="format">Image format.</param>
    public void SaveImage(string imageFileName, ChartImageFormat format)
    {
        // Check arguments
        if (imageFileName == null)
            throw new ArgumentNullException(nameof(imageFileName));

        // Create file stream for the specified file name
        FileStream fileStream = new FileStream(imageFileName, FileMode.Create);

        // Save into stream
        try
        {
            SaveImage(fileStream, format);
        }
        finally
        {
            // Close file stream
            fileStream.Close();
        }
    }

    /// <summary>
    /// Saves chart image into the file.
    /// </summary>
    /// <param name="imageFileName">Image file name</param>
    /// <param name="format">Image format.</param>
    public void SaveImage(string imageFileName, ImageFormat format)
    {
        // Check arguments
        if (imageFileName == null)
            throw new ArgumentNullException(nameof(imageFileName));
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        // Create file stream for the specified file name
        FileStream fileStream = new FileStream(imageFileName, FileMode.Create);

        // Save into stream
        try
        {
            SaveImage(fileStream, format);
        }
        finally
        {
            // Close file stream
            fileStream.Close();
        }
    }

    /// <summary>
    /// Saves chart image into the stream.
    /// </summary>
    /// <param name="imageStream">Image stream.</param>
    /// <param name="format">Image format.</param>
    public void SaveImage(Stream imageStream, ImageFormat format)
    {
        // Check arguments
        if (imageStream == null)
            throw new ArgumentNullException(nameof(imageStream));
        if (format == null)
            throw new ArgumentNullException(nameof(format));

        // Indicate that chart is saved into the image
        this.chartPicture.isSavingAsImage = true;

        if (format == ImageFormat.Emf || format == ImageFormat.Wmf)
        {
            this.chartPicture.SaveIntoMetafile(imageStream, EmfType.EmfOnly);
        }
        else
        {
            // Get chart image
            Image chartImage = this.chartPicture.GetImage();

            // Save image into the file
            chartImage.Save(imageStream, format);

            // Dispose image
            chartImage.Dispose();
        }

        // Reset flag
        this.chartPicture.isSavingAsImage = false;
    }

    /// <summary>
    /// Saves chart image into the stream.
    /// </summary>
    /// <param name="imageStream">Image stream.</param>
    /// <param name="format">Image format.</param>
    public void SaveImage(Stream imageStream, ChartImageFormat format)
    {
        // Check arguments
        if (imageStream == null)
            throw new ArgumentNullException(nameof(imageStream));

        // Indicate that chart is saved into the image
        this.chartPicture.isSavingAsImage = true;

        if (format == ChartImageFormat.Emf ||
            format == ChartImageFormat.EmfPlus ||
            format == ChartImageFormat.EmfDual)
        {
            EmfType emfType = EmfType.EmfOnly;
            if (format == ChartImageFormat.EmfDual)
            {
                emfType = EmfType.EmfPlusDual;
            }
            else if (format == ChartImageFormat.EmfPlus)
            {
                emfType = EmfType.EmfPlusOnly;
            }

            this.chartPicture.SaveIntoMetafile(imageStream, emfType);
        }
        else
        {
            // Get chart image
            Image chartImage = this.chartPicture.GetImage();

            ImageFormat standardImageFormat = ImageFormat.Png;

            switch (format)
            {
                case ChartImageFormat.Bmp:
                    standardImageFormat = ImageFormat.Bmp;
                    break;
                case ChartImageFormat.Gif:
                    standardImageFormat = ImageFormat.Gif;
                    break;
                case ChartImageFormat.Jpeg:
                    standardImageFormat = ImageFormat.Jpeg;
                    break;
                case ChartImageFormat.Png:
                    standardImageFormat = ImageFormat.Png;
                    break;
                case ChartImageFormat.Tiff:
                    standardImageFormat = ImageFormat.Tiff;
                    break;
            }

            // Save image into the file
            chartImage.Save(imageStream, standardImageFormat);

            // Dispose image
            chartImage.Dispose();
        }

        // Reset flag
        this.chartPicture.isSavingAsImage = false;
    }


    #endregion

    #region Control public properties

    /// <summary>
    /// Array of custom palette colors.
    /// </summary>
    /// <remarks>
    /// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
    /// </remarks>
    [
    SRCategory("CategoryAttributeAppearance"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    SerializationVisibilityAttribute(SerializationVisibility.Attribute),
    SRDescription("DescriptionAttributeChart_PaletteCustomColors"),
    TypeConverter(typeof(ColorArrayConverter))
    ]
    public Color[] PaletteCustomColors
    {
        set
        {
            this._dataManager.PaletteCustomColors = value;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
        get => this._dataManager.PaletteCustomColors;
    }

    /// <summary>
    /// Method resets custom colors array. Internal use only.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal void ResetPaletteCustomColors()
    {
        this.PaletteCustomColors = Array.Empty<Color>();
    }

    /// <summary>
    /// Method resets custom colors array. Internal use only.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal bool ShouldSerializePaletteCustomColors()
    {
        if (this.PaletteCustomColors == null ||
            this.PaletteCustomColors.Length == 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Indicates that non-critical chart exceptions will be suppressed.
    /// </summary>
    [
    SRCategory("CategoryAttributeMisc"),
    DefaultValue(false),
    SRDescription("DescriptionAttributeSuppressExceptions"),
    ]
    public bool SuppressExceptions
    {
        set => this.chartPicture.SuppressExceptions = value;
        get => this.chartPicture.SuppressExceptions;
    }


    /// <summary>
    /// "The data source used to populate series data. Series ValueMember properties must be also set."
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    Bindable(true),
    SRDescription("DescriptionAttributeDataSource"),
    DefaultValue(null),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden),
    AttributeProvider(typeof(IListSource))
    ]
    public object DataSource
    {
        get => chartPicture.DataSource;
        set => chartPicture.DataSource = value;
    }

    /// <summary>
    /// Chart named images collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    Bindable(false),
    SRDescription("DescriptionAttributeChart_Images"),
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    ]
    public NamedImagesCollection Images { get; }


    /// <summary>
    /// Chart printing object.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    Bindable(false),
    SRDescription("DescriptionAttributeChart_Printing"),
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public PrintingManager Printing => _printingManager;


    /// <summary>
    /// Chart series collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeChart_Series"),
    Editor("SeriesCollectionEditor", typeof(UITypeEditor)),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    ]
    public SeriesCollection Series => _dataManager.Series;

    /// <summary>
    /// Chart legend collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeLegends"),
    Editor("LegendCollectionEditor", typeof(UITypeEditor)),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    ]
    public LegendCollection Legends => chartPicture.Legends;

    /// <summary>
    /// Chart title collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeTitles"),
    Editor("ChartCollectionEditor", typeof(UITypeEditor)),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    ]
    public TitleCollection Titles => chartPicture.Titles;

    /// <summary>
    /// Chart annotation collection.
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    SRDescription("DescriptionAttributeAnnotations3"),
    Editor("AnnotationCollectionEditor", typeof(UITypeEditor)),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    ]
    public AnnotationCollection Annotations => chartPicture.Annotations;

    /// <summary>
    /// BackImage is not used. Use BackImage property instead.
    /// </summary>
    [
    Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    EditorBrowsableAttribute(EditorBrowsableState.Never),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public override Image BackgroundImage
    {
        get => base.BackgroundImage;
        set => base.BackgroundImage = value;
    }

    /// <summary>
    /// Color palette to use
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    SRDescription("DescriptionAttributePalette"),
    DefaultValue(ChartColorPalette.BrightPastel),
    Editor("ColorPaletteEditor", typeof(UITypeEditor)),
    ]
    public ChartColorPalette Palette
    {
        get => _dataManager.Palette;
        set
        {
            _dataManager.Palette = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Specifies whether smoothing (antialiasing) is applied while drawing chart.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(typeof(AntiAliasingStyles), "All"),
    SRDescription("DescriptionAttributeAntiAlias"),
    Editor("FlagsEnumUITypeEditor", typeof(UITypeEditor)),
    ]
    public System.Windows.Forms.DataVisualization.Charting.AntiAliasingStyles AntiAliasing
    {
        get => chartPicture.AntiAliasing;
        set
        {
            if (chartPicture.AntiAliasing != value)
            {
                chartPicture.AntiAliasing = value;

                this.dirtyFlag = true;
                if (!this.disableInvalidates)
                {
                    this.Invalidate();
                }
            }
        }
    }

    /// <summary>
    /// Specifies the quality of text antialiasing.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(typeof(TextAntiAliasingQuality), "High"),
    SRDescription("DescriptionAttributeTextAntiAliasingQuality")
    ]
    public TextAntiAliasingQuality TextAntiAliasingQuality
    {
        get => chartPicture.TextAntiAliasingQuality;
        set
        {
            chartPicture.TextAntiAliasingQuality = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Specifies whether smoothing is applied while drawing shadows.
    /// </summary>
    [
    SRCategory("CategoryAttributeImage"),
    Bindable(true),
    DefaultValue(true),
    SRDescription("DescriptionAttributeChart_SoftShadows"),
    ]
    public bool IsSoftShadows
    {
        get => chartPicture.IsSoftShadows;
        set
        {
            chartPicture.IsSoftShadows = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Reference to chart area collection
    /// </summary>
    [
    SRCategory("CategoryAttributeChart"),
    Bindable(true),
    SRDescription("DescriptionAttributeChartAreas"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
    Editor("ChartCollectionEditor", typeof(UITypeEditor)),
    ]
    public ChartAreaCollection ChartAreas => chartPicture.ChartAreas;

    /// <summary>
    /// Back ground color for the Chart
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), "White"),
    SRDescription("DescriptionAttributeBackColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public override Color BackColor
    {
        get => chartPicture.BackColor;
        set
        {
            if (chartPicture.BackColor != value)
            {
                chartPicture.BackColor = value;
                this.dirtyFlag = true;
                if (!this.disableInvalidates)
                {
                    this.Invalidate();
                }

                // Call notification event
                this.OnBackColorChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Fore color propery (not used)
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(false),
    Browsable(false),
    DefaultValue(typeof(Color), ""),
    SRDescription("DescriptionAttributeForeColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public override Color ForeColor
    {
        get => Color.Empty;
        set
        {
        }
    }

    /// <summary>
    /// Fore color propery (not used)
    /// </summary>
    [
    SRCategory("CategoryAttributeLayout"),
    Bindable(true),
    DefaultValue(typeof(Size), "300, 300"),
    SRDescription("DescriptionAttributeChart_Size"),
    ]
    public new System.Drawing.Size Size
    {
        get => base.Size;
        set
        {
            chartPicture.InspectChartDimensions(value.Width, value.Height);
            base.Size = value;
        }
    }

    /// <summary>
    /// Series data manipulator
    /// </summary>
    [
    SRCategory("CategoryAttributeData"),
    SRDescription("DescriptionAttributeDataManipulator"),
    Browsable(false),
    DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public DataManipulator DataManipulator => chartPicture.DataManipulator;

    /// <summary>
    /// Chart serializer object.
    /// </summary>
    [
    SRCategory("CategoryAttributeSerializer"),
    SRDescription("DescriptionAttributeChart_Serializer"),
    Browsable(false),
    DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public ChartSerializer Serializer { get; }


    /// <summary>
    /// Title font
    /// </summary>
    [
    SRCategory("CategoryAttributeCharttitle"),
    Bindable(false),
    Browsable(false),
    EditorBrowsableAttribute(EditorBrowsableState.Never),
    DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public new Font Font
    {
        get => base.Font;
        set => base.Font = value;
    }


    /// <summary>
    /// Back Hatch style
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartHatchStyle.None),
    SRDescription("DescriptionAttributeBackHatchStyle"),
    Editor("HatchStyleEditor", typeof(UITypeEditor)),
    ]
    public ChartHatchStyle BackHatchStyle
    {
        get => chartPicture.BackHatchStyle;
        set
        {
            chartPicture.BackHatchStyle = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }



    /// <summary>
    /// Chart area background image
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(""),
    SRDescription("DescriptionAttributeBackImage"),
    NotifyParentPropertyAttribute(true),
    Editor("ImageValueEditor", typeof(UITypeEditor)),
    ]
    public string BackImage
    {
        get => chartPicture.BackImage;
        set
        {
            chartPicture.BackImage = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Chart area background image drawing mode.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartImageWrapMode.Tile),
    NotifyParentPropertyAttribute(true),
    SRDescription("DescriptionAttributeImageWrapMode"),
    ]
    public ChartImageWrapMode BackImageWrapMode
    {
        get => chartPicture.BackImageWrapMode;
        set
        {
            chartPicture.BackImageWrapMode = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Background image transparent color.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), ""),
    NotifyParentPropertyAttribute(true),
    SRDescription("DescriptionAttributeImageTransparentColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public Color BackImageTransparentColor
    {
        get => chartPicture.BackImageTransparentColor;
        set
        {
            chartPicture.BackImageTransparentColor = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Background image alignment used by ClampUnscale drawing mode.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartImageAlignmentStyle.TopLeft),
    NotifyParentPropertyAttribute(true),
    SRDescription("DescriptionAttributeBackImageAlign"),
    ]
    public ChartImageAlignmentStyle BackImageAlignment
    {
        get => chartPicture.BackImageAlignment;
        set
        {
            chartPicture.BackImageAlignment = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// A type for the background gradient
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(GradientStyle.None),
    SRDescription("DescriptionAttributeBackGradientStyle"),
    Editor("GradientEditor", typeof(UITypeEditor)),
    ]
    public GradientStyle BackGradientStyle
    {
        get => chartPicture.BackGradientStyle;
        set
        {
            chartPicture.BackGradientStyle = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// The second color which is used for a gradient
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), ""),
    SRDescription("DescriptionAttributeBackSecondaryColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public Color BackSecondaryColor
    {
        get => chartPicture.BackSecondaryColor;
        set
        {
            chartPicture.BackSecondaryColor = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Border color for the Chart
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(false),
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never),
    DefaultValue(typeof(Color), "White"),
    SRDescription("DescriptionAttributeBorderColor"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public Color BorderColor
    {
        get => chartPicture.BorderColor;
        set
        {
            chartPicture.BorderColor = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// The width of the border line
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(false),
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never),
    DefaultValue(1),
    SRDescription("DescriptionAttributeChart_BorderlineWidth"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public int BorderWidth
    {
        get => chartPicture.BorderWidth;
        set
        {
            chartPicture.BorderWidth = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// The style of the border line
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(false),
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never),
    DefaultValue(ChartDashStyle.NotSet),
    SRDescription("DescriptionAttributeBorderDashStyle"),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    SerializationVisibilityAttribute(SerializationVisibility.Hidden)
    ]
    public ChartDashStyle BorderDashStyle
    {
        get => chartPicture.BorderDashStyle;
        set
        {
            chartPicture.BorderDashStyle = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Border color for the Chart
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(typeof(Color), "White"),
    SRDescription("DescriptionAttributeBorderColor"),
    TypeConverter(typeof(ColorConverter)),
    Editor("ChartColorEditor", typeof(UITypeEditor)),
    ]
    public Color BorderlineColor
    {
        get => chartPicture.BorderColor;
        set
        {
            chartPicture.BorderColor = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// The width of the border line
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(1),
    SRDescription("DescriptionAttributeChart_BorderlineWidth"),
    ]
    public int BorderlineWidth
    {
        get => chartPicture.BorderWidth;
        set
        {
            chartPicture.BorderWidth = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// The style of the border line
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(ChartDashStyle.NotSet),
    SRDescription("DescriptionAttributeBorderDashStyle"),
    ]
    public ChartDashStyle BorderlineDashStyle
    {
        get => chartPicture.BorderDashStyle;
        set
        {
            chartPicture.BorderDashStyle = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }


    /// <summary>
    /// Chart border skin style.
    /// </summary>
    [
    SRCategory("CategoryAttributeAppearance"),
    Bindable(true),
    DefaultValue(BorderSkinStyle.None),
    SRDescription("DescriptionAttributeBorderSkin"),
    NotifyParentPropertyAttribute(true),
    TypeConverterAttribute(typeof(LegendConverter)),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
    ]
    public BorderSkin BorderSkin
    {
        get => chartPicture.BorderSkin;
        set
        {
            chartPicture.BorderSkin = value;
            this.dirtyFlag = true;
            if (!this.disableInvalidates)
            {
                this.Invalidate();
            }
        }
    }

    /// <summary>
    /// Build number of the control
    /// </summary>
    [
    SRDescription("DescriptionAttributeChart_BuildNumber"),
    Browsable(false),
    EditorBrowsable(EditorBrowsableState.Never),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
    DefaultValue("")
    ]
    public string BuildNumber
    {
        get
        {
            // Get build number from the assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly is null)
                return string.Empty;

            var buildNumber = assembly.FullName.AsSpan();
            int versionIndex = buildNumber.IndexOf("VERSION=", StringComparison.OrdinalIgnoreCase);
            if (versionIndex >= 0)
            {
                buildNumber = buildNumber[(versionIndex + 8)..];
            }

            versionIndex = buildNumber.IndexOf(',');
            if (versionIndex >= 0)
            {
                buildNumber = buildNumber[..versionIndex];
            }

            return buildNumber.ToString();
        }
    }

    #endregion

    #region internal properties
    /// <summary>
    /// Initial DPi. Will fill only after first DPI scaling in PerMonitorV2 mode.
    /// </summary>
    internal static float InitialDPI { get; private set; }

    /// <summary>
    /// CurrentDPI / InitialDPI for PerMonitorV2 mode. <br/>
    /// </summary>
    internal static float DPIScale { get; private set; } = 1;
    #endregion

    #region Control public methods

    /// <summary>
    /// Loads chart appearance template from file.
    /// </summary>
    /// <param name="name">Template file name to load from.</param>
    public void LoadTemplate(string name)
    {
        chartPicture.LoadTemplate(name);
    }

    /// <summary>
    /// Loads chart appearance template from stream.
    /// </summary>
    /// <param name="stream">Template stream to load from.</param>
    public void LoadTemplate(Stream stream)
    {
        chartPicture.LoadTemplate(stream);
    }

    /// <summary>
    /// Applies palette colors to series or data points.
    /// </summary>
    public void ApplyPaletteColors()
    {
        // Apply palette colors to series
        this._dataManager.ApplyPaletteColors();

        // Apply palette colors to data Points in series
        foreach (Series series in this.Series)
        {
            // Check if palette colors should be aplied to the points
            bool applyToPoints;
            if (series.Palette != ChartColorPalette.None)
            {
                applyToPoints = true;
            }
            else
            {
                IChartType chartType = this._chartTypeRegistry.GetChartType(series.ChartType);
                applyToPoints = chartType.ApplyPaletteColorsToPoints;
            }

            // Apply palette colors to the points
            if (applyToPoints)
            {
                series.ApplyPaletteColors();
            }
        }
    }

    /// <summary>
    /// Checks if control is in design mode.
    /// </summary>
    /// <returns>True if control is in design mode.</returns>
    internal bool IsDesignMode()
    {
        return this.DesignMode;
    }

    /// <summary>
    /// Reset auto calculated chart properties values to "Auto".
    /// </summary>
    public void ResetAutoValues()
    {
        // Reset auto calculated series properties values 
        foreach (Series series in this.Series)
        {
            series.ResetAutoValues();
        }

        // Reset auto calculated axis properties values 
        foreach (ChartArea chartArea in this.ChartAreas)
        {
            chartArea.ResetAutoValues();
        }
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult objects.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Hit test result object</returns>
    public HitTestResult HitTest(int x, int y)
    {
        return selection.HitTest(x, y);
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult object.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="ignoreTransparent">Indicates that transparent elements should be ignored.</param>
    /// <returns>Hit test result object</returns>
    public HitTestResult HitTest(int x, int y, bool ignoreTransparent)
    {
        return selection.HitTest(x, y, ignoreTransparent);
    }

    /// <summary>
    /// This method performs the hit test and returns a HitTestResult object.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="requestedElement">Only this chart element will be hit tested.</param>
    /// <returns>Hit test result object</returns>
    public HitTestResult HitTest(int x, int y, ChartElementType requestedElement)
    {
        return selection.HitTest(x, y, requestedElement);
    }

    /// <summary>
    /// Call this method to determine the  chart element,
    /// if any, that is located at a point defined by the given X and Y 
    /// coordinates.
    /// <seealso cref="HitTestResult"/></summary>
    /// <param name="x">The X coordinate for the point in question.
    /// Often obtained from a parameter in an event
    /// (e.g. the X parameter value in the MouseDown event).</param>
    /// <param name="y">The Y coordinate for the point in question.
    /// Often obtained from a parameter in an event
    /// (e.g. the Y parameter value in the MouseDown event).</param>
    /// <param name="ignoreTransparent">Indicates that transparent 
    /// elements should be ignored.</param>
    /// <param name="requestedElement">
    /// An array of type which specify the types                  
    /// to test for, on order to filter the result. If omitted checking for                 
    /// elementTypes will be ignored and all kind of elementTypes will be 
    /// valid.
    ///  </param>
    /// <returns>
    /// A array of <see cref="HitTestResult"/> objects,
    /// which provides information concerning the  chart element
    /// (if any) that is at the specified location. Result contains at least
    /// one element, which could be ChartElementType.Nothing. 
    /// The objects in the result are sorted in from top to bottom of 
    /// different layers of control. </returns>
    /// <remarks>Call this method to determine the  gauge element
    /// (if any) that is located at a specified point. Often this method is used in
    /// some mouse-related event (e.g. MouseDown)
    /// to determine what  gauge element the end-user clicked on.
    /// The X and Y mouse coordinates obtained from the
    /// event parameters are then used for the X and Y parameter              
    /// values of this method call.   The returned 
    /// <see cref="HitTestResult"/> object's properties
    /// can then be used to determine what  chart element was clicked on,
    /// and also provides a reference to the actual object selected (if 
    /// any).</remarks>
    public IList<HitTestResult> HitTest(int x, int y, bool ignoreTransparent, params ChartElementType[] requestedElement)
    {
        return this.selection.HitTest(x, y, ignoreTransparent, requestedElement);
    }


    /// <summary>
    /// Gets the chart element outline.
    /// </summary>
    /// <param name="element">The chart object.</param>
    /// <param name="elementType">Type of the element.</param>
    /// <returns> A <see cref="ChartElementOutline"/> object which contains
    /// 1) An array of points in absolute coordinates which can be used as outline markers around this chart element.
    /// 2) A GraphicsPath for drawing outline around this chart element.
    /// </returns>
    /// <remarks>
    /// If the <paramref name="element"/> is not part of the chart or <paramref name="elementType"/> cannot be combined 
    /// with <paramref name="element"/> then the result will contain empty array of marker points. 
    /// The marker points are sorted clockwise.
    /// </remarks>
    public ChartElementOutline GetChartElementOutline(object element, ChartElementType elementType)
    {
        return this.selection.GetChartElementOutline(element, elementType);
    }

    #endregion

    #region Control protected methods

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);

        using Graphics g = Graphics.FromHwndInternal(Handle);
        ControlPaint.DrawFocusRectangle(g, new Rectangle(1, 1, Size.Width - 2, Size.Height - 2));
    }

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        using Graphics g = Graphics.FromHwndInternal(Handle);
        using Brush b = new SolidBrush(BackColor);
        Rectangle topBorder = new Rectangle(1, 1, Size.Width - 2, 1);
        g.FillRectangle(b, topBorder);

        Rectangle rightBorder = new Rectangle(Size.Width - 2, 1, 1, Size.Height - 2);
        g.FillRectangle(b, rightBorder);

        Rectangle bottomBorder = new Rectangle(1, Size.Height - 2, Size.Width - 2, 1);
        g.FillRectangle(b, bottomBorder);

        Rectangle leftBorder = new Rectangle(1, 1, 1, Size.Height - 2);
        g.FillRectangle(b, leftBorder);
    }

    /// <inheritdoc cref="Control.RescaleConstantsForDpi"/>
    protected override void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
    {
        if (InitialDPI == 0)
            InitialDPI = deviceDpiOld;


        Font oldDefFont = FontCache.DefaultFont;
        Font oldDefBoldFont = FontCache.TryGetDefaultBoldFont();
        float factor = (float)deviceDpiNew / deviceDpiOld;
        DPIScale = MathF.Round(deviceDpiNew / InitialDPI, 2);

        // We need to scale all fonts.
        foreach (var s in this.Series)
        {
            foreach (var p in s.Points)
            {
                if (p.IsCustomPropertySet(CommonCustomProperties.Font) && p.GetAttributeObject(CommonCustomProperties.Font) is Font font)
                {
                    if (font.Equals(oldDefFont))
                        font = FontCache.DefaultFont;
                    else
                        font = chartPicture.FontCache.GetFont(s.font, s.font.Size * factor);

                    p.SetAttributeObject(CommonCustomProperties.Font, font.WithSize(font.Size * factor));
                }
            }

            if (s.font is null)
                continue;

            if (s.font.Equals(oldDefFont))
                s.font = FontCache.DefaultFont;
            else
                s.font = chartPicture.FontCache.GetFont(s.font, s.font.Size * factor);
        }

        foreach (var ca in this.ChartAreas)
        {
            foreach (var a in ca.Axes)
            {
                if (a is null)
                    continue;

                if (a._titleFont is not null)
                {
                    if (a._titleFont.Equals(oldDefFont))
                        a._titleFont = FontCache.DefaultFont;
                    else
                        a._titleFont = chartPicture.FontCache.GetFont(a._titleFont, a._titleFont.Size * factor);
                }

                if (a.labelStyle?._font is not null)
                {
                    if (a.labelStyle._font.Equals(oldDefFont))
                        a.labelStyle._font = FontCache.DefaultFont;
                    else
                        a.labelStyle._font = chartPicture.FontCache.GetFont(a.labelStyle._font, a.labelStyle._font.Size * factor);
                }

                if (a.autoLabelFont is not null)
                {
                    if (a.autoLabelFont.Equals(oldDefFont))
                        a.autoLabelFont = FontCache.DefaultFont;
                    else
                        a.autoLabelFont = chartPicture.FontCache.GetFont(a.autoLabelFont, a.autoLabelFont.Size * factor);
                }

                foreach (var sl in a.StripLines)
                {
                    if (sl._font is not null)
                    {
                        if (sl._font.Equals(oldDefFont))
                            sl._font = FontCache.DefaultFont;
                        else
                            sl._font = chartPicture.FontCache.GetFont(sl._font, sl._font.Size * factor);
                    }
                }
            }
        }

        foreach (var a in this.Annotations)
        {
            if (a._textFont is not null)
            {
                if (a._textFont.Equals(oldDefFont))
                    a._textFont = FontCache.DefaultFont;
                else
                    a._textFont = chartPicture.FontCache.GetFont(a._textFont, a._textFont.Size * factor);
            }
        }

        foreach (var t in this.Titles)
        {
            if (t._font is not null)
            {
                if (t._font.Equals(oldDefFont))
                    t._font = FontCache.DefaultFont;
                else
                    t._font = chartPicture.FontCache.GetFont(t._font, t._font.Size * factor);
            }
        }

        foreach (var l in this.Legends)
        {
            if (l._font is not null)
            {
                if (l._font.Equals(oldDefFont))
                    l._font = FontCache.DefaultFont;
                else
                    l._font = chartPicture.FontCache.GetFont(l._font, l._font.Size * factor);
            }

            if (l._titleFont is not null)
            {
                if (l._titleFont.Equals(oldDefBoldFont))
                    l._titleFont = FontCache.DefaultBoldFont;
                else
                    l._titleFont = chartPicture.FontCache.GetFont(l._titleFont, l._titleFont.Size * factor);
            }

            if (l.autofitFont is not null)
            {
                if (l.autofitFont.Equals(oldDefFont))
                    l.autofitFont = FontCache.DefaultFont;
                else
                    l.autofitFont = chartPicture.FontCache.GetFont(l.autofitFont, l.autofitFont.Size * factor);
            }

            foreach (var lc in l.CellColumns)
            {
                if (lc._font is not null)
                {
                    if (lc._font.Equals(oldDefFont))
                        lc._font = FontCache.DefaultFont;
                    else
                        lc._font = chartPicture.FontCache.GetFont(lc._font, lc._font.Size * factor);
                }

                if (lc._headerFont is not null)
                {
                    if (lc._headerFont.Equals(oldDefBoldFont))
                        lc._headerFont = FontCache.DefaultBoldFont;
                    else
                        lc._headerFont = chartPicture.FontCache.GetFont(lc._headerFont, lc._headerFont.Size * factor);
                }
            }

            foreach (var li in l.legendItems)
            {
                foreach (var lc in li.Cells)
                {
                    if (lc._font is not null)
                    {
                        if (lc._font.Equals(oldDefFont))
                            lc._font = FontCache.DefaultFont;
                        else
                            lc._font = chartPicture.FontCache.GetFont(lc._font, lc._font.Size * factor);
                    }
                }
            }
        }

        base.RescaleConstantsForDpi(deviceDpiOld, deviceDpiNew);
    }

    #endregion

    #region ISupportInitialize implementation

    /// <summary>
    /// Signals the object that initialization is starting.
    /// </summary>
    public void BeginInit()
    {
        // Disable control invalidation
        disableInvalidates = true;
    }

    /// <summary>
    /// Signals the object that initialization is complete.
    /// </summary>
    public void EndInit()
    {
        // Enable control invalidation
        disableInvalidates = false;

        // If control is dirty - invalidate it
        if (this.dirtyFlag)
        {
            base.Invalidate();
        }
    }

    #endregion

    #region Control mouse events

    /// <summary>
    /// Raises the <see cref="E:System.Windows.Forms.Control.CursorChanged"/> event.
    /// </summary>
    /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
    protected override void OnCursorChanged(EventArgs e)
    {
        this.defaultCursor = this.Cursor;
        base.OnCursorChanged(e);
    }


    /// <summary>
    /// Mouse button pressed in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        OnChartMouseDown(e);
    }

    /// <summary>
    /// Mouse button pressed in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    internal void OnChartMouseDown(MouseEventArgs e)
    {
        bool handled = false;

        if (!handled)
        {
            // Notify annotation object collection about the mouse down event
            this.Annotations.OnMouseDown(e, ref handled);
        }

        // Loop through all areas and notify required object about the event
        if (!handled)
        {
            foreach (ChartArea area in this.ChartAreas)
            {
                // No cursor or scroll bar support in 3D
                if (!area.Area3DStyle.Enable3D &&
                    !area.chartAreaIsCurcular
                    && area.Visible)
                {
                    foreach (Axis axis in area.Axes)
                    {
                        // Notify axis scroll bar
                        axis.ScrollBar.ScrollBar_MouseDown(this, e);
                    }

                    // Notify area X and Y cursors
                    area.CursorX.Cursor_MouseDown(this, e);
                    area.CursorY.Cursor_MouseDown(this, e);
                }
            }
        }

        // Call the base class
        base.OnMouseDown(e);
    }

    /// <summary>
    /// Mouse button up in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnMouseUp(MouseEventArgs e)
    {
        OnChartMouseUp(e);
    }

    /// <summary>
    /// Mouse button up in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    internal void OnChartMouseUp(MouseEventArgs e)
    {
        // Loop through all areas and notify required object about the event
        foreach (ChartArea area in this.ChartAreas)
        {
            // No cursor or scroll bar support in 3D
            if (!area.Area3DStyle.Enable3D &&
                !area.chartAreaIsCurcular
                && area.Visible)
            {
                foreach (Axis axis in area.Axes)
                {
                    // Notify axis scroll bar
                    axis.ScrollBar.ScrollBar_MouseUp(this, e);
                }

                // Notify area X and Y cursors
                area.CursorX.Cursor_MouseUp(this, e);
                area.CursorY.Cursor_MouseUp(this, e);
            }
        }

        // Notify annotation object collection about the mouse down event
        this.Annotations.OnMouseUp(e);

        // Call the base class
        base.OnMouseUp(e);
    }

    /// <summary>
    /// Mouse moved in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected override void OnMouseMove(MouseEventArgs e)
    {
        OnChartMouseMove(e);
    }

    /// <summary>
    /// Mouse moved in the control.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    internal void OnChartMouseMove(MouseEventArgs e)
    {
        // Flag which indicates if event was already isHandled
        bool handled = false;

        // Loop through all areas and notify required object about the event
        foreach (ChartArea area in this.ChartAreas)
        {
            // No cursor or scroll bar support in 3D
            if (!area.Area3DStyle.Enable3D &&
                !area.chartAreaIsCurcular
                && area.Visible)
            {
                foreach (Axis axis in area.Axes)
                {
                    // Notify axis scroll bar
                    axis.ScrollBar.ScrollBar_MouseMove(e, ref handled);
                }

                // Notify area X and Y cursors
                area.CursorX.Cursor_MouseMove(e, ref handled);
                area.CursorY.Cursor_MouseMove(e, ref handled);
            }
        }

        // Notify Selection object for tool tips processing
        if (!handled)
        {
            this.selection.Selection_MouseMove(this, e);
            // Notify annotation object collection about the mouse down event
            this.Annotations.OnMouseMove(e);
        }

        // Call the base class
        base.OnMouseMove(e);
    }

    /// <summary>
    /// Mouse was double clicked on the control.
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnDoubleClick(EventArgs e)
    {
        // Notify annotation object collection about the mouse down event
        this.Annotations.OnDoubleClick();

        // Call the base class
        base.OnDoubleClick(e);
    }

    #endregion

    #region Chart get tool tip text events

    /// <summary>
    /// Called before showing the tooltip to get the tooltip text.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_GetToolTipText"),
    SRCategory("CategoryAttributeToolTips")]
    public event EventHandler<ToolTipEventArgs> GetToolTipText;

    /// <summary>
    /// Checks if GetToolTipEvent is used
    /// </summary>
    /// <returns>True if event is used</returns>
    internal bool IsToolTipEventUsed()
    {
        if (GetToolTipText != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Cursor event arguments.</param>
    internal void OnGetToolTipText(ToolTipEventArgs arguments)
    {
        GetToolTipText?.Invoke(this, arguments);
    }

    #endregion

    #region Chart area cursor and selection events

    /// <summary>
    /// Called when cursor position is about to change.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_CursorPositionChanging"),
    SRCategory("CategoryAttributeCursor")]
    public event EventHandler<CursorEventArgs> CursorPositionChanging;

    /// <summary>
    /// Called when cursor position is changed.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_CursorPositionChanged"),
    SRCategory("CategoryAttributeCursor")]
    public event EventHandler<CursorEventArgs> CursorPositionChanged;

    /// <summary>
    /// Called when selection start/end position is about to change.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_SelectionRangeChanging"),
    SRCategory("CategoryAttributeCursor")]
    public event EventHandler<CursorEventArgs> SelectionRangeChanging;

    /// <summary>
    /// Called when selection start/end position is changed.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_SelectionRangeChanged"),
    SRCategory("CategoryAttributeCursor")]
    public event EventHandler<CursorEventArgs> SelectionRangeChanged;

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Cursor event arguments.</param>
    internal void OnCursorPositionChanging(CursorEventArgs arguments)
    {
        CursorPositionChanging?.Invoke(this, arguments);
    }

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Cursor event arguments.</param>
    internal void OnCursorPositionChanged(CursorEventArgs arguments)
    {
        CursorPositionChanged?.Invoke(this, arguments);
    }

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Cursor event arguments.</param>
    internal void OnSelectionRangeChanging(CursorEventArgs arguments)
    {
        SelectionRangeChanging?.Invoke(this, arguments);
    }

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Cursor event arguments.</param>
    internal void OnSelectionRangeChanged(CursorEventArgs arguments)
    {

        SelectionRangeChanged?.Invoke(this, arguments);
    }

    #endregion

    #region Axis data scaleView position/size changing events

    /// <summary>
    /// Called when axis scaleView position/size is about to change.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_AxisViewChanging"),
    SRCategory("CategoryAttributeAxisView")]
    public event EventHandler<ViewEventArgs> AxisViewChanging;

    /// <summary>
    /// Called when axis scaleView position/size is changed.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_AxisViewChanged"),
    SRCategory("CategoryAttributeAxisView")]
    public event EventHandler<ViewEventArgs> AxisViewChanged;


    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Axis scaleView event arguments.</param>
    internal void OnAxisViewChanging(ViewEventArgs arguments)
    {
        AxisViewChanging?.Invoke(this, arguments);
    }

    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Axis scaleView event arguments.</param>
    internal void OnAxisViewChanged(ViewEventArgs arguments)
    {
        AxisViewChanged?.Invoke(this, arguments);
    }

    #endregion

    #region Axis scroll bar events

    /// <summary>
    /// Called when axis scroll bar is used by user.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_AxisScrollBarClicked"),
    SRCategory("CategoryAttributeAxisView")]
    public event EventHandler<ScrollBarEventArgs> AxisScrollBarClicked;


    /// <summary>
    /// Calls event delegate.
    /// </summary>
    /// <param name="arguments">Axis scroll bar event arguments.</param>
    internal void OnAxisScrollBarClicked(ScrollBarEventArgs arguments)
    {
        AxisScrollBarClicked?.Invoke(this, arguments);
    }

    #endregion

    #region Painting events

    /// <summary>
    /// Called when chart element is painted.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_PostPaint"),
    SRCategory("CategoryAttributeAppearance")]
    public event EventHandler<ChartPaintEventArgs> PostPaint;

    /// <summary>
    /// Called when chart element back ground is painted.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_PrePaint"),
    SRCategory("CategoryAttributeAppearance")]
    public event EventHandler<ChartPaintEventArgs> PrePaint;

    /// <summary>
    /// Fires when chart element backround must be drawn. 
    /// This event is fired for elements like: ChartPicture, ChartArea and Legend
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnPrePaint(ChartPaintEventArgs e)
    {
        PrePaint?.Invoke(this, e);
    }

    /// <summary>
    /// Fires when chart element backround must be drawn. 
    /// This event is fired for elements like: ChartPicture, ChartArea and Legend
    /// </summary>
    /// <param name="e">Event arguments.</param>
    internal void CallOnPrePaint(ChartPaintEventArgs e)
    {
        this.OnPrePaint(e);
    }

    /// <summary>
    /// Fires when chart element must be drawn. 
    /// This event is fired for elements like: ChartPicture, ChartArea and Legend
    /// </summary>
    /// <param name="e">Event arguments.</param>
    protected virtual void OnPostPaint(ChartPaintEventArgs e)
    {
        PostPaint?.Invoke(this, e);
    }

    /// <summary>
    /// Fires when chart element must be drawn. 
    /// This event is fired for elements like: ChartPicture, ChartArea and Legend
    /// </summary>
    /// <param name="e">Event arguments.</param>
    internal void CallOnPostPaint(ChartPaintEventArgs e)
    {
        this.OnPostPaint(e);
    }

    #endregion

    #region Customize event

    /// <summary>
    /// Fires just before the chart image is drawn. Use this event to customize the chart picture.
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_Customize")
    ]
    public event EventHandler Customize;


    /// <summary>
    /// Fires when all chart data is prepared to be customized before drawing. 
    /// </summary>
    [
    SRDescription("DescriptionAttributeChart_OnCustomize")
    ]
    protected virtual void OnCustomize()
    {
        Customize?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Fires when all chart data is prepared to be customized before drawing. 
    /// </summary>
    internal void CallOnCustomize()
    {
        this.OnCustomize();
    }

    /// <summary>
    /// Use this event to customize chart legend.
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_CustomizeLegend")
    ]
    public event EventHandler<CustomizeLegendEventArgs> CustomizeLegend;


    /// <summary>
    /// Fires when all chart data is prepared to be customized before drawing. 
    /// </summary>
    [
        SRDescription("DescriptionAttributeChart_OnCustomizeLegend")
    ]
    protected virtual void OnCustomizeLegend(LegendItemsCollection legendItems, string legendName)
    {
        CustomizeLegend?.Invoke(this, new CustomizeLegendEventArgs(legendItems, legendName));
    }

    /// <summary>
    /// Fires when all chart data is prepared to be customized before drawing. 
    /// </summary>
    internal void CallOnCustomizeLegend(LegendItemsCollection legendItems, string legendName)
    {
        this.OnCustomizeLegend(legendItems, legendName);
    }
    #endregion

    #region Annotation events

    /// <summary>
    /// Fires when annotation text was changed. 
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_AnnotationTextChanged"),
    SRCategory("CategoryAttributeAnnotation")
    ]
    public event EventHandler AnnotationTextChanged;

    /// <summary>
    /// Fires when annotation text is changed.
    /// </summary>
    /// <param name="annotation">Annotation which text was changed.</param>
    internal void OnAnnotationTextChanged(Annotation annotation)
    {
        AnnotationTextChanged?.Invoke(annotation, EventArgs.Empty);
    }

    /// <summary>
    /// Fires when selected annotation changes. 
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_AnnotationSelectionChanged"),
    SRCategory("CategoryAttributeAnnotation")
    ]
    public event EventHandler AnnotationSelectionChanged;
    /// <summary>
    /// Fires when annotation position was changed.
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_AnnotationPositionChanged"),
    SRCategory("CategoryAttributeAnnotation")
    ]
    public event EventHandler AnnotationPositionChanged;

    /// <summary>
    /// Fires when annotation position is changing.
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_AnnotationPositionChanging"),
    SRCategory("CategoryAttributeAnnotation")
    ]
    public event EventHandler<AnnotationPositionChangingEventArgs> AnnotationPositionChanging;

    /// <summary>
    /// Fires when annotation is placed by the user on the chart. 
    /// </summary>
    [
    SRDescription("DescriptionAttributeChartEvent_AnnotationPlaced"),
    SRCategory("CategoryAttributeAnnotation")
    ]
    public event EventHandler AnnotationPlaced;


    /// <summary>
    /// Fires when annotation is placed by the user on the chart.
    /// </summary>
    /// <param name="annotation">Annotation which was placed.</param>
    internal void OnAnnotationPlaced(Annotation annotation)
    {
        AnnotationPlaced?.Invoke(annotation, EventArgs.Empty);
    }

    /// <summary>
    /// Fires when selected annotation changes. 
    /// </summary>
    /// <param name="annotation">Annotation which have it's selection changed.</param>
    internal void OnAnnotationSelectionChanged(Annotation annotation)
    {
        AnnotationSelectionChanged?.Invoke(annotation, EventArgs.Empty);
    }

    /// <summary>
    /// Fires when annotation position was changed.
    /// </summary>
    /// <param name="annotation">Annotation which have it's position changed.</param>
    internal void OnAnnotationPositionChanged(Annotation annotation)
    {
        AnnotationPositionChanged?.Invoke(annotation, EventArgs.Empty);
    }

    /// <summary>
    /// Fires when annotation position is changing.
    /// </summary>
    /// <param name="args">Event arguments.</param>
    /// <returns>True if event was processed.</returns>
    internal bool OnAnnotationPositionChanging(ref AnnotationPositionChangingEventArgs args)
    {
        if (AnnotationPositionChanging != null)
        {
            AnnotationPositionChanging(args.Annotation, args);
            return true;
        }

        return false;
    }

    #endregion

    #region Control DataBind method

    /// <summary>
    /// Data binds control to the selected data source.
    /// </summary>
    public void DataBind()
    {
        this.chartPicture.DataBind();
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    public void AlignDataPointsByAxisLabel()
    {
        this.chartPicture.AlignDataPointsByAxisLabel(false, PointSortOrder.Ascending);
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    /// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
    public void AlignDataPointsByAxisLabel(string series)
    {
        //Check arguments
        if (series == null)
            throw new ArgumentNullException(nameof(series));

        // Create list of series
        ArrayList seriesList = new ArrayList();
        string[] seriesNames = series.Split(',');
        foreach (string name in seriesNames)
        {
            seriesList.Add(this.Series[name.Trim()]);
        }

        // Align series
        this.chartPicture.AlignDataPointsByAxisLabel(seriesList, false, PointSortOrder.Ascending);
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    /// <param name="series">Comma separated list of series that should be aligned by axis label.</param>
    /// <param name="sortingOrder">Points sorting order by axis labels.</param>
    public void AlignDataPointsByAxisLabel(string series, PointSortOrder sortingOrder)
    {
        //Check arguments
        if (series == null)
            throw new ArgumentNullException(nameof(series));

        // Create list of series
        ArrayList seriesList = new ArrayList();
        string[] seriesNames = series.Split(',');
        foreach (string name in seriesNames)
        {
            seriesList.Add(this.Series[name.Trim()]);
        }

        // Align series
        this.chartPicture.AlignDataPointsByAxisLabel(seriesList, true, sortingOrder);
    }

    /// <summary>
    /// Aligns data points using their axis labels.
    /// </summary>
    /// <param name="sortingOrder">Points sorting order by axis labels.</param>
    public void AlignDataPointsByAxisLabel(PointSortOrder sortingOrder)
    {
        this.chartPicture.AlignDataPointsByAxisLabel(true, sortingOrder);
    }

    /// <summary>
    /// Automatically creates and binds series to specified data table. 
    /// Each column of the table becomes a Y value in a separate series.
    /// Series X value field may also be provided. 
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="xField">Name of the field for series X values.</param>
    public void DataBindTable(
        IEnumerable dataSource,
        string xField)
    {
        this.chartPicture.DataBindTable(
            dataSource,
            xField);
    }

    /// <summary>
    /// Automatically creates and binds series to specified data table. 
    /// Each column of the table becomes a Y value in a separate series.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    public void DataBindTable(IEnumerable dataSource)
    {
        this.chartPicture.DataBindTable(
            dataSource,
            string.Empty);
    }

    /// <summary>
    /// Data bind chart to the table. Series will be automatically added to the chart depending on
    /// the number of unique values in the seriesGroupByField column of the data source.
    /// Data source can be the DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
    /// <param name="xField">Name of the field for X values.</param>
    /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
    /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
    public void DataBindCrossTable(
        IEnumerable dataSource,
        string seriesGroupByField,
        string xField,
        string yFields,
        string otherFields)
    {
        this.chartPicture.DataBindCrossTab(
            dataSource,
            seriesGroupByField,
            xField,
            yFields,
            otherFields,
            false,
            PointSortOrder.Ascending);
    }

    /// <summary>
    /// Data bind chart to the table. Series will be automatically added to the chart depending on
    /// the number of unique values in the seriesGroupByField column of the data source.
    /// Data source can be the DataView, DataSet, DataTable or DataRow.
    /// </summary>
    /// <param name="dataSource">Data source.</param>
    /// <param name="seriesGroupByField">Name of the field used to group data into series.</param>
    /// <param name="xField">Name of the field for X values.</param>
    /// <param name="yFields">Comma separated name(s) of the field(s) for Y value(s).</param>
    /// <param name="otherFields">Other point properties binding rule in format: PointProperty=Field[{Format}] [,PointProperty=Field[{Format}]]. For example: "Tooltip=Price{C1},Url=WebSiteName".</param>
    /// <param name="sortingOrder">Series will be sorted by group field values in specified order.</param>
    public void DataBindCrossTable(
        IEnumerable dataSource,
        string seriesGroupByField,
        string xField,
        string yFields,
        string otherFields,
        PointSortOrder sortingOrder)
    {
        this.chartPicture.DataBindCrossTab(
            dataSource,
            seriesGroupByField,
            xField,
            yFields,
            otherFields,
            true,
            sortingOrder);
    }

    #endregion

    #region Special Extension Methods and Properties


    /// <summary>
    /// Gets the requested chart service.
    /// </summary>
    /// <param name="serviceType">AxisName of requested service.</param>
    /// <returns>Instance of the service or null if it can't be found.</returns>
    public new object GetService(Type serviceType)
    {
        // Check arguments
        if (serviceType is null)
            throw new ArgumentNullException(nameof(serviceType));

        return serviceContainer?.GetService(serviceType) ?? base.GetService(serviceType);
    }

    /// <summary>
    /// Called when a numeric value has to be converted to a string.
    /// </summary>
    [SRDescription("DescriptionAttributeChartEvent_PrePaint")]
    public event EventHandler<FormatNumberEventArgs> FormatNumber;

    /// <summary>
    /// Called when a numeric value has to be converted to a string.
    /// </summary>
    /// <param name="caller">Event caller. Can be <see cref="ChartPicture"/>, <see cref="ChartArea"/>, <see cref="Legend"/>, <see cref="Charting.Series"/>, <see cref="DataPoint"/> etc.</param>
    /// <param name="e">Event arguments.</param>
    protected internal void OnFormatNumber(object caller, FormatNumberEventArgs e)
    {
        FormatNumber?.Invoke(caller, e);
    }

    #endregion

    #region Accessibility

    // Current chart accessibility object
    private ChartAccessibleObject _chartAccessibleObject;

    /// <summary>
    /// Overridden to return the custom AccessibleObject for the entire chart.
    /// </summary>
    /// <returns>Chart accessibility object.</returns>
    protected override AccessibleObject CreateAccessibilityInstance()
    {
        this._chartAccessibleObject ??= new ChartAccessibleObject(this);
        return this._chartAccessibleObject;
    }

    /// <summary>
    /// Reset accessibility object children.
    /// </summary>
    private void ResetAccessibilityObject()
    {
        this._chartAccessibleObject?.ResetChildren();
    }

    #endregion // Accessibility

    #region IDisposable override

    /// <summary>
    /// Disposing control resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        // call first because font cache
        base.Dispose(disposing);
        if (disposing)
        {
            // Dispose managed objects here
            _imageLoader.Dispose();
            Images.Dispose();
            _chartTypeRegistry.Dispose();
            serviceContainer.RemoveService(typeof(Chart));
            serviceContainer.Dispose();
            // Dispose selection manager
            selection.Dispose();
            // Dispose print manager
            _printingManager.Dispose();
            // Dispose buffer
            if (paintBufferBitmap is not null)
            {
                paintBufferBitmap.Dispose();
                paintBufferBitmap = null;
            }

            if (paintBufferBitmapGraphics is not null)
            {
                paintBufferBitmapGraphics.Dispose();
                paintBufferBitmapGraphics = null;
            }

            // The chart picture will be the last to be disposed
            chartPicture.Dispose();
        }

        _disposedValue = true;
    }
    #endregion
}

#region Customize event delegate

/// <summary>
/// Chart legend customize events arguments
/// </summary>
public class CustomizeLegendEventArgs : EventArgs
{
    /// <summary>
    /// Default constructor is not accessible
    /// </summary>
    private CustomizeLegendEventArgs()
    {
    }

    /// <summary>
    /// Customize legend event arguments constructor
    /// </summary>
    /// <param name="legendItems">Legend items collection.</param>
    public CustomizeLegendEventArgs(LegendItemsCollection legendItems)
    {
        this.LegendItems = legendItems;
    }

    /// <summary>
    /// Customize legend event arguments constructor
    /// </summary>
    /// <param name="legendItems">Legend items collection.</param>
    /// <param name="legendName">Legend name.</param>
    public CustomizeLegendEventArgs(LegendItemsCollection legendItems, string legendName)
    {
        this.LegendItems = legendItems;
        this.LegendName = legendName;
    }

    /// <summary>
    /// Legend name.
    /// </summary>
    public string LegendName { get; } = string.Empty;

    /// <summary>
    /// Legend items collection.
    /// </summary>
    public LegendItemsCollection LegendItems { get; }
}

#endregion


