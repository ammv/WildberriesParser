﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WildberriesParser.Model;
using WildberriesParser.Model.Data;

namespace WildberriesParser.Services
{
    public interface ILoggerService
    {
    }

    public class DBLoggerService : ILoggerService
    {
        public void AddLog(string message, LogTypeEnum logType = LogTypeEnum.COMMON)
        {
            DBEntities.GetContext().Log.Add(new Log
            {
                UserID = App.CurrentUser.ID,
                Description = message,
                TypeID = (int)logType,
                Date = DateTime.Now
            });
        }
    }
}