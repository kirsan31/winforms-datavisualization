using System.Collections.Generic;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using System.Windows.Forms.DataVisualization.Charting.Utilities;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class KeywordsStringEditorEditValueResponse : Response
    {
        public bool IsEmpty => RegisteredKeywords is null && MaxYValueNumber == 0;
        internal IReadOnlyList<KeywordInfo>? RegisteredKeywords { get; private set; }
        public int MaxYValueNumber { get; private set; }


        public KeywordsStringEditorEditValueResponse() { }

        public KeywordsStringEditorEditValueResponse(IDataPipeReader reader) : base(reader) { }

        internal KeywordsStringEditorEditValueResponse(IReadOnlyList<KeywordInfo>? registeredKeywords, int maxYValueNumber)
        {
            RegisteredKeywords = registeredKeywords;
            MaxYValueNumber = maxYValueNumber;
        }

        protected override void ReadProperties(IDataPipeReader reader)
        {
            RegisteredKeywords = reader.ReadDataPipeObjectArray<KeywordInfo>(nameof(RegisteredKeywords));
            MaxYValueNumber = reader.ReadInt32(nameof(MaxYValueNumber));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteDataPipeObjectArray(nameof(RegisteredKeywords), RegisteredKeywords);
            writer.Write(nameof(MaxYValueNumber), MaxYValueNumber);
        }
    }
}
