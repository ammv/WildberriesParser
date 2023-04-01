using System.Data.Entity;

namespace WildberriesParser.Model.Data
{
    public partial class DBEntities : DbContext
    {
        private static DBEntities context;

        public static void SetContext(string connectionString)
        {
            context = new DBEntities(connectionString);
        }

        public static bool CheckConnectionString(string connectionString)
        {
            try
            {
                var dbContext = new DBEntities(connectionString);
                dbContext.Database.Connection.Open();
                dbContext.Database.Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DBEntities GetContext()
        {
            if (context == null)
            {
                context = new DBEntities(Properties.Settings.Default.ConnectionString);
            }
            return context;
        }
    }
}