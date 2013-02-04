using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public static class ConnectionStringHelper
    {
        public static string GetSqlCeConnectionString(string fileName)
        {
            var csBuilder = new EntityConnectionStringBuilder();

            csBuilder.Provider = "System.Data.SqlServerCe.3.5";
            csBuilder.ProviderConnectionString = string.Format("Data Source={0};", fileName);

            csBuilder.Metadata = string.Format("res://{0}/Model1.csdl|res://{0}/Model1.ssdl|res://{0}/Model1.msl",
                typeof(Model1Container).Assembly.FullName);

            return csBuilder.ToString();
        }

        public static string GetSqlConnectionString(string serverName, string databaseName)
        {
            SqlConnectionStringBuilder providerCs = new SqlConnectionStringBuilder();

            providerCs.DataSource = serverName;
            providerCs.InitialCatalog = databaseName;
            providerCs.IntegratedSecurity = true;

            var csBuilder = new EntityConnectionStringBuilder();

            csBuilder.Provider = "System.Data.SqlClient";
            csBuilder.ProviderConnectionString = providerCs.ToString();

            csBuilder.Metadata = string.Format("res://{0}/Model1.csdl|res://{0}/Model1.ssdl|res://{0}/Model1.msl",
                typeof(Model1Container).Assembly.FullName);

            return csBuilder.ToString();
        }
    }
}
