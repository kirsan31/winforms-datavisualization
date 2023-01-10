using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public record SeriesDataPointDPO : IDataPipeObject
    {
        public string? SeriesName { get; private set; }

        public IReadOnlyList<object>? DataPoints { get; private set; }
        
        
        public SeriesDataPointDPO() { }

        public SeriesDataPointDPO(string seriesName, IReadOnlyList<object> dataPoints)
        {
            SeriesName = seriesName;
            DataPoints = dataPoints;
        }

        public void ReadProperties(IDataPipeReader reader)
        {
            SeriesName = reader.ReadString(nameof(SeriesName));
            DataPoints = reader.ReadArray(nameof(DataPoints), (r) => r.ReadObject()!);
        }

        public void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(SeriesName), SeriesName);
            writer.WriteArray(nameof(DataPoints), DataPoints, (w, o) => w.WriteObject(o));
        }
    }
}