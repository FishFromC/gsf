'*******************************************************************************************************
'  Tva.Communication.FileClient.vb - File-based communication client
'  Copyright � 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  07/24/2006 - Pinal C. Patel
'       Original version of source code generated
'  09/06/2006 - J. Ritchie Carroll
'       Added bypass optimizations for high-speed file data access
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.ComponentModel
Imports Tva.Common
Imports Tva.IO.Common

''' <summary>
''' Represents a File-based communication client.
''' </summary>
Public Class FileClient

    Private m_receiveOnDemand As Boolean
    Private m_receiveInterval As Double
    Private m_startingOffset As Long
    Private m_fileClient As StateKeeper(Of FileStream)
    Private m_receivingThread As Thread
    Private m_connectionThread As Thread
    Private m_connectionData As Dictionary(Of String, String)
    Private WithEvents m_receiveDataTimer As System.Timers.Timer

    ''' <summary>
    ''' Initializes a instance of Tva.Communication.FileClient with the specified data.
    ''' </summary>
    ''' <param name="connectionString">The data that is required by the client to initialize.</param>
    Public Sub New(ByVal connectionString As String)

        MyClass.New()
        MyBase.ConnectionString = connectionString

    End Sub

    ''' <summary>
    ''' Gets or sets a boolean value indicating whether receiving (reading) of data will be initiated manually 
    ''' by calling ReceiveData().
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if receiving (reading) of data will be initiated manually; otherwise False.</returns>
    <Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData()."), Category("Data"), DefaultValue(GetType(Boolean), "False")> _
    Public Property ReceiveOnDemand() As Boolean
        Get
            Return m_receiveOnDemand
        End Get
        Set(ByVal value As Boolean)
            m_receiveOnDemand = value
            If m_receiveOnDemand Then
                ' We'll disable receiving data at a set interval if user wants to receive data on demand.
                m_receiveInterval = -1
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the time in milliseconds to pause before receiving (reading) the next available set of data.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Time in milliseconds to pause before receiving (reading) the next available set of data.</returns>
    ''' <remarks>Set ReceiveInterval = -1 to receive (read) data continuously without pausing.</remarks>
    <Description("Time in milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing."), Category("Data"), DefaultValue(GetType(Double), "-1")> _
    Public Property ReceiveInterval() As Double
        Get
            Return m_receiveInterval
        End Get
        Set(ByVal value As Double)
            If value = -1 OrElse value > 0 Then
                m_receiveInterval = value
                If m_receiveInterval > 0 Then
                    ' We'll disable the ReceiveOnDemand feature if the user specifies an interval for 
                    ' automatically receiving data.
                    m_receiveOnDemand = False
                End If
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
    ''' </summary>
    ''' <value></value>
    ''' <returns>The starting point relative to the beginning of the file from where the data is to be received (read).</returns>
    <Description("The starting point relative to the beginning of the file from where the data is to be received (read)."), Category("Data"), DefaultValue(GetType(Long), "0")> _
    Public Property StartingOffset() As Long
        Get
            Return m_startingOffset
        End Get
        Set(ByVal value As Long)
            If value >= 0 Then
                m_startingOffset = value
            Else
                Throw New ArgumentOutOfRangeException("value")
            End If
        End Set
    End Property

    ''' <summary>
    ''' Cancels any active attempts of connecting to the file.
    ''' </summary>
    Public Overrides Sub CancelConnect()

        If MyBase.Enabled AndAlso m_connectionThread.IsAlive Then m_connectionThread.Abort()

    End Sub

    ''' <summary>
    ''' Connects to the file asynchronously.
    ''' </summary>
    Public Overrides Sub Connect()

        If MyBase.Enabled AndAlso Not MyBase.IsConnected AndAlso ValidConnectionString(ConnectionString()) Then
            If File.Exists(m_connectionData("file")) Then
                m_connectionThread = New Thread(AddressOf ConnectToFile)
                m_connectionThread.Start()
            Else
                Throw New FileNotFoundException(m_connectionData("file") & " does not exist.")
            End If
        End If

    End Sub

    ''' <summary>
    ''' Disconnects from the file (i.e., closes the file stream).
    ''' </summary>
    Public Overrides Sub Disconnect()

        CancelConnect()

        If MyBase.Enabled AndAlso MyBase.IsConnected Then
            m_receiveDataTimer.Stop()
            m_fileClient.Client.Close()
            OnDisconnected(EventArgs.Empty)
        End If

    End Sub

    ''' <summary>
    ''' Initiates receiving to data from the file.
    ''' </summary>
    ''' <remarks>This method is functional only when ReceiveOnDemand is enabled.</remarks>
    Public Sub ReceiveData()

        If MyBase.Enabled AndAlso MyBase.IsConnected _
                AndAlso m_receiveOnDemand AndAlso Not m_receivingThread.IsAlive Then
            m_receivingThread = New Thread(AddressOf ReceiveFileData)
            m_receivingThread.Start()
        End If

    End Sub

    <EditorBrowsable(EditorBrowsableState.Never)> _
    Protected Overrides Sub SendPreparedData(ByVal data As Byte())

        Throw New NotSupportedException()

    End Sub

    ''' <summary>
    ''' Determines whether specified connection string required for connecting to the file is valid.
    ''' </summary>
    ''' <param name="connectionString">The connection string to be validated.</param>
    ''' <returns>True is the connection string is valid; otherwise False.</returns>
    Protected Overrides Function ValidConnectionString(ByVal connectionString As String) As Boolean

        If Not String.IsNullOrEmpty(connectionString) Then
            m_connectionData = Tva.Text.Common.ParseKeyValuePairs(connectionString)
            If m_connectionData.ContainsKey("file") Then
                Return True
            Else
                ' Connection string is not in the expected format.
                With New StringBuilder()
                    .Append("Connection string must be in the following format:")
                    .Append(Environment.NewLine)
                    .Append("   File=[Name of the file]")
                    Throw New ArgumentException(.ToString())
                End With
            End If
        Else
            Throw New ArgumentNullException()
        End If

    End Function

    ''' <summary>
    ''' Connects to the file.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ConnectToFile()

        Dim connectionAttempts As Integer = 0

        Do While MaximumConnectionAttempts = -1 OrElse connectionAttempts < MaximumConnectionAttempts
            Try
                OnConnecting(EventArgs.Empty)
                m_fileClient.Client = New FileStream(m_connectionData("file"), FileMode.Open)
                m_fileClient.Client.Seek(m_startingOffset, SeekOrigin.Begin)   ' Move to the specified offset.
                OnConnected(EventArgs.Empty)
                If Not m_receiveOnDemand Then
                    If m_receiveInterval > 0 Then
                        ' We need to start receivng data at the specified interval.
                        m_receiveDataTimer.Interval = m_receiveInterval
                        m_receiveDataTimer.Start()
                    Else
                        ' We need to start receiving data continuously.
                        m_receivingThread = New Thread(AddressOf ReceiveFileData)
                        m_receivingThread.Start()
                    End If
                End If

                Exit Do ' We've successfully connected to the file.
            Catch ex As ThreadAbortException
                OnConnectingCancelled(EventArgs.Empty)
                Exit Do ' We must abort connecting to the file.
            Catch ex As Exception
                connectionAttempts += 1
                OnConnectingException(New ExceptionEventArgs(ex, connectionAttempts + 1))
            End Try
        Loop

    End Sub

    ''' <summary>
    ''' Receive data from the file.
    ''' </summary>
    ''' <remarks>This method is meant to be executed on a seperate thread.</remarks>
    Private Sub ReceiveFileData()

        Try
            With m_fileClient
                Dim received As Integer

                ' Process the entire file content
                Do While .Client.Position < .Client.Length
                    ' Retrieve data from the file stream
                    received = .Client.Read(m_buffer, 0, m_buffer.Length)

                    ' Post raw data to real-time function delegate if defined - this bypasses all other activity
                    If m_receiveRawDataFunction IsNot Nothing Then
                        m_receiveRawDataFunction(m_buffer, 0, received)
                        m_totalBytesReceived += received
                    Else
                        ' Unpack data and make available via event
                        OnReceivedData(New IdentifiableItemEventArgs(Of Byte())(ServerID, CopyBuffer(m_buffer, 0, received)))
                    End If

                    ' We must stop processing the file if user has either opted to receive data on 
                    ' demand or receive data at a predefined interval.
                    If m_receiveOnDemand OrElse m_receiveInterval > 0 Then Exit Do
                Loop
            End With
        Catch ex As Exception
            ' Exit gracefully when an exception is encountered while receiving data.
        End Try

    End Sub

    Private Sub m_receiveDataTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_receiveDataTimer.Elapsed

        If MyBase.Enabled AndAlso MyBase.IsConnected AndAlso _
                m_receiveInterval > 0 AndAlso Not m_receivingThread.IsAlive Then
            m_receivingThread = New Thread(AddressOf ReceiveFileData)
            m_receivingThread.Start()
        End If

    End Sub

End Class
