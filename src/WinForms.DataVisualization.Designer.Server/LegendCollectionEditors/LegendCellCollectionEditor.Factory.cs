using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCellCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.LegendCellCollectionEditor)]
    private class Factory : CollectionEditorFactory<LegendCellCollectionEditor>
    {
        protected override LegendCellCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
