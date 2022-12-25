using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class AnchorPointUITypeEditorEditValueEndpoint : Endpoint<AnchorPointUITypeEditorEditValueRequest, AnchorPointUITypeEditorEditValueResponse>
    {
        public override string Name => EndpointNames.AnchorPointUITypeEditorEditValue;

        protected override AnchorPointUITypeEditorEditValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override AnchorPointUITypeEditorEditValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
