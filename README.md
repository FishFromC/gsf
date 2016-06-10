![GSF](http://www.gridprotectionalliance.org/images/technology/GSF.png)
# Grid Solutions Framework

The Grid Solutions Framework (GSF) is an extensive open source collection of .NET code used by electric power utilities and various open source projects. GSF contains a large variety of code useful for any .NET project consisting of hundreds of class libraries that extend or expand the functionality included in the .NET Framework with a focus on tools and protocol parsers that are useful for the electric power industry.

[Time-series Application Components](http://www.gridprotectionalliance.org/docs/products/gsf/tsl-components-2015.pdf)

Example Components:

![Components](http://raw.github.com/GridProtectionAlliance/gsf/master/Source/Documentation/Images/GSF%20components%20(small).png)

* Adapter based time-series processing and data exchange library with WPF UI configuration screens for managing real-time data
* Various utility protocol libraries, e.g., PQDIF, COMTRADE, IEEE C37.118, IEEE 1344, IEC 61850-90-5, UTK F-NET, SEL Fast Message, BPA PDCstream, Macrodyne, MMS
* Configuration API for easy and secure access to application settings
* High-speed binary parsing framework for implementing protocol parsing
* Well vetted abstract asynchronous communications framework (IServer / IClient) for socket (TCP, UDP including Multicast all over IPv6 or IPv4), serial and file based communications
* Security framework for implementing role-based security in ASP.NET, WCF, WPF, Windows Forms and Windows Services
* Windows Service Template for quickly developing windows services with remoting capability
* Base WCF service for creating WCF services that can be self-hosted in Windows Service, Console, WPF or Windows Forms application
* WCF-based Message Bus that can be hosted inside ASP.NET, Windows Service, Console, WPF or Windows Forms application

![Poster](http://raw.github.com/GridProtectionAlliance/gsf/master/Source/Documentation/Images/GSF%20Poster%20(small).png)

[Click for full size image](http://raw.github.com/GridProtectionAlliance/gsf/master/Source/Documentation/Images/GSF%20Poster%20(4x3).png)

The Grid Solutions Framework, administered by the [Grid Protection Alliance](https://www.gridprotectionalliance.org/) (GPA), is a combination of the existing Time Series Framework and TVA Code Library projects that were hosted on CodePlex.  In creating the GSF, new code components have been added and the libraries have been refactored to make this integrated framework more secure and significantly better performing.

The [open Phasor Data Concentrator](https://github.com/GridProtectionAlliance/openPDC) (openPDC), [Secure Information Exchange Gateway](https://github.com/GridProtectionAlliance/SIEGate) (SIEGate) and [open Historian](https://github.com/GridProtectionAlliance/openHistorian) are examples of projects that use the Grid Solutions Framework.

# Documentation

[Online Documentation](https://www.gridprotectionalliance.org/NightlyBuilds/GridSolutionsFramework/Help/)

[NuGet Packages](https://www.nuget.org/packages?q=GSF)

# Deployment

1. Make sure your system meets all the [requirements](#requirements) below.
* Open the project you want to leverage the GSF on in Visual Studio.
* In the solution explorer, right-click on the project or solution.
* Click on manage Nuget packages.
* Search for GSF.
* Install the library you want to use.
* Enjoy.


## Requirements

* 64-bit Windows 7 or newer.
* .NET 4.6 or newer.
* Visual Studio 2012 or higher.
* NuGet (usually included in Visual Studio).

# Contributing
If you would like to contribute please:

1. Read our [styleguide.](https://www.gridprotectionalliance.org/docs/GPA_Coding_Guidelines_2011_03.pdf)
* Fork the repository.
* Code like a boss.
* Create a pull request.
