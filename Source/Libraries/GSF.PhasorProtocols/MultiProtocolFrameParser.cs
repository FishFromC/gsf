//******************************************************************************************************
//  MultiProtocolFrameParser.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/16/2006 - J. Ritchie Carroll
//       Initial version of source generated.
//  06/26/2006 - Pinal C. Patel
//       Changed out the socket code with TcpClient and UdpClient components from GSF.Communication.
//  01/31/2007 - J. Ritchie Carroll
//       Added TCP "server" support to allow listening connections from devices that act as data
//       clients, e.g., F-NET devices.
//  05/23/2007 - Pinal C. Patel
//       Added member variable 'm_clientConnectionAttempts' to track the number of attempts made for
//       connecting to the server since this information is no longer provided by the event raised by
//       any of the Communication Client components.
//  07/05/2007 - J. Ritchie Carroll
//       Wrapped all event raising for frame parsing in Try/Catch so that any exceptions thrown in
//       consumer event handlers won't have a negative effect on continuous data parsing - exceptions
//       in consumer event handlers are duly noted and raised through the ParsingException event.
//  09/28/2007 - J. Ritchie Carroll
//       Implemented new disconnect overload on communications client that allows timeout on socket
//       close to fix an issue related non-responsive threads that "lock-up" after sending connection
//       commands that attempt to close the socket for remotely connected devices.
//  12/14/2007 - J. Ritchie Carroll
//       Implemented simulated timestamp injection for published frames to allow for real-time
//       data simulations from archived sample data.
//  10/28/2008 - J. Ritchie Carroll
//       Added support for SEL's UDP_T and UDP_U protocol implementations (UDP_S was already supported),
//       implementation handled by allowing definition of a "CommandChannel" in the connection string.
//  04/27/2009 - J. Ritchie Carroll
//       Added support for SEL Fast Message protocol.
//  02/12/2010 - Pinal C. Patel
//       Modified to start the IFrameParser object in InitializeFrameParser() instead of Start().
//  03/20/2010 - J. Ritchie Carroll
//       Added property "SkipDisableRealTimeData" to allow consumer to bypass sending the command to
//       turn off the real-time data stream when automatically starting the data parsing sequence. This
//       is useful when using UDP multicast that may have many listeners, in these cases you don't want
//       to disable the stream on startup or shutdown since other applications may be subscribed to the
//       real-time stream.
//  03/21/2010 - J. Ritchie Carroll
//       Added parsing exception threshold settings and consumer event to handle situation.
//  06/13/2010 - J. Ritchie Carroll
//       Added several more run-time statistics to the frame parser (e.g., missing frames, CRC errors).
//  08/10/2010 - J. Ritchie Carroll
//       Added code to handle high-resolution input timing to support accurate input simulations.
//  05/06/2010- Jian (Ryan) Zuo
//       Updated to exclude non-data frames from frame counts and injected waiting periods.
//  05/19/2011 - Ritchie
//       Added DST file support.
//
//******************************************************************************************************

// Define this constant to enable a raw data export for debugging - do not leave this on for deployed builds
#undef RawDataCapture

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GSF.TimeSeries;
using GSF.Communication;
using GSF.IO;
using GSF.Parsing;
using GSF.Units;
using GSF;

// Ignore warnings about unused events that are required by IClient
#pragma warning disable 67

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Phasor data protocols enumeration.
    /// </summary>
    [Serializable()]
    public enum PhasorProtocol
    {
        /// <summary>
        /// IEEE C37.118.2-2011 protocol.
        /// </summary>
        IeeeC37_118V2,
        /// <summary>
        /// IEEE C37.118-2005 protocol.
        /// </summary>
        IeeeC37_118V1,
        /// <summary>
        /// IEEE C37.118, draft 6 protocol.
        /// </summary>
        IeeeC37_118D6,
        /// <summary>
        /// IEEE 1344-1995 protocol.
        /// </summary>
        Ieee1344,
        /// <summary>
        /// BPA PDCstream protocol.
        /// </summary>
        BpaPdcStream,
        /// <summary>
        /// Virgina Tech F-NET protocol.
        /// </summary>
        FNet,
        /// <summary>
        /// SEL Fast Message protocol.
        /// </summary>
        SelFastMessage,
        /// <summary>
        /// Macrodyne protocol.
        /// </summary>
        Macrodyne,
        /// <summary>
        /// IEC 61850-90-5 protocol.
        /// </summary>
        Iec61850_90_5
    }

    #endregion

    /// <summary>
    /// Protocol independent frame parser.
    /// </summary>
    /// <remarks>
    /// This class takes all protocol frame parsing implementations and reduces them to a single simple-to-use class exposing all
    /// data through abstract interfaces (e.g., IConfigurationFrame, IDataFrame, etc.) - this way new protocol implementations can
    /// be added without adversely affecting consuming code. Additionally, this class implements a variety of transport options
    /// (e.g., TCP, UDP, Serial, etc.) and hides the complexities of this connectivity and internally pushes all data received from
    /// the selected transport protocol to the selected phasor parsing protocol.
    /// </remarks>
    public sealed class MultiProtocolFrameParser : IFrameParser
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Precision input timer.
        /// </summary>
        /// <remarks>
        /// This class is used to create highly accurate simulated data inputs aligned to the local clock.<br/>
        /// One static instance of this internal class is created per encountered frame rate.
        /// </remarks>
        private sealed class PrecisionInputTimer : IDisposable
        {
            #region [ Members ]

            // Fields
            private PrecisionTimer m_timer;
            private bool m_useWaitHandleA;
            private SpinLock m_timerTickLock;
            private ManualResetEventSlim m_frameWaitHandleA;
            private ManualResetEventSlim m_frameWaitHandleB;
            private readonly int m_framesPerSecond;
            private readonly int m_frameWindowSize;
            private readonly int[] m_frameMilliseconds;
            private int m_lastFrameIndex;
            private long m_lastFrameTime;
            private long m_missedPublicationWindows;
            private long m_lastMissedWindowTime;
            private long m_resynchronizations;
            private int m_referenceCount;
            private Action<Exception> m_exceptionHandler;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Create a new <see cref="PrecisionInputTimer"/> class.
            /// </summary>
            /// <param name="framesPerSecond">Desired frame rate for <see cref="PrecisionTimer"/>.</param>
            public PrecisionInputTimer(int framesPerSecond)
            {
                // Create synchronization objects
                m_timerTickLock = new SpinLock();
                m_frameWaitHandleA = new ManualResetEventSlim(false);
                m_frameWaitHandleB = new ManualResetEventSlim(false);
                m_useWaitHandleA = true;
                m_framesPerSecond = framesPerSecond;

                // Create a new precision timer for this timer state
                m_timer = new PrecisionTimer();
                m_timer.Resolution = 1;
                m_timer.Period = 1;
                m_timer.AutoReset = true;

                // Attach handler for timer ticks
                m_timer.Tick += m_timer_Tick;

                m_frameWindowSize = (int)Math.Round(1000.0D / framesPerSecond) * 2;
                m_frameMilliseconds = new int[framesPerSecond];

                for (int frameIndex = 0; frameIndex < framesPerSecond; frameIndex++)
                {
                    m_frameMilliseconds[frameIndex] = (int)(1.0D / framesPerSecond * (frameIndex * 1000.0D));
                }

                // Start high resolution timer on a separate thread so the start
                // time can synchronized to the top of the millisecond
                ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="PrecisionInputTimer"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~PrecisionInputTimer()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets frames per second for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int FramesPerSecond
            {
                get
                {
                    return m_framesPerSecond;
                }
            }

            /// <summary>
            /// Gets array of frame millisecond times for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int[] FrameMilliseconds
            {
                get
                {
                    return m_frameMilliseconds;
                }
            }

            /// <summary>
            /// Gets reference count for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public int ReferenceCount
            {
                get
                {
                    return m_referenceCount;
                }
            }

            /// <summary>
            /// Gets number of resynchronizations that have occurred for this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public long Resynchronizations
            {
                get
                {
                    return m_resynchronizations;
                }
            }

            /// <summary>
            /// Gets time of last frame, in ticks.
            /// </summary>
            public long LastFrameTime
            {
                get
                {
                    return m_lastFrameTime;
                }
            }

            /// <summary>
            /// Gets a reference to the frame wait handle.
            /// </summary>
            public ManualResetEventSlim FrameWaitHandle
            {
                get
                {
                    if (m_useWaitHandleA)
                        return m_frameWaitHandleA;

                    return m_frameWaitHandleB;
                }
            }

            /// <summary>
            /// Gets or sets the function that handles exceptions
            /// thrown by the precision input timer.
            /// </summary>
            public Action<Exception> ExceptionHandler
            {
                get
                {
                    return m_exceptionHandler;
                }
                set
                {
                    m_exceptionHandler = value;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="PrecisionInputTimer"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="PrecisionInputTimer"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            private void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                        {
                            if ((object)m_timer != null)
                            {
                                m_timer.Tick -= m_timer_Tick;
                                m_timer.Dispose();
                            }
                            m_timer = null;

                            if ((object)m_frameWaitHandleA != null)
                            {
                                m_frameWaitHandleA.Set();
                                m_frameWaitHandleA.Dispose();
                            }
                            m_frameWaitHandleA = null;

                            if ((object)m_frameWaitHandleB != null)
                            {
                                m_frameWaitHandleB.Set();
                                m_frameWaitHandleB.Dispose();
                            }
                            m_frameWaitHandleB = null;
                        }
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            /// <summary>
            /// Adds a reference to this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public void AddReference()
            {
                m_referenceCount++;
            }

            /// <summary>
            /// Removes a reference to this <see cref="PrecisionInputTimer"/>.
            /// </summary>
            public void RemoveReference()
            {
                m_referenceCount--;
            }

            // This timer function is called every millisecond so that frames can be published at the exact desired time 
            void m_timer_Tick(object sender, EventArgs e)
            {
                // Slower systems or systems under stress may have trouble keeping up with a 1-ms timer, so
                // we only process this code if it's not already processing...
                bool locked = false;

                try
                {
                    m_timerTickLock.TryEnter(2, ref locked);

                    if (locked)
                    {
                        DateTime now = PrecisionTimer.UtcNow;
                        int frameMilliseconds, milliseconds = now.Millisecond;
                        long ticks = now.Ticks;
                        bool releaseTimer = false, resync = false;

                        // Make sure current time is reasonably close to current frame index
                        if (Math.Abs(milliseconds - m_frameMilliseconds[m_lastFrameIndex]) > m_frameWindowSize)
                            m_lastFrameIndex = 0;

                        // See if it is time to publish
                        for (int frameIndex = m_lastFrameIndex; frameIndex < m_frameMilliseconds.Length; frameIndex++)
                        {
                            frameMilliseconds = m_frameMilliseconds[frameIndex];

                            if (frameMilliseconds == milliseconds)
                            {
                                // See if system skipped a publication window
                                if (m_lastFrameIndex != frameIndex)
                                {
                                    // We monitor for missed windows in quick succession (within 1.5 seconds)
                                    if (ticks - m_lastMissedWindowTime > 15000000L)
                                    {
                                        // Threshold has passed since last missed window, so we reset counters
                                        m_lastMissedWindowTime = ticks;
                                        m_missedPublicationWindows = 0;
                                    }

                                    m_missedPublicationWindows++;

                                    // If the system is starting to skip publications it could need resynchronization,
                                    // so in this case we restart the high-resolution timer to get the timer started
                                    // closer to the top of the millisecond
                                    resync = (m_missedPublicationWindows > 4);
                                }

                                // Prepare index for next check, time moving forward
                                m_lastFrameIndex = frameIndex + 1;

                                if (m_lastFrameIndex >= m_frameMilliseconds.Length)
                                    m_lastFrameIndex = 0;

                                if (resync)
                                {
                                    if ((object)m_timer != null)
                                    {
                                        m_timer.Stop();
                                        ThreadPool.QueueUserWorkItem(SynchronizeInputTimer);
                                        m_resynchronizations++;
                                    }
                                }

                                releaseTimer = true;
                                break;
                            }
                            else if (frameMilliseconds > milliseconds)
                            {
                                // Time has yet to pass, wait till the next tick
                                break;
                            }
                        }

                        if (releaseTimer)
                        {
                            // Baseline timestamp to the top of the millisecond for frame publication
                            m_lastFrameTime = ticks - ticks % Ticks.PerMillisecond;

                            // Pulse all waiting threads toggling between ready handles
                            if (m_useWaitHandleA)
                            {
                                m_frameWaitHandleB.Reset();
                                m_useWaitHandleA = false;
                                m_frameWaitHandleA.Set();
                            }
                            else
                            {
                                m_frameWaitHandleA.Reset();
                                m_useWaitHandleA = true;
                                m_frameWaitHandleB.Set();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Process exception for logging
                    m_exceptionHandler(new InvalidOperationException("Exception thrown by precision input timer: " + ex.Message, ex));
                }
                finally
                {
                    if (locked)
                        m_timerTickLock.Exit(true);
                }
            }

            private void SynchronizeInputTimer(object state)
            {
                // Start timer at as close to the top of the millisecond as possible 
                bool repeat = true;
                long last = 0, next;

                while (repeat)
                {
                    next = PrecisionTimer.UtcNow.Ticks % Ticks.PerMillisecond % 1000;
                    repeat = (next > last);
                    last = next;
                }

                m_lastMissedWindowTime = 0;
                m_missedPublicationWindows = 0;

                if ((object)m_timer != null)
                    m_timer.Start();
            }

            #endregion
        }

        /// <summary>
        /// Shared UDP client reference.
        /// </summary>
        /// <remarks>
        /// This class is used to create multiple IClient instances which share a single UDP client.<br/>
        /// One shared UDP client instance will be created per local end point.
        /// </remarks>
        private sealed class SharedUdpClientReference : IClient
        {
            #region [ Members ]

            // Events

            public event EventHandler ConnectionAttempt;
            public event EventHandler ConnectionEstablished;
            public event EventHandler ConnectionTerminated;
            public event EventHandler<EventArgs<Exception>> ConnectionException;

            public event EventHandler SendDataStart;
            public event EventHandler SendDataComplete;
            public event EventHandler<EventArgs<Exception>> SendDataException;

            public event EventHandler<EventArgs<int>> ReceiveData;
            public event EventHandler<EventArgs<byte[], int>> ReceiveDataComplete;
            public event EventHandler<EventArgs<EndPoint, int>> ReceiveDataFrom;
            public event EventHandler<EventArgs<Exception>> ReceiveDataException;

            public event EventHandler<EventArgs<Exception>> UnhandledUserException;
            public event EventHandler Disposed;

            // Fields
            private UdpClient m_udpClient;
            private EndPoint m_sendDestination;
            private string m_connectionString;
            private int m_receiveBufferSize;
            private int m_maxConnectionAttempts;

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the data required by the client to connect to the server.
            /// </summary>
            public string ConnectionString
            {
                get
                {
                    return m_connectionString;
                }
                set
                {
                    m_connectionString = value;
                }
            }

            /// <summary>
            /// Gets the <see cref="Time"/> for which the client has been connected to the server.
            /// </summary>
            public Time ConnectionTime
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.ConnectionTime;

                    return 0.0D;
                }
            }

            /// <summary>
            /// Gets the current <see cref="ClientState"/>.
            /// </summary>
            public ClientState CurrentState
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.CurrentState;

                    return ClientState.Disconnected;
                }
            }

            /// <summary>
            /// Gets or sets the maximum number of times the client will attempt to connect to the server.
            /// </summary>
            /// <remarks>Set <see cref="MaxConnectionAttempts"/> to -1 for infinite connection attempts.</remarks>
            public int MaxConnectionAttempts
            {
                get
                {
                    return m_maxConnectionAttempts;
                }
                set
                {
                    m_maxConnectionAttempts = value;
                }
            }

            /// <summary>
            /// Gets or sets the size of the buffer used by the client for receiving data from the server.
            /// </summary>
            /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
            public int ReceiveBufferSize
            {
                get
                {
                    return m_receiveBufferSize;
                }
                set
                {
                    m_receiveBufferSize = value;
                }
            }

            /// <summary>
            /// Gets or sets the size of the buffer used by the client for sending data to the server.
            /// </summary>
            /// <exception cref="ArgumentException">The value being assigned is either zero or negative.</exception>
            public int SendBufferSize
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.SendBufferSize;

                    return 0;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Gets the server URI.
            /// </summary>
            public string ServerUri
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.ServerUri;

                    return null;
                }
            }

            /// <summary>
            /// Gets or sets the <see cref="Encoding"/> to be used for the text sent to the server.
            /// </summary>
            public Encoding TextEncoding
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.TextEncoding;

                    return null;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            /// <summary>
            /// Gets the <see cref="TransportProtocol"/> used by the client for the transportation of data with the server.
            /// </summary>
            public TransportProtocol TransportProtocol
            {
                get
                {
                    return TransportProtocol.Udp;
                }
            }

            /// <summary>
            /// Gets or sets a boolean value that indicates whether the client is currently enabled.
            /// </summary>
            /// <remarks>
            /// Setting <see cref="Enabled"/> to true will start connection cycle for the client if it
            /// is not connected, setting to false will disconnect the client if it is connected.
            /// </remarks>
            public bool Enabled
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.Enabled;

                    return false;
                }
                set
                {
                    if ((object)m_udpClient != null)
                        m_udpClient.Enabled = value;
                }
            }

            /// <summary>
            /// Gets the unique identifier of the client.
            /// </summary>
            public string Name
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.Name;

                    return null;
                }
            }

            /// <summary>
            /// Gets the descriptive status of the client.
            /// </summary>
            public string Status
            {
                get
                {
                    if ((object)m_udpClient != null)
                        return m_udpClient.Status;

                    return string.Empty;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Connects the client to the server synchronously.
            /// </summary>
            public void Connect()
            {
                ConnectAsync();
            }

            /// <summary>
            /// Connects the client to the server asynchronously.
            /// </summary>
            /// <exception cref="FormatException">Server property in <see cref="ConnectionString"/> is invalid.</exception>
            /// <exception cref="InvalidOperationException">Attempt is made to connect the client when it is not disconnected.</exception>
            /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
            /// <remarks>
            /// Derived classes are expected to override this method with protocol specific connection operations. Call the base class
            /// method to obtain an operational wait handle if protocol connection operation doesn't provide one already.
            /// </remarks>
            public WaitHandle ConnectAsync()
            {
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                Match endPointMatch;
                IPStack ipStack;
                string serverSetting;
                string remotePortSetting;
                int remotePort;

                // Set up destination used for send operations
                if (settings.TryGetValue("server", out serverSetting))
                {
                    if (settings.TryGetValue("remoteport", out remotePortSetting))
                        serverSetting = string.Format("{0}:{1}", serverSetting, remotePortSetting);

                    endPointMatch = Regex.Match(serverSetting, Transport.EndpointFormatRegex);

                    if (int.TryParse(endPointMatch.Groups["port"].Value, out remotePort))
                    {
                        ipStack = Transport.GetInterfaceIPStack(settings);
                        m_sendDestination = Transport.CreateEndPoint(endPointMatch.Groups["host"].Value, remotePort, ipStack);
                    }
                }

                m_udpClient = GetSharedClient();
                return null;
            }

            /// <summary>
            /// When overridden in a derived class, disconnects client from the server synchronously.
            /// </summary>
            public void Disconnect()
            {
                ReturnSharedClient();
            }

            /// <summary>
            /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
            /// </summary>
            /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
            /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
            /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
            /// <returns>The number of bytes read.</returns>
            /// <remarks>
            /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside
            /// this event will have unexpected results.
            /// </remarks>
            /// <exception cref="InvalidOperationException">No received data buffer has been defined to read.</exception>
            /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">
            /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
            /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
            /// </exception>
            public int Read(byte[] buffer, int startIndex, int length)
            {
                if ((object)m_udpClient != null)
                    return m_udpClient.Read(buffer, startIndex, length);

                return 0;
            }

            /// <summary>
            /// Sends data to the server synchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            public void Send(byte[] data, int offset, int length)
            {
                SendAsync(data, offset, length).WaitOne();
            }

            /// <summary>
            /// Sends data to the server asynchronously.
            /// </summary>
            /// <param name="data">The buffer that contains the binary data to be sent.</param>
            /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending data.</param>
            /// <param name="length">The number of bytes to be sent from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
            /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
            public WaitHandle SendAsync(byte[] data, int offset, int length)
            {
                if ((object)m_sendDestination != null)
                    return m_udpClient.SendDataToAsync(data, offset, length, m_sendDestination);

                return new ManualResetEvent(true);
            }

            /// <summary>
            /// Initializes the client.
            /// </summary>
            /// <remarks>
            /// <see cref="Initialize()"/> is to be called by user-code directly only if the client is not consumed through the designer surface of the IDE.
            /// </remarks>
            public void Initialize()
            {
            }

            /// <summary>
            /// Releases the unmanaged resources used by the client and optionally releases the managed resources.
            /// </summary>
            public void Dispose()
            {
                ReturnSharedClient();
            }

            /// <summary>
            /// Gets a reference to the shared client listening
            /// on this client's local end point.
            /// </summary>
            /// <returns>A reference to a shared client.</returns>
            private UdpClient GetSharedClient()
            {
                const string ConfigurationMismatchError = "Configuration mismatch detected between parsers using shared UDP client: {0}";

                bool sharing;
                EndPoint localEndPoint;
                UdpClient sharedClient;

                lock (s_sharedClients)
                {
                    localEndPoint = GetLocalEndPoint();
                    sharing = s_sharedClients.TryGetValue(localEndPoint, out sharedClient);

                    if (sharing)
                    {
                        // Validate settings to ensure that they match
                        if (sharedClient.ReceiveBufferSize != m_receiveBufferSize)
                            throw new InvalidOperationException(string.Format(ConfigurationMismatchError, "Receive buffer size"));

                        if (sharedClient.MaxConnectionAttempts != m_maxConnectionAttempts)
                            throw new InvalidOperationException(string.Format(ConfigurationMismatchError, "Max connection attempts"));
                    }
                    else
                    {
                        // Create new client and add it to the shared list
                        sharedClient = new UdpClient();
                        s_sharedClients.Add(localEndPoint, sharedClient);
                        s_sharedReferenceCount.Add(localEndPoint, 0);
                    }

                    // Attach to event handlers
                    sharedClient.ConnectionAttempt += SharedClient_ConnectionAttempt;
                    sharedClient.ConnectionEstablished += SharedClient_ConnectionEstablished;
                    sharedClient.ConnectionException += SharedClient_ConnectionException;
                    sharedClient.ConnectionTerminated += SharedClient_ConnectionTerminated;
                    sharedClient.ReceiveDataException += SharedClient_ReceiveDataException;
                    sharedClient.ReceiveDataFrom += SharedClient_ReceiveDataFrom;
                    sharedClient.SendDataException += SharedClient_SendDataException;

                    if (!sharing)
                    {
                        // Initialize settings and connect
                        sharedClient.ConnectionString = m_connectionString;
                        sharedClient.ReceiveBufferSize = m_receiveBufferSize;
                        sharedClient.MaxConnectionAttempts = m_maxConnectionAttempts;
                        sharedClient.ConnectAsync();
                    }

                    // Increment reference count
                    s_sharedReferenceCount[localEndPoint]++;
                }

                if (sharing && sharedClient.CurrentState == ClientState.Connected)
                    OnConnectionEstablished();

                return sharedClient;
            }

            /// <summary>
            /// Releases a reference to this client's shared client,
            /// and disposes of the shared client if nobody is using it.
            /// </summary>
            private void ReturnSharedClient()
            {
                EndPoint localEndPoint;

                lock (s_sharedClients)
                {
                    if ((object)m_udpClient == null)
                        return;

                    // Detach from event handlers
                    m_udpClient.ConnectionAttempt -= SharedClient_ConnectionAttempt;
                    m_udpClient.ConnectionEstablished -= SharedClient_ConnectionEstablished;
                    m_udpClient.ConnectionException -= SharedClient_ConnectionException;
                    m_udpClient.ConnectionTerminated -= SharedClient_ConnectionTerminated;
                    m_udpClient.ReceiveDataException -= SharedClient_ReceiveDataException;
                    m_udpClient.ReceiveDataFrom -= SharedClient_ReceiveDataFrom;
                    m_udpClient.SendDataException -= SharedClient_SendDataException;

                    // Decrement reference count
                    localEndPoint = GetLocalEndPoint();
                    s_sharedReferenceCount[localEndPoint]--;

                    if (s_sharedReferenceCount[localEndPoint] == 0)
                    {
                        // No more references to UDP client
                        // exist so dispose of it
                        m_udpClient.Disconnect();
                        m_udpClient.Dispose();
                        s_sharedClients.Remove(localEndPoint);
                        s_sharedReferenceCount.Remove(localEndPoint);
                    }

                    // Release reference to UDP client
                    m_udpClient = null;
                }
            }

            /// <summary>
            /// Terminates the client as quickly as possible and
            /// removes it from the collection of shared clients.
            /// </summary>
            private void TerminateSharedClient()
            {
                EndPoint localEndPoint;
                UdpClient sharedClient;

                lock (s_sharedClients)
                {
                    localEndPoint = GetLocalEndPoint();

                    if (s_sharedClients.TryGetValue(localEndPoint, out sharedClient))
                    {
                        // If the wrapped client and the shared client are the same,
                        // no one has terminated the shared client yet
                        if (m_udpClient == sharedClient)
                        {
                            // Disconnect and dispose of the old client
                            m_udpClient.Disconnect();
                            m_udpClient.Dispose();

                            // Remove the old client from the
                            // collection of shared clients
                            localEndPoint = GetLocalEndPoint();
                            s_sharedClients.Remove(localEndPoint);
                            s_sharedReferenceCount.Remove(localEndPoint);
                        }
                    }

                    m_udpClient = null;
                }
            }

            /// <summary>
            /// Determines the local end point this client intends
            /// to listen on via connection string properties.
            /// </summary>
            /// <returns>The local end point.</returns>
            private EndPoint GetLocalEndPoint()
            {
                Dictionary<string, string> settings;
                IPStack ipStack;
                string localInterface;
                string localPortSetting;
                int localPort;

                settings = m_connectionString.ParseKeyValuePairs();
                ipStack = Transport.GetInterfaceIPStack(settings);

                if (!settings.TryGetValue("interface", out localInterface))
                    localInterface = string.Empty;

                if (!settings.TryGetValue("localport", out localPortSetting) && !settings.TryGetValue("port", out localPortSetting))
                    throw new InvalidOperationException(string.Format("Local port property missing from connection string: {0}", m_connectionString));

                if (!int.TryParse(localPortSetting, out localPort))
                    throw new InvalidOperationException(string.Format("Unable to parse local port from \"{0}\".", localPortSetting));

                return Transport.CreateEndPoint(localInterface, localPort, ipStack);
            }

            // Triggers the ConnectionEstablished event.
            private void OnConnectionEstablished()
            {
                if ((object)ConnectionEstablished != null)
                    ConnectionEstablished(this, new EventArgs());
            }

            // Shared client connection attempt handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionAttempt(object sender, EventArgs e)
            {
                if ((object)ConnectionAttempt != null)
                    ConnectionAttempt(this, e);
            }

            // Shared client connection established handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionEstablished(object sender, EventArgs e)
            {
                OnConnectionEstablished();
            }

            // Shared client connection exception handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                if ((object)ConnectionException != null)
                    ConnectionException(this, e);
            }

            // Shared client connection terminated handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ConnectionTerminated(object sender, EventArgs e)
            {
                if ((object)ConnectionTerminated != null)
                    ConnectionTerminated(this, e);
            }

            // Shared client receive data exception handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ReceiveDataException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                if ((object)ReceiveDataException != null)
                    ReceiveDataException(this, e);
            }

            // Shared client receive data from handler.
            // Forwards event to users attached to this client.
            private void SharedClient_ReceiveDataFrom(object sender, EventArgs<EndPoint, int> e)
            {
                if ((object)ReceiveDataFrom != null)
                    ReceiveDataFrom(this, e);
            }

            // Shared client send data handler.
            // Forwards event to users attached to this client.
            private void SharedClient_SendDataException(object sender, EventArgs<Exception> e)
            {
                // Terminate before propagating the exception to
                // ensure that subsequent calls to ReturnSharedClient
                // and GetSharedClient will work properly
                TerminateSharedClient();

                if ((object)SendDataException != null)
                    SendDataException(this, e);
            }

            #endregion

            #region [ Static ]

            // Static Fields
            private static readonly Dictionary<EndPoint, UdpClient> s_sharedClients = new Dictionary<EndPoint, UdpClient>();
            private static readonly Dictionary<EndPoint, int> s_sharedReferenceCount = new Dictionary<EndPoint, int>();

            #endregion
        }

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="BufferSize"/> property.
        /// </summary>
        public const int DefaultBufferSize = 262144; // 256K

        /// <summary>
        /// Specifies the default value for the <see cref="DefinedFrameRate"/> property.
        /// </summary>
        public const int DefaultDefinedFrameRate = 30;

        /// <summary>
        /// Specifies the default value for the <see cref="MaximumConnectionAttempts"/> property.
        /// </summary>
        public const int DefaultMaximumConnectionAttempts = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="AutoStartDataParsingSequence"/> property.
        /// </summary>
        public const bool DefaultAutoStartDataParsingSequence = true;

        /// <summary>
        /// Specfies the default value for the <see cref="AllowedParsingExceptions"/> property.
        /// </summary>
        public const int DefaultAllowedParsingExceptions = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="ParsingExceptionWindow"/> property.
        /// </summary>
        public const long DefaultParsingExceptionWindow = 50000000L; // 5 seconds

        // Events

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="ICommandFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> ReceivedCommandFrame;

        /// <summary>
        /// Occurs when a <see cref="IConfigurationFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IConfigurationFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IConfigurationFrame>> ReceivedConfigurationFrame;

        /// <summary>
        /// Occurs when a <see cref="IDataFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IDataFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IDataFrame>> ReceivedDataFrame;

        /// <summary>
        /// Occurs when a <see cref="IHeaderFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="IHeaderFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IHeaderFrame>> ReceivedHeaderFrame;

        /// <summary>
        /// Occurs when an undetermined <see cref="IChannelFrame"/> has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the undetermined <see cref="IChannelFrame"/> that was received.
        /// </remarks>
        public event EventHandler<EventArgs<IChannelFrame>> ReceivedUndeterminedFrame;

        /// <summary>
        /// Occurs when a frame image has been received.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the length of the frame image that was received.
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, int>> ReceivedFrameImage;

        /// <summary>
        /// Occurs when a frame buffer image has been received.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument1"/> is the <see cref="FundamentalFrameType"/> of the frame buffer image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument2"/> is the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument3"/> is the offset into the buffer that contains the frame image that was received.<br/>
        /// <see cref="EventArgs{T1,T2,T3,T4}.Argument4"/> is the length of data in the buffer that contains the frame image that was received.
        /// </para>
        /// <para>
        /// Consumers should use the more efficient <see cref="ReceivedFrameImage"/> event if the buffer is not needed.
        /// </para>
        /// </remarks>
        public event EventHandler<EventArgs<FundamentalFrameType, byte[], int, int>> ReceivedFrameBufferImage;

        /// <summary>
        /// Occurs when a device sends a notification that its configuration has changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while parsing the data stream.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is the <see cref="Exception"/> encountered while parsing the data stream.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>> ParsingException;

        /// <summary>
        /// Occurs when buffer parsing has completed.
        /// </summary>
        public event EventHandler BufferParsed;

        /// <summary>
        /// Occurs when number of parsing exceptions exceed <see cref="AllowedParsingExceptions"/> during <see cref="ParsingExceptionWindow"/>.
        /// </summary>
        public event EventHandler ExceededParsingExceptionThreshold;

        /// <summary>
        /// Occurs when a <see cref="ICommandFrame"/> is sent to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is a reference to the <see cref="ICommandFrame"/> that was sent to the device.
        /// </remarks>
        public event EventHandler<EventArgs<ICommandFrame>> SentCommandFrame;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered during connection attempt to a device.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the exception that occured during the connection attempt.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is the number of connections attempted so far.
        /// </remarks>
        public event EventHandler<EventArgs<Exception, int>> ConnectionException;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> is attempting connection to a device.
        /// </summary>
        public event EventHandler ConnectionAttempt;

        /// <summary>
        /// Occurs when <see cref="MultiProtocolFrameParser"/> has established a connection to a device.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when device connection has been terminated.
        /// </summary>
        public event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been started.
        /// </summary>
        public event EventHandler ServerStarted;

        /// <summary>
        /// Occurs when the <see cref="MultiProtocolFrameParser"/> is setup as a listening connection and server connection has been stopped.
        /// </summary>
        public event EventHandler ServerStopped;

        // Fields
        private PhasorProtocol m_phasorProtocol;
        private TransportProtocol m_transportProtocol;
        private string m_connectionString;
        private int m_maximumConnectionAttempts;
        private ushort m_deviceID;
        private int m_bufferSize;
        private IFrameParser m_frameParser;
        private IClient m_dataChannel;
        private IServer m_serverBasedDataChannel;
        private IClient m_commandChannel;
        private IPAddress m_receiveFromAddress;
        private PrecisionInputTimer m_inputTimer;
        private System.Timers.Timer m_rateCalcTimer;
        private IConfigurationFrame m_configurationFrame;
        private long m_dataStreamStartTime;
        private bool m_keepCommandChannelOpen;
        private bool m_autoRepeatCapturedPlayback;
        private bool m_injectSimulatedTimestamp;
        private long m_totalFramesReceived;
        private long m_totalMissingFrames;
        private long m_missingFramesOverflow;
        private long m_totalCrcExceptions;
        private long m_totalBytesReceived;
        private double m_calculatedFrameRate;
        private double m_calculatedByteRate;
        private long m_lastFrameReceivedTime;
        private volatile int m_frameRateTotal;
        private volatile int m_byteRateTotal;
        private volatile int m_parsingExceptionCount;
        private long m_lastParsingExceptionTime;
        private int m_configuredFrameRate;
        private string m_sourceName;
        private int m_definedFrameRate;
        private double m_ticksPerFrame;
        private bool m_attachedToInputTimer;
        private bool m_autoStartDataParsingSequence;
        private bool m_skipDisableRealTimeData;
        private bool m_initiatingDataStream;
        private long m_initialBytesReceived;
        private bool m_initiatingSerialConnection;
        private bool m_deviceSupportsCommands;
        private int m_allowedParsingExceptions;
        private Ticks m_parsingExceptionWindow;
        private IConnectionParameters m_connectionParameters;
        private ManualResetEventSlim m_streamStopDataHandle;
        private int m_connectionAttempts;
        private bool m_enabled;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MultiProtocolFrameParser"/> using the default settings.
        /// </summary>
        public MultiProtocolFrameParser()
        {
            m_connectionString = "server=127.0.0.1:4712";
            m_deviceID = 1;
            m_bufferSize = DefaultBufferSize;
            m_maximumConnectionAttempts = DefaultMaximumConnectionAttempts;
            m_autoStartDataParsingSequence = DefaultAutoStartDataParsingSequence;
            m_allowedParsingExceptions = DefaultAllowedParsingExceptions;
            m_parsingExceptionWindow = DefaultParsingExceptionWindow;
            m_keepCommandChannelOpen = true;
            m_rateCalcTimer = new System.Timers.Timer();
            m_streamStopDataHandle = new ManualResetEventSlim(false);

            m_phasorProtocol = PhasorProtocol.IeeeC37_118V1;
            m_transportProtocol = TransportProtocol.Tcp;

            // Set default frame rate, this calculates milliseconds for each frame
            this.DefinedFrameRate = DefaultDefinedFrameRate;

            m_rateCalcTimer.Elapsed += m_rateCalcTimer_Elapsed;
            m_rateCalcTimer.Interval = 5000;
            m_rateCalcTimer.AutoReset = true;
            m_rateCalcTimer.Enabled = false;

            // Set minimum timer resolution to one millisecond to improve timer accuracy
            PrecisionTimer.SetMinimumTimerResolution(1);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="MultiProtocolFrameParser"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~MultiProtocolFrameParser()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.PhasorProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public PhasorProtocol PhasorProtocol
        {
            get
            {
                return m_phasorProtocol;
            }
            set
            {
                m_phasorProtocol = value;
                m_deviceSupportsCommands = DeriveCommandSupport();

                // Setup protocol specific connection parameters, for those protocols that have them...
                switch (value)
                {
                    case GSF.PhasorProtocols.PhasorProtocol.BpaPdcStream:
                        m_connectionParameters = new BpaPdcStream.ConnectionParameters();
                        break;
                    case GSF.PhasorProtocols.PhasorProtocol.FNet:
                        m_connectionParameters = new FNet.ConnectionParameters();
                        break;
                    case GSF.PhasorProtocols.PhasorProtocol.SelFastMessage:
                        m_connectionParameters = new SelFastMessage.ConnectionParameters();
                        break;
                    case GSF.PhasorProtocols.PhasorProtocol.Iec61850_90_5:
                        m_connectionParameters = new Iec61850_90_5.ConnectionParameters();
                        break;
                    case GSF.PhasorProtocols.PhasorProtocol.Macrodyne:
                        m_connectionParameters = new Macrodyne.ConnectionParameters();
                        break;
                    default:
                        m_connectionParameters = null;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets <see cref="TransportProtocol"/> to use with this <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public TransportProtocol TransportProtocol
        {
            get
            {
                return m_transportProtocol;
            }
            set
            {
                m_transportProtocol = value;
                m_deviceSupportsCommands = DeriveCommandSupport();

                // File based input connections are handled more carefully
                if (m_transportProtocol == TransportProtocol.File)
                {
                    if (m_maximumConnectionAttempts < 1)
                        m_maximumConnectionAttempts = 1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the key/value pair based connection information required by the <see cref="MultiProtocolFrameParser"/> to connect to a device.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;

                // Parse connection string to see if a phasor or transport protocol was assigned
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                string setting;

                if (settings.TryGetValue("phasorProtocol", out setting))
                    PhasorProtocol = (PhasorProtocol)Enum.Parse(typeof(PhasorProtocol), setting, true);

                if (settings.TryGetValue("transportProtocol", out setting) || settings.TryGetValue("protocol", out setting))
                    TransportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), setting, true);

                if (settings.TryGetValue("keepCommandChannelOpen", out setting))
                    m_keepCommandChannelOpen = setting.ParseBoolean();

                m_deviceSupportsCommands = DeriveCommandSupport();
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether to keep the
        /// command channel open after the initial startup sequence.
        /// </summary>
        public bool KeepCommandChannelOpen
        {
            get
            {
                return m_keepCommandChannelOpen;
            }
            set
            {
                m_keepCommandChannelOpen = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if a device supports commands.
        /// </summary>
        /// <remarks>
        /// This property is automatically derived based on the selected <see cref="PhasorProtocol"/>, <see cref="TransportProtocol"/>
        /// and <see cref="ConnectionString"/>, but can be overriden if the consumer already knows that a device supports commands.
        /// </remarks>
        public bool DeviceSupportsCommands
        {
            get
            {
                return m_deviceSupportsCommands;
            }
            set
            {
                // Consumers can choose to override command support if needed
                m_deviceSupportsCommands = value;
            }
        }

        /// <summary>
        /// Gets or sets the device identification code often needed to establish a connection.
        /// </summary>
        /// <remarks>
        /// Most devices validate this ID when sending commands, so it must be correct in order to start parsing sequence.
        /// </remarks>
        public ushort DeviceID
        {
            get
            {
                return m_deviceID;
            }
            set
            {
                m_deviceID = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer used by the <see cref="MultiProtocolFrameParser"/> for sending and receiving data from a device.
        /// </summary>
        /// <exception cref="ArgumentException">The value specified is either zero or negative.</exception>
        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value cannot be zero or negative.");

                m_bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of times the <see cref="MultiProtocolFrameParser"/> will attempt to connect to a device.
        /// </summary>
        /// <remarks>Set to -1 for infinite connection attempts.</remarks>
        public int MaximumConnectionAttempts
        {
            get
            {
                return m_maximumConnectionAttempts;
            }
            set
            {
                m_maximumConnectionAttempts = value;

                // All values below zero are assumed to mean infinite connection attempts
                if (m_maximumConnectionAttempts < 1)
                    m_maximumConnectionAttempts = -1;

                // We don't allow maximum connection attempts set to infinite if using file based source since file based
                // connection errors are like "file not found", "invalid path", etc. These connection exceptions are returned
                // so quickly that they will queue up much faster than they will be reported.
                if (m_maximumConnectionAttempts < 1 && m_transportProtocol == TransportProtocol.File)
                    m_maximumConnectionAttempts = 1;
            }
        }

        /// <summary>
        /// Gets or sets flag to automatically send the ConfigFrame2 and EnableRealTimeData command frames used to start a typical data parsing sequence.
        /// </summary>
        /// <remarks>
        /// For devices that support IEEE commands, setting this property to true will automatically start the data parsing sequence.
        /// </remarks>
        public bool AutoStartDataParsingSequence
        {
            get
            {
                return m_autoStartDataParsingSequence;
            }
            set
            {
                m_autoStartDataParsingSequence = value;
            }
        }

        /// <summary>
        /// Gets or sets flag to skip automatic disabling of the real-time data stream on shutdown or startup.
        /// </summary>
        /// <remarks>
        /// This flag may important when using UDP multicast with several subscribed clients.
        /// </remarks>
        public bool SkipDisableRealTimeData
        {
            get
            {
                return m_skipDisableRealTimeData;
            }
            set
            {
                m_skipDisableRealTimeData = value;
            }
        }

        /// <summary>
        /// Gets or sets number of parsing exceptions allowed during <see cref="ParsingExceptionWindow"/> before connection is reset.
        /// </summary>
        public int AllowedParsingExceptions
        {
            get
            {
                return m_allowedParsingExceptions;
            }
            set
            {
                m_allowedParsingExceptions = value;
            }
        }

        /// <summary>
        /// Gets or sets time duration, in <see cref="Ticks"/>, to monitor parsing exceptions.
        /// </summary>
        public Ticks ParsingExceptionWindow
        {
            get
            {
                return m_parsingExceptionWindow;
            }
            set
            {
                m_parsingExceptionWindow = value;
            }
        }

        /// <summary>
        /// Gets or sets a descriptive name for a device connection.
        /// </summary>
        public string SourceName
        {
            get
            {
                return m_sourceName;
            }
            set
            {
                m_sourceName = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if a high-resolution precision timer should be used for file based input.
        /// </summary>
        /// <remarks>
        /// Useful when input frames need be accurately time-aligned to the local clock to better simulate
        /// an input device and calculate downstream latencies.<br/>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public bool UseHighResolutionInputTimer
        {
            get
            {
                return ((object)m_inputTimer != null);
            }
            set
            {
                // Note that a 1-ms timer and debug mode don't mix, so the high-resolution timer is disabled while debugging
                if (value && (object)m_inputTimer == null && !System.Diagnostics.Debugger.IsAttached)
                    m_inputTimer = AttachToInputTimer(m_definedFrameRate);
                else if (!value && (object)m_inputTimer != null)
                    DetachFromInputTimer(ref m_inputTimer);
            }
        }

        /// <summary>
        /// Gets or sets desired frame rate to use for maintaining captured frame replay timing.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public int DefinedFrameRate
        {
            get
            {
                return m_definedFrameRate;
            }
            set
            {
                if (m_definedFrameRate != value)
                {
                    bool timerActive = UseHighResolutionInputTimer;

                    // Deactivate timer before changing defined frame rate
                    if (timerActive)
                        UseHighResolutionInputTimer = false;

                    m_definedFrameRate = value;
                    m_ticksPerFrame = Ticks.PerSecond / (double)m_definedFrameRate;

                    // Reactivate timer if it was active
                    if (timerActive)
                        UseHighResolutionInputTimer = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets flag indicating whether or not to inject local system time into parsed data frames.
        /// </summary>
        /// <remarks>
        /// When connection is made to a file for replay purposes or consumer doesn't trust remote clock source, this flag
        /// can be set to true replace all frame timestamps with a UTC timestamp dervied from the local system clock.
        /// </remarks>
        public bool InjectSimulatedTimestamp
        {
            get
            {
                return m_injectSimulatedTimestamp;
            }
            set
            {
                m_injectSimulatedTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets a flag that determines if a file used for replaying data should be restarted at the beginning once it has been completed.
        /// </summary>
        /// <remarks>
        /// This is only applicable when connection is made to a file for replay purposes.
        /// </remarks>
        public bool AutoRepeatCapturedPlayback
        {
            get
            {
                return m_autoRepeatCapturedPlayback;
            }
            set
            {
                m_autoRepeatCapturedPlayback = value;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="IConfigurationFrame"/> used for parsing <see cref="IDataFrame"/>'s encountered in the data stream from a device.
        /// </summary>
        /// <remarks>
        /// If a <see cref="IConfigurationFrame"/> has been parsed, this will return a reference to the parsed frame.  Consumer can manually assign a
        /// <see cref="IConfigurationFrame"/> to start parsing data if one has not been encountered in the stream.
        /// </remarks>
        public IConfigurationFrame ConfigurationFrame
        {
            get
            {
                return m_configurationFrame;
            }
            set
            {
                m_configurationFrame = value;

                // Pass new config frame onto appropriate parser, casting into appropriate protocol if needed...
                if ((object)m_frameParser != null)
                    m_frameParser.ConfigurationFrame = value;
            }
        }

        /// <summary>
        /// Gets the number of redundant frames in each packet.
        /// </summary>
        /// <remarks>
        /// This value is used when calculating statistics. It is assumed that for each
        /// frame that is received, that frame will be included in the next <c>n</c>
        /// packets, where <c>n</c> is the number of redundant frames per packet.
        /// </remarks>
        public int RedundantFramesPerPacket
        {
            get
            {
                if ((object)m_frameParser != null)
                    return m_frameParser.RedundantFramesPerPacket;

                return 0;
            }
        }

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="PhasorProtocol"/> is an IEEE standard protocol.
        /// </summary>
        public bool IsIEEEProtocol
        {
            get
            {
                return (m_phasorProtocol == GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118V2 ||
                        m_phasorProtocol == GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118V1 ||
                        m_phasorProtocol == GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118D6 ||
                        m_phasorProtocol == GSF.PhasorProtocols.PhasorProtocol.Ieee1344);
            }
        }

        /// <summary>
        /// Gets a flag that determines if the currently selected <see cref="TransportProtocol"/> is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if ((object)m_commandChannel != null && m_keepCommandChannelOpen)
                    return (m_commandChannel.CurrentState == ClientState.Connected);

                if ((object)m_dataChannel != null)
                    return (m_dataChannel.CurrentState == ClientState.Connected);

                if ((object)m_serverBasedDataChannel != null)
                    return (m_serverBasedDataChannel.ClientIDs.Length > 0);

                return false;
            }
        }

        /// <summary>
        /// Gets flag that determines if the connection type is multicast.
        /// </summary>
        public bool ConnectionIsMulticast
        {
            get
            {
                // Multicast will only be for UDP style connections
                if (m_transportProtocol != TransportProtocol.Udp)
                    return false;

                if (!string.IsNullOrWhiteSpace(m_connectionString))
                {
                    Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                    string server;
                    Match endPoint;
                    IPAddress serverAddress;

                    if (settings.TryGetValue("server", out server))
                    {
                        if (!settings.ContainsKey("remotePort"))
                        {
                            endPoint = Regex.Match(server, Transport.EndpointFormatRegex);
                            server = endPoint.Groups["host"].Value;
                        }

                        if (IPAddress.TryParse(server, out serverAddress))
                            return Transport.IsMulticastIP(serverAddress);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets total time connection has been active.
        /// </summary>
        public Time ConnectionTime
        {
            get
            {
                if ((object)m_commandChannel != null)
                    return m_commandChannel.ConnectionTime;

                if ((object)m_dataChannel != null)
                    return m_dataChannel.ConnectionTime;

                if ((object)m_serverBasedDataChannel != null)
                    return m_serverBasedDataChannel.RunTime;

                return 0;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the <see cref="MultiProtocolFrameParser"/> is currently enabled.
        /// </summary>
        /// <remarks>
        /// Setting <see cref="Enabled"/> to true will start the <see cref="MultiProtocolFrameParser"/> if it is not started,
        /// setting to false will stop the <see cref="MultiProtocolFrameParser"/> if it is started.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
            set
            {
                if (value && !m_enabled)
                    Start();
                else if (!value && m_enabled)
                    Stop();
            }
        }

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public int QueuedBuffers
        {
            get
            {
                return (object)m_frameParser != null ? m_frameParser.QueuedBuffers : 0;
            }
        }

        /// <summary>
        /// Gets the total number of frames that are currently queued for publication, if any.
        /// </summary>
        public int QueuedOutputs
        {
            get
            {
                return (object)m_frameParser != null ? m_frameParser.QueuedOutputs : 0;
            }
        }

        /// <summary>
        /// Gets a boolean value that determines if data channel is defined as a server based connection.
        /// </summary>
        public bool DataChannelIsServerBased
        {
            get
            {
                if ((object)m_dataChannel != null)
                    return false;

                if ((object)m_serverBasedDataChannel == null)
                {
                    // No connection is currently active, see if connection string defines a server based connection
                    if (!string.IsNullOrWhiteSpace(m_connectionString))
                    {
                        Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                        string setting;

                        if (settings.TryGetValue("islistener", out setting))
                            return setting.ParseBoolean();

                        return false;
                    }

                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets total number of frames that have been received from a device so far.
        /// </summary>
        public long TotalFramesReceived
        {
            get
            {
                return m_totalFramesReceived;
            }
        }

        /// <summary>
        /// Gets total number of bytes that have been received from a device so far.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return m_totalBytesReceived;
            }
        }

        /// <summary>
        /// Gets total number of frames that were missing from device so far.
        /// </summary>
        public long TotalMissingFrames
        {
            get
            {
                return m_totalMissingFrames;
            }
        }

        /// <summary>
        /// Gets total number of CRC exceptions encountered from device so far.
        /// </summary>
        public long TotalCrcExceptions
        {
            get
            {
                return m_totalCrcExceptions;
            }
        }

        /// <summary>
        /// Gets the configured frame rate as reported by the connected device.
        /// </summary>
        public int ConfiguredFrameRate
        {
            get
            {
                return m_configuredFrameRate;
            }
        }

        /// <summary>
        /// Gets the calculated frame rate (i.e., frames per second) based on data received from device connection.
        /// </summary>
        public double CalculatedFrameRate
        {
            get
            {
                return m_calculatedFrameRate;
            }
        }

        /// <summary>
        /// Gets the calculated byte rate (i.e., bytes per second) based on data received from device connection.
        /// </summary>
        public double ByteRate
        {
            get
            {
                return m_calculatedByteRate;
            }
        }

        /// <summary>
        /// Gets the calculated bit rate (i.e., bits per second (bps)) based on data received from device connection.
        /// </summary>
        public double BitRate
        {
            get
            {
                return m_calculatedByteRate * 8.0D;
            }
        }

        /// <summary>
        /// Gets the calculated megabits per second (Mbps) rate based on data received from device connection.
        /// </summary>
        public double MegaBitRate
        {
            get
            {
                return BitRate / SI2.Mega;
            }
        }

        /// <summary>
        /// Gets a descriptive name for a device connection that includes <see cref="SourceName"/>, if provided.
        /// </summary>
        public string Name
        {
            get
            {

                if (string.IsNullOrWhiteSpace(m_sourceName))
                    return "ID " + m_deviceID + " using " + m_phasorProtocol.GetFormattedProtocolName() + " over " + m_transportProtocol;
                else
                    return m_sourceName + " (" + m_deviceID + ")";
            }
        }

        /// <summary>
        /// Gets current descriptive status of the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("      Device Connection ID: {0}", m_deviceID);
                status.AppendLine();
                status.AppendFormat("           Phasor protocol: {0}", m_phasorProtocol.GetFormattedProtocolName());
                status.AppendLine();
                status.AppendFormat("           Connection type: {0}", ConnectionType);

                if (ConnectionIsMulticast)
                    status.Append(" - Multicast");

                status.AppendLine();
                status.AppendFormat("               Buffer size: {0}", m_bufferSize);
                status.AppendLine();
                status.AppendFormat("     Total frames received: {0}", m_totalFramesReceived);
                status.AppendLine();
                status.AppendFormat("      Total missing frames: {0}", m_totalMissingFrames);
                status.AppendLine();
                status.AppendFormat("      Total CRC exceptions: {0}", m_totalCrcExceptions);
                status.AppendLine();
                status.AppendFormat("     Calculated frame rate: {0}", m_calculatedFrameRate);
                status.AppendLine();
                status.AppendFormat("      Calculated data rate: {0} bytes/sec, {1} Mbps", m_calculatedByteRate.ToString("0.0"), MegaBitRate.ToString("0.0000"));
                status.AppendLine();
                status.AppendFormat("Allowed parsing exceptions: {0}", m_allowedParsingExceptions);
                status.AppendLine();
                status.AppendFormat("  Parsing exception window: {0} seconds", m_parsingExceptionWindow.ToSeconds().ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("Using simulated timestamps: {0}", m_injectSimulatedTimestamp ? "Yes" : "No");
                status.AppendLine();

                if (m_transportProtocol == TransportProtocol.File)
                {
                    status.AppendFormat("  Defined input frame rate: {0} frames/sec", m_definedFrameRate);
                    status.AppendLine();
                    status.AppendFormat("     Precision input timer: {0}", UseHighResolutionInputTimer ? "Enabled" : "Offline");
                    status.AppendLine();

                    if ((object)m_inputTimer != null)
                    {
                        status.AppendFormat("  Timer resynchronizations: {0}", m_inputTimer.Resynchronizations);
                        status.AppendLine();
                    }
                }

                if ((object)m_frameParser != null)
                    status.Append(m_frameParser.Status);

                if ((object)m_dataChannel != null)
                    status.Append(m_dataChannel.Status);

                if ((object)m_serverBasedDataChannel != null)
                    status.Append(m_serverBasedDataChannel.Status);

                if ((object)m_commandChannel != null)
                    status.Append(m_commandChannel.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the connection type (Active, Passive or Hybrid) based on defined channels and transport selections.
        /// </summary>
        public string ConnectionType
        {
            get
            {
                switch (m_transportProtocol)
                {
                    case TransportProtocol.Tcp:
                    case TransportProtocol.Serial:
                        return "Active";
                    case TransportProtocol.Udp:
                    case TransportProtocol.File:
                        if ((object)m_commandChannel != null)
                            return "Hybrid";
                        return "Passive";
                    default:
                        return "Undetermined";
                }
            }
        }

        /// <summary>
        /// Gets or sets any connection specific <see cref="IConnectionParameters"/> that may be applicable for the current <see cref="PhasorProtocol"/>.
        /// </summary>
        public IConnectionParameters ConnectionParameters
        {
            get
            {
                return m_connectionParameters;
            }
            set
            {
                m_connectionParameters = value;

                // Pass new connection parameters along to derived frame parser if instantiated
                if ((object)m_frameParser != null)
                    m_frameParser.ConnectionParameters = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="MultiProtocolFrameParser"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MultiProtocolFrameParser"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        Stop();

                        DetachFromInputTimer(ref m_inputTimer);

                        if ((object)m_rateCalcTimer != null)
                        {
                            m_rateCalcTimer.Elapsed -= m_rateCalcTimer_Elapsed;
                            m_rateCalcTimer.Dispose();
                        }
                        m_rateCalcTimer = null;

                        if ((object)m_streamStopDataHandle != null)
                            m_streamStopDataHandle.Dispose();

                        m_streamStopDataHandle = null;

                        // Clear minimum timer resolution.
                        PrecisionTimer.ClearMinimumTimerResolution(1);
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public void Start()
        {
            // Stop parser if it is already running - thus calling start after already started will have the effect
            // of "restarting" the parsing engine...
            Stop();

            // Reset statistics...
            m_totalFramesReceived = 0;
            m_totalMissingFrames = 0;
            m_totalCrcExceptions = 0;
            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_totalBytesReceived = 0;
            m_calculatedFrameRate = 0.0D;
            m_calculatedByteRate = 0.0D;
            m_lastParsingExceptionTime = 0;
            m_parsingExceptionCount = 0;

            try
            {
                // Parse connection string to check for special parameters
                Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();
                string setting;

                // Reset connection attempt counter
                m_connectionAttempts = 0;

                // Validate that the high-precision input timer is necessary
                if (m_transportProtocol != TransportProtocol.File && UseHighResolutionInputTimer)
                    UseHighResolutionInputTimer = false;

                // Establish protocol specific frame parser
                InitializeFrameParser(settings);

                if (settings.TryGetValue("commandChannel", out setting))
                {
                    // Establish command channel connection, if defined...
                    InitializeCommandChannel(setting);
                }
                else
                {
                    // Establish data channel connection - must be defined.
                    InitializeDataChannel(settings);
                }

                m_rateCalcTimer.Enabled = true;
                m_enabled = true;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Stop();

                // Check for common error when using an IPv4 address on an IPv6 stack
                if (ex.ErrorCode == 10014)
                    OnConnectionException(new InvalidOperationException(string.Format("Bad IP address format in \"{0}\": {1}\r\n\r\nUse a DNS name or an IPv6 formatted IP address (e.g., ::1); otherwise, force IPv4 mode.", m_connectionString, ex.Message), ex), 1);
                else
                    OnConnectionException(new InvalidOperationException(string.Format("{0} in \"{1}\"", ex.Message, m_connectionString), ex), 1);
            }
            catch (Exception ex)
            {
                Stop();
                OnConnectionException(new InvalidOperationException(string.Format("{0} in \"{1}\"", ex.Message, m_connectionString), ex), 1);
            }
        }

        /// <summary>
        /// Initialize frame parser.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        private void InitializeFrameParser(Dictionary<string, string> settings)
        {
            string setting;

            // Instantiate protocol specific frame parser
            switch (m_phasorProtocol)
            {
                case PhasorProtocol.IeeeC37_118V2:
                case PhasorProtocol.IeeeC37_118V1:
                    m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft7);
                    break;
                case PhasorProtocol.IeeeC37_118D6:
                    m_frameParser = new IeeeC37_118.FrameParser(IeeeC37_118.DraftRevision.Draft6);
                    break;
                case PhasorProtocol.Ieee1344:
                    m_frameParser = new Ieee1344.FrameParser();
                    break;
                case PhasorProtocol.Iec61850_90_5:
                    m_frameParser = new Iec61850_90_5.FrameParser();

                    // Check for IEC 61850-90-5 protocol specific parameters in connection string
                    Iec61850_90_5.ConnectionParameters iecParameters = m_connectionParameters as Iec61850_90_5.ConnectionParameters;

                    if ((object)iecParameters != null)
                    {
                        if (settings.TryGetValue("useETRConfiguration", out setting))
                            iecParameters.UseETRConfiguration = setting.ParseBoolean();

                        if (settings.TryGetValue("guessConfiguration", out setting))
                            iecParameters.GuessConfiguration = setting.ParseBoolean();

                        if (settings.TryGetValue("parseRedundantASDUs", out setting))
                            iecParameters.ParseRedundantASDUs = setting.ParseBoolean();

                        if (settings.TryGetValue("ignoreSignatureValidationFailures", out setting))
                            iecParameters.IgnoreSignatureValidationFailures = setting.ParseBoolean();

                        if (settings.TryGetValue("ignoreSampleSizeValidationFailures", out setting))
                            iecParameters.IgnoreSampleSizeValidationFailures = setting.ParseBoolean();
                    }

                    break;
                case PhasorProtocol.BpaPdcStream:
                    m_frameParser = new BpaPdcStream.FrameParser();

                    // Check for BPA PDCstream protocol specific parameters in connection string
                    BpaPdcStream.ConnectionParameters bpaPdcParameters = m_connectionParameters as BpaPdcStream.ConnectionParameters;

                    if ((object)bpaPdcParameters != null)
                    {
                        // INI file name setting is required
                        if (settings.TryGetValue("iniFileName", out setting))
                            bpaPdcParameters.ConfigurationFileName = FilePath.GetAbsolutePath(setting);
                        else if (string.IsNullOrWhiteSpace(bpaPdcParameters.ConfigurationFileName))
                            throw new ArgumentException("BPA PDCstream INI filename setting (e.g., \"inifilename=DEVICE_PDC.ini\") was not found. This setting is required for BPA PDCstream protocol connections - frame parser initialization terminated.");

                        if (settings.TryGetValue("refreshConfigFileOnChange", out setting))
                            bpaPdcParameters.RefreshConfigurationFileOnChange = setting.ParseBoolean();

                        if (settings.TryGetValue("parseWordCountFromByte", out setting))
                            bpaPdcParameters.ParseWordCountFromByte = setting.ParseBoolean();

                        if (settings.TryGetValue("usePhasorDataFileFormat", out setting))
                            bpaPdcParameters.UsePhasorDataFileFormat = setting.ParseBoolean();
                    }
                    break;
                case PhasorProtocol.FNet:
                    m_frameParser = new FNet.FrameParser();

                    // Check for F-NET protocol specific parameters in connection string
                    FNet.ConnectionParameters fnetParameters = m_connectionParameters as FNet.ConnectionParameters;

                    if ((object)fnetParameters != null)
                    {
                        if (settings.TryGetValue("timeOffset", out setting))
                            fnetParameters.TimeOffset = long.Parse(setting);

                        if (settings.TryGetValue("stationName", out setting))
                            fnetParameters.StationName = setting;

                        if (settings.TryGetValue("frameRate", out setting))
                            fnetParameters.FrameRate = ushort.Parse(setting);

                        if (settings.TryGetValue("nominalFrequency", out setting))
                            fnetParameters.NominalFrequency = (LineFrequency)int.Parse(setting);
                    }
                    break;
                case PhasorProtocol.SelFastMessage:
                    m_frameParser = new SelFastMessage.FrameParser();

                    // Check for SEL Fast Message protocol specific parameters in connection string
                    SelFastMessage.ConnectionParameters selParameters = m_connectionParameters as SelFastMessage.ConnectionParameters;

                    if ((object)selParameters != null)
                    {
                        if (settings.TryGetValue("messagePeriod", out setting))
                            selParameters.MessagePeriod = (SelFastMessage.MessagePeriod)Enum.Parse(typeof(SelFastMessage.MessagePeriod), setting, true);
                    }
                    break;
                case PhasorProtocol.Macrodyne:
                    m_frameParser = new Macrodyne.FrameParser();

                    // Check for Macrodyne protocol specific parameters in connection string
                    Macrodyne.ConnectionParameters macrodyneParameters = m_connectionParameters as Macrodyne.ConnectionParameters;

                    if ((object)macrodyneParameters != null)
                    {
                        Macrodyne.ProtocolVersion protocolVersion;

                        if (settings.TryGetValue("protocolVersion", out setting) && Enum.TryParse(setting, true, out protocolVersion))
                            macrodyneParameters.ProtocolVersion = protocolVersion;

                        // INI file name setting is required for 1690G protocol
                        if (settings.TryGetValue("iniFileName", out setting))
                            macrodyneParameters.ConfigurationFileName = FilePath.GetAbsolutePath(setting);
                        else if (macrodyneParameters.ProtocolVersion == Macrodyne.ProtocolVersion.G && string.IsNullOrWhiteSpace(macrodyneParameters.ConfigurationFileName))
                            throw new ArgumentException("Macrodyne INI filename setting (e.g., \"iniFileName=DEVICE_PDC.ini\") was not found. This setting is required for 1690G devices - frame parser initialization terminated.");

                        // Device label setting is required for 1690G protocol
                        if (settings.TryGetValue("deviceLabel", out setting))
                            macrodyneParameters.DeviceLabel = setting;
                        else if (macrodyneParameters.ProtocolVersion == Macrodyne.ProtocolVersion.G && string.IsNullOrWhiteSpace(macrodyneParameters.DeviceLabel))
                            throw new ArgumentException("Macrodyne device label setting (e.g., \"deviceLabel=DEVICE1\") was not found. This setting is required for 1690G devices - frame parser initialization terminated.");

                        if (settings.TryGetValue("refreshConfigFileOnChange", out setting))
                            macrodyneParameters.RefreshConfigurationFileOnChange = setting.ParseBoolean();
                    }
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Phasor protocol \"{0}\" is not recognized, failed to initialize frame parser", m_phasorProtocol));
            }

            // Assign frame parser properties
            m_frameParser.ConnectionParameters = m_connectionParameters;

            // Setup event handlers
            m_frameParser.ReceivedCommandFrame += m_frameParser_ReceivedCommandFrame;
            m_frameParser.ReceivedConfigurationFrame += m_frameParser_ReceivedConfigurationFrame;
            m_frameParser.ReceivedDataFrame += m_frameParser_ReceivedDataFrame;
            m_frameParser.ReceivedHeaderFrame += m_frameParser_ReceivedHeaderFrame;
            m_frameParser.ReceivedUndeterminedFrame += m_frameParser_ReceivedUndeterminedFrame;
            m_frameParser.ReceivedFrameImage += m_frameParser_ReceivedFrameImage;
            m_frameParser.ConfigurationChanged += m_frameParser_ConfigurationChanged;
            m_frameParser.ParsingException += m_frameParser_ParsingException;
            m_frameParser.BufferParsed += m_frameParser_BufferParsed;

            // Only attach to this event if consumer needs buffer image (i.e., has attached to our event)
            if ((object)ReceivedFrameBufferImage != null)
                m_frameParser.ReceivedFrameBufferImage += m_frameParser_ReceivedFrameBufferImage;

            // Start parsing engine
            m_frameParser.Start();
        }

        /// <summary>
        /// Initialize command channel.
        /// </summary>
        /// <param name="connectionString">Command channel connection string.</param>
        private void InitializeCommandChannel(string connectionString)
        {
            // Parse command channel connection settings
            TransportProtocol transportProtocol;
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
            string setting;

            // Verify user did not attempt to setup command channel as a TCP server
            if (settings.ContainsKey("islistener") && settings["islistener"].ParseBoolean())
                throw new ArgumentException("Command channel cannot be setup as a TCP server.");

            // Determine what transport protocol user selected
            if (settings.TryGetValue("transportProtocol", out setting) || settings.TryGetValue("protocol", out setting))
            {
                transportProtocol = (TransportProtocol)Enum.Parse(typeof(TransportProtocol), setting, true);

                // The communications engine only recognizes the transport protocol key as "protocol"
                connectionString = connectionString.ReplaceCaseInsensitive("transportProtocol", "protocol");
            }
            else
                throw new ArgumentException("No transport protocol was specified for command channel. For example: \"transportProtocol=Tcp\".");

            // Validate command channel transport protocol selection
            if (transportProtocol != TransportProtocol.Tcp && transportProtocol != TransportProtocol.Serial && transportProtocol != TransportProtocol.File)
                throw new ArgumentException("Command channel transport protocol can only be defined as TCP, Serial or File");

            // Instantiate command channel based on defined transport layer
            m_commandChannel = ClientBase.Create(connectionString);

            // Setup event handlers
            m_commandChannel.ConnectionEstablished += m_commandChannel_ConnectionEstablished;
            m_commandChannel.ConnectionAttempt += m_commandChannel_ConnectionAttempt;
            m_commandChannel.ConnectionException += m_commandChannel_ConnectionException;
            m_commandChannel.ConnectionTerminated += m_commandChannel_ConnectionTerminated;
            m_commandChannel.ReceiveData += m_commandChannel_ReceiveData;
            m_commandChannel.ReceiveDataException += m_commandChannel_ReceiveDataException;
            m_commandChannel.SendDataException += m_commandChannel_SendDataException;

            // Attempt connection to device over command channel
            m_commandChannel.ReceiveBufferSize = m_bufferSize;
            m_commandChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
            m_commandChannel.ConnectAsync();
        }

        /// <summary>
        /// Initialize data channel.
        /// </summary>
        /// <param name="settings">Key/value pairs dictionary parsed from connection string.</param>
        private void InitializeDataChannel(Dictionary<string, string> settings)
        {
            string setting;

            // Instantiate selected transport layer
            switch (m_transportProtocol)
            {
                case TransportProtocol.Tcp:
                    // The TCP transport may be set up as a server or as a client, we distinguish
                    // this simply by deriving the value of an added key/value pair in the
                    // connection string called "IsListener"
                    if (settings.TryGetValue("islistener", out setting))
                    {
                        if (setting.ParseBoolean())
                            m_serverBasedDataChannel = new TcpServer();
                        else
                            m_dataChannel = new TcpClient();
                    }
                    else
                    {
                        // If the key doesn't exist, we assume it's a client connection
                        m_dataChannel = new TcpClient();
                    }
                    break;
                case TransportProtocol.Udp:
                    InitializeUdpDataChannel(settings);
                    break;
                case TransportProtocol.Serial:
                    m_dataChannel = new SerialClient();
                    m_initiatingSerialConnection = true;
                    break;
                case TransportProtocol.File:
                    m_dataChannel = new FileClient();

                    // For file based playback, we allow the option of auto-repeat
                    FileClient fileClient = m_dataChannel as FileClient;
                    fileClient.FileOpenMode = FileMode.Open;
                    fileClient.FileAccessMode = FileAccess.Read;
                    fileClient.FileShareMode = FileShare.Read;
                    fileClient.ReceiveOnDemand = true;
                    fileClient.AutoRepeat = m_autoRepeatCapturedPlayback;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Transport protocol \"{0}\" is not recognized, failed to initialize data channel", m_transportProtocol));
            }

            // Handle primary data connection, this *must* be defined...
            if ((object)m_dataChannel != null)
            {
                // Setup event handlers
                m_dataChannel.ConnectionEstablished += m_dataChannel_ConnectionEstablished;
                m_dataChannel.ConnectionAttempt += m_dataChannel_ConnectionAttempt;
                m_dataChannel.ConnectionException += m_dataChannel_ConnectionException;
                m_dataChannel.ConnectionTerminated += m_dataChannel_ConnectionTerminated;
                m_dataChannel.ReceiveData += m_dataChannel_ReceiveData;
                m_dataChannel.ReceiveDataException += m_dataChannel_ReceiveDataException;
                m_dataChannel.SendDataException += m_dataChannel_SendDataException;

                // Attempt connection to device
                m_dataChannel.ReceiveBufferSize = m_bufferSize;
                m_dataChannel.ConnectionString = m_connectionString;
                m_dataChannel.MaxConnectionAttempts = m_maximumConnectionAttempts;
                m_dataChannel.ConnectAsync();
            }
            else if ((object)m_serverBasedDataChannel != null)
            {
                // Setup event handlers
                m_serverBasedDataChannel.ClientConnected += m_serverBasedDataChannel_ClientConnected;
                m_serverBasedDataChannel.ClientDisconnected += m_serverBasedDataChannel_ClientDisconnected;
                m_serverBasedDataChannel.ServerStarted += m_serverBasedDataChannel_ServerStarted;
                m_serverBasedDataChannel.ServerStopped += m_serverBasedDataChannel_ServerStopped;
                m_serverBasedDataChannel.ReceiveClientData += m_serverBasedDataChannel_ReceiveClientData;
                m_serverBasedDataChannel.ReceiveClientDataException += m_serverBasedDataChannel_ReceiveClientDataException;
                m_serverBasedDataChannel.SendClientDataException += m_serverBasedDataChannel_SendClientDataException;

                // Listen for device connection
                m_serverBasedDataChannel.ReceiveBufferSize = m_bufferSize;
                m_serverBasedDataChannel.ConfigurationString = m_connectionString;
                m_serverBasedDataChannel.MaxClientConnections = 1;
                m_serverBasedDataChannel.Start();
            }
            else
                throw new InvalidOperationException("No data channel was initialized, cannot start frame parser");
        }

        private void InitializeUdpDataChannel(Dictionary<string, string> settings)
        {
            SharedUdpClientReference udpRef;
            IPStack ipStack;

            string receiveFromSetting;

            if (!settings.TryGetValue("receiveFrom", out receiveFromSetting))
            {
                m_dataChannel = new UdpClient();
            }
            else
            {
                ipStack = Transport.GetInterfaceIPStack(settings);
                m_receiveFromAddress = Transport.CreateEndPoint(receiveFromSetting, 0, ipStack).Address;

                // Set up data channel
                udpRef = new SharedUdpClientReference();
                udpRef.ReceiveDataFrom += m_dataChannel_ReceiveDataFrom;
                m_dataChannel = udpRef;
            }
        }

        /// <summary>
        /// Stops the <see cref="MultiProtocolFrameParser"/>.
        /// </summary>
        public void Stop()
        {
            WaitHandle commandWaitHandle;

            m_enabled = false;
            m_rateCalcTimer.Enabled = false;
            m_configurationFrame = null;

            // Make sure data stream is disabled
            if (!m_skipDisableRealTimeData)
            {
                commandWaitHandle = SendDeviceCommand(DeviceCommand.DisableRealTimeData);

                if ((object)commandWaitHandle != null)
                    commandWaitHandle.WaitOne(1000);
            }

            if ((object)m_dataChannel != null)
            {
                try
                {
                    m_dataChannel.Disconnect();
                }
                catch (Exception ex)
                {
                    OnParsingException(ex, "Failed to properly disconnect data channel: {0}", ex.Message);
                }
                finally
                {
                    m_dataChannel.ConnectionEstablished -= m_dataChannel_ConnectionEstablished;
                    m_dataChannel.ConnectionAttempt -= m_dataChannel_ConnectionAttempt;
                    m_dataChannel.ConnectionException -= m_dataChannel_ConnectionException;
                    m_dataChannel.ConnectionTerminated -= m_dataChannel_ConnectionTerminated;
                    m_dataChannel.ReceiveData -= m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException -= m_dataChannel_ReceiveDataException;
                    m_dataChannel.SendDataException -= m_dataChannel_SendDataException;
                    m_dataChannel.Dispose();
                }
            }
            m_dataChannel = null;

            if ((object)m_serverBasedDataChannel != null)
            {
                try
                {
                    m_serverBasedDataChannel.DisconnectAll();
                }
                catch (Exception ex)
                {
                    OnParsingException(ex, "Failed to properly disconnect server based data channel: {0}", ex.Message);
                }
                finally
                {
                    m_serverBasedDataChannel.ClientConnected -= m_serverBasedDataChannel_ClientConnected;
                    m_serverBasedDataChannel.ClientDisconnected -= m_serverBasedDataChannel_ClientDisconnected;
                    m_serverBasedDataChannel.ServerStarted -= m_serverBasedDataChannel_ServerStarted;
                    m_serverBasedDataChannel.ServerStopped -= m_serverBasedDataChannel_ServerStopped;
                    m_serverBasedDataChannel.ReceiveClientData -= m_serverBasedDataChannel_ReceiveClientData;
                    m_serverBasedDataChannel.ReceiveClientDataException -= m_serverBasedDataChannel_ReceiveClientDataException;
                    m_serverBasedDataChannel.SendClientDataException -= m_serverBasedDataChannel_SendClientDataException;
                    m_serverBasedDataChannel.Dispose();
                }
            }
            m_serverBasedDataChannel = null;

            if ((object)m_commandChannel != null)
            {
                try
                {
                    m_commandChannel.Disconnect();
                }
                catch (Exception ex)
                {
                    OnParsingException(ex, "Failed to properly disconnect command channel: {0}", ex.Message);
                }
                finally
                {
                    m_commandChannel.ConnectionEstablished -= m_commandChannel_ConnectionEstablished;
                    m_commandChannel.ConnectionAttempt -= m_commandChannel_ConnectionAttempt;
                    m_commandChannel.ConnectionException -= m_commandChannel_ConnectionException;
                    m_commandChannel.ConnectionTerminated -= m_commandChannel_ConnectionTerminated;
                    m_commandChannel.ReceiveData -= m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException -= m_commandChannel_ReceiveDataException;
                    m_commandChannel.SendDataException -= m_commandChannel_SendDataException;
                    m_commandChannel.Dispose();
                }
            }
            m_commandChannel = null;

            if ((object)m_frameParser != null)
            {
                try
                {
                    m_frameParser.Stop();
                }
                catch (Exception ex)
                {
                    OnParsingException(ex, "Failed to properly stop frame parser: {0}", ex.Message);
                }
                finally
                {
                    m_frameParser.ReceivedCommandFrame -= m_frameParser_ReceivedCommandFrame;
                    m_frameParser.ReceivedConfigurationFrame -= m_frameParser_ReceivedConfigurationFrame;
                    m_frameParser.ReceivedDataFrame -= m_frameParser_ReceivedDataFrame;
                    m_frameParser.ReceivedHeaderFrame -= m_frameParser_ReceivedHeaderFrame;
                    m_frameParser.ReceivedUndeterminedFrame -= m_frameParser_ReceivedUndeterminedFrame;
                    m_frameParser.ReceivedFrameImage -= m_frameParser_ReceivedFrameImage;
                    m_frameParser.ConfigurationChanged -= m_frameParser_ConfigurationChanged;
                    m_frameParser.ParsingException -= m_frameParser_ParsingException;
                    m_frameParser.BufferParsed -= m_frameParser_BufferParsed;

                    if ((object)ReceivedFrameBufferImage != null)
                        m_frameParser.ReceivedFrameBufferImage -= m_frameParser_ReceivedFrameBufferImage;

                    m_frameParser.Dispose();
                }
            }
            m_frameParser = null;
        }

        /// <summary>
        /// Sends the specified <see cref="DeviceCommand"/> to the remote device.
        /// </summary>
        /// <param name="command"><see cref="DeviceCommand"/> to send to the remote device.</param>
        /// <remarks>
        /// Command will only be sent if <see cref="DeviceSupportsCommands"/> is <c>true</c> and <see cref="MultiProtocolFrameParser"/>.
        /// </remarks>
        /// <returns>A <see cref="WaitHandle"/>.</returns>
        public WaitHandle SendDeviceCommand(DeviceCommand command)
        {
            WaitHandle handle = null;

            try
            {
                if (m_deviceSupportsCommands && ((object)m_dataChannel != null || (object)m_serverBasedDataChannel != null || (object)m_commandChannel != null))
                {
                    ICommandFrame commandFrame;

                    // Only the IEEE, SEL Fast Message and Macrodyne protocols support commands
                    switch (m_phasorProtocol)
                    {
                        case GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118V2:
                        case GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118V1:
                        case GSF.PhasorProtocols.PhasorProtocol.IeeeC37_118D6:
                            commandFrame = new IeeeC37_118.CommandFrame(m_deviceID, command, 1);
                            break;
                        case GSF.PhasorProtocols.PhasorProtocol.Ieee1344:
                            commandFrame = new Ieee1344.CommandFrame(m_deviceID, command);
                            break;
                        case GSF.PhasorProtocols.PhasorProtocol.Iec61850_90_5:
                            commandFrame = new Iec61850_90_5.CommandFrame(m_deviceID, command, 1);
                            break;
                        case GSF.PhasorProtocols.PhasorProtocol.SelFastMessage:
                            // Get defined message period
                            SelFastMessage.MessagePeriod messagePeriod = SelFastMessage.MessagePeriod.DefaultRate;
                            SelFastMessage.ConnectionParameters connectionParameters = m_connectionParameters as SelFastMessage.ConnectionParameters;

                            if ((object)connectionParameters != null)
                                messagePeriod = connectionParameters.MessagePeriod;

                            commandFrame = new SelFastMessage.CommandFrame(command, messagePeriod);
                            break;
                        case GSF.PhasorProtocols.PhasorProtocol.Macrodyne:
                            commandFrame = new Macrodyne.CommandFrame(command);
                            break;
                        default:
                            commandFrame = null;
                            break;
                    }

                    if ((object)commandFrame != null)
                    {
                        byte[] buffer = commandFrame.BinaryImage();

                        // Send command over appropriate communications channel - command channel, if defined,
                        // will take precedence over other communications channels for command traffic...
                        if ((object)m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
                        {
                            handle = m_commandChannel.SendAsync(buffer, 0, buffer.Length);
                        }
                        else if ((object)m_dataChannel != null && m_dataChannel.CurrentState == ClientState.Connected)
                        {
                            handle = m_dataChannel.SendAsync(buffer, 0, buffer.Length);
                        }
                        else if ((object)m_serverBasedDataChannel != null && m_serverBasedDataChannel.CurrentState == ServerState.Running)
                        {
                            WaitHandle[] handles = m_serverBasedDataChannel.MulticastAsync(buffer, 0, buffer.Length);

                            if ((object)handles != null && handles.Length > 0)
                                handle = handles[0];
                        }

                        if ((object)SentCommandFrame != null)
                            SentCommandFrame(this, new EventArgs<ICommandFrame>(commandFrame));
                    }
                }
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "Failed to send device command \"{0}\": {1}", command, ex.Message);
            }

            return handle;
        }

        /// <summary>
        /// Writes data directly to the frame parsing engine buffer.
        /// </summary>
        /// <remarks>
        /// This method is public to allow consumer to "manually send extra data" to the parsing engine to be parsed, if desired.
        /// </remarks>
        /// <param name="buffer">Buffer containing data to be parsed.</param>
        /// <param name="offset">Offset into buffer where data begins.</param>
        /// <param name="count">Length of data in buffer to be parsed.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            Parse(SourceChannel.Other, buffer, offset, count);
        }

        /// <summary>
        /// Writes a sequence of bytes onto the <see cref="IBinaryImageParser"/> stream for parsing.
        /// </summary>
        /// <param name="source">Defines the source channel for the data.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        internal void Parse(SourceChannel source, byte[] buffer, int offset, int count)
        {
            // Pass data from communications client into protocol specific frame parser
            m_frameParser.Parse(source, buffer, offset, count);

            m_byteRateTotal += count;

            if (m_initiatingDataStream)
                m_initialBytesReceived += count;
        }

        void IFrameParser.Parse(SourceChannel source, byte[] buffer, int offset, int count)
        {
            Parse(source, buffer, offset, count);
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ParsingException"/> event.</param>
        private void OnParsingException(Exception ex)
        {
            if ((object)ParsingException != null && !(ex is ThreadAbortException) && !(ex is ObjectDisposedException))
                ParsingException(this, new EventArgs<Exception>(ex));

            if (DateTime.Now.Ticks - m_lastParsingExceptionTime > m_parsingExceptionWindow)
            {
                // Exception window has passed since last exception, so we reset counters
                m_lastParsingExceptionTime = DateTime.Now.Ticks;
                m_parsingExceptionCount = 0;
            }

            m_parsingExceptionCount++;

            if (m_parsingExceptionCount > m_allowedParsingExceptions)
            {
                try
                {
                    // When the parsing exception threshold has been exceeded, connection is stopped
                    Stop();
                }
                finally
                {
                    // Notify consumer of parsing exception threshold deviation
                    OnExceededParsingExceptionThreshold();
                    m_lastParsingExceptionTime = 0;
                    m_parsingExceptionCount = 0;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ParsingException"/> event.
        /// </summary>
        /// <param name="innerException">Actual exception to send as inner exception to <see cref="ParsingException"/> event.</param>
        /// <param name="message">Message of new exception to send to <see cref="ParsingException"/> event.</param>
        /// <param name="args">Arguments of message of new exception to send to <see cref="ParsingException"/> event.</param>
        private void OnParsingException(Exception innerException, string message, params object[] args)
        {
            if (!(innerException is ThreadAbortException) && !(innerException is ObjectDisposedException))
                OnParsingException(new Exception(string.Format(message, args), innerException));
        }

        /// <summary>
        /// Raises the <see cref="ExceededParsingExceptionThreshold"/> event.
        /// </summary>
        private void OnExceededParsingExceptionThreshold()
        {
            if ((object)ExceededParsingExceptionThreshold != null)
                ExceededParsingExceptionThreshold(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to raise.</param>
        /// <param name="connectionAttempts">Number of connection attempts to report.</param>
        private void OnConnectionException(Exception ex, int connectionAttempts)
        {
            if ((object)ConnectionException != null && !(ex is ThreadAbortException) && !(ex is ObjectDisposedException))
                ConnectionException(this, new EventArgs<Exception, int>(ex, connectionAttempts));
        }

        /// <summary>
        /// Derives a flag based on settings that determines if the current connection supports device commands.
        /// </summary>
        /// <returns>Derived flag that determines if the current connection supports device commands.</returns>
        private bool DeriveCommandSupport()
        {
            // Command support is based on phasor protocol, transport protocol and connection style
            if (IsIEEEProtocol || m_phasorProtocol == PhasorProtocol.Iec61850_90_5 || m_phasorProtocol == PhasorProtocol.SelFastMessage || m_phasorProtocol == PhasorProtocol.Macrodyne)
            {
                // IEEE protocols using TCP or Serial connection support device commands
                if (m_transportProtocol == TransportProtocol.Tcp || m_transportProtocol == TransportProtocol.Serial)
                    return true;

                if (!string.IsNullOrWhiteSpace(m_connectionString))
                {
                    Dictionary<string, string> settings = m_connectionString.ParseKeyValuePairs();

                    // A defined command channel inherently means commands are supported
                    if (settings.ContainsKey("commandchannel"))
                    {
                        return true;
                    }
                    else if (m_transportProtocol == TransportProtocol.Udp)
                    {
                        string server;
                        Match endPoint;
                        IPAddress serverAddress;

                        // IEEE protocols "can" use UDP connection to support devices commands, but only
                        // when remote device acts as a UDP listener (i.e., a "server" connection)
                        // and remote device is not a multicast end point
                        if (settings.TryGetValue("server", out server))
                        {
                            if (!settings.ContainsKey("remotePort"))
                            {
                                endPoint = Regex.Match(server, Transport.EndpointFormatRegex);
                                server = endPoint.Groups["host"].Value;
                            }

                            if (IPAddress.TryParse(server, out serverAddress))
                                return !Transport.IsMulticastIP(serverAddress);
                        }
                    }
                }
            }

            return false;
        }

        // Starts data parsing sequence.
        private void StartDataParsingSequence(object state)
        {
            try
            {
                // Attempt to stop real-time data, waiting a maximum of three seconds for this activity
                if (!m_skipDisableRealTimeData && m_phasorProtocol != PhasorProtocol.Iec61850_90_5)
                {
                    m_streamStopDataHandle.Reset();
                    ThreadPool.QueueUserWorkItem(AttemptToStopRealTimeData);
                    m_streamStopDataHandle.Wait(3000);
                }

                m_initiatingDataStream = false;

                // Request configuration frame once real-time data has been disabled. Note that data stream
                // will be enabled when we receive a configuration frame. 
                switch (m_phasorProtocol)
                {
                    case PhasorProtocol.SelFastMessage:
                        // SEL Fast Message doesn't define a binary configuration frame so we skip
                        // requesting one and jump straight to enabling the data stream.
                        SendDeviceCommand(DeviceCommand.EnableRealTimeData);
                        break;
                    case PhasorProtocol.Macrodyne:
                        // We collect the station name (i.e. the unit ID via 0xBB 0x48)) from the Macrodyne
                        // protocol interpreted as a header frame before we get the configuration frame
                        bool sendCommand = true;
                        Macrodyne.ConnectionParameters parameters = m_connectionParameters as Macrodyne.ConnectionParameters;

                        if ((object)parameters != null)
                            sendCommand = (parameters.ProtocolVersion != Macrodyne.ProtocolVersion.G);

                        if (sendCommand)
                            SendDeviceCommand(DeviceCommand.SendHeaderFrame);

                        break;
                    default:
                        // Otherwise we just rquest the configuration frame
                        SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);
                        break;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
        }

        private void AttemptToStopRealTimeData(object state)
        {
            try
            {
                // Some devices will only send a config frame once data streaming has been disabled, so
                // we use this code to disable real-time data and wait for data to stop streaming...
                int attempts = 0;

                // Make sure data stream is disabled
                SendDeviceCommand(DeviceCommand.DisableRealTimeData);

                Thread.Sleep(1000);

                // Wait for real-time data stream to cease for up to two seconds
                while (m_initialBytesReceived > 0)
                {
                    m_initialBytesReceived = 0;
                    Thread.Sleep(100);

                    attempts++;
                    if (attempts >= 20)
                        break;
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
            finally
            {
                if ((object)m_streamStopDataHandle != null)
                    m_streamStopDataHandle.Set();
            }
        }

        // Calculate frame and data rates
        private void m_rateCalcTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            double time = Ticks.ToSeconds(DateTime.Now.Ticks - m_dataStreamStartTime);

            m_calculatedFrameRate = (double)m_frameRateTotal / time;
            m_calculatedByteRate = (double)m_byteRateTotal / time;

            // Since rate calculation timer is not precise, the missing frames calculation can be calculated out
            // of sequence with the total frames. If there is a negative balance, we cache the value so it can
            // be applied to the next calculation to keep calculation more accurate.
            long missingFrames = (long)(m_configuredFrameRate * m_rateCalcTimer.Interval * SI.Milli * (RedundantFramesPerPacket + 1)) - m_frameRateTotal;

            if (missingFrames > 0)
            {
                m_totalMissingFrames += (missingFrames + m_missingFramesOverflow);
                m_missingFramesOverflow = 0;
            }
            else
                m_missingFramesOverflow = missingFrames;

            m_totalFramesReceived += m_frameRateTotal;
            m_totalBytesReceived += m_byteRateTotal;

            m_frameRateTotal = 0;
            m_byteRateTotal = 0;
            m_dataStreamStartTime = DateTime.Now.Ticks;
        }

        // Handles needed start-up actions once a client is connected
        private void ClientConnectedHandler()
        {
            try
            {
                if ((object)ConnectionEstablished != null)
                    ConnectionEstablished(this, EventArgs.Empty);

                // Begin data parsing sequence to handle reception of configuration frame
                if (m_deviceSupportsCommands && m_autoStartDataParsingSequence)
                {
                    m_initialBytesReceived = 0;
                    m_initiatingDataStream = true;
                    ThreadPool.QueueUserWorkItem(StartDataParsingSequence);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnParsingException(ex);
            }
        }

        private void MaintainCapturedFrameReplayTiming(IFrame sourceFrame)
        {
            long simulatedTimestamp = 0;

            if ((object)m_inputTimer == null)
            {
                if (m_lastFrameReceivedTime > 0)
                {
                    // To maintain timing on "frames per second", we wait for defined frame rate interval
                    double sleepTime = (1.0D / m_definedFrameRate) - ((double)(PrecisionTimer.UtcNow.Ticks - m_lastFrameReceivedTime) / (double)Ticks.PerSecond);

                    // Thread sleep time is a minimum suggested sleep time depending on system activity, when not using high-resolution
                    // input timer we assume getting close is good enough
                    if (sleepTime > 0)
                        Thread.Sleep((int)(sleepTime * 1000.0D));
                }

                m_lastFrameReceivedTime = PrecisionTimer.UtcNow.Ticks;

                if (m_injectSimulatedTimestamp)
                {
                    long baseTicks, ticksBeyondSecond, frameIndex, nextFrameTicks;

                    simulatedTimestamp = m_lastFrameReceivedTime;

                    // Baseline timestamp to the top of the second
                    baseTicks = simulatedTimestamp - simulatedTimestamp % Ticks.PerSecond;

                    // Remove the seconds from ticks
                    ticksBeyondSecond = simulatedTimestamp - baseTicks;

                    // Calculate a frame index between 0 and m_framesPerSecond-1, corresponding to ticks
                    // rounded down to the nearest frame
                    frameIndex = (long)(ticksBeyondSecond / m_ticksPerFrame);

                    // Calculate the timestamp of the nearest frame rounded up
                    nextFrameTicks = (frameIndex + 1) * Ticks.PerSecond / m_definedFrameRate;

                    // Determine whether the desired frame is the nearest frame rounded down or the nearest frame rounded up
                    // After translating nextDestinationTicks to millisecond resolution, if next ticks are less than or equal
                    // to ticks, nextDestinationTicks corresponds to the desired frame
                    if ((nextFrameTicks / Ticks.PerMillisecond) * Ticks.PerMillisecond <= ticksBeyondSecond)
                        simulatedTimestamp = nextFrameTicks;
                    else
                        simulatedTimestamp = frameIndex * Ticks.PerSecond / m_definedFrameRate;

                    // Recover the seconds that were removed
                    simulatedTimestamp += baseTicks;
                }
            }
            else
            {
                // When high resolution input timing is requested, we only need to wait for the next signal...
                m_inputTimer.FrameWaitHandle.Wait();

                // Input timer can be disabled while thread is waiting, so we make sure it is not null
                if ((object)m_inputTimer != null)
                    simulatedTimestamp = m_inputTimer.LastFrameTime;
            }

            // If injecting a simulated timestamp, use the last received time
            if (m_injectSimulatedTimestamp)
                sourceFrame.Timestamp = simulatedTimestamp;

            // Read next buffer if output frames are almost all processed
            if (QueuedOutputs < 2)
                ThreadPool.QueueUserWorkItem(ReadNextFileBuffer);
        }

        // Handle attach to input timer
        private PrecisionInputTimer AttachToInputTimer(int framesPerSecond)
        {
            PrecisionInputTimer timer;

            lock (s_inputTimers)
            {
                // Get static input timer for given frames per second creating it if needed
                if (!s_inputTimers.TryGetValue(framesPerSecond, out timer))
                {
                    try
                    {
                        // Create a new precision input timer
                        timer = new PrecisionInputTimer(framesPerSecond);

                        // Add timer state for given rate to static collection
                        s_inputTimers.Add(framesPerSecond, timer);
                    }
                    catch (Exception ex)
                    {
                        // Process exception for logging
                        OnParsingException(new InvalidOperationException("Failed to create precision input timer due to exception: " + ex.Message, ex));
                    }
                }

                // Increment reference count for input timer at given frame rate
                timer.AddReference();
                m_attachedToInputTimer = true;
            }

            return timer;
        }

        // Handle detach from input timer
        private void DetachFromInputTimer(ref PrecisionInputTimer timer)
        {
            if ((object)timer != null)
            {
                lock (s_inputTimers)
                {
                    if (m_attachedToInputTimer)
                    {
                        // Verify static frame rate timer for given frames per second exists
                        if (s_inputTimers.ContainsKey(timer.FramesPerSecond))
                        {
                            // Decrement reference count
                            timer.RemoveReference();
                            m_attachedToInputTimer = false;

                            // If timer is no longer being referenced we stop it and remove it from static collection
                            if (timer.ReferenceCount == 0)
                            {
                                timer.Dispose();
                                s_inputTimers.Remove(timer.FramesPerSecond);
                            }
                        }
                    }
                }
            }

            timer = null;
        }

        #region [ Data Channel Event Handlers ]

        private void m_dataChannel_ReceiveDataFrom(object sender, EventArgs<EndPoint, int> e)
        {
            IPEndPoint remoteEndPoint = e.Argument1 as IPEndPoint;
            int length = e.Argument2;
            byte[] buffer = null;

            if ((object)remoteEndPoint == null)
                return;

            if (!remoteEndPoint.Address.Equals(m_receiveFromAddress))
                return;

            try
            {
                buffer = BufferPool.TakeBuffer(length);
                length = m_dataChannel.Read(buffer, 0, length);
                Parse(SourceChannel.Data, buffer, 0, length);
            }
            finally
            {
                if ((object)buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }
        }

        private void m_dataChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            int length = e.Argument;
            byte[] buffer = null;

            try
            {
                buffer = BufferPool.TakeBuffer(length);
                length = m_dataChannel.Read(buffer, 0, length);
                Parse(SourceChannel.Data, buffer, 0, length);
            }
            finally
            {
                if ((object)buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }
        }

        private void m_dataChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            // Handle client connection from data channel
            ClientConnectedHandler();

            try
            {
                // Start reading file data
                if (m_transportProtocol == TransportProtocol.File)
                    ThreadPool.QueueUserWorkItem(ReadNextFileBuffer);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnParsingException(new InvalidOperationException("Failed to queue file read operation due to exception: " + ex.Message, ex));
            }
        }

        private void m_dataChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;

            if ((object)ConnectionAttempt != null)
                ConnectionAttempt(this, EventArgs.Empty);
        }

        private void m_dataChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnConnectionException(ex, m_connectionAttempts);
        }

        private void m_dataChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            if ((object)ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        private void m_dataChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument, "Data channel send exception: {0}", ex.Message);
        }

        private void m_dataChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            // For some serially connected devices, a frame exception on initial connection is very common - so we ignore this during startup
            if (m_initiatingSerialConnection && string.Compare(ex.Message, "The hardware detected a framing error.") == 0)
            {
                m_initiatingSerialConnection = false;
                return;
            }

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument, "Data channel receive exception: {0}", ex.Message);
        }

        #endregion

        #region [ Server Based Data Channel Event Handlers ]

        private void m_serverBasedDataChannel_ReceiveClientData(object sender, EventArgs<Guid, int> e)
        {
            Guid clientID = e.Argument1;
            int length = e.Argument2;
            byte[] buffer = null;

            try
            {
                buffer = BufferPool.TakeBuffer(length);
                length = m_serverBasedDataChannel.Read(clientID, buffer, 0, length);
                Parse(SourceChannel.Data, buffer, 0, length);
            }
            finally
            {
                if ((object)buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }
        }

        private void m_serverBasedDataChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            ClientConnectedHandler();
        }

        private void m_serverBasedDataChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            if ((object)ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        private void m_serverBasedDataChannel_ServerStarted(object sender, EventArgs e)
        {
            if ((object)ServerStarted != null)
                ServerStarted(this, EventArgs.Empty);
        }

        private void m_serverBasedDataChannel_ServerStopped(object sender, EventArgs e)
        {
            if ((object)ServerStopped != null)
                ServerStopped(this, EventArgs.Empty);
        }

        private void m_serverBasedDataChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument2, "Server based data channel send exception: {0}", ex.Message);
        }

        private void m_serverBasedDataChannel_ReceiveClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument2, "Server based data channel receive exception: {0}", ex.Message);
        }

        #endregion

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            int length = e.Argument;
            byte[] buffer = null;

            try
            {
                buffer = BufferPool.TakeBuffer(length);
                length = m_commandChannel.Read(buffer, 0, length);
                Parse(SourceChannel.Command, buffer, 0, length);
            }
            finally
            {
                if ((object)buffer != null)
                    BufferPool.ReturnBuffer(buffer);
            }
        }

        private void m_commandChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            try
            {
                // We'll start data channel once command channel has been established...
                InitializeDataChannel(m_connectionString.ParseKeyValuePairs());
            }
            catch (Exception ex)
            {
                OnConnectionException(ex, m_connectionAttempts);
            }
        }

        private void m_commandChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            m_connectionAttempts++;

            if ((object)ConnectionAttempt != null)
                ConnectionAttempt(this, EventArgs.Empty);
        }

        private void m_commandChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnConnectionException(ex, m_connectionAttempts);
        }

        private void m_commandChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            if (m_keepCommandChannelOpen)
            {
                if ((object)ConnectionTerminated != null)
                    ConnectionTerminated(this, EventArgs.Empty);
            }
        }

        private void m_commandChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument, "Command channel send exception: {0}", ex.Message);
        }

        private void m_commandChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is NullReferenceException) && !(ex is ObjectDisposedException))
                OnParsingException(e.Argument, "Command channel send exception: {0}", ex.Message);
        }

        #endregion

        #region [ Frame Parser Event Handlers ]

        private void m_frameParser_ReceivedCommandFrame(object sender, EventArgs<ICommandFrame> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = PrecisionTimer.UtcNow.Ticks;

                if ((object)ReceivedCommandFrame != null)
                    ReceivedCommandFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedCommandFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedConfigurationFrame(object sender, EventArgs<IConfigurationFrame> e)
        {
            // We automatically request enabling of real-time data upon reception of config frame if requested. Note that SEL Fast Message will
            // have already been enabled at this point so we don't duplicate request for enabling real-time data stream
            if ((object)m_configurationFrame == null && m_deviceSupportsCommands && m_autoStartDataParsingSequence && m_phasorProtocol != PhasorProtocol.SelFastMessage && m_phasorProtocol != PhasorProtocol.Iec61850_90_5)
                SendDeviceCommand(DeviceCommand.EnableRealTimeData);

            m_configurationFrame = e.Argument;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = PrecisionTimer.UtcNow.Ticks;

                if ((object)ReceivedConfigurationFrame != null)
                    ReceivedConfigurationFrame(this, e);

                if ((object)m_configurationFrame != null)
                    m_configuredFrameRate = m_configurationFrame.FrameRate;
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedConfigurationFrame\" consumer event handler exception: {0}", ex.Message);
            }

            // If user has requested to not keep the command channel open, disconnect it once the system has received a configuration frame
            if (!m_keepCommandChannelOpen && (object)m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
            {
                m_commandChannel.Disconnect();
            }
        }

        private void m_frameParser_ReceivedDataFrame(object sender, EventArgs<IDataFrame> e)
        {
            m_frameRateTotal++;

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_transportProtocol == TransportProtocol.File)
                    MaintainCapturedFrameReplayTiming(e.Argument);
                else if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = PrecisionTimer.UtcNow.Ticks;

                if ((object)ReceivedDataFrame != null)
                    ReceivedDataFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedDataFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedHeaderFrame(object sender, EventArgs<IHeaderFrame> e)
        {
            // Macrodyne receives header frame which contains station name before configuration frame (this gets online data format: 0xBB 0x24)
            if ((object)m_configurationFrame == null && m_phasorProtocol == PhasorProtocol.Macrodyne)
                SendDeviceCommand(DeviceCommand.SendConfigurationFrame2);

            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = PrecisionTimer.UtcNow.Ticks;

                if ((object)ReceivedHeaderFrame != null)
                    ReceivedHeaderFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedHeaderFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedUndeterminedFrame(object sender, EventArgs<IChannelFrame> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if (m_injectSimulatedTimestamp)
                    e.Argument.Timestamp = PrecisionTimer.UtcNow.Ticks;

                if ((object)ReceivedUndeterminedFrame != null)
                    ReceivedUndeterminedFrame(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedUndeterminedFrame\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedFrameImage(object sender, EventArgs<FundamentalFrameType, int> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if ((object)ReceivedFrameImage != null)
                    ReceivedFrameImage(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedFrameImage\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ReceivedFrameBufferImage(object sender, EventArgs<FundamentalFrameType, byte[], int, int> e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if ((object)ReceivedFrameBufferImage != null)
                    ReceivedFrameBufferImage(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ReceivedFrameBufferImage\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ConfigurationChanged(object sender, EventArgs e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if ((object)ConfigurationChanged != null)
                    ConfigurationChanged(this, e);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"ConfigurationChanged\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void m_frameParser_ParsingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (ex is CrcException)
                m_totalCrcExceptions++;

            OnParsingException(ex);
        }

        private void m_frameParser_BufferParsed(object sender, EventArgs e)
        {
            // We don't stop parsing for exceptions thrown in consumer event handlers
            try
            {
                if ((object)BufferParsed != null)
                    BufferParsed(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnParsingException(ex, "MultiProtocolFrameParser \"BufferParsed\" consumer event handler exception: {0}", ex.Message);
            }
        }

        private void ReadNextFileBuffer(object state)
        {
            try
            {
                FileClient fileClient = m_dataChannel as FileClient;

                if ((object)fileClient != null)
                {
                    if (fileClient.CurrentState == ClientState.Connected)
                        fileClient.ReadNextBuffer();
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnParsingException(new InvalidOperationException("Encountered an exception while reading file data: " + ex.Message, ex));
            }
        }

        #endregion

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Dictionary<int, PrecisionInputTimer> s_inputTimers = new Dictionary<int, PrecisionInputTimer>();

        #endregion
    }
}