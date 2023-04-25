using System;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Threading.Tasks;
using WildberriesParser.Model.Data;

namespace WildberriesParser.Helpers
{
    internal enum SQLProvider
    {
        MSSQL,
        MySQL
    }

    public struct CheckResult
    {
        public bool Result;
        public Exception Exception;

        public CheckResult(bool result, Exception exception)
        {
            Result = result;
            Exception = exception;
        }
    }

    internal class DBHelper
    {
        public static bool ServerIsAvailableEF(string connectionString, out Exception exception)
        {
            try
            {
                DBEntities.SetContext(connectionString);
                DBEntities.GetContext().Database.Connection.Open();
                DBEntities.GetContext().Database.Connection.Close();
                exception = null;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return exception == null;
        }

        public static async Task<CheckResult> ServerIsAvailableSql(string connectionString)
        {
            CheckResult checkResult = new CheckResult();
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                await conn.OpenAsync();
                conn.Close();
                checkResult.Result = true;
            }
            catch (Exception ex)
            {
                checkResult.Exception = ex;
                checkResult.Result = false;
            }
            return checkResult;
        }

        public static string CreateConnectionString(string server, string databaseName, string userName, string password)
        {
            bool windowsAuth = userName == string.Empty && password == string.Empty;

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server, // server address
                InitialCatalog = databaseName, // database name
                IntegratedSecurity = windowsAuth, // server auth(false)/win auth(true)
                MultipleActiveResultSets = true, // activate/deactivate MARS
                PersistSecurityInfo = windowsAuth, // hide login credentials
                UserID = userName, // user name
                Password = userName, // password
                ApplicationName = "EntityFramework" // app name
            };
            return builder.ConnectionString;
        }

        public static string CreateEFConnectionString(string connectionString, SQLProvider provider = SQLProvider.MSSQL)
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