# Data Visualization

This repository is a fork of https://github.com/dotnet/winforms-datavisualization that contains source code of the `System.Windows.Forms.DataVisualization` namespace that provides charting for WinForms.

## What is done.
- .Net6+ support.
- Lots of performance improvements.
- Partial new WinForms designer support. Serialisation/Desirialisation, and most of the editors are somehow working. [More info](https://github.com/kirsan31/winforms-datavisualization/issues?q=is%3Aissue+is%3Aopen+label%3ADesigner).
- Small bugs fixes.

## To Do.
- Properly designer support.  
  Probably full parity with net freimwork version 🤔 _But I'm afraid that for this we need to wait for the release of the designer and the publication of full documentation for the new SDK._
- NuGet package.

## Getting started with Chart Controls

The best way to learn about Chart Controls is by looking at the [sample solution](https://github.com/kirsan31/winforms-datavisualization/tree/dev/sample/ChartSamples) where via interactive experience with the app you can learn about every chart type and every major feature. While modifying the control parameters and instantly seeing how that affects the look of the control, you can also get the generated C# or Visual Basic code to use in your apps.

![Chart Controls](sample-screenshot.png)
