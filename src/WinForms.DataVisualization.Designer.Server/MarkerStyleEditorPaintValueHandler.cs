using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.MarkerStyleEditorPaintValue)]
internal class MarkerStyleEditorPaintValueHandler : RequestHandler<MarkerStyleEditorPaintValueRequest, MarkerStyleEditorPaintValueResponse>
{
    public override MarkerStyleEditorPaintValueResponse HandleRequest(MarkerStyleEditorPaintValueRequest request)
    {
        // Get marker properties
        DataPointCustomProperties? attributes = null;
        var attrObj = request.MarkerOwnerObj;

        // Check what kind of object is selected
        if (attrObj is Series)
        {
            attributes = (DataPointCustomProperties)attrObj;
        }
        else if (attrObj is DataPoint)
        {
            attributes = (DataPointCustomProperties)attrObj;
        }
        else if (attrObj is DataPointCustomProperties properties)
        {
            attributes = properties;
        }
        else if (attrObj is LegendItem item)
        {
            attributes = new DataPointCustomProperties
            {
                MarkerColor = item.markerColor,
                MarkerBorderColor = item.markerBorderColor,
                MarkerSize = item.markerSize
            };
        }

        if (attributes is null)
            return MarkerStyleEditorPaintValueResponse.Empty;

        return new MarkerStyleEditorPaintValueResponse(attributes.MarkerColor, attributes.MarkerSize, attributes.MarkerBorderColor, attributes.MarkerBorderWidth);
    }
}
