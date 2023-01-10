using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;
using System.Diagnostics.CodeAnalysis;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class ImageValueEditorPaintValueRequest : Request
    {
        public object? ImageOwnerObj { get; private set; }
        public string? ImageURL { get; private set; }

        public ImageValueEditorPaintValueRequest(object imageOwnerObj, string imageURL)
        {
            ImageOwnerObj = imageOwnerObj;
            ImageURL = imageURL;
        }

        public ImageValueEditorPaintValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            ImageOwnerObj = reader.ReadObject(nameof(ImageOwnerObj));
            ImageURL = reader.ReadString(nameof(ImageURL));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(ImageOwnerObj), ImageOwnerObj);
            writer.Write(nameof(ImageURL), ImageURL);
        }
    }
}
