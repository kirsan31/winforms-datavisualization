using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class SeriesCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.SeriesCollectionEditor)]
    private class Factory : CollectionEditorFactory<SeriesCollectionEditor>
    {
        protected override SeriesCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
