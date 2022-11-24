using Microsoft.DotNet.DesignTools.Client.TypeRouting;
using System.Collections.Generic;

namespace WinForms.DataVisualization.Designer.Client
{
    [ExportTypeRoutingDefinitionProvider]
    internal class TypeRoutingProvider : TypeRoutingDefinitionProvider
    {
        public override IEnumerable<TypeRoutingDefinition> GetDefinitions()
        {
            return new[]
            {
                new TypeRoutingDefinition(
                    TypeRoutingKinds.Editor, 
                    nameof(ImageValueEditor), 
                    typeof(ImageValueEditor)),
            };
        }
    }
}
