using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class AnchorPointUITypeEditorEditValueResponse : Response
    {
        internal IReadOnlyList<SeriesDataPointDPO>? DataPointsBySeries { get; private set; }


        public AnchorPointUITypeEditorEditValueResponse() { }

        public AnchorPointUITypeEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        internal AnchorPointUITypeEditorEditValueResponse(IReadOnlyList<SeriesDataPointDPO>? dataPointsBySeries)
        {
            DataPointsBySeries = dataPointsBySeries;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            DataPointsBySeries = reader.ReadDataPipeObjectArray<SeriesDataPointDPO>(nameof(DataPointsBySeries));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteDataPipeObjectArray(nameof(DataPointsBySeries), DataPointsBySeries);
        }
    }
}
