using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCellCollectionEditor : ChartCollectionEditor
{
    public LegendCellCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }

    protected override object CreateInstance(Type itemType)
    {
        if (Context?.Instance is not LegendItem legendItem)
            return base.CreateInstance(itemType);

        int itemCount = legendItem.Cells.Count + 1;
        string itemName = "Cell" + itemCount.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // Check if this name already in use
        bool itemFound = true;
        while (itemFound)
        {
            itemFound = false;
            foreach (LegendCell cell in legendItem.Cells)
            {
                if (cell.Name == itemName)
                {
                    itemFound = true;
                    break;
                }
            }

            if (itemFound)
            {
                ++itemCount;
                itemName = "Cell" + itemCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        // Create new legend cell
        LegendCell legendCell = new LegendCell();
        legendCell.Name = itemName;
        return legendCell;
    }
}