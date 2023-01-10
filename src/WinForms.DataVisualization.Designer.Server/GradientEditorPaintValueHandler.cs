using System.Drawing;
using System.Reflection;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.GradientEditorPaintValue)]
internal class GradientEditorPaintValueHandler : RequestHandler<GradientEditorPaintValueRequest, GradientEditorPaintValueResponse>
{
    public override GradientEditorPaintValueResponse HandleRequest(GradientEditorPaintValueRequest request)
    {
        var ownerObj = request.GradientStyleOwnerObj;
        if (ownerObj is null)
            return new GradientEditorPaintValueResponse();

        Color color1 = Color.Empty, color2 = Color.Empty;
        // Get color properties using reflection
        PropertyInfo? propertyInfo = ownerObj.GetType().GetProperty("BackColor");
        if (propertyInfo is not null)
        {
            color1 = (Color)(propertyInfo.GetValue(ownerObj) ?? Color.Empty);
        }
        else
        {
            // If object do not have "BackColor" property try using "Color" property 
            propertyInfo = ownerObj.GetType().GetProperty("Color");
            if (propertyInfo is not null)
            {
                color1 = (Color)(propertyInfo.GetValue(ownerObj) ?? Color.Empty);
            }
        }

        propertyInfo = ownerObj.GetType().GetProperty("BackSecondaryColor");
        if (propertyInfo is not null)
        {
            color2 = (Color)(propertyInfo.GetValue(ownerObj) ?? Color.Empty);
        }

        return new GradientEditorPaintValueResponse(color1, color2);
    }
}
