using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationProviderNetFramework
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            var configurationProvider = new ConfigurationProvider();
            Console.WriteLine(configurationProvider.EmailTemplatesPath);
            Console.WriteLine(configurationProvider.PaymentGatewayServiceUrl);
            Console.WriteLine(configurationProvider.FiscalYearStart.ToLongDateString());
            Console.WriteLine(configurationProvider.NotifyOnUpload);

            var dbConnectionInformation = configurationProvider.DbConnectionInformation;
            Console.WriteLine(dbConnectionInformation.ConnectionStringName);
            Console.WriteLine(dbConnectionInformation.ConnectionString);
            Console.WriteLine(dbConnectionInformation.ProviderName);
        }
    }
}
