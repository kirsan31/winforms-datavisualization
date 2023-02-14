## New API elements
This is not a complete list. For example some methods overloads are not listed here.

- `ChartArea3DStyle.ZDepthRealCalc`
    ```cs
    /// <summary>
    /// Gets or sets a value indicating whether Z depth will be calculated base on series ZValue property. Only for Lines and Points family charts.
    /// </summary>
    public bool ZDepthRealCalc { get; set; }
    ```
- `ZValue` *custom property for Lines and Points series in `ZDepthRealCalc` 3D mode:  
  Z value of the series (`float` number from 0 to 100% of the chart area)*.

- `System.Windows.Forms.DataVisualization.Charting.Utilities.CustomPropertyName` *contains constant strings defining names of all custom properties used in the chart*. Are public now.

- `DataPointCollection.ClearAfter(int)`
    ```cs
    /// <summary>
    /// Removes all elements after "index" from the "DataPointCollection".
    /// </summary>
    /// <param name="index">The index after witch to remove elements. To remove all elements pass -1 here.</param>
    public void ClearAfter(int index);
    ```

 - `Axis.PixelToRelativePosition(Double)`
     ```cs
    /// <summary>
    /// This function converts a pixel position to relative position.
    /// </summary>
    /// <param name="position">Pixel position.</param>
    /// <returns>Relative position.</returns>
    public double PixelToRelativePosition(double position)
    ```