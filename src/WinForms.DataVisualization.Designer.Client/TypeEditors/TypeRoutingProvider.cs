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
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(ImageValueEditor), typeof(ImageValueEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(AxesArrayEditor), typeof(AxesArrayEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(GradientEditor), typeof(GradientEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(ColorPaletteEditor), typeof(ColorPaletteEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(ChartColorEditor), typeof(ChartColorEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(FlagsEnumUITypeEditor), typeof(FlagsEnumUITypeEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(HatchStyleEditor), typeof(HatchStyleEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(MarkerStyleEditor), typeof(MarkerStyleEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(KeywordsStringEditor), typeof(KeywordsStringEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(ChartTypeEditor), typeof(ChartTypeEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(ChartCollectionEditor), typeof(ChartCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(SeriesDataSourceMemberValueAxisUITypeEditor), typeof(SeriesDataSourceMemberValueAxisUITypeEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(DataPointCollectionEditor), typeof(DataPointCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(SeriesCollectionEditor), typeof(SeriesCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(LegendItemCollectionEditor), typeof(LegendItemCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(LegendCollectionEditor), typeof(LegendCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(LegendCellColumnCollectionEditor), typeof(LegendCellColumnCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(LegendCellCollectionEditor), typeof(LegendCellCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(AnnotationCollectionEditor), typeof(AnnotationCollectionEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(AnchorPointUITypeEditor), typeof(AnchorPointUITypeEditor)),
                new TypeRoutingDefinition(TypeRoutingKinds.Editor, nameof(AnnotationAxisUITypeEditor), typeof(AnnotationAxisUITypeEditor)),
            };
        }
    }
}
