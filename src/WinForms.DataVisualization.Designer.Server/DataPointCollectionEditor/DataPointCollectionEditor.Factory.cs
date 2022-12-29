using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class DataPointCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.DataPointCollectionEditor)]
    private class Factory : CollectionEditorFactory<DataPointCollectionEditor>
    {
        protected override DataPointCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
