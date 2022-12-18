using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class MarkerStyleEditorPaintValueEndpoint : Endpoint<MarkerStyleEditorPaintValueRequest, MarkerStyleEditorPaintValueResponse>
    {
        public override string Name => EndpointNames.MarkerStyleEditorPaintValue;

        protected override MarkerStyleEditorPaintValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override MarkerStyleEditorPaintValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
