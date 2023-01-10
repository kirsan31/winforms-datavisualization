using System.Drawing;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class GradientEditorPaintValueResponse : Response
    {
        public Color Color1 { get; private set; }
        public Color Color2 { get; private set; }


        public GradientEditorPaintValueResponse() { }

        public GradientEditorPaintValueResponse(IDataPipeReader reader) : base(reader) { }

        public GradientEditorPaintValueResponse(Color color1, Color color2)
        {
            Color1 = color1;
            Color2 = color2;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            Color1 = reader.ReadColor(nameof(Color1));
            Color2 = reader.ReadColor(nameof(Color2));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(Color1), Color1);
            writer.Write(nameof(Color2), Color2);
        }
    }
}
