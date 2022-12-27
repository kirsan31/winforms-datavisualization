using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Utilities;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.ImageValueEditorPaintValue)]
internal class ImageValueEditorPaintValueHandler : RequestHandler<ImageValueEditorPaintValueRequest, ImageValueEditorPaintValueResponse>
{
    public override ImageValueEditorPaintValueResponse HandleRequest(ImageValueEditorPaintValueRequest request)
    {
        var ownerObj = request.ImageOwnerObj;
        ImageLoader? imageLoader = null;

        // Get image loader
        if (ownerObj is Chart chart)
        {
            imageLoader = (ImageLoader)chart.GetService(typeof(ImageLoader));
        }
        else if (ownerObj is IChartElement chartElement)
        {
            imageLoader = chartElement.Common.ImageLoader;
        }

        if (imageLoader is null || string.IsNullOrEmpty(request.ImageURL))
            return new ImageValueEditorPaintValueResponse();

        // Load a image
        try
        {
            Image image = imageLoader.LoadImage(request.ImageURL);
            using var ms = new MemoryStream();
            image.Save(ms, image.RawFormat);
            return new ImageValueEditorPaintValueResponse(ms.ToArray());
        }
        catch 
        {
            return new ImageValueEditorPaintValueResponse();
        }
    }
}
