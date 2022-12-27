using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System.Composition;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    [Shared]
    [ExportEndpoint]
    public class KeywordsStringEditorEditValueEndpoint : Endpoint<KeywordsStringEditorEditValueRequest, KeywordsStringEditorEditValueResponse>
    {
        public override string Name => EndpointNames.KeywordsStringEditorEditValue;

        protected override KeywordsStringEditorEditValueRequest CreateRequest(IDataPipeReader reader)
            => new(reader);

        protected override KeywordsStringEditorEditValueResponse CreateResponse(IDataPipeReader reader)
            => new(reader);
    }
}
