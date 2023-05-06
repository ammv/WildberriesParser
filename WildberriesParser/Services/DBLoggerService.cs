using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Model;
using DataLayer;

namespace WildberriesParser.Services
{
    public class DBLoggerService : ILoggerService
    {
        public void AddLog(string message = null, LogTypeEnum logType = LogTypeEnum.COMMON)
        {
            try
            {
                DBEntities.GetContext()?.Log.Add(new Log
                {
                    User = App.CurrentUser,
                    Description = message,
                    Type = (int)logType,
                    Date = DateTime.Now
                });
                DBEntities.GetContext()?.SaveChanges();
            }
            catch
            {
            }
        }
    }
}