using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class AnnotationAxisUITypeEditorEditValueResponse : Response
    {
        internal IReadOnlyList<ChartAreasAxesDPO>? AxesByChartAreas { get; private set; }


        public AnnotationAxisUITypeEditorEditValueResponse() { }

        public AnnotationAxisUITypeEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        internal AnnotationAxisUITypeEditorEditValueResponse(IReadOnlyList<ChartAreasAxesDPO>? axesByChartAreas)
        {
            AxesByChartAreas = axesByChartAreas;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            AxesByChartAreas = reader.ReadDataPipeObjectArray<ChartAreasAxesDPO>(nameof(AxesByChartAreas));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteDataPipeObjectArray(nameof(AxesByChartAreas), AxesByChartAreas);
        }
    }
}
