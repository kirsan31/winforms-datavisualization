using System;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using Xunit;

namespace Tests;

public sealed class ReplaceKeywords_Tests : IDisposable
{
    private readonly Chart _chart = new Chart();

    public ReplaceKeywords_Tests()
    {
        _chart.Series.Add("Series1");
        _chart.Series[0].YValuesPerPoint = 32;
        _chart.Series[0].SetCustomProperty("cp1", "SER1_CUSTOMPROPERTY");
        _chart.Series.Add("Series2");
        _chart.Series[1].SetCustomProperty("cp1", "SER2_CUSTOMPROPERTY");
        _chart.Series.Add("Series3");
        _chart.Series[2].SetCustomProperty("cp1", "SE3_CUSTOMPROPERTY");
        _chart.Series[2].XValueType = ChartValueType.String;
        _chart.Series.Add("Series4");
        _chart.Series[3].YValueType = ChartValueType.DateTime;
        for (int s = 0; s < _chart.Series.Count; s++)
        {
            Series ser = _chart.Series[s];
            for (int p = 0; p < 2; p++)
            {
                DataPoint dp = new()
                {
                    XValue = 0.235 + p + s,
                    Label = ser.Name != "Series4" ? "#AVG{N4}\\n" +
                        "#INDEX\\n" +
                        "#VALX{N0} #VALX{N2}\\n" +
                        "#VAL #VAL{N4} #VALY{N3} #VALY00{N2} #VALY1{N1} #VALY01{N0}\\n" +
                        "#VALY22{0.000} #VALY32{N3}\\n" +
                        "#VALY33{N3}#VALY99{N3}#VALY1003245{N3}\\n" +
                        "#TOTAL{N4}\\n" +
                        "#PERCENT{N2}\\n" +
                        "#AXISLABEL\\n" +
                        "#LEGENDTEXT\\n" +
                        "#SERIESNAME\\n" +
                        "#MAX{N4}\\n" +
                        "#MIN{N4}\\n" +
                        "#LAST{N4}\\n" +
                        "#FIRST{N4}\\n" +
                        "#CUSTOMPROPERTY(cp1)\\n" +
                        "#CUSTOMPROPERTY(cp2)" :

                        "#VAL{dd.MM.yyyy HH:mm:ss}"
                };

                if (p == 1)
                    dp.SetCustomProperty("cp2", "POINT_CUSTOMPROPERTY_" + ser.Name);

                if (s == 2)
                    dp.AxisLabel = p.ToString();

                if (s < 3)
                {
                    double[] yVals = new double[ser.YValuesPerPoint];
                    for (int i = 0; i < ser.YValuesPerPoint; i++)
                    {
                        yVals[i] = p + i + Random.Shared.NextDouble();
                    }

                    dp.YValues = yVals;
                }
                else
                {
                    dp.SetValueY(DateTime.Now.AddDays(p));
                }

                ser.Points.Add(dp);
            }

            for (int i = 0; i < ser.Points.Count; i++)
            {
                DataPoint dp = ser.Points[i];
                if (ser.Name != "Series4")
                {
                    dp.Tag = $"{ser.Points.Average(p => p.YValues[0]):N4}\n" +
                        $"{i}\n" +
                        (ser.XValueType == ChartValueType.String ? $"{dp.AxisLabel} {dp.AxisLabel}\n" : $"{dp.XValue:N0} {dp.XValue:N2}\n") +
                        $"{dp.YValues[0]} {dp.YValues[0]:N4} {dp.YValues[0]:N3} {dp.YValues[0]:N2} {dp.YValues[0]:N1} {dp.YValues[0]:N0}\n" +
                        (s == 0 ? $"{dp.YValues[21]:0.000} {dp.YValues[31]:N3}\n" : " \n") +
                        $"\n" +
                        $"{ser.Points.Sum(p => p.YValues[0]):N4}\n" +
                        $"{(dp.YValues[0] / ser.Points.Sum(p => p.YValues[0])):N2}\n" +
                        $"{dp.AxisLabel}\n" +
                        $"{dp.LegendText}\n" +
                        $"{ser.Name}\n" +
                        $"{ser.Points.Max(p => p.YValues[0]):N4}\n" +
                        $"{ser.Points.Min(p => p.YValues[0]):N4}\n" +
                        $"{ser.Points[^1].YValues[0]:N4}\n" +
                        $"{ser.Points[0].YValues[0]:N4}\n" +
                        $"{ser["cp1"]}\n" +
                        $"{dp.TryGetCustomProperty("cp2")}";
                }
                else
                {
                    dp.Tag = $"{DateTime.FromOADate(dp.YValues[0]):dd.MM.yyyy HH:mm:ss}";
                }
            }
        }
    }

    [Fact] 
    public void ReplaceKeywords_Test()
    {
        foreach (var ser in _chart.Series)
        {
            foreach (var dp in ser.Points)
            {
                string expected = (string)dp.Tag;
                string label = dp.ReplaceKeywords(dp.Label);
                Assert.Equal(expected, label);
            }
        }
    }

    public void Dispose()
    {
        _chart.Dispose();
    }
}