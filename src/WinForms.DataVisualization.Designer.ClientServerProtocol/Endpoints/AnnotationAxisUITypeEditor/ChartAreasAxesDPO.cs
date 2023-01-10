using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public record ChartAreasAxesDPO : IDataPipeObject
    {
        public string? ChartAreaName { get; private set; }

        public IReadOnlyList<object>? Axes { get; private set; }
        
        
        public ChartAreasAxesDPO() { }

        public ChartAreasAxesDPO(string chartAreaName, IReadOnlyList<object> axes)
        {
            ChartAreaName = chartAreaName;
            Axes = axes;
        }

        public void ReadProperties(IDataPipeReader reader)
        {
            ChartAreaName = reader.ReadString(nameof(ChartAreaName));
            Axes = reader.ReadArray(nameof(Axes), (r) => r.ReadObject()!);
        }

        public void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(ChartAreaName), ChartAreaName);
            writer.WriteArray(nameof(Axes), Axes, (w, o) => w.WriteObject(o));
        }
    }
}