//******************************************************************************************************
//  CommonFrameHeader.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  05/19/2011 - Ritchie
//       Added DST file support.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using GSF.Parsing;
using GSF;

namespace GSF.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the common header for all BPA PDCstream frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : ICommonHeader<FrameType>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>, streaming data or data row in phasor file format data.
        /// </summary>
        public const ushort FixedLength = 4;

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/> for header in phasor file format data.
        /// </summary>
        public const ushort DstHeaderFixedLength = 132;

        // Fields
        private byte m_packetNumber;
        private ushort m_wordCount;
        private uint m_rowFlags;
        private Ticks m_roughTimestamp;
        private bool m_usePhasorDataFileFormat;
        private FileType m_fileType;
        private FileVersion m_fileVersion;
        private string m_sourceID;
        private uint m_startSample;
        private ushort m_sampleInterval;
        private ushort m_sampleRate;
        private uint m_rowLength;
        private uint m_totalRows;
        private Ticks m_triggerTime;
        private uint m_triggerSample;
        private uint m_preTriggerRows;
        private ushort m_triggerPMU;
        private ushort m_triggerType;
        private string m_userInformation;
        private uint m_pmuCount;
        private IChannelParsingState m_state;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from specified parameters.
        /// </summary>
        /// <param name="packetNumber">The BPA PDCstream packet number, configuration frame is packet zero.</param>
        public CommonFrameHeader(byte packetNumber)
        {
            m_packetNumber = packetNumber;
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="parseWordCountFromByte">Defines flag that interprets word count in packet header from a byte instead of a word.</param>
        /// <param name="usePhasorDataFileFormat">Defines flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).</param>
        /// <param name="configFrame">Previously parsed configuration frame, if available.</param>
        /// <param name="buffer">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        /// <param name="length">Maximum length of valid data from start index.</param>
        public CommonFrameHeader(bool parseWordCountFromByte, bool usePhasorDataFileFormat, ConfigurationFrame configFrame, byte[] buffer, int startIndex, int length)
        {
            uint secondOfCentury;

            // Determines if format is for DST file or streaming data
            m_usePhasorDataFileFormat = usePhasorDataFileFormat;

            if (m_usePhasorDataFileFormat)
            {
                // Handle phasor file format data protocol steps
                if (buffer[startIndex] == GSF.PhasorProtocols.Common.SyncByte && buffer[startIndex + 1] == Common.PhasorFileFormatFlag)
                {
                    // Bail out and leave frame length zero if there's not enough buffer to parse complete fixed portion of header
                    if (length >= DstHeaderFixedLength)
                    {
                        uint headerLength;

                        // Read full DST header
                        m_packetNumber = (byte)BpaPdcStream.FrameType.ConfigurationFrame;
                        m_fileType = (FileType)buffer[startIndex + 2];
                        m_fileVersion = (FileVersion)buffer[startIndex + 3];
                        m_sourceID = Encoding.ASCII.GetString(buffer, startIndex + 4, 4);
                        headerLength = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 8);
                        secondOfCentury = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 12);

                        switch (m_fileType)
                        {
                            case FileType.PdcNtp:
                                m_roughTimestamp = (new NtpTimeTag(secondOfCentury, 0)).ToDateTime().Ticks;
                                break;
                            case FileType.PdcUnix:
                                m_roughTimestamp = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
                                break;
                            default:
                                m_roughTimestamp = 0;
                                break;
                        }

                        m_startSample = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 16);
                        m_sampleInterval = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 20);
                        m_sampleRate = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 22);
                        m_rowLength = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 24);
                        m_totalRows = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 28);
                        secondOfCentury = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 32);

                        switch (m_fileType)
                        {
                            case FileType.PdcNtp:
                                m_triggerTime = (new NtpTimeTag(secondOfCentury, 0)).ToDateTime().Ticks;
                                break;
                            case FileType.PdcUnix:
                                m_triggerTime = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
                                break;
                            default:
                                m_triggerTime = 0;
                                break;
                        }

                        m_triggerSample = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 36);
                        m_preTriggerRows = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 40);
                        m_triggerPMU = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 44);
                        m_triggerType = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 46);
                        m_userInformation = Encoding.ASCII.GetString(buffer, startIndex + 48, 80).Trim();
                        m_pmuCount = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 128);
                        FrameLength = unchecked((ushort)headerLength);
                    }
                }
                else
                {
                    // Must assume this is a data row if there are no sync bytes
                    CommonFrameHeader configFrameHeader;

                    m_packetNumber = (byte)BpaPdcStream.FrameType.DataFrame;
                    m_rowFlags = EndianOrder.BigEndian.ToUInt32(buffer, startIndex);

                    if (configFrame != null)
                    {
                        uint sampleIndex = configFrame.SampleIndex;
                        configFrameHeader = configFrame.CommonHeader;

                        if (configFrameHeader != null)
                        {
                            // Assign row length to make sure parser knows how much data it needs
                            FrameLength = unchecked((ushort)configFrameHeader.RowLength);

                            // Calculate timestamp as offset plus sample index * frame rate
                            m_roughTimestamp = configFrameHeader.RoughTimestamp + Ticks.FromSeconds(sampleIndex * (1.0D / configFrameHeader.FrameRate));
                        }
                        else
                        {
                            FrameLength = FixedLength;
                        }

                        // Increment sample index for next row
                        configFrame.SampleIndex = (sampleIndex + 1);
                    }
                    else
                    {
                        FrameLength = FixedLength;
                    }
                }
            }
            else
            {
                // Handle streaming data protocol steps
                if (buffer[startIndex] != GSF.PhasorProtocols.Common.SyncByte)
                    throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA as first byte in BPA PDCstream frame, got 0x" + buffer[startIndex].ToString("X").PadLeft(2, '0'));

                // Get packet number
                m_packetNumber = buffer[startIndex + 1];

                // Some older streams have a bad word count (e.g., some data streams have a 0x01 as the third byte
                // in the stream - this should be a 0x00 to make the word count come out correctly).  The following
                // compensates for this erratic behavior
                if (parseWordCountFromByte)
                    m_wordCount = buffer[startIndex + 3];
                else
                    m_wordCount = EndianOrder.BigEndian.ToUInt16(buffer, startIndex + 2);

                // If this is a data frame get a rough timestamp down to the second (full parse will get accurate timestamp), this way
                // data frames that don't get fully parsed because configuration hasn't been received will still show a timestamp
                if (m_packetNumber > 0 && length > 8)
                {
                    secondOfCentury = EndianOrder.BigEndian.ToUInt32(buffer, startIndex + 4);

                    // Until configuration is available, we make a guess at time tag type - this will just be
                    // used for display purposes until a configuration frame arrives.  If second of century
                    // is greater than 3155673600 (SOC value for NTP timestamp 1/1/2007), then this is likely
                    // an NTP time stamp (else this is a Unix time tag for the year 2069 - not likely).
                    if (secondOfCentury > 3155673600)
                        m_roughTimestamp = (new NtpTimeTag(secondOfCentury, 0)).ToDateTime().Ticks;
                    else
                        m_roughTimestamp = (new UnixTimeTag(secondOfCentury)).ToDateTime().Ticks;
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_packetNumber = info.GetByte("packetNumber");
            m_wordCount = info.GetUInt16("wordCount");

            // The usePhasorDataFileFormat flag and other new elements did not exist in prior versions so we protect against possible deserialization failures
            try
            {
                m_usePhasorDataFileFormat = info.GetBoolean("usePhasorDataFileFormat");
                m_roughTimestamp = info.GetInt64("roughTimestamp");
                m_fileType = (FileType)info.GetValue("fileType", typeof(FileType));
                m_fileVersion = (FileVersion)info.GetValue("fileVersion", typeof(FileVersion));
                m_sourceID = info.GetString("sourceID");
                m_startSample = info.GetUInt32("startSample");
            }
            catch (SerializationException)
            {
                m_usePhasorDataFileFormat = false;
                m_roughTimestamp = 0;
                m_fileType = FileType.PdcUnix;
                m_fileVersion = FileVersion.PdcWithoutDbuf;
                m_sourceID = "UNDF";
                m_startSample = 0;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the BPA PDCstream specific frame type of this frame.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This returns the protocol specific frame classification which uniquely identifies the frame type.
        /// </para>
        /// <para>
        /// This is the <see cref="ICommonHeader{TTypeIdentifier}.TypeID"/> implementation.
        /// </para>
        /// </remarks>
        public FrameType TypeID
        {
            get
            {
                return (m_packetNumber == 0 ? BpaPdcStream.FrameType.ConfigurationFrame : BpaPdcStream.FrameType.DataFrame);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat
        {
            get
            {
                return m_usePhasorDataFileFormat;
            }
            set
            {
                m_usePhasorDataFileFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDCstream packet number of this frame - set to 00 for configuration frame
        /// </summary>
        public byte PacketNumber
        {
            get
            {
                return m_packetNumber;
            }
            set
            {
                m_packetNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDCstream frame length of this frame.
        /// </summary>
        public ushort FrameLength
        {
            get
            {
                return (ushort)(2 * m_wordCount);
            }
            set
            {
                m_wordCount = (ushort)(value / 2);
            }
        }

        /// <summary>
        /// Gets or sets the BPA PDcstream word count.
        /// </summary>
        public ushort WordCount
        {
            get
            {
                return m_wordCount;
            }
            set
            {
                m_wordCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the data in the BPA PDCstream frame (i.e., the <see cref="FrameLength"/> minus the header length and checksum: <see cref="FrameLength"/> - 8).
        /// </summary>
        public ushort DataLength
        {
            get
            {
                // Data length will be frame length minus common header length minus crc16
                return (ushort)(FrameLength - FixedLength - (m_usePhasorDataFileFormat ? 0 : 2));
            }
            set
            {
                if (value > Common.MaximumDataLength)
                    throw new OverflowException("Data length value cannot exceed " + Common.MaximumDataLength);

                FrameLength = (ushort)(value + FixedLength + (m_usePhasorDataFileFormat ? 0 : 2));
            }
        }

        /// <summary>
        /// Gets rough timestamp, accuarate to the second, that can be used until configuration frame arrives.
        /// </summary>
        public Ticks RoughTimestamp
        {
            get
            {
                return m_roughTimestamp;
            }
        }

        /// <summary>
        /// Gets or sets row flags for this <see cref="CommonFrameHeader"/> when frame is a data frame and use phasor file format is true.
        /// </summary>
        public uint RowFlags
        {
            get
            {
                return m_rowFlags;
            }
            set
            {
                m_rowFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets file type for this <see cref="CommonFrameHeader"/> used when frame is configuration frame and use phasor file format is true.
        /// </summary>
        public FileType FileType
        {
            get
            {
                return m_fileType;
            }
            set
            {
                m_fileType = value;
            }
        }

        /// <summary>
        /// Gets or sets file version for this <see cref="CommonFrameHeader"/> used when frame is configuration frame and use phasor file format is true.
        /// </summary>
        public FileVersion FileVersion
        {
            get
            {
                return m_fileVersion;
            }
            set
            {
                m_fileVersion = value;
            }
        }

        /// <summary>
        /// Gets or sets source ID for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public string SourceID
        {
            get
            {
                return m_sourceID;
            }
            set
            {
                value = value.Trim();

                if (!string.IsNullOrEmpty(value))
                    m_sourceID = value;
                else
                    m_sourceID = "UNDF";

                if (m_sourceID.Length > 4)
                    m_sourceID = m_sourceID.Substring(0, 4);

                m_sourceID = m_sourceID.PadRight(4);
            }
        }

        /// <summary>
        /// Gets or sets start sample for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public uint StartSample
        {
            get
            {
                return m_startSample;
            }
            set
            {
                m_startSample = value;
            }
        }

        /// <summary>
        /// Gets or sets sample interval for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public ushort SampleInterval
        {
            get
            {
                return m_sampleInterval;
            }
            set
            {
                m_sampleInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets sample rate for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public ushort SampleRate
        {
            get
            {
                return m_sampleRate;
            }
            set
            {
                m_sampleRate = value;
            }
        }

        /// <summary>
        /// Gets frame rate based on sample rate and sample interval for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public double FrameRate
        {
            get
            {
                return m_sampleRate / (double)m_sampleInterval;
            }
        }

        /// <summary>
        /// Gets or sets row length for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public uint RowLength
        {
            get
            {
                return m_rowLength;
            }
            set
            {
                m_rowLength = value;
            }
        }

        /// <summary>
        /// Gets or sets total number of data rows for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public uint TotalRows
        {
            get
            {
                return m_totalRows;
            }
            set
            {
                m_totalRows = value;
            }
        }

        /// <summary>
        /// Gets or sets trigger time for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public Ticks TriggerTime
        {
            get
            {
                return m_triggerTime;
            }
            set
            {
                m_triggerTime = value;
            }
        }

        /// <summary>
        /// Gets or sets trigger sample for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public uint TriggerSample
        {
            get
            {
                return m_triggerSample;
            }
            set
            {
                m_triggerSample = value;
            }
        }

        /// <summary>
        /// Gets or sets pre-trigger rows for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public uint PreTriggerRows
        {
            get
            {
                return m_preTriggerRows;
            }
            set
            {
                m_preTriggerRows = value;
            }
        }

        /// <summary>
        /// Gets or sets trigger PMU number for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public ushort TriggerPMU
        {
            get
            {
                return m_triggerPMU;
            }
            set
            {
                m_triggerPMU = value;
            }
        }

        /// <summary>
        /// Gets or sets trigger type for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public ushort TriggerType
        {
            get
            {
                return m_triggerType;
            }
            set
            {
                m_triggerType = value;
            }
        }

        /// <summary>
        /// Gets or sets free-form user information that can be added to the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public string UserInformation
        {
            get
            {
                return m_userInformation;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    m_userInformation = value;
                else
                    m_userInformation = "";

                if (m_userInformation.Length > 80)
                    m_userInformation = m_userInformation.Substring(0, 80);
            }
        }

        /// <summary>
        /// Gets or sets PMU count (i.e., number of cells) for the <see cref="CommonFrameHeader"/> object when use phasor file format is true.
        /// </summary>
        public uint PmuCount
        {
            get
            {
                return m_pmuCount;
            }
            set
            {
                m_pmuCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the parsing state for the <see cref="CommonFrameHeader"/> object.
        /// </summary>
        public IChannelParsingState State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value;
            }
        }

        // Gets or sets any additional state information - satifies ICommonHeader<FrameType>.State interface property
        object ICommonHeader<FrameType>.State
        {
            get
            {
                return m_state;
            }
            set
            {
                m_state = value as IChannelParsingState;
            }
        }

        /// <summary>
        /// Gets the fundamental frame type of this frame.
        /// </summary>
        /// <remarks>
        /// Frames are generally classified as data, configuration or header frames. This returns the general frame classification.
        /// </remarks>
        public FundamentalFrameType FrameType
        {
            get
            {
                // Translate BPA PDCstream specific frame type to fundamental frame type
                switch (TypeID)
                {
                    case BpaPdcStream.FrameType.DataFrame:
                        return FundamentalFrameType.DataFrame;
                    case BpaPdcStream.FrameType.ConfigurationFrame:
                        return FundamentalFrameType.ConfigurationFrame;
                    default:
                        return FundamentalFrameType.Undetermined;
                }
            }
        }

        /// <summary>
        /// Gets the binary image of the common header portion of this frame.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                if (!m_usePhasorDataFileFormat)
                {
                    byte[] buffer = new byte[FixedLength];

                    buffer[0] = GSF.PhasorProtocols.Common.SyncByte;
                    buffer[1] = m_packetNumber;
                    EndianOrder.BigEndian.CopyBytes(m_wordCount, buffer, 2);

                    return buffer;
                }
                else
                {
                    throw new NotSupportedException("Creation of the phasor file format (i.e., DST files) is not currently supported.");
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Appends header specific attributes to <paramref name="attributes"/> dictionary.
        /// </summary>
        /// <param name="attributes">Dictionary to append header specific attributes to.</param>
        internal void AppendHeaderAttributes(Dictionary<string, string> attributes)
        {
            attributes.Add("Frame Type", (ushort)TypeID + ": " + TypeID);
            attributes.Add("Using Phasor File Format", UsePhasorDataFileFormat.ToString());
            attributes.Add("Frame Length", FrameLength.ToString());
            attributes.Add("Packet Number", PacketNumber.ToString());
            attributes.Add("Word Count", WordCount.ToString());
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("packetNumber", m_packetNumber);
            info.AddValue("wordCount", m_wordCount);
            info.AddValue("usePhasorDataFileFormat", m_usePhasorDataFileFormat);
            info.AddValue("roughTimestamp", (long)m_roughTimestamp);
            info.AddValue("fileType", m_fileType, typeof(FileType));
            info.AddValue("fileVersion", m_fileVersion, typeof(FileVersion));
            info.AddValue("sourceID", m_sourceID);
            info.AddValue("startSample", m_startSample);
        }

        #endregion
    }
}