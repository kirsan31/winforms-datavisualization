using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartSamples
{
    partial class ChartArea3DZRC
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ChartArea chartArea1 = new ChartArea();
            Legend legend1 = new Legend();
            LegendItem legendItem1 = new LegendItem();
            LegendItem legendItem2 = new LegendItem();
            Series series1 = new Series();
            DataPoint dataPoint1 = new DataPoint(0D, 0D);
            DataPoint dataPoint2 = new DataPoint(1D, 1D);
            DataPoint dataPoint3 = new DataPoint(2D, 4D);
            DataPoint dataPoint4 = new DataPoint(3D, 10D);
            DataPoint dataPoint5 = new DataPoint(4D, 20D);
            Series series2 = new Series();
            DataPoint dataPoint6 = new DataPoint(0D, 1D);
            DataPoint dataPoint7 = new DataPoint(1D, 1D);
            DataPoint dataPoint8 = new DataPoint(2D, 6D);
            DataPoint dataPoint9 = new DataPoint(3D, 10D);
            DataPoint dataPoint10 = new DataPoint(12D, 20D);
            DataPoint dataPoint11 = new DataPoint(12.1D, 20.1D);
            DataPoint dataPoint12 = new DataPoint(12.2D, 20.2D);
            Series series3 = new Series();
            DataPoint dataPoint13 = new DataPoint(0D, 0D);
            DataPoint dataPoint14 = new DataPoint(0D, 20D);
            DataPoint dataPoint15 = new DataPoint(0D, 2D);
            DataPoint dataPoint16 = new DataPoint(0D, 5D);
            DataPoint dataPoint17 = new DataPoint(0D, 7D);
            DataPoint dataPoint18 = new DataPoint(0D, 2D);
            DataPoint dataPoint19 = new DataPoint(0D, 8D);
            DataPoint dataPoint20 = new DataPoint(0D, 20D);
            DataPoint dataPoint21 = new DataPoint(3D, 24D);
            DataPoint dataPoint22 = new DataPoint(4D, 20D);
            Series series4 = new Series();
            DataPoint dataPoint23 = new DataPoint(0D, 1D);
            DataPoint dataPoint24 = new DataPoint(0D, 8D);
            DataPoint dataPoint25 = new DataPoint(0D, 6D);
            DataPoint dataPoint26 = new DataPoint(0D, 1D);
            DataPoint dataPoint27 = new DataPoint(0D, 22D);
            DataPoint dataPoint28 = new DataPoint(0D, 21D);
            Series series5 = new Series();
            DataPoint dataPoint29 = new DataPoint(0D, 15D);
            DataPoint dataPoint30 = new DataPoint(2D, 7D);
            DataPoint dataPoint31 = new DataPoint(4D, 4D);
            DataPoint dataPoint32 = new DataPoint(5D, 16D);
            DataPoint dataPoint33 = new DataPoint(7D, 12D);
            DataPoint dataPoint34 = new DataPoint(9D, 20D);
            Series series6 = new Series();
            DataPoint dataPoint35 = new DataPoint(0D, 8D);
            DataPoint dataPoint36 = new DataPoint(5D, 4D);
            DataPoint dataPoint37 = new DataPoint(10D, 10D);
            DataPoint dataPoint38 = new DataPoint(15D, 6D);
            DataPoint dataPoint39 = new DataPoint(16D, 30D);
            DataPoint dataPoint40 = new DataPoint(18D, 6D);
            Series series7 = new Series();
            DataPoint dataPoint41 = new DataPoint(0D, 5D);
            DataPoint dataPoint42 = new DataPoint(2D, 20D);
            DataPoint dataPoint43 = new DataPoint(6D, 2D);
            DataPoint dataPoint44 = new DataPoint(8D, 4D);
            DataPoint dataPoint45 = new DataPoint(9D, 15D);
            DataPoint dataPoint46 = new DataPoint(14D, 5D);
            Series series8 = new Series();
            DataPoint dataPoint47 = new DataPoint(0D, 0D);
            DataPoint dataPoint48 = new DataPoint(0D, 2D);
            DataPoint dataPoint49 = new DataPoint(0D, 3D);
            DataPoint dataPoint50 = new DataPoint(0D, 4D);
            DataPoint dataPoint51 = new DataPoint(0D, 9D);
            DataPoint dataPoint52 = new DataPoint(0D, 2D);
            chart1 = new Chart();
            label9 = new Label();
            ((ISupportInitialize)chart1).BeginInit();
            SuspendLayout();
            // 
            // chart1
            // 
            chart1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chartArea1.Area3DStyle.Enable3D = true;
            chartArea1.Area3DStyle.IsRightAngleAxes = false;
            chartArea1.Area3DStyle.LightStyle = LightStyle.None;
            chartArea1.Area3DStyle.WallWidth = 0;
            chartArea1.Area3DStyle.ZDepthRealCalc = true;
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Format = "0.#";
            chartArea1.AxisX.LineColor = Color.DarkSlateGray;
            chartArea1.AxisX.LineWidth = 2;
            chartArea1.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea1.AxisX.Maximum = 20D;
            chartArea1.AxisY.IsMarginVisible = false;
            chartArea1.AxisY.LabelStyle.Format = "0.#";
            chartArea1.AxisY.LineColor = Color.DarkOrchid;
            chartArea1.AxisY.LineWidth = 2;
            chartArea1.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea1.BackColor = Color.Transparent;
            chartArea1.BorderWidth = 0;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chartArea1.Name = "ChartArea1";
            chart1.ChartAreas.Add(chartArea1);
            legendItem1.BorderColor = Color.Empty;
            legendItem1.BorderWidth = 4;
            legendItem1.Color = Color.Green;
            legendItem1.ImageStyle = LegendImageStyle.Line;
            legendItem1.Name = "Series2";
            legendItem2.Color = Color.Green;
            legendItem2.Name = "Series2";
            legend1.CustomItems.Add(legendItem1);
            legend1.CustomItems.Add(legendItem2);
            legend1.Enabled = false;
            legend1.Name = "Legend1";
            chart1.Legends.Add(legend1);
            chart1.Location = new Point(14, 77);
            chart1.Name = "chart1";
            series1.BorderWidth = 4;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = SeriesChartType.Line;
            series1.Color = Color.Red;
            series1.CustomProperties = "ZValue=2, EmptyPointValue=Zero";
            series1.Legend = "Legend1";
            series1.MarkerSize = 15;
            series1.MarkerStyle = MarkerStyle.Star5;
            series1.Name = "Series1";
            dataPoint5.MarkerSize = 30;
            dataPoint5.MarkerStyle = MarkerStyle.Star5;
            series1.Points.Add(dataPoint1);
            series1.Points.Add(dataPoint2);
            series1.Points.Add(dataPoint3);
            series1.Points.Add(dataPoint4);
            series1.Points.Add(dataPoint5);
            series1.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.Yes;
            series1.SmartLabelStyle.IsMarkerOverlappingAllowed = true;
            series1.SmartLabelStyle.IsOverlappedHidden = false;
            series1.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series2.BorderWidth = 2;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = SeriesChartType.Line;
            series2.Color = Color.Green;
            series2.CustomProperties = "ZValue=20, EmptyPointValue=Zero";
            series2.Legend = "Legend1";
            series2.MarkerSize = 10;
            series2.MarkerStyle = MarkerStyle.Cross;
            series2.Name = "Series2";
            dataPoint8.MarkerSize = 14;
            dataPoint8.MarkerStyle = MarkerStyle.Circle;
            dataPoint9.BorderWidth = 2;
            dataPoint9.Color = Color.Green;
            dataPoint9.MarkerColor = Color.DarkGreen;
            dataPoint9.MarkerSize = 14;
            dataPoint9.MarkerStyle = MarkerStyle.Circle;
            dataPoint10.MarkerColor = Color.Blue;
            dataPoint10.MarkerSize = 20;
            series2.Points.Add(dataPoint6);
            series2.Points.Add(dataPoint7);
            series2.Points.Add(dataPoint8);
            series2.Points.Add(dataPoint9);
            series2.Points.Add(dataPoint10);
            series2.Points.Add(dataPoint11);
            series2.Points.Add(dataPoint12);
            series2.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = SeriesChartType.Spline;
            series3.CustomProperties = "ZValue=30";
            series3.Legend = "Legend1";
            series3.MarkerSize = 10;
            series3.MarkerStyle = MarkerStyle.Circle;
            series3.Name = "Series7";
            series3.Points.Add(dataPoint13);
            series3.Points.Add(dataPoint14);
            series3.Points.Add(dataPoint15);
            series3.Points.Add(dataPoint16);
            series3.Points.Add(dataPoint17);
            series3.Points.Add(dataPoint18);
            series3.Points.Add(dataPoint19);
            series3.Points.Add(dataPoint20);
            series3.Points.Add(dataPoint21);
            series3.Points.Add(dataPoint22);
            series3.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = SeriesChartType.Point;
            series4.CustomProperties = "ZValue=50";
            series4.IsXValueIndexed = true;
            series4.Legend = "Legend1";
            series4.MarkerSize = 10;
            series4.Name = "Series8";
            dataPoint26.MarkerColor = Color.DarkGreen;
            dataPoint26.MarkerSize = 20;
            dataPoint26.MarkerStyle = MarkerStyle.Circle;
            series4.Points.Add(dataPoint23);
            series4.Points.Add(dataPoint24);
            series4.Points.Add(dataPoint25);
            series4.Points.Add(dataPoint26);
            series4.Points.Add(dataPoint27);
            series4.Points.Add(dataPoint28);
            series4.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series5.BorderWidth = 5;
            series5.ChartArea = "ChartArea1";
            series5.ChartType = SeriesChartType.Line;
            series5.Color = Color.Blue;
            series5.CustomProperties = "ZValue=60, EmptyPointValue=Zero";
            series5.Legend = "Legend1";
            series5.Name = "Series3";
            series5.Points.Add(dataPoint29);
            series5.Points.Add(dataPoint30);
            series5.Points.Add(dataPoint31);
            series5.Points.Add(dataPoint32);
            series5.Points.Add(dataPoint33);
            series5.Points.Add(dataPoint34);
            series5.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series6.BorderDashStyle = ChartDashStyle.Dash;
            series6.BorderWidth = 14;
            series6.ChartArea = "ChartArea1";
            series6.ChartType = SeriesChartType.Line;
            series6.Color = Color.Peru;
            series6.CustomProperties = "ZValue=85, EmptyPointValue=Zero";
            series6.Legend = "Legend1";
            series6.MarkerBorderColor = Color.Black;
            series6.MarkerBorderWidth = 2;
            series6.MarkerColor = Color.Transparent;
            series6.MarkerSize = 24;
            series6.MarkerStyle = MarkerStyle.Circle;
            series6.Name = "Series4";
            series6.Points.Add(dataPoint35);
            series6.Points.Add(dataPoint36);
            series6.Points.Add(dataPoint37);
            series6.Points.Add(dataPoint38);
            series6.Points.Add(dataPoint39);
            series6.Points.Add(dataPoint40);
            series6.ShadowColor = Color.Empty;
            series6.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series7.ChartArea = "ChartArea1";
            series7.ChartType = SeriesChartType.FastPoint;
            series7.CustomProperties = "ZValue=90, EmptyPointValue=Zero";
            series7.Legend = "Legend1";
            series7.MarkerSize = 20;
            series7.MarkerStyle = MarkerStyle.Star10;
            series7.Name = "Series5";
            dataPoint45.MarkerSize = 40;
            dataPoint45.MarkerStyle = MarkerStyle.Star10;
            series7.Points.Add(dataPoint41);
            series7.Points.Add(dataPoint42);
            series7.Points.Add(dataPoint43);
            series7.Points.Add(dataPoint44);
            series7.Points.Add(dataPoint45);
            series7.Points.Add(dataPoint46);
            series7.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            series8.BorderWidth = 5;
            series8.ChartArea = "ChartArea1";
            series8.ChartType = SeriesChartType.StepLine;
            series8.CustomProperties = "ZValue=98";
            series8.IsXValueIndexed = true;
            series8.Legend = "Legend1";
            series8.MarkerSize = 10;
            series8.MarkerStyle = MarkerStyle.Diamond;
            series8.Name = "Series6";
            series8.Points.Add(dataPoint47);
            series8.Points.Add(dataPoint48);
            series8.Points.Add(dataPoint49);
            series8.Points.Add(dataPoint50);
            series8.Points.Add(dataPoint51);
            series8.Points.Add(dataPoint52);
            series8.ToolTip = "#SERIESNAME\\nX = #VALX\\nY = #VAL";
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Series.Add(series3);
            chart1.Series.Add(series4);
            chart1.Series.Add(series5);
            chart1.Series.Add(series6);
            chart1.Series.Add(series7);
            chart1.Series.Add(series8);
            chart1.Size = new Size(409, 400);
            chart1.TabIndex = 0;
            chart1.Text = "chart1";
            chart1.MouseClick += chart1_MouseClick;
            chart1.MouseDown += chart1_MouseDown;
            chart1.MouseMove += chart1_MouseMove;
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label9.Font = new Font("Verdana", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            label9.Location = new Point(16, 0);
            label9.Name = "label9";
            label9.Size = new Size(702, 74);
            label9.TabIndex = 4;
            label9.Text = "This sample shows how you can emulate Z axis using ZDepthRealCalc 3DStyle mode.\r\nYou can rotate chart holding left mouse button. Right mouse click will move red series in Z order.";
            label9.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ChartArea3DZRC
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(label9);
            Controls.Add(chart1);
            Name = "ChartArea3DZRC";
            Size = new Size(728, 480);
            ((ISupportInitialize)chart1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Chart chart1;
        private Label label9;
    }
}
