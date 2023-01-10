using System;

using Microsoft.DotNet.DesignTools.Editors;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class AnnotationCollectionEditor
{
    [ExportCollectionEditorFactory(CollectionEditorNames.AnnotationCollectionEditor)]
    private class Factory : CollectionEditorFactory<AnnotationCollectionEditor>
    {
        protected override AnnotationCollectionEditor CreateCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
        {
            return new(serviceProvider, collectionType);
        }
    }
}
