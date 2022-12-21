// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


//
//  Purpose:	Design-time editor for the strings that may contain
//				keywords. Form automatically retrieves the list of 
//				recognizable keywords from the chart keywords 
//				registry.
//


using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

using Microsoft.DotNet.DesignTools.Client;
using Microsoft.DotNet.DesignTools.Client.Proxies;

using WinForms.DataVisualization.Designer.Protocol.Endpoints;

namespace WinForms.DataVisualization.Designer.Client
{
    /// <summary>
    /// Editor for the string properties that may contain keywords.
    /// </summary>
    internal class KeywordsStringEditor : System.Drawing.Design.UITypeEditor
    {
        #region Editor methods and properties

        /// <summary>
        /// Edit label format by showing the form
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <param name="provider">Provider.</param>
        /// <param name="value">Value to edit.</param>
        /// <returns>Result</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context?.Instance is null || provider is null || context.PropertyDescriptor is null || provider.GetService(typeof(IWindowsFormsEditorService)) is not IWindowsFormsEditorService edSvc)
                return value;

            var client = provider.GetRequiredService<IDesignToolsClient>();
            var sender = client.Protocol.GetEndpoint<KeywordsStringEditorEditValueEndpoint>().GetSender(client);
            var response = sender.SendRequest(new KeywordsStringEditorEditValueRequest(context.Instance));

            if (response.IsEmpty)
                return value;

            // Show editor form
            using KeywordsStringEditorForm form = new KeywordsStringEditorForm(
                (string)value,
                (context.Instance as ObjectProxy)?.TypeIdentity.Name ?? string.Empty,
                context.PropertyDescriptor.Name,
                response.MaxYValueNumber);

            if (response.RegisteredKeywords is not null)
                form.registeredKeywords = response.RegisteredKeywords;

            edSvc.ShowDialog(form);
            value = form.ResultString;

            return value;
        }

        /// <summary>
        /// Show modal form.
        /// </summary>
        /// <param name="context">Editing context.</param>
        /// <returns>Editor style.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context?.Instance is not null)
            {
                return UITypeEditorEditStyle.Modal;
            }

            return base.GetEditStyle(context);
        }

        #endregion
    }
}