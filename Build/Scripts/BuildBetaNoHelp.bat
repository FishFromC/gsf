::*******************************************************************************************************
::  BuildBetaNoHelp.bat - Gbtc
::
::  Copyright � 2013, Grid Protection Alliance.  All Rights Reserved.
::
::  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
::  the NOTICE file distributed with this work for additional information regarding copyright ownership.
::  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
::  not use this file except in compliance with the License. You may obtain a copy of the License at:
::
::      http://www.opensource.org/licenses/eclipse-1.0.php
::
::  Unless agreed to in writing, the subject software distributed under the License is distributed on an
::  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
::  License for the specific language governing permissions and limitations.
::
::  Code Modification History:
::  -----------------------------------------------------------------------------------------------------
::  10/05/2009 - Pinal C. Patel
::       Generated original version of source code.
::  10/20/2009 - Pinal C. Patel
::       Modified to force a build and suppress archives from being published to public locations.
::  10/03/2010 - Pinal C. Patel
::       Updated to use MSBuild 4.0.
::  08/25/2014 - Gavin E. Holden
::       Modified to call CommonBuild.bat 
::  08/14/2015 - Stephen C. Wills
::       Adapted from BuildBeta.bat.
::
::*******************************************************************************************************

echo off

SetLocal EnableDelayedExpansion

ECHO BuildBetaNoHelp: CALL CommonBuild.bat %*
CALL CommonBuild.bat %*

ECHO BuildBetaNoHelp: C:\WINDOWS\Microsoft.NET\Framework\v4.0.30319\msbuild.exe GridSolutionsFramework.buildproj /p:BuildDeployFolder=%deploy%;SkipNuGetPackages=true;ForceBuild=true;SkipHelpFiles=true /l:FileLogger,Microsoft.Build.Engine;logfile=%logger%
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" GridSolutionsFramework.buildproj /p:BuildDeployFolder=%deploy%;SkipNuGetPackages=true;ForceBuild=true;SkipHelpFiles=true /l:FileLogger,Microsoft.Build.Engine;logfile=%logger%  
endlocal