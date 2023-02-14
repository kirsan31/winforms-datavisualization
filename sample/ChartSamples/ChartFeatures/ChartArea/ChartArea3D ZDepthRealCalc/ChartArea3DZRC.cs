using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Globalization;

namespace ChartSamples
{
	public partial class ChartArea3DZRC : System.Windows.Forms.UserControl
	{
        private Point _prevPount;

        public ChartArea3DZRC()
        {
            InitializeComponent();
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var yDiff = -_prevPount.Y + e.Y;
            var xDiff = _prevPount.X - e.X;

            if (yDiff != 0)
            {
                var newDiff = (int)(yDiff * 360 / chart1.Height + Math.Sign(yDiff) * 0.5);
                if (newDiff == 0)
                    yDiff = Math.Sign(yDiff);

                if (yDiff != 0)
                {
                    yDiff += chart1.ChartAreas[0].Area3DStyle.Inclination;
                    if (yDiff > 90)
                        yDiff = 90;
                    else if (yDiff < -90)
                        yDiff = -90;

                    chart1.ChartAreas[0].Area3DStyle.Inclination = yDiff;
                }
            }

            if (xDiff != 0)
            {
                var newDiff = (int)(xDiff * 720 / chart1.Width + Math.Sign(xDiff) * 0.5);
                if (newDiff == 0)
                    xDiff = Math.Sign(xDiff);

                xDiff += chart1.ChartAreas[0].Area3DStyle.Rotation;
                if (xDiff > 180)
                    xDiff = -180 + xDiff - 180;
                else if (xDiff < -180)
                    xDiff = 180 + xDiff + 180;

                chart1.ChartAreas[0].Area3DStyle.Rotation = xDiff;
            }

            _prevPount = e.Location;
        }

        private void chart1_MouseDown(object sender, MouseEventArgs e)
        {
            _prevPount = e.Location;
        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var r = Convert.ToSingle(chart1.Series[0][CustomPropertyName.ZValue], NumberFormatInfo.InvariantInfo);
                if (r + 10 > 100)
                    r = 0;

                chart1.Series[0][CustomPropertyName.ZValue] = (r + 10).ToString(NumberFormatInfo.InvariantInfo);
                chart1.ChartAreas[0].RecalculateAxesScale();
            }
        }
    }
}
