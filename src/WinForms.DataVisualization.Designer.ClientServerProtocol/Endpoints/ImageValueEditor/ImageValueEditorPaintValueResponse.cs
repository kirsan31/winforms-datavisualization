using System.Drawing;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class ImageValueEditorPaintValueResponse : Response
    {
        public byte[]? Image { get; private set; }


        public ImageValueEditorPaintValueResponse() { }

        public ImageValueEditorPaintValueResponse(IDataPipeReader reader) : base(reader) { }

        public ImageValueEditorPaintValueResponse(byte[] img)
        {
            Image = img;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            Image = reader.ReadByteArray(nameof(Image));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(Image), Image);
        }
    }
}
