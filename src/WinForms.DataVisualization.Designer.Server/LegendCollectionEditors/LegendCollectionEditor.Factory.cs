using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class LegendCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.LegendCollectionEditor)]
    private class Factory : CollectionEditorFactory<LegendCollectionEditor>
    {
        protected override LegendCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
