using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;
using System.Diagnostics.CodeAnalysis;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class KeywordsStringEditorEditValueRequest : Request
    {
        public object? EditingObj { get; private set; }

        public KeywordsStringEditorEditValueRequest(object gradientStyleOwnerObj)
        {
            EditingObj = gradientStyleOwnerObj ?? throw new ArgumentNullException(nameof(gradientStyleOwnerObj));
        }

        public KeywordsStringEditorEditValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            EditingObj = reader.ReadObject(nameof(EditingObj));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(EditingObj), EditingObj);
        }
    }
}
