// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time classes for the image maps.
//


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;

using Microsoft.DotNet.DesignTools.Client;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Image string editor class.
    /// </summary>
    internal class ImageValueEditor : FileNameEditor, IDisposable
    {
        private Dictionary<string, Image>? _imgCache;

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
            if (e.Context?.Instance is null || e.Value is not string imageURL || string.IsNullOrEmpty(imageURL))
                return;

            Image? image = null;
            _imgCache?.TryGetValue(imageURL, out image);
            if (image is null)
            {
                var client = e.Context.GetRequiredService<IDesignToolsClient>();
                var sender = client.Protocol.GetEndpoint<ImageValueEditorPaintValueEndpoint>().GetSender(client);
                var response = sender.SendRequest(new ImageValueEditorPaintValueRequest(e.Context.Instance, imageURL));
                if (response.Image is not null)
                {
                    try
                    {
                        using var ms = new MemoryStream(response.Image);
                        image = Image.FromStream(ms);
                        _imgCache ??= new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
                        _imgCache[imageURL] = image;
                    }
                    catch { }
                }
            }

            if (image is not null)
                e.Graphics.DrawImage(image, e.Bounds);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_imgCache is null)
                return;

            if (disposing)
            {
                foreach (var img in _imgCache.Values)
                    img?.Dispose();
            }

            _imgCache = null;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}