﻿//******************************************************************************************************
//  UserRoleCache.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  05/07/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Collections;
using GSF.IO;
using GSF.Security.Cryptography;
using GSF.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace GSF.Security
{
    /// <summary>
    /// Represents a secured interprocess cache for a <see cref="Dictionary{TKey,TValue}"/> of serialized user role information.
    /// </summary>
    public class UserRoleCache : InterprocessCache
    {
        #region [ Members ]

        // Constants

        // Default user role cache file name
        private const string DefaultCacheFileName = "UserRoleCache.bin";

        // Fields
        private Dictionary<string, string> m_userRoles; // Internal dictionary of serialized user roles
        private readonly object m_userRolesLock;        // Lock object

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="UserRoleCache"/> with the specified number of <paramref name="maximumConcurrentLocks"/>.
        /// </summary>
        /// <param name="maximumConcurrentLocks">Maximum concurrent reader locks to allow.</param>
        public UserRoleCache(int maximumConcurrentLocks = InterprocessReaderWriterLock.DefaultMaximumConcurrentLocks)
            : base(maximumConcurrentLocks)
        {
            m_userRoles = new Dictionary<string, string>();
            m_userRolesLock = new object();
        }

        #region [ Properties ]

        /// <summary>
        /// Gets a copy of the internal user role dictionary.
        /// </summary>
        public Dictionary<string, string> UserRoles
        {
            get
            {
                Dictionary<string, string> userRoles;

                // We wait until the user roles cache is loaded before attempting to access it
                WaitForDataReady();

                // Wait for thread level lock on dictionary
                lock (m_userRolesLock)
                {
                    // Make a copy of the user role table for external use
                    userRoles = new Dictionary<string, string>(m_userRoles);
                }

                return userRoles;
            }
        }

        /// <summary>
        /// Gets or sets access role for given <paramref name="userName"/>.
        /// </summary>
        /// <param name="userName">User name for associated access role to load or save.</param>
        /// <returns>Access role for given <paramref name="userName"/> if found; otherwise <c>null</c>.</returns>
        public string this[string userName]
        {
            get
            {
                string role;
                TryGetUserRole(userName, out role);
                return role;
            }
            set
            {
                SaveUserRole(userName, value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to retrieve access role for given <paramref name="userName"/>.
        /// </summary>
        /// <param name="userName">User name associated with access role to retrieve.</param>
        /// <param name="role">Access role to populate if found.</param>
        /// <returns><c>true</c> if access role for given <paramref name="userName"/> was retrieved; otherwise <c>false</c>.</returns>
        public bool TryGetUserRole(string userName, out string role)
        {
            string hash = HashLoginID(userName);
            bool result;

            // We wait until the cache is loaded before attempting to access it
            WaitForDataReady();

            // Wait for thread level lock on user info table
            lock (m_userRolesLock)
            {
                // Attempt to lookup persisted access role based on hash of user name
                result = m_userRoles.TryGetValue(hash, out role);
            }

            return result;
        }

        /// <summary>
        /// Serializes the <paramref name="role"/> for the given <paramref name="userName"/> into the <see cref="UserRoleCache"/>.
        /// </summary>
        /// <param name="userName">User name associated with access role to retrieve.</param>
        /// <param name="role">Access role to update or populate.</param>
        /// <remarks>
        /// <para>
        /// This will add an entry into the user info cache for <paramref name="userName"/> if it doesn't exist;
        /// otherwise existing entry will be updated.
        /// </para>
        /// <para>
        /// Updates are automatically queued up for serialization so user does not need to call <see cref="Save"/>.
        /// </para>
        /// </remarks>
        public void SaveUserRole(string userName, string role)
        {
            string hash = HashLoginID(userName);

            // We wait until the cache is loaded before attempting to access it
            WaitForDataReady();

            // Wait for thread level lock on user data table
            lock (m_userRolesLock)
            {
                // Assign new user information to user data table
                m_userRoles[hash] = role;
            }

            // Queue up a serialization for this new user information
            Save();
        }

        /// <summary>
        /// Merge user roles from another <see cref="UserRoleCache"/>, local cache taking precedence.
        /// </summary>
        /// <param name="other">Other <see cref="UserRoleCache"/> to merge with.</param>
        public void MergeLeft(UserRoleCache other)
        {
            // Merge other roles into local ones
            Dictionary<string, string> mergedUserRoles = UserRoles.Merge(other.UserRoles);

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                // Replace local user roles dictionary with merged roles
                m_userRoles = mergedUserRoles;
            }

            // Queue up a serialization for any newly added roles
            Save();
        }

        /// <summary>
        /// Merge user roles from another <see cref="UserRoleCache"/>, other cache taking precedence.
        /// </summary>
        /// <param name="other">Other <see cref="UserRoleCache"/> to merge with.</param>
        public void MergeRight(UserRoleCache other)
        {
            // Merge other roles into local ones
            Dictionary<string, string> mergedUserRoles = other.UserRoles.Merge(UserRoles);

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                // Replace local user roles dictionary with merged roles
                m_userRoles = mergedUserRoles;
            }

            // Queue up a serialization for any newly added roles
            Save();
        }

        /// <summary>
        /// Initiates interprocess synchronized save of user role cache.
        /// </summary>
        public override void Save()
        {
            byte[] serializedUserDataTable;

            // Wait for thread level lock on dictionary
            lock (m_userRolesLock)
            {
                serializedUserDataTable = Serialization.Serialize(m_userRoles, SerializationFormat.Binary);
            }

            // File data is the serialized user roles dictionary, assigmnent will initiate auto-save if needed
            FileData = serializedUserDataTable;
        }

        /// <summary>
        /// Handles serialization of file to disk; virtual method allows customization (e.g., pre-save encryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to serialize data.</param>
        /// <param name="fileData">File data to be serialized.</param>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="InterprocessCache.FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected override void SaveFileData(FileStream fileStream, byte[] fileData)
        {
            // Encrypt data local to this machine (this way user cannot copy user role cache to another machine)
            base.SaveFileData(fileStream, ProtectedData.Protect(fileData, null, DataProtectionScope.LocalMachine));
        }

        /// <summary>
        /// Handles deserialization of file from disk; virtual method allows customization (e.g., pre-load decryption and/or data merge).
        /// </summary>
        /// <param name="fileStream"><see cref="FileStream"/> used to deserialize data.</param>
        /// <returns>Deserialized file data.</returns>
        /// <remarks>
        /// Consumers overriding this method should not directly call <see cref="InterprocessCache.FileData"/> property to avoid potential dead-locks.
        /// </remarks>
        protected override byte[] LoadFileData(FileStream fileStream)
        {
            // Decrypt data that was encrypted local to this machine
            byte[] serializedUserRoles = ProtectedData.Unprotect(fileStream.ReadStream(), null, DataProtectionScope.LocalMachine);
            Dictionary<string, string> userRoles = Serialization.Deserialize<Dictionary<string, string>>(serializedUserRoles, SerializationFormat.Binary);

            // Wait for thread level lock on user role dictionary
            lock (m_userRolesLock)
            {
                // Merge new and existing dictionaries since new user roles may have been queued for serialization, but not saved yet
                m_userRoles = userRoles.Merge(m_userRoles);
            }

            return serializedUserRoles;
        }

        /// <summary>
        /// Calculates the hash of the <paramref name="userName"/> used as the key for the user roles dictionary.
        /// </summary>
        /// <param name="userName">User name to hash.</param>
        /// <returns>The Base64 encoded calculated SHA-2 hash of the <paramref name="userName"/> used as the key for the user roles dictionary.</returns>
        /// <remarks>
        /// For added security, a hash of the <paramref name="userName"/> is used as the key for access roles
        /// in the user roles cache instead of the actual <paramref name="userName"/>. This method allows the
        /// consumer to properly calculate this hash when directly using the user data cache.
        /// </remarks>
        protected string HashLoginID(string userName)
        {
            return Cipher.GetPasswordHash(userName.ToLower(), 128);
        }

        // Waits until the cache is loaded before attempting to access it
        private void WaitForDataReady()
        {
            try
            {
                // Just wrapping this method to provide a more detailed exception message if there is an issue loading cache
                WaitForLoad();
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("User role cache access failure: timeout while attempting to load user role cache.", ex);
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Loads the <see cref="UserRoleCache"/> for the current local user.
        /// </summary>
        /// <returns>Loaded instance of the <see cref="UserRoleCache"/>.</returns>
        public static UserRoleCache GetCurrentCache()
        {
            UserRoleCache currentCache;
            UserRoleCache localUserRoleCache;
            string localCacheFileName = FilePath.GetAbsolutePath(DefaultCacheFileName);

            // Initialize local user role cache (application may only have read-only access to this cache)
            localUserRoleCache = new UserRoleCache
            {
                FileName = localCacheFileName,
#if DNF45
                ReloadOnChange = true,
#else
                // Reload on change is disabled to eliminate GC handle leaks on .NET 4.0, this prevents
                // automatic runtime reloading of key/iv data cached by another application.
                ReloadOnChange = false,
#endif
                AutoSave = false
            };

            // Load initial user roles
            localUserRoleCache.Load();

            try
            {
                // Validate that user has write access to the local cryptographic cache folder
                string tempFile = FilePath.GetDirectoryName(localCacheFileName) + Guid.NewGuid().ToString() + ".tmp";

                using (File.Create(tempFile))
                {
                }

                if (File.Exists(tempFile))
                    File.Delete(tempFile);

                // No access issues exist, use local cache as the primary cryptographic key and initialization vector cache
                currentCache = localUserRoleCache;
                currentCache.AutoSave = true;
            }
            catch (UnauthorizedAccessException)
            {
                // User does not have needed serialization access to common cryptographic cache folder,
                // use a path where user will have rights
                string userCacheFolder = FilePath.AddPathSuffix(FilePath.GetApplicationDataFolder());
                string userCacheFileName = userCacheFolder + FilePath.GetFileName(localCacheFileName);

                // Make sure user directory exists
                if (!Directory.Exists(userCacheFolder))
                    Directory.CreateDirectory(userCacheFolder);

                // Copy existing common cryptographic cache if none exists
                if (File.Exists(localCacheFileName) && !File.Exists(userCacheFileName))
                    File.Copy(localCacheFileName, userCacheFileName);

                // Initialize primary cryptographic key and initialization vector cache within user folder
                currentCache = new UserRoleCache
                {
                    FileName = userCacheFileName,
#if DNF45
                    ReloadOnChange = true,
#else
                    // Reload on change is disabled to eliminate GC handle leaks on .NET 4.0, this prevents
                    // automatic runtime reloading of key/iv data cached by another application.
                    ReloadOnChange = false,
#endif
                    AutoSave = true
                };

                // Load initial keys
                currentCache.Load();

                // Merge new or updated keys, protected folder keys taking precendence over user keys
                currentCache.MergeRight(localUserRoleCache);
            }

            return currentCache;
        }

        #endregion
    }
}
