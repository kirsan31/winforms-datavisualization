using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class ChartCollectionEditorEditValueRequest : Request
    {
        public object? NameController { get; private set; }
        public bool Save { get; private set; }

        public ChartCollectionEditorEditValueRequest(object nameController, bool save)
        {
            NameController = nameController ?? throw new ArgumentNullException(nameof(nameController));
            Save = save;
        }

        public ChartCollectionEditorEditValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            NameController = reader.ReadObject(nameof(NameController));
            Save = reader.ReadBoolean(nameof(Save));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(NameController), NameController);
            writer.Write(nameof(Save), Save);
        }
    }
}
