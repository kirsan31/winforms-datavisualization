using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class AnnotationAxisUITypeEditorEditValueRequest : Request
    {
        public object? OwnerObj { get; private set; }

        public AnnotationAxisUITypeEditorEditValueRequest(object ownerObj)
        {
            OwnerObj = ownerObj ?? throw new ArgumentNullException(nameof(ownerObj));
        }

        public AnnotationAxisUITypeEditorEditValueRequest(IDataPipeReader reader) : base(reader) {}

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
