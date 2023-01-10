using System;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.AnnotationAxisUITypeEditorEditValue)]
public class AnnotationAxisUITypeEditorEditValueHandler : RequestHandler<AnnotationAxisUITypeEditorEditValueRequest, AnnotationAxisUITypeEditorEditValueResponse>
{
    public override AnnotationAxisUITypeEditorEditValueResponse HandleRequest(AnnotationAxisUITypeEditorEditValueRequest request)
    {
        if (request.OwnerObj is not Annotation annotation)
            return new AnnotationAxisUITypeEditorEditValueResponse();

        if (annotation.AnnotationGroup is not null)
            return new AnnotationAxisUITypeEditorEditValueResponse(Array.Empty<ChartAreasAxesDPO>());

        var areas = annotation.Chart?.ChartAreas;
        if (areas is null)
            return new AnnotationAxisUITypeEditorEditValueResponse();

        return new AnnotationAxisUITypeEditorEditValueResponse(areas.Select(a => new ChartAreasAxesDPO(a.Name, a.Axes)).ToList().AsReadOnly());
    }
}
