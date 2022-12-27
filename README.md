# WinForms Data Visualization

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/WinForms.DataVisualization)](https://www.nuget.org/packages/WinForms.DataVisualization/)

This repository is a .NET continuation of .NET Framework [`System.Windows.Forms.DataVisualization`](https://github.com/dotnet/winforms-datavisualization) that provides charting for [WinForms](https://github.com/dotnet/winforms).

## What is done.
- .NET 6+ support.
- Lots of performance improvements.
- New WinForms designer support (not complete yet).  
  - Serialization / deserialization - all existing forms will work and you can drag chart control from toolbox on new forms. 
  - Editors porting status information is [here](Editors.md).
- Small bugs fixes.
- [Nuget package](https://www.nuget.org/packages/WinForms.DataVisualization/).

## To Do.
- Full designer parity with .net framework version.

## Getting started with Chart Controls

The best way to learn about Chart Controls is by looking at the [sample solution](sample/ChartSamples) where via interactive experience with the app you can learn about every chart type and every major feature. While modifying the control parameters and instantly seeing how that affects the look of the control, you can also get the generated C# or Visual Basic code to use in your apps.

![Chart Controls](sample-screenshot.png)
