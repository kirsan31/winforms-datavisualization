using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.AnchorPointUITypeEditorEditValue)]
public class AnchorPointUITypeEditorEditValueHandler : RequestHandler<AnchorPointUITypeEditorEditValueRequest, AnchorPointUITypeEditorEditValueResponse>
{
    public override AnchorPointUITypeEditorEditValueResponse HandleRequest(AnchorPointUITypeEditorEditValueRequest request)
    {
        if (request.OwnerObj is not Annotation annotation)
            return new AnchorPointUITypeEditorEditValueResponse();

        var series = annotation.Chart?.Series;
        if (series is null)
            return new AnchorPointUITypeEditorEditValueResponse();

        return new AnchorPointUITypeEditorEditValueResponse(series.Select(s => new SeriesDataPointDPO(s.Name, s.Points)).ToList().AsReadOnly());
    }
}
