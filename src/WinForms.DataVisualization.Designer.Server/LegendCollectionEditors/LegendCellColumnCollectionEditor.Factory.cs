using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCellColumnCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.LegendCellColumnCollectionEditor)]
    private class Factory : CollectionEditorFactory<LegendCellColumnCollectionEditor>
    {
        protected override LegendCellColumnCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
