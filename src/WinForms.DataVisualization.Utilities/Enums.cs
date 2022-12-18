// Here are enumerations that are used in editors (for example in FlagsEnumUITypeEditor) and in the control itself.

namespace System.Windows.Forms.DataVisualization.Charting
{
    /// <summary>
    /// An enumeration of anti-aliasing flags.
    /// </summary>
    [Flags]
    public enum AntiAliasingStyles
    {
        /// <summary>
        /// No anti-aliasing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use anti-aliasing when drawing text.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Use anti-aliasing when drawing graphics primitives (e.g. lines, rectangle)
        /// </summary>
        Graphics = 2,

        /// <summary>
        /// Use anti-alias for everything.
        /// </summary>
        All = Text | Graphics
    }


    /// <summary>
    /// An enumeration that specifies a label alignment.
    /// </summary>
    [Flags]
    public enum LabelAlignmentStyles
    {
        /// <summary>
        /// Label is aligned to the top of the data point.
        /// </summary>
        Top = 1,

        /// <summary>
        /// Label is aligned to the bottom of the data point.
        /// </summary>
        Bottom = 2,

        /// <summary>
        /// Label is aligned to the right of the data point.
        /// </summary>
        Right = 4,

        /// <summary>
        /// Label is aligned to the left of the data point.
        /// </summary>
        Left = 8,

        /// <summary>
        /// Label is aligned to the top-left corner of the data point.
        /// </summary>
        TopLeft = 16,

        /// <summary>
        /// Label is aligned to the top-right corner of the data point.
        /// </summary>
        TopRight = 32,

        /// <summary>
        /// Label is aligned to the bottom-left of the data point.
        /// </summary>
        BottomLeft = 64,

        /// <summary>
        /// Label is aligned to the bottom-right of the data point.
        /// </summary>
        BottomRight = 128,

        /// <summary>
        /// Label is aligned to the center of the data point.
        /// </summary>
        Center = 256,
    }


    /// <summary>
    /// An enumeration of scrollbar button style flags.
    /// </summary>
    [Flags]
    public enum ScrollBarButtonStyles
    {
        /// <summary>
        /// No buttons are shown.
        /// </summary>
        None = 0,

        /// <summary>
        /// Small increment or decrement buttons are shown.
        /// </summary>
        SmallScroll = 1,

        /// <summary>
        /// Reset zoom buttons are shown.
        /// </summary>
        ResetZoom = 2,

        /// <summary>
        /// All buttons are shown.
        /// </summary>
        All = SmallScroll | ResetZoom
    }


    /// <summary>
    /// An enumeration of the alignment orientations of a ChartArea
    /// </summary>
    [Flags]
    public enum AreaAlignmentOrientations
    {
        /// <summary>
        /// Chart areas are not automatically aligned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Chart areas are aligned vertically.
        /// </summary>
        Vertical = 1,

        /// <summary>
        /// Chart areas are aligned horizontally.
        /// </summary>
        Horizontal = 2,

        /// <summary>
        /// Chart areas are aligned using all values (horizontally and vertically).
        /// </summary>
        All = Vertical | Horizontal
    }


    /// <summary>
    /// An enumeration of the alignment styles of a ChartArea
    /// </summary>
    [Flags]
    public enum AreaAlignmentStyles
    {
        /// <summary>
        /// Chart areas are not automatically aligned.
        /// </summary>
        None = 0,

        /// <summary>
        /// Chart areas are aligned by positions.
        /// </summary>
        Position = 1,

        /// <summary>
        /// Chart areas are aligned by inner plot positions.
        /// </summary>
        PlotPosition = 2,

        /// <summary>
        /// Chart areas are aligned by axes views.
        /// </summary>
        AxesView = 4,

        /// <summary>
        /// Cursor and Selection alignment.
        /// </summary>
        Cursor = 8,

        /// <summary>
        /// Complete alignment.
        /// </summary>
        All = Position | PlotPosition | Cursor | AxesView
    }


    /// <summary>
    /// An enumeration of custom grid lines and tick marks flags used in the custom labels.
    /// </summary>
    [Flags]
    public enum GridTickTypes
    {
        /// <summary>
        /// No tick mark or grid line are shown.
        /// </summary>
        None = 0,

        /// <summary>
        /// Tick mark is shown.
        /// </summary>
        TickMark = 1,

        /// <summary>
        /// Grid line is shown.
        /// </summary>
        Gridline = 2,

        /// <summary>
        /// Tick mark and grid line are shown.
        /// </summary>
        All = TickMark | Gridline
    }


    /// <summary>
    /// An enumeration of auto-fitting styles of the axis labels.
    /// </summary>
    [Flags]
    public enum LabelAutoFitStyles
    {
        /// <summary>
        /// No auto-fitting.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow font size increasing.
        /// </summary>
        IncreaseFont = 1,

        /// <summary>
        /// Allow font size decreasing.
        /// </summary>
        DecreaseFont = 2,

        /// <summary>
        /// Allow using staggered labels.
        /// </summary>
        StaggeredLabels = 4,

        /// <summary>
        /// Allow changing labels angle using values of 0, 30, 60 and 90 degrees.
        /// </summary>
        LabelsAngleStep30 = 8,

        /// <summary>
        /// Allow changing labels angle using values of 0, 45, 90 degrees.
        /// </summary>
        LabelsAngleStep45 = 16,

        /// <summary>
        /// Allow changing labels angle using values of 0 and 90 degrees.
        /// </summary>
        LabelsAngleStep90 = 32,

        /// <summary>
        /// Allow replacing spaces with the new line character.
        /// </summary>
        WordWrap = 64,
    }


    /// <summary>
    /// An enumeration of marker styles.
    /// </summary>
    public enum MarkerStyle
    {
        /// <summary>
        /// No marker is displayed for the series/data point.
        /// </summary>
        None = 0,

        /// <summary>
        /// A square marker is displayed.
        /// </summary>
        Square = 1,

        /// <summary>
        /// A circle marker is displayed.
        /// </summary>
        Circle = 2,

        /// <summary>
        /// A diamond-shaped marker is displayed.
        /// </summary>
        Diamond = 3,

        /// <summary>
        /// A triangular marker is displayed.
        /// </summary>
        Triangle = 4,

        /// <summary>
        /// A cross-shaped marker is displayed.
        /// </summary>
        Cross = 5,

        /// <summary>
        /// A 4-point star-shaped marker is displayed.
        /// </summary>
        Star4 = 6,

        /// <summary>
        /// A 5-point star-shaped marker is displayed.
        /// </summary>
        Star5 = 7,

        /// <summary>
        /// A 6-point star-shaped marker is displayed.
        /// </summary>
        Star6 = 8,

        /// <summary>
        /// A 10-point star-shaped marker is displayed.
        /// </summary>
        Star10 = 9
    };
}