# WinForms Data Visualization

[![Nuget](https://img.shields.io/nuget/v/WinForms.DataVisualization?label=Nuget%20latest%20release)](https://www.nuget.org/packages/WinForms.DataVisualization/) 
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/WinForms.DataVisualization?label=Nuget%20latest%20preview)](https://www.nuget.org/packages/WinForms.DataVisualization#versions-body-tab)

This repository is a [.NET](https://dotnet.microsoft.com/) continuation of [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework) [`System.Windows.Forms.DataVisualization`](https://github.com/dotnet/winforms-datavisualization) that provides charting for [WinForms](https://github.com/dotnet/winforms).

## What is done
- .NET 6+ support.  
- **Full support of new WinForms designer.**  
- Lots of performance improvements.
- [Nuget package](https://www.nuget.org/packages/WinForms.DataVisualization/).  
- Small bugs fixes.

### Main differences from available free .NET WinForms chart controls

- Lots of customization options with wide designer support.
- Using of GDI+ graphics. *This is the killer feature when using charts over RDP.*

### Getting started with Chart Controls

The best way to learn about Chart Controls is by looking at the [sample solution](sample/ChartSamples) where via interactive experience with the app you can learn about every chart type and every major feature. While modifying the control parameters and instantly seeing how that affects the look of the control, you can also get the generated C# or Visual Basic code to use in your apps.

![Chart Controls](sample-screenshot.png)

### Build instructions

To build without errors you need the latest version of [`nuget.exe`](https://www.nuget.org/downloads) and it must be present in your `PATH` variable. It's needed to restore nuget from local storage in the `DesignerTest` project after the package is built.