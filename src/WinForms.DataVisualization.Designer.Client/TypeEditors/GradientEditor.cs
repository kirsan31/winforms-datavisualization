// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time gradient editor class. 
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
    /// AxisName editor for the gradient type.
    /// Paints a rectangle with gradient sample.
    /// </summary>
    internal class GradientEditor : UITypeEditor
    {
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
            GradientStyle gradientStyle;
            if (e.Value is not EnumProxy enumProxy || (gradientStyle = enumProxy.AsEnumValue<GradientStyle>()) == GradientStyle.None)
                return;

            // Try to get original color from the object
            Color color1 = Color.Black;
            Color color2 = Color.White;
            if (e.Context?.Instance is not null)
            {
                var client = e.Context.GetRequiredService<IDesignToolsClient>();
                var sender = client.Protocol.GetEndpoint<GradientEditorPaintValueEndpoint>().GetSender(client);
                var response = sender.SendRequest(new GradientEditorPaintValueRequest(e.Context.Instance));
                color1 = response.Color1;
                color2 = response.Color2;
            }

            // Check if colors are valid
            if (color1 == Color.Empty)
            {
                color1 = Color.Black;
            }

            if (color2 == Color.Empty)
            {
                color2 = Color.White;
            }

            if (color1 == color2)
            {
                color2 = Color.FromArgb(color1.B, color1.R, color1.G);
            }

            // Draw gradient sample
            using Brush brush = ChartGraphics.GetGradientBrush(e.Bounds, color1, color2, gradientStyle);
            e.Graphics.FillRectangle(brush, e.Bounds);
        }
    }
}