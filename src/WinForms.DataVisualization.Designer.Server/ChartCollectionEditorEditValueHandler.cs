using System;
using System.ComponentModel.Design;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Protocol.Endpoints;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Server;

[ExportRequestHandler(EndpointNames.ChartCollectionEditorEditValue)]
public class ChartCollectionEditorEditValueHandler : RequestHandler<ChartCollectionEditorEditValueRequest, Response.Empty>
{
    public override Response.Empty HandleRequest(ChartCollectionEditorEditValueRequest request)
    {
        if (request.NameController is not INameController nameController)
            return new Response.Empty();

        if (!request.Save)
            nameController.IsColectionEditing = false;

        nameController.DoSnapshot(request.Save,
            new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanging),
            new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanged));

        if (request.Save)
            nameController.IsColectionEditing = true;

        return new Response.Empty();
    }

    /// <summary>
    /// Called when [name reference changing].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
    private void OnNameReferenceChanging(object? sender, NameReferenceChangedEventArgs e)
    {
        if (sender is ChartAreaCollection chartAreas)
        {
            var svc = chartAreas.Chart?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            // this call is necessary for properly name resolution, for example in situation when we rename chartArea (with dependent series etc) and then press cancel
            svc?.OnComponentChanging(chartAreas.Chart!, null);
        }
        else if (sender is LegendCollection legends)
        {
            var svc = legends.Chart?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            // this call is necessary for properly name resolution, for example in situation when we rename legend (with dependent series etc) and then press cancel
            svc?.OnComponentChanging(legends.Chart!, null);
        }
    }

    /// <summary>
    /// Called when [name reference changed].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
    private void OnNameReferenceChanged(object? sender, NameReferenceChangedEventArgs e)
    {
        if (sender is ChartAreaCollection chartAreas)
        {
            var svc = chartAreas.Chart?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            // this call is necessary for properly name resolution, for example in situation when we rename chartArea (with dependent series etc) and then press cancel
            svc?.OnComponentChanged(chartAreas.Chart!, null, null, null);
        }
        else if (sender is LegendCollection legends)
        {
            var svc = legends.Chart?.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            // this call is necessary for properly name resolution, for example in situation when we rename legend (with dependent series etc) and then press cancel
            svc?.OnComponentChanged(legends.Chart!, null, null, null);
        }
    }
}
