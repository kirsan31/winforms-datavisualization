using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class SeriesDSMemberValueAxisEditorEditValueResponse : Response
    {
        public IReadOnlyList<string>? DSMemberNames { get; private set; }


        public SeriesDSMemberValueAxisEditorEditValueResponse() { }

        public SeriesDSMemberValueAxisEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        public SeriesDSMemberValueAxisEditorEditValueResponse(IReadOnlyList<string>? dSMemberNames)
        {
            DSMemberNames = dSMemberNames;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            DSMemberNames = reader.ReadArray(nameof(DSMemberNames), (r) => r.ReadString()!);
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteArray(nameof(DSMemberNames), DSMemberNames, (w, s) => w.Write(s));
        }
    }
}
