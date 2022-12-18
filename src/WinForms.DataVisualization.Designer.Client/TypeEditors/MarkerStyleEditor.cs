// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time marker style editor class. 
//


using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms.DataVisualization.Charting;

using Microsoft.DotNet.DesignTools.Client;
using Microsoft.DotNet.DesignTools.Client.Proxies;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{

    /// <summary>
    /// AxisName editor for the marker style.
    /// Paints a rectangle with marker sample.
    /// </summary>
    internal class MarkerStyleEditor : UITypeEditor
    {
        private ChartGraphics? _chartGraph;


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
            MarkerStyle markerStyle;
            if (e.Context?.Instance is null || e.Value is not EnumProxy enumProxy || (markerStyle = enumProxy.AsEnumValue<MarkerStyle>()) == MarkerStyle.None)
                return;

            // Check if several object selected
            object attrObject = e.Context.Instance;
            if (e.Context.Instance is Array array && array.Length > 0)
                attrObject = array.GetValue(0);

            // Get marker properties
            var client = e.Context.GetRequiredService<IDesignToolsClient>();
            var sender = client.Protocol.GetEndpoint<MarkerStyleEditorPaintValueEndpoint>().GetSender(client);
            var response = sender.SendRequest(new MarkerStyleEditorPaintValueRequest(attrObject));
            if (response.IsEmpty)
                return;

            // Create chart graphics object
            _chartGraph ??= new ChartGraphics();
            _chartGraph.Graphics = e.Graphics;
            // Draw marker sample
            PointF point = new PointF(e.Bounds.X + e.Bounds.Width / 2F - 0.5F, e.Bounds.Y + e.Bounds.Height / 2F - 0.5F);
            Color color = (response.MarkerColor == Color.Empty) ? Color.Black : response.MarkerColor;
            int size = response.MarkerSize;
            if (size > e.Bounds.Height - 4)
                size = e.Bounds.Height - 4;

            _chartGraph.DrawMarkerAbs(point, markerStyle, size, color, response.MarkerBorderColor, response.MarkerBorderWidth, 0, Color.Empty, true);
            _chartGraph.Graphics = null;
        }
    }
}