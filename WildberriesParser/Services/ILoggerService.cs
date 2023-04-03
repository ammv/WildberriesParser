using WildberriesParser.Model;

namespace WildberriesParser.Services
{
    public interface ILoggerService
    {
        void AddLog(string message = null, LogTypeEnum logType = LogTypeEnum.COMMON);
    }
}