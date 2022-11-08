// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Design-time marker style editor class. 
//

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.DataVisualization.Charting;

namespace System.Windows.Forms.Design.DataVisualization.Charting;

/// <summary>
/// AxisName editor for the marker style.
/// Paints a rectangle with marker sample.
/// </summary>
internal class MarkerStyleEditor : UITypeEditor, IDisposable
{
    #region Editor method and properties

    private ChartGraphics _chartGraph;
    private bool _disposed;

    /// <summary>
    /// Override this function to support palette colors drawing
    /// </summary>
    /// <param name="context">Descriptor context.</param>
    /// <returns>Can paint values.</returns>
    public override bool GetPaintValueSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    /// <summary>
    /// Override this function to support palette colors drawing
    /// </summary>
    /// <param name="e">Paint value event arguments.</param>
    public override void PaintValue(PaintValueEventArgs e)
    {
        if (e.Value is not MarkerStyle markerStyle)
            return;

        // Create chart graphics object
        _chartGraph ??= new ChartGraphics(null);
        _chartGraph.Graphics = e.Graphics;

        // Get marker properties
        DataPointCustomProperties attributes = null;
        if (e.Context is not null && e.Context.Instance is not null && markerStyle != MarkerStyle.None)
        {
            // Check if several object selected
            object attrObject = e.Context.Instance;
            if (e.Context.Instance is Array array && array.Length > 0)
                attrObject = array.GetValue(0);

            // Check what kind of object is selected
            if (attrObject is Series)
            {
                attributes = (DataPointCustomProperties)attrObject;
            }
            else if (attrObject is DataPoint)
            {
                attributes = (DataPointCustomProperties)attrObject;
            }
            else if (attrObject is DataPointCustomProperties)
            {
                attributes = (DataPointCustomProperties)attrObject;
            }
            else if (attrObject is LegendItem)
            {
                attributes = new DataPointCustomProperties();
                attributes.MarkerColor = ((LegendItem)attrObject).markerColor;
                attributes.MarkerBorderColor = ((LegendItem)attrObject).markerBorderColor;
                attributes.MarkerSize = ((LegendItem)attrObject).markerSize;
            }
        }

        // Draw marker sample
        if (attributes is not null)
        {
            PointF point = new PointF(e.Bounds.X + e.Bounds.Width / 2F - 0.5F, e.Bounds.Y + e.Bounds.Height / 2F - 0.5F);
            Color color = (attributes.MarkerColor == Color.Empty) ? Color.Black : attributes.MarkerColor;
            int size = attributes.MarkerSize;
            if (size > e.Bounds.Height - 4)
                size = e.Bounds.Height - 4;

            _chartGraph.DrawMarkerAbs(
                point,
                markerStyle,
                size,
                color,
                attributes.MarkerBorderColor,
                attributes.MarkerBorderWidth,
                string.Empty,
                Color.Empty,
                0,
                Color.Empty,
                RectangleF.Empty,
                true);
        }
    }

    #endregion Editor method and properties

    #region IDisposable Members

    /// <summary>
    /// Disposes resources used by this object.
    /// </summary>
    /// <param name="disposing">Whether this method was called form Dispose() or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _chartGraph?.Dispose();

        _disposed = true;
    }

    /// <summary>
    /// Disposes all resources used by this object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    #endregion IDisposable Members
}
