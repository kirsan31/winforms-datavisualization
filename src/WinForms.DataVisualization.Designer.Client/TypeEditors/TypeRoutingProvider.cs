using Microsoft.DotNet.DesignTools.Client.TypeRouting;
using System.Collections.Generic;

namespace Charting.Designer.Client
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
