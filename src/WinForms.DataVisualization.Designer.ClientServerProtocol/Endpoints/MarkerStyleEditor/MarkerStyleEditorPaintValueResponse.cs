using System.Drawing;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class MarkerStyleEditorPaintValueResponse : Response
    {
        public static new readonly MarkerStyleEditorPaintValueResponse Empty = new MarkerStyleEditorPaintValueResponse();

        public Color MarkerColor { get; private set; }
        public int MarkerSize { get; private set; }
        public Color MarkerBorderColor { get; private set; }
        public int MarkerBorderWidth { get; private set; }

        public bool IsEmpty => MarkerSize == default;


        public MarkerStyleEditorPaintValueResponse() { }

        public MarkerStyleEditorPaintValueResponse(IDataPipeReader reader) : base(reader) { }

        public MarkerStyleEditorPaintValueResponse(Color markerColor, int markerSize, Color markerBorderColor, int markerBorderWidth)
        {
            MarkerColor = markerColor;
            MarkerSize = markerSize;
            MarkerBorderColor = markerBorderColor;
            MarkerBorderWidth = markerBorderWidth;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            MarkerColor = reader.ReadColor(nameof(MarkerColor));
            MarkerSize = reader.ReadInt32(nameof(MarkerSize));
            MarkerBorderColor = reader.ReadColor(nameof(MarkerBorderColor));
            MarkerBorderWidth = reader.ReadInt32(nameof(MarkerBorderWidth));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(MarkerColor), MarkerColor);
            writer.Write(nameof(MarkerSize), MarkerSize);
            writer.Write(nameof(MarkerBorderColor), MarkerBorderColor);
            writer.Write(nameof(MarkerBorderWidth), MarkerBorderWidth);
        }
    }
}
