using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Editors;

namespace WinForms.DataVisualization.Designer.Server;

internal partial class ChartCollectionEditor : CollectionEditor
{
    private Chart? _chart;
    private ITypeDescriptorContext? _context;
    private INameController? _nameController;


    public ChartCollectionEditor(IServiceProvider serviceProvider, Type collectionType)
            : base(serviceProvider, collectionType)
    {
    }


    protected override CollectionEditorViewModel BeginEditValue(ITypeDescriptorContext context, object value)
    {
        _context = context;
        if (context?.Instance is Chart chart)
            // Save current control type descriptor context
            _chart = chart;

        if ((value is ChartAreaCollection || value is LegendCollection) && value is INameController controller)
        {
            _nameController = controller;
            controller.DoSnapshot(true,
                new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanging),
                new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanged));

            controller.IsColectionEditing = true;
        }
        else
        {
            _nameController = null;
        }

        return base.BeginEditValue(context!, value);
    }

    protected override object EndEditValue(bool commitChange)
    {
        if (_nameController is not null)
        {
            _nameController.IsColectionEditing = false;
            _nameController.DoSnapshot(false,
                new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanging),
                new EventHandler<NameReferenceChangedEventArgs>(OnNameReferenceChanged));

            _nameController = null;
        }

        return base.EndEditValue(commitChange);
    }

    protected override object SetItems(object editValue, object[] value)
    {
        var result = base.SetItems(editValue, value);
        if (_chart is null || _nameController is null || _context?.GetService(typeof(IComponentChangeService)) is not IComponentChangeService svc)
            return result;

        if (result is not IList newList)
            return result;

        bool elementsRemoved = false;
        foreach (ChartNamedElement element in _nameController.Snapshot)
        {
            if (newList.IndexOf(element) < 0)
            {
                elementsRemoved = true;
                break;
            }
        }

        if (elementsRemoved)
        {
            svc.OnComponentChanging(this._chart, null);
            ChartNamedElement? defaultElement = (ChartNamedElement?)(newList.Count > 0 ? newList[0] : null);
            foreach (ChartNamedElement element in _nameController.Snapshot)
            {
                if (newList.IndexOf(element) < 0)
                    _nameController.OnNameReferenceChanged(new NameReferenceChangedEventArgs(element, defaultElement));
            }

            svc.OnComponentChanged(this._chart, null, null, null);
        }

        return result;
    }


    /// <summary>
    /// Called when [name reference changing].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
    private void OnNameReferenceChanging(object? sender, NameReferenceChangedEventArgs e)
    {
        if (_chart is not null && _context?.GetService(typeof(IComponentChangeService)) is IComponentChangeService svc)
            svc.OnComponentChanging(_chart, null);
    }

    /// <summary>
    /// Called when [name reference changed].
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="NameReferenceChangedEventArgs"/> instance containing the event data.</param>
    private void OnNameReferenceChanged(object? sender, NameReferenceChangedEventArgs e)
    {
        if (_chart is not null && _context?.GetService(typeof(IComponentChangeService)) is IComponentChangeService svc)
            svc.OnComponentChanged(_chart, null, null, null);
    }
}