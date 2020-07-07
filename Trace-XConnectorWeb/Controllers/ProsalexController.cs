using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using NLog;
using Trace_XConnectorWeb.Trace_X;

namespace Trace_XConnectorWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProsalexController : ControllerBase//, IDisposable
    {
        //private readonly ConsumeScopedServiceHostedService _scopedProcessingService;
        public IConfiguration Configuration { get; }

        public ProsalexController(IUserActionLogsSender userActionLogsSender, ILogger<ProsalexController> logger, IConfiguration configuration)//, ConsumeScopedServiceHostedService scopedProcessingService)
        {
            //_scopedProcessingService = scopedProcessingService;
            //_userActionLogsSender = userActionLogsSender;
            Configuration = configuration;
        }

        [HttpGet()]
        public IEnumerable<WeatherForecast> Get()
        {
            string str = String.Empty;
            if (Prosalex.JsonOrderData != null)
            {
                str = $"Ордер дата orderId {Prosalex.JsonOrderData.orderId} gtin {Prosalex.JsonOrderData.gtin} series {Prosalex.JsonOrderData.series}";
            }
            else
            {
                str = $"Ордер дата отсутствует (JsonOrderData == null)";
            }
            Program.logger.Debug("Get Info ");

            //nlogger.Debug("nlogger IEnumerable<WeatherForecast>");
            return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = 100,
                    Summary = str
                })
                .ToArray();
        }


        /// GET api/EtalonByerFilials/5
        [HttpGet("{commandId}")]
        //public async Task<IEnumerable<WeatherForecast>> Get(int commandId)
        public IEnumerable<WeatherForecast> Get(int commandId)
        {
            Program.logger.Debug($"Get commandId {commandId} ManagedThreadId {Thread.CurrentThread.ManagedThreadId}");

            string str = String.Empty;
            switch (commandId)
            {
                case -3: Prosalex.Instance.SendMailAsync("case -3: Prosalex.Instance"); break;
                case -2: Prosalex.Instance.InitProcess();
                    str = $"Сервис проинициализирован для запуска нажмите Старт";
                    break;
                case -1: return RejectJsonOrderData();
                case 0: return StartStopUpdate(false);
                case 1: return StartStopUpdate(true);
                case 2:
                default: return null;
            }

            return Enumerable.Range(1, 1).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = -1,
                Summary = str
            }).ToArray();
        }

        private bool enableRejectJsonOrderData = false;
        IEnumerable<WeatherForecast> RejectJsonOrderData()
        {
            string str = $"Сброс ордер даты (RejectJsonOrderData)";

            Program.logger.Debug(str);

            var requestResult = string.Empty;
            enableRejectJsonOrderData = Convert.ToBoolean(Configuration["enableRejectJsonOrderData"]);
            
            if (Prosalex.JsonOrderData != null)
            {
                var rejectedJsonOrderData = FileManager.Instance.Reject(Prosalex.JsonOrderData);

                var fullRejectInfo = Convert.ToBoolean(Configuration["enableFullRejectInfo"]);
                if(fullRejectInfo)
                    Program.logger.Debug($"Reject rejectedJsonOrderData data: {JsonConvert.SerializeObject(rejectedJsonOrderData)}");

                if (enableRejectJsonOrderData)
                {
                    Program.logger.Debug($"Send Reject network request");
                    requestResult = HttpManager.Instance.PostOrderExportRequest(rejectedJsonOrderData);
                    Program.logger.Debug($"Reject response {requestResult}");

                    str = $"Запрос на сброс даты (request): orderId: {rejectedJsonOrderData.orderId}  Count: {rejectedJsonOrderData?.data?.Count}" +
                          $"\nСброс ордер даты: результат сброса RejectJsonOrderData requestResult: {requestResult}";
                }
            }
            else
            {
                str = "Сброс ордер даты: Ордер дата не найдена (RejectJsonOrderData not found)";
            }

            Program.logger.Debug(str);

            var isRestart = Convert.ToBoolean(Configuration["enableRestartAfterReject"]);
            if(isRestart)
                RestartServise();

            return Enumerable.Range(1, 1).Select(index => new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = -1,
                Summary = str + " Перезапустите сервис - Stop Init Start (Service restarted)"
            }).ToArray();
        }

        void RestartServise()
        {
            Prosalex.Instance.Restart();
        }

        //async Task<IEnumerable<WeatherForecast>> StartStopUpdate(bool isStart)
        IEnumerable<WeatherForecast> StartStopUpdate(bool isStart)
        {
            try
            {
                var str = Prosalex.Instance.StartStopUpdate(isStart);

                var rng = new Random();

                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = str
                })
                    .ToArray();
            }
            catch (Exception e)
            {
                Prosalex.Instance.Stop();
                Prosalex.Instance.SendMailAsync($"Exception e {e.ToString()}");

                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = -1,
                    Summary = $"Exception e {e.ToString()}"
                })
                    .ToArray();
            }
        }
    }

    public class OrderExportXmlFile
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
    }
}
