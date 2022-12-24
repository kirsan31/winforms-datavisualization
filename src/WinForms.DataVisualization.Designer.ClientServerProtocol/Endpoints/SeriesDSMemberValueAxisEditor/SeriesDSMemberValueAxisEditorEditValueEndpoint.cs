using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class SeriesDSMemberValueAxisEditorEditValueEndpoint : Endpoint<SeriesDSMemberValueAxisEditorEditValueRequest, SeriesDSMemberValueAxisEditorEditValueResponse>
    {
        public override string Name => EndpointNames.SeriesDSMemberValueAxisEditorEditValue;

        protected override SeriesDSMemberValueAxisEditorEditValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override SeriesDSMemberValueAxisEditorEditValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
