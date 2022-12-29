// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time UI editor for Annotations.
//


using System;
using WinForms.DataVisualization.Designer.Protocol;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Designer editor for the Annotation Collection.
    /// </summary>
    internal class AnnotationCollectionEditor : ChartCollectionEditor
    {
        /// <summary>
        /// Object constructor.
        /// </summary>
        public AnnotationCollectionEditor(Type type) : base(type) { }

        protected override string Name => CollectionEditorNames.AnnotationCollectionEditor;
    }
}
