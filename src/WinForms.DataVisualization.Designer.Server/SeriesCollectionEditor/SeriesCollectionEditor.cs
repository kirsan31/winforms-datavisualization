using System;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class SeriesCollectionEditor : ChartCollectionEditor
{
    public SeriesCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    protected override object CreateInstance(Type itemType)
    {
        if (Context is not null && Context.Instance is not null)
        {
            Chart control = Helpers.GetChartReference(Context.Instance);
            return CreateNewSeries(control, string.Empty);
        }

        return base.CreateInstance(itemType);
    }

    internal static Series CreateNewSeries(Chart control, string suggestedChartArea)
    {
        int countSeries = control.Series.Count + 1;
        string seriesName = "Series" + countSeries.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Check if this name already in use
        bool seriesFound = true;
        while (seriesFound)
        {
            seriesFound = false;
            foreach (Series series in control.Series)
            {
                if (series.Name == seriesName)
                {
                    seriesFound = true;
                    break;
                }
            }

            if (seriesFound)
            {
                ++countSeries;
                seriesName = "Series" + countSeries.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        // Create new series
        Series newSeries = new Series(seriesName);

        // Check if default chart area name exists
        if (control.ChartAreas.Count > 0)
        {
            bool defaultFound = false;

            if (!string.IsNullOrEmpty(suggestedChartArea) &&
                control.ChartAreas.IndexOf(suggestedChartArea) != -1)
            {
                newSeries.ChartArea = suggestedChartArea;
                defaultFound = true;
            }
            else
            {
                foreach (ChartArea area in control.ChartAreas)
                {
                    if (area.Name == newSeries.ChartArea)
                    {
                        defaultFound = true;
                        break;
                    }
                }
            }

            // If default chart area was not found - use name of the first area
            if (!defaultFound)
            {
                newSeries.ChartArea = control.ChartAreas[0].Name;
            }

            // Check if series area is circular
            if (control.ChartAreas[newSeries.ChartArea].chartAreaIsCurcular)
            {
                // Change default chart type
                newSeries.ChartTypeName = ChartTypeNames.Radar;

                // Check if it's a Polar chart type
                if (control.ChartAreas[newSeries.ChartArea].GetCircularChartType() is IChartType chartType && string.Equals(chartType.Name, ChartTypeNames.Polar, StringComparison.OrdinalIgnoreCase))
                {
                    newSeries.ChartTypeName = ChartTypeNames.Polar;
                }
            }
        }

        return newSeries;
    }
}