using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class AnnotationAxisUITypeEditorEditValueEndpoint : Endpoint<AnnotationAxisUITypeEditorEditValueRequest, AnnotationAxisUITypeEditorEditValueResponse>
    {
        public override string Name => EndpointNames.AnnotationAxisUITypeEditorEditValue;

        protected override AnnotationAxisUITypeEditorEditValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override AnnotationAxisUITypeEditorEditValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
