using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class KeywordsStringEditorEditValueResponse : Response
    {
        public bool IsEmpty => RegisteredKeywords is null && MaxYValueNumber == 0;
        internal IReadOnlyList<KeywordInfoDPO>? RegisteredKeywords { get; private set; }
        public int MaxYValueNumber { get; private set; }


        public KeywordsStringEditorEditValueResponse() { }

        public KeywordsStringEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        internal KeywordsStringEditorEditValueResponse(IReadOnlyList<KeywordInfoDPO>? registeredKeywords, int maxYValueNumber)
        {
            RegisteredKeywords = registeredKeywords;
            MaxYValueNumber = maxYValueNumber;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            RegisteredKeywords = reader.ReadDataPipeObjectArray<KeywordInfoDPO>(nameof(RegisteredKeywords));
            MaxYValueNumber = reader.ReadInt32(nameof(MaxYValueNumber));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteDataPipeObjectArray(nameof(RegisteredKeywords), RegisteredKeywords);
            writer.Write(nameof(MaxYValueNumber), MaxYValueNumber);
        }
    }
}
