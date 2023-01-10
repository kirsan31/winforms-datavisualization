using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCollectionEditor : ChartCollectionEditor
{
    public LegendCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    protected override object CreateInstance(Type itemType)
    {
        if (Context?.Instance is not Chart control)
            return base.CreateInstance(itemType);

        // Create legend with unique name
        int countLegend = control.Legends.Count + 1;
        string legendName = "Legend" + countLegend.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Check if this name already in use
        bool legendFound = true;
        while (legendFound)
        {
            legendFound = false;
            foreach (Legend legend in control.Legends)
            {
                if (legend.Name == legendName)
                {
                    legendFound = true;
                    break;
                }
            }

            if (legendFound)
            {
                ++countLegend;
                legendName = "Legend" + countLegend.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        // Create new legend
        Legend newLegend = new Legend(legendName);
        return newLegend;
    }
}