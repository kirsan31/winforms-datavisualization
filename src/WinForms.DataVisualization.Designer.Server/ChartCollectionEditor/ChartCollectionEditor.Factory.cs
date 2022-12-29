using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class ChartCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.ChartCollectionEditor)]
    private class Factory : CollectionEditorFactory<ChartCollectionEditor>
    {
        protected override ChartCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
