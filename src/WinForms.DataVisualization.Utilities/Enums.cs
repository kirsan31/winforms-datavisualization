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


    /// <summary>
    /// An enumeration of gradient styles.
    /// </summary>
    public enum GradientStyle
    {
        /// <summary>
        /// No gradient is used.
        /// </summary>
        None,

        /// <summary>
        /// Gradient is applied from left to right.
        /// </summary>
        LeftRight,

        /// <summary>
        /// Gradient is applied from top to bottom.
        /// </summary>
        TopBottom,

        /// <summary>
        /// Gradient is applied from the center outwards.
        /// </summary>
        Center,

        /// <summary>
        /// Gradient is applied diagonally from left to right.
        /// </summary>
        DiagonalLeft,

        /// <summary>
        /// Gradient is applied diagonally from right to left.
        /// </summary>
        DiagonalRight,

        /// <summary>
        /// Gradient is applied horizontally from the center outwards.
        /// </summary>
        HorizontalCenter,

        /// <summary>
        /// Gradient is applied vertically from the center outwards.
        /// </summary>
        VerticalCenter
    };

    /// <summary>
    /// An enumeration that specifies a hatching style.
    /// </summary>
    public enum ChartHatchStyle
    {
        /// <summary>
        /// No hatching style.
        /// </summary>
        None,

        /// <summary>
        /// Backward diagonal style.
        /// </summary>
        BackwardDiagonal,

        /// <summary>
        /// Cross style.
        /// </summary>
        Cross,

        /// <summary>
        /// Dark downward diagonal style.
        /// </summary>
        DarkDownwardDiagonal,

        /// <summary>
        /// Dark horizontal style.
        /// </summary>
        DarkHorizontal,

        /// <summary>
        /// Dark upward diagonal style.
        /// </summary>
        DarkUpwardDiagonal,

        /// <summary>
        /// Dark vertical style.
        /// </summary>
        DarkVertical,

        /// <summary>
        /// Dashed downward diagonal style.
        /// </summary>
        DashedDownwardDiagonal,

        /// <summary>
        /// Dashed horizontal style.
        /// </summary>
        DashedHorizontal,

        /// <summary>
        /// Dashed upward diagonal style.
        /// </summary>
        DashedUpwardDiagonal,

        /// <summary>
        /// Dashed vertical style.
        /// </summary>
        DashedVertical,

        /// <summary>
        /// Diagonal brick style.
        /// </summary>
        DiagonalBrick,

        /// <summary>
        /// Diagonal cross style.
        /// </summary>
        DiagonalCross,

        /// <summary>
        /// Divot style.
        /// </summary>
        Divot,

        /// <summary>
        /// Dotted diamond style.
        /// </summary>
        DottedDiamond,

        /// <summary>
        /// Dotted grid style.
        /// </summary>
        DottedGrid,

        /// <summary>
        /// Forward diagonal style.
        /// </summary>
        ForwardDiagonal,

        /// <summary>
        /// Horizontal style.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Horizontal brick style.
        /// </summary>
        HorizontalBrick,

        /// <summary>
        /// Large checker board style.
        /// </summary>
        LargeCheckerBoard,

        /// <summary>
        /// Large confetti style.
        /// </summary>
        LargeConfetti,

        /// <summary>
        /// Large grid style.
        /// </summary>
        LargeGrid,

        /// <summary>
        /// Light downward diagonal style.
        /// </summary>
        LightDownwardDiagonal,

        /// <summary>
        /// Light horizontal style.
        /// </summary>
        LightHorizontal,

        /// <summary>
        /// Light upward diagonal style.
        /// </summary>
        LightUpwardDiagonal,

        /// <summary>
        /// Light vertical style.
        /// </summary>
        LightVertical,

        /// <summary>
        /// Narrow horizontal style.
        /// </summary>
        NarrowHorizontal,

        /// <summary>
        /// Narrow vertical style.
        /// </summary>
        NarrowVertical,

        /// <summary>
        /// Outlined diamond style.
        /// </summary>
        OutlinedDiamond,

        /// <summary>
        /// Percent05 style.
        /// </summary>
        Percent05,

        /// <summary>
        /// Percent10 style.
        /// </summary>
        Percent10,

        /// <summary>
        /// Percent20 style.
        /// </summary>
        Percent20,

        /// <summary>
        /// Percent25 style.
        /// </summary>
        Percent25,

        /// <summary>
        /// Percent30 style.
        /// </summary>
        Percent30,

        /// <summary>
        /// Percent40 style.
        /// </summary>
        Percent40,

        /// <summary>
        /// Percent50 style.
        /// </summary>
        Percent50,

        /// <summary>
        /// Percent60 style.
        /// </summary>
        Percent60,

        /// <summary>
        /// Percent70 style.
        /// </summary>
        Percent70,

        /// <summary>
        /// Percent75 style.
        /// </summary>
        Percent75,

        /// <summary>
        /// Percent80 style.
        /// </summary>
        Percent80,

        /// <summary>
        /// Percent90 style.
        /// </summary>
        Percent90,

        /// <summary>
        /// Plaid style.
        /// </summary>
        Plaid,

        /// <summary>
        /// Shingle style.
        /// </summary>
        Shingle,

        /// <summary>
        /// Small checker board style.
        /// </summary>
        SmallCheckerBoard,

        /// <summary>
        /// Small confetti style.
        /// </summary>
        SmallConfetti,

        /// <summary>
        /// Small grid style.
        /// </summary>
        SmallGrid,

        /// <summary>
        /// Solid diamond style.
        /// </summary>
        SolidDiamond,

        /// <summary>
        /// Sphere style.
        /// </summary>
        Sphere,

        /// <summary>
        /// Trellis style.
        /// </summary>
        Trellis,

        /// <summary>
        /// Vertical style.
        /// </summary>
        Vertical,

        /// <summary>
        /// Wave style.
        /// </summary>
        Wave,

        /// <summary>
        /// Weave style.
        /// </summary>
        Weave,

        /// <summary>
        /// Wide downward diagonal style.
        /// </summary>
        WideDownwardDiagonal,

        /// <summary>
        /// Wide upward diagonal style.
        /// </summary>
        WideUpwardDiagonal,

        /// <summary>
        /// ZigZag style.
        /// </summary>
        ZigZag
    };

    /// <summary>
    /// An enumeration of chart types.
    /// </summary>
    public enum SeriesChartType
    {
        /// <summary>
        /// Point chart type.
        /// </summary>
        Point,

        /// <summary>
        /// FastPoint chart type.
        /// </summary>
        FastPoint,

        /// <summary>
        /// Bubble chart type.
        /// </summary>
        Bubble,

        /// <summary>
        /// Line chart type.
        /// </summary>
        Line,

        /// <summary>
        /// Spline chart type.
        /// </summary>
        Spline,

        /// <summary>
        /// StepLine chart type.
        /// </summary>
        StepLine,

        /// <summary>
        /// FastLine chart type.
        /// </summary>
        FastLine,

        /// <summary>
        /// Bar chart type.
        /// </summary>
        Bar,

        /// <summary>
        /// Stacked bar chart type.
        /// </summary>
        StackedBar,

        /// <summary>
        /// Hundred percent stacked bar chart type.
        /// </summary>
        StackedBar100,

        /// <summary>
        /// Column chart type.
        /// </summary>
        Column,

        /// <summary>
        /// Stacked column chart type.
        /// </summary>
        StackedColumn,

        /// <summary>
        /// Hundred percent stacked column chart type.
        /// </summary>
        StackedColumn100,

        /// <summary>
        /// Area chart type.
        /// </summary>
        Area,

        /// <summary>
        /// Spline area chart type.
        /// </summary>
        SplineArea,

        /// <summary>
        /// Stacked area chart type.
        /// </summary>
        StackedArea,

        /// <summary>
        /// Hundred percent stacked area chart type.
        /// </summary>
        StackedArea100,

        /// <summary>
        /// Pie chart type.
        /// </summary>
        Pie,

        /// <summary>
        /// Doughnut chart type.
        /// </summary>
        Doughnut,

        /// <summary>
        /// Stock chart type.
        /// </summary>
        Stock,

        /// <summary>
        /// CandleStick chart type.
        /// </summary>
        Candlestick,

        /// <summary>
        /// Range chart type.
        /// </summary>
        Range,

        /// <summary>
        /// Spline range chart type.
        /// </summary>
        SplineRange,

        /// <summary>
        /// RangeBar chart type.
        /// </summary>
        RangeBar,

        /// <summary>
        /// Range column chart type.
        /// </summary>
        RangeColumn,

        /// <summary>
        /// Radar chart type.
        /// </summary>
        Radar,

        /// <summary>
        /// Polar chart type.
        /// </summary>
        Polar,

        /// <summary>
        /// Error bar chart type.
        /// </summary>
        ErrorBar,

        /// <summary>
        /// Box plot chart type.
        /// </summary>
        BoxPlot,

        /// <summary>
        /// Renko chart type.
        /// </summary>
        Renko,

        /// <summary>
        /// ThreeLineBreak chart type.
        /// </summary>
        ThreeLineBreak,

        /// <summary>
        /// Kagi chart type.
        /// </summary>
        Kagi,

        /// <summary>
        /// PointAndFigure chart type.
        /// </summary>
        PointAndFigure,

        /// <summary>
        /// Funnel chart type.
        /// </summary>
        Funnel,

        /// <summary>
        /// Pyramid chart type.
        /// </summary>
        Pyramid,
    };
}