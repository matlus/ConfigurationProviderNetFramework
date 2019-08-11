using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationProviderNetFramework
{
    internal sealed class ConfigurationProvider : ConfigurationProviderBase
    {
        protected override string GetConfigurationSettingValue(string configurationSettingKey)
        {
            return ConfigurationManager.AppSettings[configurationSettingKey];
        }

        protected override string GetConfigurationSettingValueThrowIfNotFound(string configurationSettingKey)
        {
            var valueAsConfigured = GetConfigurationSettingValue(configurationSettingKey);

            EnsureConfigSettingIsPresent(valueAsConfigured, configurationSettingState =>
            {
                switch (configurationSettingState)
                {
                    case ConfigurationSettingState.IsNull:
                        return new ConfigurationErrorsException($"The AppSettings Key: {configurationSettingKey} is Missing in the configuration file. This setting is a Required setting");
                    case ConfigurationSettingState.IsWhiteSpaces:
                        return new ConfigurationErrorsException($"The value of AppSettings Key: {configurationSettingKey} in the configuration file is Blank (only spaces). This setting is a Required setting");
                    case ConfigurationSettingState.IsEmpty:
                    default:
                        return new ConfigurationErrorsException($"The value of AppSettings Key: {configurationSettingKey} in the configuration file is Empty. This setting is a Required setting");
                }
            });

            return valueAsConfigured;
        }

        protected override DbConnectionInformation GetDbConnectionInformationCore(string connectionStringName)
        {
            var connectionStringSection = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (connectionStringSection == null)
            {
                throw new ConfigurationErrorsException($"The ConnectionString setting with the Name: {connectionStringName} is Missing in the configuration file. This setting is a Required setting");
            }

            var connectionString = connectionStringSection.ConnectionString;
            EnsureConnectionStringIsPresent(connectionStringName, connectionString);

            var providerName = connectionStringSection.ProviderName;
            EnsureProviderNameIsPresent(connectionStringName, providerName);

            return new DbConnectionInformation(connectionStringName, connectionString, connectionStringSection.ProviderName);
        }
    }    
}
