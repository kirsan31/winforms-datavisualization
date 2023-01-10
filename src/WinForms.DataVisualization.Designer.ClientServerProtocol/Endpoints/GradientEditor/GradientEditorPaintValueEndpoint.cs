using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class GradientEditorPaintValueEndpoint : Endpoint<GradientEditorPaintValueRequest, GradientEditorPaintValueResponse>
    {
        public override string Name => EndpointNames.GradientEditorPaintValue;

        protected override GradientEditorPaintValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override GradientEditorPaintValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
