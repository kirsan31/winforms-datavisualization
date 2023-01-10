using System.Windows.Forms.DataVisualization.Charting.Utilities;

using Microsoft.DotNet.DesignTools.Protocol.DataPipe;

namespace WinForms.DataVisualization.Designer.Protocol.Endpoints
{
    internal record KeywordInfoDPO : KeywordInfo, IDataPipeObject
    {
        public KeywordInfoDPO() { }
        public KeywordInfoDPO(KeywordInfo keywordInfo) : base(keywordInfo) { }

        public void ReadProperties(IDataPipeReader reader)
        {
            Name = reader.ReadString(nameof(Name));
            Keyword = reader.ReadString(nameof(Keyword));
            KeywordAliases = reader.ReadString(nameof(KeywordAliases));
            Description = reader.ReadString(nameof(Description));
            AppliesToTypes = reader.ReadString(nameof(AppliesToTypes));
            AppliesToProperties = reader.ReadString(nameof(AppliesToProperties));
            SupportsFormatting = reader.ReadBoolean(nameof(SupportsFormatting));
            SupportsValueIndex = reader.ReadBoolean(nameof(SupportsValueIndex));
        }

        public void WriteProperties(IDataPipeWriter writer)
        {
            writer.Write(nameof(Name), Name);
            writer.Write(nameof(Keyword), Keyword);
            writer.Write(nameof(KeywordAliases), KeywordAliases);
            writer.Write(nameof(Description), Description);
            writer.Write(nameof(AppliesToTypes), AppliesToTypes);
            writer.Write(nameof(AppliesToProperties), AppliesToProperties);
            writer.Write(nameof(SupportsFormatting), SupportsFormatting);
            writer.Write(nameof(SupportsValueIndex), SupportsValueIndex);
        }
    }
}