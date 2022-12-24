using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;

using DataVisualization.ClientServerProtocol;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.SeriesDSMemberValueAxisEditorEditValue)]
internal class SeriesDSMemberValueAxisEditorEditValueHandler : RequestHandler<SeriesDSMemberValueAxisEditorEditValueRequest, SeriesDSMemberValueAxisEditorEditValueResponse>
{
    public override SeriesDSMemberValueAxisEditorEditValueResponse HandleRequest(SeriesDSMemberValueAxisEditorEditValueRequest request)
    {
        if (request.OwnerObj is null)
            return new SeriesDSMemberValueAxisEditorEditValueResponse();

        var chart = ConverterHelper.GetChartFromContextInstance(request.OwnerObj);
        if (chart is null)
            return new SeriesDSMemberValueAxisEditorEditValueResponse();

        var dataSource = ChartWinDesigner.controlDesigner?.GetControlDataSource(chart);
        if (dataSource is null)
            return new SeriesDSMemberValueAxisEditorEditValueResponse();

        // Get list of members
        var dSMemberNamesList = ChartImage.GetDataSourceMemberNames(dataSource, true);
        if (dSMemberNamesList is null)
            return new SeriesDSMemberValueAxisEditorEditValueResponse();

        var dSMemberNamesListDPO = dSMemberNamesList.Select(m => new StringDPO(m)).ToList().AsReadOnly();
        return new SeriesDSMemberValueAxisEditorEditValueResponse(dSMemberNamesListDPO);
    }
}
