using System.Collections.Generic;

using DataVisualization.ClientServerProtocol;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class SeriesDSMemberValueAxisEditorEditValueResponse : Response
    {
        internal IReadOnlyList<StringDPO>? DSMemberNames { get; private set; }


        public SeriesDSMemberValueAxisEditorEditValueResponse() { }

        public SeriesDSMemberValueAxisEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        internal SeriesDSMemberValueAxisEditorEditValueResponse(IReadOnlyList<StringDPO>? registeredKeywords)
        {
            DSMemberNames = registeredKeywords;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            DSMemberNames = reader.ReadDataPipeObjectArray<StringDPO>(nameof(DSMemberNames));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteDataPipeObjectArray(nameof(DSMemberNames), DSMemberNames);
        }
    }
}
