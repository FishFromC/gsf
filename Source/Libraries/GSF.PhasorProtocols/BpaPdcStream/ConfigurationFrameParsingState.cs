﻿//******************************************************************************************************
//  ConfigurationFrameParsingState.cs - Gbtc
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
//  04/23/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//******************************************************************************************************

namespace GSF.PhasorProtocols.BpaPdcStream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of the parsing state used by a <see cref="ConfigurationFrame"/>.
    /// </summary>
    public class ConfigurationFrameParsingState : GSF.PhasorProtocols.ConfigurationFrameParsingState
    {
        #region [ Members ]

        // Fields
        private string m_configurationFileName;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrameParsingState"/> from specified parameters.
        /// </summary>
        /// <param name="parsedBinaryLength">Binary length of the <see cref="ConfigurationFrame"/> being parsed.</param>
        /// <param name="configurationFileName">The required external BPA PDCstream INI based configuration file.</param>
        /// <param name="createNewCellFunction">Reference to delegate to create new <see cref="ConfigurationCell"/> instances.</param>
        public ConfigurationFrameParsingState(int parsedBinaryLength, string configurationFileName, CreateNewCellFunction<IConfigurationCell> createNewCellFunction)
            : base(parsedBinaryLength, createNewCellFunction)
        {
            m_configurationFileName = configurationFileName;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets required external BPA PDCstream INI based configuration file.
        /// </summary>
        public string ConfigurationFileName
        {
            get
            {
                return m_configurationFileName;
            }
            set
            {
                m_configurationFileName = value;
            }
        }

        #endregion
    }
}