﻿//*******************************************************************************************************
//  SettingsBase.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Configuration;
using System.Reflection;
using System.ComponentModel;
using System.Linq;
using PCS.Reflection;

namespace PCS.Configuration
{
    /// <summary>
    /// Represents the base class for application settings that are synchronized with its configuration file.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SettingsBase : IDisposable
    {
        #region [ Members ]

        // Fields
        private ConfigurationFile m_configFile;
        private Func<string, string> m_getter;
        private Action<string, string> m_setter;
        private Action<string, string> m_creator;
        private bool m_requireSerializeSettingAttribute;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="SettingsBase"/> class for the application's configuration file.
        /// </summary>
        /// <param name="requireSerializeSettingAttribute">
        /// Assigns flag that determines if <see cref="SerializeSettingAttribute"/> is required
        /// to exist before a field or property is serialized to the configuration file.
        /// </param>
        protected SettingsBase(bool requireSerializeSettingAttribute)
        {
            m_requireSerializeSettingAttribute = requireSerializeSettingAttribute;
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="CategorizedSettingsBase"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~SettingsBase()
        {
            // If user failed to dispose class, we make sure settings get saved...
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets reference to working configuration file.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected ConfigurationFile ConfigFile
        {
            get
            {
                return m_configFile;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_configFile = value;
            }
        }

        /// <summary>
        /// Gets or sets reference to delegate used to retireve settings values.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected Func<string, string> Getter
        {
            get
            {
                return m_getter;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_getter = value;
            }
        }

        /// <summary>
        /// Gets or sets reference to delegate used to assign settings values.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected Action<string, string> Setter
        {
            get
            {
                return m_setter;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_setter = value;
            }
        }

        /// <summary>
        /// Gets or sets reference to delgate used to create settings with a default value, if they don't exist.
        /// </summary>
        /// <exception cref="NullReferenceException">value cannot be null.</exception>
        protected Action<string, string> Creator
        {
            get
            {
                return m_creator;
            }
            set
            {
                if (value == null)
                    throw new NullReferenceException("value cannot be null");

                m_creator = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="SerializeSettingAttribute"/> is
        /// required to exist before a field or property is serialized to the configuration
        /// file; defaults to False.
        /// </summary>
        public bool RequireSerializeSettingAttribute
        {
            get
            {
                return m_requireSerializeSettingAttribute;
            }
            set
            {
                m_requireSerializeSettingAttribute = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="CategorizedSettingsBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="CategorizedSettingsBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // We'll make sure settings are saved when class is properly disposed...
                    if (disposing)
                        Save();
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Adds a setting to the application's configuration file, if it doesn't already exist.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <remarks>
        /// Use this function to ensure a setting exists, it will not override an existing value.
        /// </remarks>
        public void Create(string name, string value)
        {
            m_creator(name, value.ToNonNullString());
        }

        /// <summary>
        /// Adds a setting to the application's configuration file, if it doesn't already exist.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        /// <remarks>
        /// Use this function to ensure a setting exists, it will not override an existing value.
        /// </remarks>
        public void Create<T>(string name, T value)
        {
            if (value == null)
                m_creator(name, "");
            else
                m_creator(name, value.ToString());
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Setting name.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public T Value<T>(string name)
        {
            T value = (T)Value(name, typeof(T));

            // If the element's value string is not present, uses the default.
            if (value == null)
                return default(T);

            return value;
        }

        /// <summary>
        /// Gets the application's configuration file setting converted to the given type.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="type">Setting type.</param>
        /// <returns>Value of specified configuration file setting converted to the given type.</returns>
        public object Value(string name, Type type)
        {
            string stringValue = m_getter(name);

            if (!string.IsNullOrEmpty(stringValue))
            {
                // Converts the element's value string, if present, to the proper type.
                if (type.IsEnum)
                {
                    // Parses the string to the equivalent enumeration.
                    return Enum.Parse(type, stringValue);
                }
                else if (type == typeof(bool))
                {
                    // Handles bool as a special case allowing numeric entries as well as true/false
                    return Convert.ChangeType(stringValue.ParseBoolean(), type);
                }
                else
                {
                    // Casts the string to the specified type.
                    return Convert.ChangeType(stringValue, type);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Copies the specified application setting into the given value.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        public void Get<T>(string name, out T value)
        {
            value = Value<T>(name);
        }

        /// <summary>
        /// Copies the given value into the specified application setting.
        /// </summary>
        /// <typeparam name="T">Type to use for setting conversion.</typeparam>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        public void Set<T>(string name, T value)
        {
            if (value == null)
                m_setter(name, "");
            else
                m_setter(name, value.ToString());
        }

        /// <summary>
        /// Initializes configuration settings from derived class fields.
        /// </summary>
        protected virtual void Initialize()
        {
            // Make sure all desired settings exist initialized with default values. Settings are
            // assumed to be public fields or public properties in derived class - so we enumerate
            // through of these making sure a setting exists for each field and property

            // Verify a configuration setting exists for each field
            ExecuteActionForFields(field => Create(field.Name, field.GetValue(this).ToNonNullString()));

            // Verify a configuration setting exists for each property
            ExecuteActionForProperties(property => Create(property.Name, property.GetValue(this, null).ToNonNullString()), BindingFlags.GetProperty);

            // If any new values were encountered, make sure they are flushed to config file
            m_configFile.Save();

            // Load current settings
            Load();
        }

        /// <summary>
        /// Loads configuration file into setting fields.
        /// </summary>
        public virtual void Load()
        {
            // Load configuration file settings into fields
            ExecuteActionForFields(field => field.SetValue(this, Value(field.Name, field.FieldType)));
            
            // Load configuration file settings into properties
            ExecuteActionForProperties(property => property.SetValue(this, Value(property.Name, property.PropertyType), null), BindingFlags.SetProperty);
        }

        /// <summary>
        /// Saves setting fields into configuration file.
        /// </summary>
        public virtual void Save()
        {
            // Saves setting fields into configuration file values
            ExecuteActionForFields(field => m_setter(field.Name, field.GetValue(this).ToNonNullString()));
            
            // Saves setting properties into configuration file values
            ExecuteActionForProperties(property => m_setter(property.Name, property.GetValue(this, null).ToNonNullString()), BindingFlags.GetProperty);

            // Make sure any changes are flushed to config file
            m_configFile.Save();
        }

        // Execute specified action over all public dervied class fields
        private void ExecuteActionForFields(Action<FieldInfo> fieldAction)
        {
            ExecuteActionForMembers(fieldAction, this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        }

        // Execute specified action over all public dervied class "set" properties
        private void ExecuteActionForProperties(Action<PropertyInfo> propertyAction, BindingFlags isGetOrSet)
        {
            // Make sure only non-indexer properties are used for settings
            ExecuteActionForMembers(property => { if (property.GetIndexParameters().Length == 0) propertyAction(property); }, this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | isGetOrSet));
        }

        // Execute specified action over specified memembers
        private void ExecuteActionForMembers<T>(Action<T> memberAction, T[] members) where T : MemberInfo
        {
            SerializeSettingAttribute attribute;

            // Execute action for each member
            foreach (T member in members)
            {
                // See if serialize setting attribute exists
                if (member.TryGetAttribute(out attribute))
                {
                    // Found serialze setting attribute, perform action if setting is true
                    if (attribute.Serialize)
                        memberAction(member);
                }
                else if (!m_requireSerializeSettingAttribute)
                {
                    // Didn't find serialize setting attribute and it's not required, so we perform action
                    memberAction(member);
                }
            }
        }

        #endregion
    }
}