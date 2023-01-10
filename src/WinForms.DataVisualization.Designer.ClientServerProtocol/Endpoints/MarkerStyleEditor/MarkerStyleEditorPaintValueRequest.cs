using Microsoft.DotNet.DesignTools.Protocol.DataPipe;
using Microsoft.DotNet.DesignTools.Protocol.Endpoints;
using System;
using System.Diagnostics.CodeAnalysis;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    public class MarkerStyleEditorPaintValueRequest : Request
    {
        public object? MarkerOwnerObj { get; private set; }

        public MarkerStyleEditorPaintValueRequest(object markerOwnerObj)
        {
            MarkerOwnerObj = markerOwnerObj ?? throw new ArgumentNullException(nameof(markerOwnerObj));
        }

        public MarkerStyleEditorPaintValueRequest(IDataPipeReader reader) : base(reader) {}

        protected override void ReadProperties(IDataPipeReader reader)
        {
            MarkerOwnerObj = reader.ReadObject(nameof(MarkerOwnerObj));
        }

        protected override void WriteProperties(IDataPipeWriter writer)
        {
            writer.WriteObject(nameof(MarkerOwnerObj), MarkerOwnerObj);
        }
    }
}
