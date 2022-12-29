using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCellColumnCollectionEditor : ChartCollectionEditor
{
    public LegendCellColumnCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    protected override object CreateInstance(Type itemType)
    {
        if (Context?.Instance is not Legend legend)
            return base.CreateInstance(itemType);

        int itemCount = legend.CellColumns.Count + 1;
        string itemName = "Column" + itemCount.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Check if this name already in use
        bool itemFound = true;
        while (itemFound)
        {
            itemFound = false;
            foreach (LegendCellColumn column in legend.CellColumns)
            {
                if (column.Name == itemName)
                {
                    itemFound = true;
                    break;
                }
            }

            if (itemFound)
            {
                ++itemCount;
                itemName = "Column" + itemCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        // Create new legend column
        LegendCellColumn legendColumn = new LegendCellColumn();
        legendColumn.Name = itemName;
        return legendColumn;
    }
}