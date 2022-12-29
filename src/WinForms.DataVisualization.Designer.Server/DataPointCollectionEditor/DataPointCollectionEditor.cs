using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Editors;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class DataPointCollectionEditor : ChartCollectionEditor
{
    public DataPointCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    protected override CollectionEditorViewModel BeginEditValue(ITypeDescriptorContext context, object value)
    {
        if (context?.Instance is not null && context.Instance is not Series)
            throw new InvalidOperationException(SR.ExceptionEditorMultipleSeriesEditiingUnsupported);

        return base.BeginEditValue(context!, value);
    }

    protected override object CreateInstance(Type itemType)
    {
        if (Context?.Instance is not null)
        {
            if (Context.Instance is Series series)
            {
                DataPoint newDataPoint = new DataPoint(series);
                return newDataPoint;
            }
            else if (Context.Instance is Array)
            {
                throw new InvalidOperationException(SR.ExceptionEditorMultipleSeriesEditiingUnsupported);
            }
        }

        return base.CreateInstance(itemType);
    }
}