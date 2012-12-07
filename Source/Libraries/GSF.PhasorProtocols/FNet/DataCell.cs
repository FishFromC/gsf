//*******************************************************************************************************
//  DataCell.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/08/2007 - J. Ritchie Carroll & Jian Ryan Zuo
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC § 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using GSF.Units;
using GSF;

namespace GSF.PhasorProtocols.FNet
{
    /// <summary>
    /// Represents the F-NET implementation of a <see cref="IDataCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class DataCell : DataCellBase
    {
        #region [ Members ]

        // Fields
        private double m_analogValue;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IDataFrame"/> of this <see cref="DataCell"/>.</param>
        /// <param name="configurationCell">The <see cref="IConfigurationCell"/> associated with this <see cref="DataCell"/>.</param>
        public DataCell(IDataFrame parent, IConfigurationCell configurationCell)
            : base(parent, configurationCell, 0x0000, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Initialize single phasor value and frequency value with an empty value
            PhasorValues.Add(new PhasorValue(this, configurationCell.PhasorDefinitions[0]));

            // Initialize frequency and df/dt
            FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="DataCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DataCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the reference to parent <see cref="DataFrame"/> of this <see cref="DataCell"/>.
        /// </summary>
        public new DataFrame Parent
        {
            get
            {
                return base.Parent as DataFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationCell"/> associated with this <see cref="DataCell"/>.
        /// </summary>
        public new ConfigurationCell ConfigurationCell
        {
            get
            {
                return base.ConfigurationCell as ConfigurationCell;
            }
            set
            {
                base.ConfigurationCell = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="DataCell"/> is valid.
        /// </summary>
        public override bool DataIsValid
        {
            get
            {
                return true;
            }
            set
            {
                // We just ignore updates to this value; F-NET defines no flags to determine if data is valid
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="DataCell"/> is valid based on GPS lock.
        /// </summary>
        /// <remarks>
        /// F-NET defines synchronization validity as a derived value based on the number of available satellites, i.e.,
        /// synchronization is valid if number of visible sattellites is greater than zero.
        /// </remarks>
        public override bool SynchronizationIsValid
        {
            get
            {
                return ConfigurationCell.NumberOfSatellites > 0;
            }
            set
            {
                // We just ignore updates to this value; F-NET defines synchronization validity as a derived value based on the number of available satellites
            }
        }

        /// <summary>
        /// Gets or sets <see cref="GSF.PhasorProtocols.DataSortingType"/> of this <see cref="DataCell"/>.
        /// </summary>
        public override DataSortingType DataSortingType
        {
            get
            {
                return (SynchronizationIsValid ? GSF.PhasorProtocols.DataSortingType.ByTimestamp : GSF.PhasorProtocols.DataSortingType.ByArrival);
            }
            set
            {
                // We just ignore updates to this value; data sorting type has been defined as a derived value based on synchronization validity
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if source device of this <see cref="DataCell"/> is reporting an error.
        /// </summary>
        /// <remarks>F-NET doesn't define any flags for device errors.</remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool DeviceError
        {
            get
            {
                return false;
            }
            set
            {
                // We just ignore updates to this value; F-NET defines no flags for data errors
            }
        }

        /// <summary>
        /// Gets date in F-NET format.
        /// </summary>
        public string FNetDate
        {
            get
            {
                return ((DateTime)Parent.Timestamp).ToString("MMddyy");
            }
        }

        /// <summary>
        /// Gets time in F-NET format.
        /// </summary>
        public string FNetTime
        {
            get
            {
                return ((DateTime)Parent.Timestamp).ToString("HHmmss");
            }
        }

        /// <summary>
        /// Gets or sets analog value for F-NET data row.
        /// </summary>
        public double AnalogValue
        {
            get
            {
                return m_analogValue;
            }
            set
            {
                m_analogValue = value;
            }
        }

        /// <summary>
        /// Gets image of this <see cref="DataCell"/> as a F-NET formatted data row.
        /// </summary>
        public string FNetDataRow
        {
            get
            {
                StringBuilder dataRow = new StringBuilder();

                dataRow.Append(Common.StartByte);
                dataRow.Append(IDCode);
                dataRow.Append(' ');
                dataRow.Append(FNetDate);
                dataRow.Append(' ');
                dataRow.Append(FNetTime);
                dataRow.Append(' ');
                dataRow.Append(Parent.SampleIndex);
                dataRow.Append(' ');
                dataRow.Append(m_analogValue);
                dataRow.Append(' ');
                dataRow.Append(FrequencyValue.Frequency);
                dataRow.Append(' ');
                dataRow.Append(PhasorValues[0].Magnitude);
                dataRow.Append(' ');
                dataRow.Append(PhasorValues[0].Angle);
                dataRow.Append(Common.EndByte);

                return dataRow.ToString();
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return FNetDataRow.Length;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="DataCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                return Encoding.ASCII.GetBytes(FNetDataRow);
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("F-NET Date", FNetDate);
                baseAttributes.Add("F-NET Time", FNetTime);
                baseAttributes.Add("Analog Value", m_analogValue.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The longitude, latitude and number of satellites arrive at the top of minute in F-NET data as the analog
        /// data in a siggle row, each on their own row, as sample 1, 2, and 3 respectively.
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            DataFrame parent = Parent;
            CommonFrameHeader commonHeader = parent.CommonHeader;
            string[] data = commonHeader.DataElements;
            ConfigurationCell configurationCell = ConfigurationCell;

            // Assign sample index
            parent.SampleIndex = short.Parse(data[Element.SampleIndex]);

            // Get timestamp of data record
            parent.Timestamp = configurationCell.TimeOffset + ParseTimestamp(data[Element.Date], data[Element.Time], parent.SampleIndex, configurationCell.FrameRate);

            // Parse out first analog value (can be long/lat at top of minute)
            m_analogValue = double.Parse(data[Element.Analog]);

            if (int.Parse(data[Element.Time].Substring(4, 2)) == 0)
            {
                switch (parent.SampleIndex)
                {
                    case 1:
                        configurationCell.Latitude = m_analogValue;
                        break;
                    case 2:
                        configurationCell.Longitude = m_analogValue;
                        break;
                    case 3:
                        configurationCell.NumberOfSatellites = (int)m_analogValue;
                        break;
                }
            }

            // Update (or create) frequency value
            double frequency = double.Parse(data[Element.Frequency]);

            if (FrequencyValue != null)
                FrequencyValue.Frequency = frequency;
            else
                FrequencyValue = new FrequencyValue(this, configurationCell.FrequencyDefinition as FrequencyDefinition, frequency, 0.0D);

            // Update (or create) phasor value
            Angle angle = double.Parse(data[Element.Angle]);
            double magnitude = double.Parse(data[Element.Voltage]);
            PhasorValue phasor = null;

            if (PhasorValues.Count > 0)
                phasor = PhasorValues[0] as PhasorValue;

            if (phasor != null)
            {
                phasor.Angle = angle;
                phasor.Magnitude = magnitude;
            }
            else
            {
                phasor = new PhasorValue(this, configurationCell.PhasorDefinitions[0] as PhasorDefinition, angle, magnitude);
                PhasorValues.Add(phasor);
            }

            return commonHeader.ParsedLength;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Converts F-NET date (mm/dd/yy), time (hh:mm:ss) and subsecond to time in ticks
        internal static Ticks ParseTimestamp(string fnetDate, string fnetTime, int sampleIndex, int frameRate)
        {
            fnetDate = fnetDate.PadLeft(6, '0');
            fnetTime = fnetTime.PadLeft(6, '0');

            if (sampleIndex == 10)
                return new DateTime(2000 + int.Parse(fnetDate.Substring(4, 2)), int.Parse(fnetDate.Substring(0, 2).Trim()), int.Parse(fnetDate.Substring(2, 2)), int.Parse(fnetTime.Substring(0, 2)), int.Parse(fnetTime.Substring(2, 2)), int.Parse(fnetTime.Substring(4, 2)), 0).AddSeconds(1.0D).Ticks;
            else
                return new DateTime(2000 + int.Parse(fnetDate.Substring(4, 2)), int.Parse(fnetDate.Substring(0, 2).Trim()), int.Parse(fnetDate.Substring(2, 2)), int.Parse(fnetTime.Substring(0, 2)), int.Parse(fnetTime.Substring(2, 2)), int.Parse(fnetTime.Substring(4, 2)), (int)(sampleIndex / (double)frameRate * 1000.0D)).Ticks;
        }

        // Delegate handler to create a new F-NET data cell
        internal static IDataCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IDataCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            DataCell dataCell = new DataCell(parent as IDataFrame, (state as IDataFrameParsingState).ConfigurationFrame.Cells[index]);

            parsedLength = dataCell.ParseBinaryImage(buffer, startIndex, 0);

            return dataCell;
        }

        #endregion
    }
}