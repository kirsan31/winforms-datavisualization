using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class ImageValueEditorPaintValueEndpoint : Endpoint<ImageValueEditorPaintValueRequest, ImageValueEditorPaintValueResponse>
    {
        public override string Name => EndpointNames.ImageValueEditorPaintValue;

        protected override ImageValueEditorPaintValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override ImageValueEditorPaintValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
