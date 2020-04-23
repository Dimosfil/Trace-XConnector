using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Trace_XConnectorWeb
{
    public interface IUserActionLogsSender
    {
        void SendInfo(LogSend log);
        void SendError(LogSend log);
        void SendCriticalError(LogSend log);
    }
    public class LogSend
    {
        public LogSend() { }
        //public int OrgId { get; set; }
        public string LogMsg { get; set; } = String.Empty;
        
    }
    public class UserActionLogsSender : IUserActionLogsSender
    {
        public static UserActionLogsSender Instance;

        private readonly ILogger<UserActionLogsSender> _logger;

        //public LogControllerId LogControllerId;

        public UserActionLogsSender(ILogger<UserActionLogsSender> logger)
        {
            _logger = logger;
        }

        public void SendInfo(LogSend log)
        {
            //log.LogMsg.LogObjectId = log.LogObjectId;
            _logger.LogInformation("Info logging {LogMsg}",
                log.LogMsg);
        }

        public void SendError(LogSend log)
        {
            //log.LogMsg.LogObjectId = log.LogObjectId;
            //_logger.LogInformation("Info Error {OrgId}{LogActionId}{LogObjectId}{UserId}{LogMsg}",
            //    log.OrgId, (int)LogActionId.Error, (int)LogControllerId, log.UserId, GetJson(log.LogMsg));
        }

        public void SendCriticalError(LogSend log)
        {
            //log.LogMsg.LogObjectId = log.LogObjectId;
            //_logger.LogInformation("Info CriticalError {OrgId}{LogActionId}{LogControllerId}{UserId}{LogMsg}",
            //    log.OrgId, (int)LogActionId.CriticalError, (int)LogControllerId, log.UserId, GetJson(log.LogMsg));
        }

        private string GetJson(JsonInfo jsonInfo)
        {
            if (jsonInfo != null)
            {
                return JsonConvert.SerializeObject(jsonInfo);
            }

            return String.Empty;
        }
    }

    public class JsonInfo
    {
        public string OldInfo { get; set; } = String.Empty;
        public string NewInfo { get; set; } = String.Empty;
        public string ErrorInfo { get; set; } = String.Empty;
    }
}