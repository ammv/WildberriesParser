using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildberriesParser.Model
{
    public enum LogTypeEnum
    {
        COMMON = 1,
        AUTH_USER,
        EXIT_USER,
        CREATE_USER,
        AUTH_USER_TRY = 5,
        FIND_PRODUCTS = 10,
        CHANGE_DB_SETTINGS,
        DELETE_USER,
        LOAD_UPDATE,
    }
}