using Microsoft.DotNet.DesignTools.Protocol.DataPipe;

namespace DataVisualization.ClientServerProtocol
{
    class StringDPO : IDataPipeObject
    {
        public string? String { get; private set; }


        public StringDPO() { }

        public StringDPO(string? str)
        {
            String = str;
        }


        public void ReadProperties(IDataPipeReader reader)
        {
            String = reader.ReadString(nameof(String));
        }

        public void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(String), String);
        }
    }
}