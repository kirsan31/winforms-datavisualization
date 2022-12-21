using System;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.KeywordsStringEditorEditValue)]
internal class KeywordsStringEditorEditValueHandler : RequestHandler<KeywordsStringEditorEditValueRequest, KeywordsStringEditorEditValueResponse>
{
    public override KeywordsStringEditorEditValueResponse HandleRequest(KeywordsStringEditorEditValueRequest request)
    {
        var editingObj = request.EditingObj;
        if (editingObj is null)
            return new KeywordsStringEditorEditValueResponse();

        // Try getting access to the associated series
        Series? series = null;
        Chart? chart = null;

        // Check object editingObj edited
        if (editingObj is Series series1)
        {
            series = series1;
        }
        else if (editingObj is DataPoint point)
        {
            series = point.series;
        }
        else if (editingObj is LegendItem item)
        {
            if (item.Common != null)
            {
                chart = item.Common.Chart;
                if (item.Common.DataManager.Series.IndexOf(item.SeriesName) >= 0)
                {
                    series = item.Common.DataManager.Series[item.SeriesName];
                }
            }
        }
        else if (editingObj is LegendCellColumn column)
        {
            if (column.Legend is not null)
            {
                chart = column.Legend.Common.Chart;
            }
        }
        else if (editingObj is Annotation annotation)
        {
            chart = annotation.Chart;
            if (annotation.AnchorDataPoint is not null)
            {
                series = annotation.AnchorDataPoint.series;
            }
            else if (chart is not null && chart.Series.Count > 0)
            {
                series = chart.Series[0];
            }
        }

        //Make sure chart reference was found
        if (chart is null && series is not null)
        {
            chart = series.Chart;
        }

        // Get maximum number of Y values
        int maxYValueNumber = 9;
        if (series is not null)
        {
            maxYValueNumber = series.YValuesPerPoint - 1;
        }
        else if (chart is not null)
        {
            // Find MAX number of Y values use in all series
            maxYValueNumber = 0;
            foreach (Series ser in chart.Series)
            {
                maxYValueNumber = Math.Max(maxYValueNumber, ser.YValuesPerPoint - 1);
            }
        }

        return new KeywordsStringEditorEditValueResponse((chart?.GetService(typeof(KeywordsRegistry)) as KeywordsRegistry)?.registeredKeywords.AsReadOnly(), maxYValueNumber);
    }
}
