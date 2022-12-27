using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;
using System.Diagnostics.CodeAnalysis;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class GradientEditorPaintValueRequest : Request
    {
        public object? GradientStyleOwnerObj { get; private set; }

        public GradientEditorPaintValueRequest(object gradientStyleOwnerObj)
        {
            GradientStyleOwnerObj = gradientStyleOwnerObj ?? throw new ArgumentNullException(nameof(gradientStyleOwnerObj));
        }

        public GradientEditorPaintValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            GradientStyleOwnerObj = reader.ReadObject(nameof(GradientStyleOwnerObj));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(GradientStyleOwnerObj), GradientStyleOwnerObj);
        }
    }
}
