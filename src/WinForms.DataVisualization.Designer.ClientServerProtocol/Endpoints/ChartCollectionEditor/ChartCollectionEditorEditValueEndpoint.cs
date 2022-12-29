using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class ChartCollectionEditorEditValueEndpoint : Endpoint<ChartCollectionEditorEditValueRequest, Response.Empty>
    {
        public override string Name => EndpointNames.ChartCollectionEditorEditValue;

        protected override ChartCollectionEditorEditValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override Response.Empty CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
