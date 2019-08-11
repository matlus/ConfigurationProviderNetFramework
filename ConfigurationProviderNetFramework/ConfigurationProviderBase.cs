using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationProviderNetFramework
{
    /// <summary>
    /// This class acts as the base class (the Interface) for all configuration providers
    /// Additionally, this class is:
    /// 1. Agnostics of the source of configuration (app.config, web.config, Database, json file etc.)
    /// 2. This class bakes in all of the business requirements, that is what's optional, what's required and any default values in Production
    /// 3. The class provides a strongly typed interface to configuration information
    /// 4. This class prevents any incorrect or bad data from entering the system by throwing Exceptions that include the following
    ///     a. Clearly indicating the exact nature of the problem (not a generalized problem)
    ///     b. Providing clear indication of what the value is (in the config file)
    ///     c. Providing a set of valid values in cases where the set of possible values is finite (for example, in the case of boolean, provide true and false
    ///        or providing information such as, "The value in the config file MUST be parsable to a DateTime"
    /// </summary>
    internal abstract class ConfigurationProviderBase
    {
        protected enum ConfigurationSettingState { IsNull, IsWhiteSpaces, IsEmpty, IsPresent }

        /// <summary>
        /// The EmailTemplatesPath is essentially a portion of a Directory Path
        /// The application has a notion of a "Root Folder or Diretory" and the 
        /// EmailTemplatesPath (folder) hangs off of that
        /// This Property returns the EmailTemplatesPath as configured 
        /// This property is Required (Not Optional)
        /// The property ensures that the EmailTemplatesPath always starts with a "\"
        /// </summary>
        public string EmailTemplatesPath
        {
            get
            {
                var value = GetConfigurationSettingValueThrowIfNotFound("EmailTemplatesPath");
                return !value.StartsWith(@"\", StringComparison.OrdinalIgnoreCase) ? @"\" + value : value;
            }
        }

        /// <summary>
        /// This property returns the Base Url for the Credit Card Payment Gateway
        /// This property is required (Not Optional)
        /// If the Url Configured in the config file does not end with a "/", then this property
        /// will will append a "/" at the end of the Url and return it rather than trowing an exception
        /// </summary>
        public string PaymentGatewayServiceUrl
        {
            get
            {
                var paymentGatewayServiceUrlAsConfigured = GetConfigurationSettingValueThrowIfNotFound("PaymentGatewayServiceUrl");
                return !paymentGatewayServiceUrlAsConfigured.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? paymentGatewayServiceUrlAsConfigured + "/" : paymentGatewayServiceUrlAsConfigured;

            }
        }

        /// <summary>
        /// This property returns a <see cref="DateTime" /> type that is the start of the fiscal year
        /// The Year component of the DateTime is not important. The Month and Day component however, is
        /// This property is an Optional property. if the setting does not exist this property should return
        /// 1st October 0001 (in MM/dd/yyyy format this is 10/01/0001) 
        /// A value that can not be converted to a DateTime is a problem and an exception should be thrown
        /// </summary>
        public DateTime FiscalYearStart
        {
            get
            {
                var fiscalYearStartAsConfigured = GetConfigurationSettingValue("FiscalYearStart");
                DateTime fiscalYear = EnsureConfiguredFiscalYearStartIsValidDate(fiscalYearStartAsConfigured);
                return new DateTime(1, fiscalYear.Month, fiscalYear.Day);
            }
        }

        /// <summary>
        /// This property returns a bool indicating whether or not to send notifications on successfull uploads
        /// of videos and other assets.
        /// This property is Optional.
        /// If no config setting is found (in production there is no need for this setting to be present) this property
        /// should return true
        /// If the setting is not a valid bool, and exception should be thrown
        /// </summary>
        public bool NotifyOnUpload
        {
            get
            {
                var notifyOnUploadAsConfigured = GetConfigurationSettingValue("NotifyOnUpload");
                return EnsureNotifyOnLoadIsValidBool(notifyOnUploadAsConfigured);
            }
        }

        private bool EnsureNotifyOnLoadIsValidBool(string notifyOnUploadAsConfigured)
        {
            if (string.IsNullOrWhiteSpace(notifyOnUploadAsConfigured))
            {
                return true;
            }

            return bool.TryParse(notifyOnUploadAsConfigured, out var notifyOnUpload)
                ? notifyOnUpload
                : throw new ConfigurationErrorsException($"The NotifyOnUpload configuration setting value of: {notifyOnUploadAsConfigured}, is not a valid Boolean. This property is expected to be parseable to a Boolean value. Possible values are \"True\" or \"False\"");
        }

        /// <summary>
        /// This property returns an instance of <see cref="DbConnectionInformation"/> that is extracted from either
        /// the connectionStrings section of a config file or some other location.
        /// Descendants should implement this method in such as way that specialized exceptions are thrown
        /// indicating exactly what settings are missing and from where (what section of a config file or other location)
        /// </summary>
        public DbConnectionInformation DbConnectionInformation
        {
            get
            {
                return GetDbConnectionInformationCore("MyDb");
            }
        }

        private static DateTime EnsureConfiguredFiscalYearStartIsValidDate(string fiscalYearStartAsConfigured)
        {
            if (string.IsNullOrWhiteSpace(fiscalYearStartAsConfigured))
            {
                return new DateTime(1, 10, 1);
            }

            return DateTime.TryParse(fiscalYearStartAsConfigured, out var fiscalYearStart)
                ? fiscalYearStart
                : throw new ConfigurationErrorsException($"The FiscalYearStartDate configuration setting value of: {fiscalYearStartAsConfigured}, is not a valid DateTime. This property is expected to be Valid parseable DateTime");
        }

        protected static void EnsureConfigSettingIsPresent(string configurationValue, Func<ConfigurationSettingState, Exception> exceptionCallback)
        {
            var configurationSettingState = ConfigurationSettingState.IsPresent;

            if (string.IsNullOrWhiteSpace(configurationValue))
            {
                if (configurationValue == null)
                {
                    configurationSettingState = ConfigurationSettingState.IsNull;
                }
                else
                {
                    configurationSettingState = ConfigurationSettingState.IsWhiteSpaces;
                }
            }
            else if (configurationValue.Length == 0)
            {
                configurationSettingState = ConfigurationSettingState.IsEmpty;
            }

            if (configurationSettingState != ConfigurationSettingState.IsPresent)
            {
                throw exceptionCallback(configurationSettingState);
            }
        }

        protected static void EnsureProviderNameIsPresent(string connectionStringName, string providerName)
        {
            EnsureConfigSettingIsPresent(providerName, configurationSettingState =>
            {
                switch (configurationSettingState)
                {
                    case ConfigurationSettingState.IsWhiteSpaces:
                        return new ConfigurationErrorsException($"The value of the providerName attribute for ConnectionString setting Name: {connectionStringName} is Blank (only spaces). This setting is a Required setting");
                    case ConfigurationSettingState.IsEmpty:
                    default:
                        return new ConfigurationErrorsException($"The value of the providerName attribute for ConnectionString setting Name: {connectionStringName} is Empty. This setting is a Required setting");
                }
            });
        }

        protected static void EnsureConnectionStringIsPresent(string connectionStringName, string connectionString)
        {
            EnsureConfigSettingIsPresent(connectionString, configurationSettingState =>
            {
                switch (configurationSettingState)
                {
                    case ConfigurationSettingState.IsWhiteSpaces:
                        return new ConfigurationErrorsException($"The value of the connectionString attribute for ConnectionString setting Name: {connectionStringName} is Blank (only spaces). This setting is a Required setting");
                    case ConfigurationSettingState.IsEmpty:
                    default:
                        return new ConfigurationErrorsException($"The value of the connectionString attribute for ConnectionString setting Name: {connectionStringName} is Empty. This setting is a Required setting");
                }
            });
        }

        /// <summary>
        /// Specialized Properties can call this method in Descendant's when configuration settings
        /// are NOT optional. Decendant's can implement this method in such as way that their implementation
        /// throws an exception with the proper exception message indicating the missing configuration setting
        /// </summary>
        /// <param name="configurationSettingKey">The Configuration Setting Key</param>
        /// <returns>The Configuration Setting Value</returns>
        protected abstract string GetConfigurationSettingValueThrowIfNotFound(string configurationSettingKey);

        /// <summary>
        /// Specialized Properties can call this method in Descendant's when configuration settings are optional
        /// or have a default value when not present/specified
        /// </summary>
        /// <param name="configurationSettingKey">The Configuration Setting Key</param>
        /// <returns>The Configuration Setting Value</returns>
        protected abstract string GetConfigurationSettingValue(string configurationSettingKey);

        /// <summary>
        /// This method returns an instance of <see cref="ConnectionInformation"/> that is extracted from either
        /// the connectionStrings section of a config file or some other location.
        /// Descendants should implement this method in such as way that specialized exceptions are thrown
        /// indicating exactly what settings are missing and from where (what section of a config file or other location)
        /// </summary>
        /// <param name="connectionName">The value of the \"name\" attribute of a connectionString item in a config file or an identifier if another location is used</param>
        /// <returns>An instance of a <see cref="DbConnectionInformation"/> class that contains the ProviderinvariantName and ConnectionString properties</returns>
        protected abstract DbConnectionInformation GetDbConnectionInformationCore(string connectionStringName);
    }
}
