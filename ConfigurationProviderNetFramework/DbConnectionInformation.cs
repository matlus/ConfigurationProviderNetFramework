using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationProviderNetFramework
{
    /// <summary>
    /// This class holds ADO.NET Database connection specific information
    /// </summary>
    internal sealed class DbConnectionInformation
    {
        public string ConnectionStringName { get; }
        public string ConnectionString { get; }
        public string ProviderInvariantName { get; }

        public DbConnectionInformation(string connectionStringName, string connectionString, string providerInvariantName)
        {
            ConnectionStringName = connectionStringName;
            ConnectionString = connectionString;
            ProviderInvariantName = providerInvariantName;
        }
    }
}
