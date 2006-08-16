'*******************************************************************************************************
'  MainModule.vb - Main module for phasor measurement receiver service
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  05/19/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Text
Imports System.Security.Principal
Imports Tva.Common
Imports Tva.Assembly
Imports Tva.IO.FilePath
Imports Tva.Configuration.Common
Imports Tva.Text.Common

Module MainModule

    Private m_receivers As PhasorMeasurementReceiver()

    Public Sub Main()

        Dim consoleLine, receiverCategory As String
        Dim mapper As PhasorMeasurementMapper = Nothing

        Console.WriteLine(MonitorInformation)

        ' Make sure service settings exist
        With CategorizedSettings("MeasurementReceiver")
            .Add("TotalReceivers", "2", "Total receiver instances to setup (typically one per archive)")
            .Add("PMUDatabase", "Data Source=RGOCSQLD;Initial Catalog=PMU_SDS;Integrated Security=False;user ID=ESOPublic;pwd=4all2see", "PMU metaData database connect string", True)
            .Add("PMUStatusInterval", "5", "Number of seconds of deviation from UTC time (according to local clock) that last PMU reporting time is allowed before considering it offline")
        End With

        With CategorizedSettings("Receiver1")
            .Add("ArchiverIP", "127.0.0.1", "DatAWare Archiver IP")
            .Add("ArchiverCode", "PM", "DatAWare Archiver Plant Code")
            .Add("InitializeOnStartup", "True", "Set to True to intialize phasor measurement mapper at startup")
        End With

        With CategorizedSettings("Receiver2")
            .Add("ArchiverIP", "152.85.38.12", "DatAWare Archiver IP")
            .Add("ArchiverCode", "P0", "DatAWare Archiver Plant Code")
            .Add("InitializeOnStartup", "True", "Set to True to intialize phasor measurement mapper at startup")
        End With

        With CategorizedSettings("ReferenceAngle")
            .Add("AngleCount", "3", "Number of phase angles to use to calculate reference phase angle")
            .Add("FramesPerSecond", "30", "Expected frames per second for incoming data (used for pre-sorting data for reference angle calculations)")
            .Add("LagTime", "0.134", "Allowed lag time, in seconds, for incoming data before starting reference angle calculations")
            .Add("LeadTime", "0.5", "Allowed advanced time, in seconds, to tolerate before assuming incoming measurement time is floating (i.e., not locked)")
        End With

        SaveSettings()

        m_receivers = CreateArray(Of PhasorMeasurementReceiver)(CategorizedIntegerSetting("MeasurementReceiver", "TotalReceivers"))

        For x As Integer = 0 To m_receivers.Length - 1
            receiverCategory = "Receiver" & (x + 1)

            m_receivers(x) = New PhasorMeasurementReceiver( _
                CategorizedStringSetting(receiverCategory, "ArchiverIP"), _
                CategorizedStringSetting(receiverCategory, "ArchiverCode"), _
                CategorizedStringSetting("MeasurementReceiver", "PMUDatabase"), _
                CategorizedIntegerSetting("MeasurementReceiver", "PMUStatusInterval"), _
                CategorizedIntegerSetting("ReferenceAngle", "AngleCount"), _
                CategorizedIntegerSetting("ReferenceAngle", "FramesPerSecond"), _
                CategorizedDoubleSetting("ReferenceAngle", "LagTime"), _
                CategorizedDoubleSetting("ReferenceAngle", "LeadTime"))

            With m_receivers(x)
                AddHandler .StatusMessage, AddressOf DisplayStatusMessage
                If CategorizedBooleanSetting(receiverCategory, "InitializeOnStartup") Then .Initialize()
            End With
        Next

        Do While True
            ' This console window stays open by continually reading in console lines
            consoleLine = Console.ReadLine()

            ' While doing this, we monitor for special commands...
            If consoleLine.StartsWith("connect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Connect()
                End If
            ElseIf consoleLine.StartsWith("disconnect", True, Nothing) Then
                If TryGetMapper(consoleLine, mapper) Then
                    mapper.Disconnect()
                End If
            ElseIf consoleLine.StartsWith("reload", True, Nothing) Then
                Console.WriteLine()
                For x As Integer = 0 To m_receivers.Length - 1
                    m_receivers(x).Initialize()
                Next
            ElseIf consoleLine.StartsWith("status", True, Nothing) Then
                Console.WriteLine()
                For x As Integer = 0 To m_receivers.Length - 1
                    Console.WriteLine(m_receivers(x).Status)
                Next
            ElseIf consoleLine.StartsWith("list", True, Nothing) Then
                Console.WriteLine()
                DisplayConnectionList()
            ElseIf consoleLine.StartsWith("version", True, Nothing) Then
                Console.WriteLine()
                Console.WriteLine(MonitorInformation)
            ElseIf consoleLine.StartsWith("help", True, Nothing) OrElse consoleLine = "?" Then
                Console.WriteLine()
                DisplayCommandList()
            ElseIf consoleLine.StartsWith("exit", True, Nothing) Then
                Exit Do
            Else
                Console.WriteLine()
                Console.Write("Command unrecognized.  ")
                DisplayCommandList()
            End If
        Loop

        ' Attempt an orderly shutdown...
        For x As Integer = 0 To m_receivers.Length - 1
            m_receivers(x).DisconnectAll()
        Next

        End

    End Sub

    Private Function TryGetMapper(ByVal consoleLine As String, ByRef mapper As PhasorMeasurementMapper) As Boolean

        Try
            Dim pmuID As String = RemoveDuplicateWhiteSpace(consoleLine).Split(" "c)(1).ToUpper()
            Dim foundMapper As Boolean

            For x As Integer = 0 To m_receivers.Length - 1
                If m_receivers(x).Mappers.TryGetValue(pmuID, mapper) Then
                    foundMapper = True
                    Exit For
                End If
            Next

            If Not foundMapper Then Console.WriteLine("Failed to find a PMU or PDC in the connection list named """ & pmuID & """, type ""List"" to see avaialable list.")

            Return foundMapper
        Catch ex As Exception
            Console.WriteLine("Failed to lookup specified mapper due to exception: " & ex.Message)
            Return False
        End Try

    End Function

    Private ReadOnly Property MonitorInformation() As String
        Get
            With New StringBuilder
                .Append(EntryAssembly.Title)
                .Append(Environment.NewLine)
                .Append("   Assembly: ")
                .Append(EntryAssembly.Name)
                .Append(Environment.NewLine)
                .Append("    Version: ")
                .Append(EntryAssembly.Version)
                .Append(Environment.NewLine)
                .Append("   Location: ")
                .Append(EntryAssembly.Location)
                .Append(Environment.NewLine)
                .Append("    Created: ")
                .Append(EntryAssembly.BuildDate)
                .Append(Environment.NewLine)
                .Append("    NT User: ")
                .Append(WindowsIdentity.GetCurrent.Name)
                .Append(Environment.NewLine)
                .Append("    Runtime: ")
                .Append(EntryAssembly.ImageRuntimeVersion)
                .Append(Environment.NewLine)
                .Append("    Started: ")
                .Append(DateTime.Now.ToString())
                .Append(Environment.NewLine)

                Return .ToString()
            End With
        End Get
    End Property

    Private Sub DisplayConnectionList()

        For x As Integer = 0 To m_receivers.Length - 1
            Console.WriteLine("Phasor Measurement Retriever for Archive """ & m_receivers(x).ArchiverName & """")
            Console.WriteLine(">> PMU/PDC Connection List (" & m_receivers(x).Mappers.Count & " Total)")
            Console.WriteLine()

            Console.WriteLine("  Last Data Report Time:   PDC/PMU [PMU list]:")
            Console.WriteLine("  ------------------------ ----------------------------------------------------")
            '                    01-JAN-2006 12:12:24.000 SourceName [Pmu0, Pmu1, Pmu2, Pmu3, Pmu4]
            '                    >> No data frame has been parsed for SourceName - 00000 bytes received"

            For Each mapper As PhasorMeasurementMapper In m_receivers(x).Mappers.Values
                With mapper
                    Console.Write("  ")
                    If .LastReportTime > 0 Then
                        Console.Write((New DateTime(mapper.LastReportTime)).ToString("dd-MMM-yyyy HH:mm:ss.fff"))
                        Console.Write(" "c)
                        Console.WriteLine(mapper.Name)
                    Else
                        Console.WriteLine(">> No data frame has been parsed for " & mapper.Name & " - " & .TotalBytesReceived & " bytes received")
                    End If
                End With
            Next

            Console.WriteLine()
        Next

    End Sub

    Private Sub DisplayCommandList()

        Console.WriteLine("Possible commands:")
        Console.WriteLine()
        Console.WriteLine("    ""Connect PmuID""        - Restarts PMU connection cycle")
        Console.WriteLine("    ""Disconnect PmuID""     - Terminates PMU connection")
        Console.WriteLine("    ""Reload""               - Reloads the entire service process")
        Console.WriteLine("    ""Status""               - Returns current service status")
        Console.WriteLine("    ""List""                 - Displays loaded PMU/PDC connections")
        Console.WriteLine("    ""Version""              - Displays service version information")
        Console.WriteLine("    ""Help""                 - Displays this help information")
        Console.WriteLine("    ""Exit""                 - Exits this console monitor")
        Console.WriteLine()

    End Sub

    ' Display status messages bubbled up from phasor measurement receiver and its internal components
    Private Sub DisplayStatusMessage(ByVal status As String)

        Console.WriteLine(status)
        Console.WriteLine()

    End Sub

End Module
