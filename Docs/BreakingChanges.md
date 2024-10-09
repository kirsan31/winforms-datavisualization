## Most notable breaking changes

- `SqlCommand`, `SqlDataAdapter`, `SqlDataReader` can't be used as chart's `DataSource` anymore.

- ~~**Since 1.8 preview:**~~ **reverted in 1.9**  
  ~~If you use Axes auto min / max values and call `DataPointCollection.Clear()` method for already empty collection:~~
    - ~~Old behavior - axes min / max values will be recalculated on next chart paint.~~
    - ~~New behavior - axes min / max values will not be recalculated.~~  

- **Since 1.9 preview:**  
  The algorithm for determining indexed series has been changed. Previously, a series was considered indexed if the `IsXValueIndexed` flag was set or if all its data points had an X-axis value equal to 0.
  To draw a vertical line at an X-axis value of 0, you had to set a custom flag, `IsXAxisQuantitative`.  
  Now, a series is considered indexed only if the `IsXValueIndexed` flag is set. The custom flag, `IsXAxisQuantitative`, has been removed.

    Recommended actions:
    - If your indexed series were defined solely by having all their X-axis values equal to zero, you need to set the `IsXValueIndexed` flag for these series.
    - If you were using the `IsXAxisQuantitative` flag, you should remove it.

- **Since 1.10 preview:**  
  - Deprecated `#SER` keyword removed - use `#SERIESNAME` instead.
  - `DataPointCustomProperties.Item[int]` indexer removed. *It was no use anyway.*
  - `IDisposable` Interface implementation removed from: `AnnotationCollection`, `SeriesCollection`, `LabelStyle`, `StripLinesCollection`, `StripLine`, `Title`, `TitleCollection`, `LegendCellColumn`, `LegendCell`, `LegendCellCollection`, `LegendItem`, `LegendCellColumnCollection`, `Legend`, `LegendCollection`, `Annotation` and all it's derived classes except `PolylineAnnotation`.