using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class AnchorPointUITypeEditorEditValueRequest : Request
    {
        public object? OwnerObj { get; private set; }

        public AnchorPointUITypeEditorEditValueRequest(object ownerObj)
        {
            OwnerObj = ownerObj ?? throw new ArgumentNullException(nameof(ownerObj));
        }

        public AnchorPointUITypeEditorEditValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            OwnerObj = reader.ReadObject(nameof(OwnerObj));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(OwnerObj), OwnerObj);
        }
    }
}
