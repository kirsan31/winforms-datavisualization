# Data Visualization

This repository is a fork of https://github.com/dotnet/winforms-datavisualization that contains source code of the `System.Windows.Forms.DataVisualization` namespace that provides charting for WinForms.

## What is done.
- .Net6+ support.
- Lots of performance improvements.
- Partial new WinForms designer support.  
  - Serialisation / Desirialisation - all existing forms will work and you can drag chart control from toolbox on new forms. 
  - All not custom editors are fully working.
  - **Most of the custom editors are woking as defaults - without any custom changes.**
- Small bugs fixes.
- [Nuget package](https://www.nuget.org/packages/WinForms.DataVisualization/).

## To Do.
- Properly designer support. Adapt custom editors as match as possible without full rewright.
- Probably full designer parity with .net freimwork version 🤔

## Getting started with Chart Controls

The best way to learn about Chart Controls is by looking at the [sample solution](https://github.com/kirsan31/winforms-datavisualization/tree/dev/sample/ChartSamples) where via interactive experience with the app you can learn about every chart type and every major feature. While modifying the control parameters and instantly seeing how that affects the look of the control, you can also get the generated C# or Visual Basic code to use in your apps.

![Chart Controls](sample-screenshot.png)
