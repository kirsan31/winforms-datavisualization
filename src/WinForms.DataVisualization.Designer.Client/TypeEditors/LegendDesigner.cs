// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time support classes for Legend.
//


using System;
using System.ComponentModel;

using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Designer editor for the custom legend items collection.
    /// </summary>
    internal class LegendItemCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public LegendItemCollectionEditor(Type type) : base(type) { }


        /// <summary>
        /// Edit object's value.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="provider">Service provider.</param>
        /// <param name="value">Value.</param>
        /// <returns>Object.</returns>
        public override object? EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            return base.EditValue(context, provider, value);
        }
    }


    /// <summary>
    /// Designer editor for the legend collection.
    /// </summary>
    internal class LegendCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public LegendCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.LegendCollectionEditor;
    }


    /// <summary>
    /// Designer editor for the legend column collection.
    /// </summary>
    internal class LegendCellColumnCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public LegendCellColumnCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.LegendCellColumnCollectionEditor;
    }


    /// <summary>
    /// Designer editor for the legend cell collection.
    /// </summary>
    internal class LegendCellCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public LegendCellCollectionEditor(Type type) : base(type) { }


        protected override string Name => CollectionEditorNames.LegendCellCollectionEditor;
    }
}