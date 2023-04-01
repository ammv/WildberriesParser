using System;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace WildberriesParser.Helpers
{
    internal enum SQLProvider
    {
        MSSQL,
        MySQL
    }

    internal class DBHelper
    {
        public static bool CheckConnectionString(string connectionString, out Exception exception)
        {
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                conn.Close();
                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static string CreateConnectionString(string server, string databaseName, string userName, string password)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server, // server address
                InitialCatalog = databaseName, // database name
                IntegratedSecurity = false, // server auth(false)/win auth(true)
                MultipleActiveResultSets = false, // activate/deactivate MARS
                PersistSecurityInfo = true, // hide login credentials
                UserID = userName, // user name
                Password = password // password
            };
            return builder.ConnectionString;
        }

        public static string GetEntityConnectionString(string connectionString, SQLProvider provider = SQLProvider.MSSQL)
        {
            var entityBuilder = new EntityConnectionStringBuilder();

            if (provider == SQLProvider.MSSQL)
            {
                // WARNING
                // Check app config and set the appropriate DBModel
                entityBuilder.Provider = "System.Data.SqlClient";
            }
            else if (provider == SQLProvider.MySQL)
            {
                entityBuilder.Provider = "MySql.Data.MySqlClient";
            }

            entityBuilder.ProviderConnectionString = connectionString + ";App=EntityFramework;";
            entityBuilder.Metadata = @"res://*/Model.Data.DBModel.csdl|res://*/Model.Data.DBModel.ssdl|res://*/Model.Data.DBModel.msl";

            return entityBuilder.ToString();
        }
    }
}