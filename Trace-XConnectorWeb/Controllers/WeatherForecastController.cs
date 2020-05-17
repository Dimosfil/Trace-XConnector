using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using NLog;
using Trace_XConnectorWeb.Trace_X;

namespace Trace_XConnectorWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static bool needSave = true;
        private static bool runing = false;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IUserActionLogsSender _userActionLogsSender;
        private readonly ILogger<WeatherForecastController> _logger;
        private Logger nlogger;

        public WeatherForecastController(IUserActionLogsSender userActionLogsSender, ILogger<WeatherForecastController> logger)
        {
            _userActionLogsSender = userActionLogsSender;
            //_logger = logger;
        }

        /// GET api/EtalonByerFilials/5
        [HttpGet("{commandId}")]
        public async Task<IEnumerable<WeatherForecast>> Get(int commandId)
        {
            Program.logger.Debug($" commandId {commandId}");

            switch (commandId)
            {
                case 0: return await StartStopUpdate(false);
                case 1: return await StartStopUpdate(true);
                case 2:
                default: return await StartOnce();
            }
        }

        async Task<IEnumerable<WeatherForecast>> StartOnce()
        {
            try
            {
                string str = "Task<IEnumerable<WeatherForecast>>";
                str = "Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: STARTED!!!!";
                Program.logger.Debug(str);

                await Process(false);

                var rng = new Random();
                //_userActionLogsSender.SendInfo(new LogSend() { LogMsg = "IEnumerable<WeatherForecast> Get() rng: " });

                Program.logger.Debug("Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: ");

                //nlogger.Debug("nlogger IEnumerable<WeatherForecast>");
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
                runing = false;

                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = -1,
                    Summary = $"Exception e {e.ToString()}"
                })
                    .ToArray();
            }
        }

        async Task<IEnumerable<WeatherForecast>> StartStopUpdate(bool isStart)
        {
            try
            {
                string str = "Task<IEnumerable<WeatherForecast>>";
                runing = isStart;

                if (runing)
                {
                    runing = true;
                    str = "Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: STARTED!!!!";
                    Program.logger.Debug(str);

                    await Process(true);
                }

                var rng = new Random();
                //_userActionLogsSender.SendInfo(new LogSend() { LogMsg = "IEnumerable<WeatherForecast> Get() rng: " });

                Program.logger.Debug("Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: ");

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
                runing = false;

                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = -1,
                    Summary = $"Exception e {e.ToString()}"
                })
                    .ToArray();
            }
        }

        public async Task Process(bool isUpdate)
        {
            Program.logger.Debug("Hello World!");

            Program.logger.Debug("Init FileManager!");
            FileManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init Converter!");
            ConverterManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init LogDBManager");
            LogDBManager.Init();
            Program.logger.Debug("LogDBManager Inited!");

            Program.logger.Debug("Init HttpManager!");
            HttpManager.Init();
            Program.logger.Debug("Inited!");

            if (isUpdate)
            {
                await Update();
            }
            else
            {
                await ConvertAlgoritm();
            }

            //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
            //var xmlFromFile = ConverterManager.Instance.ConvertOrderExportToXml(jsonFromFile);

            Program.logger.Debug("Press enter to Stop !");

            LogDBManager.GetInstance().Stop();
            runing = false;

            Program.logger.Debug("Programm End!");
        }

        private int period = 5000;
        async Task Update()
        {
            Program.logger.Debug("Start new thread Update");

            while (runing)
            {
                Program.logger.Debug("Update timer");
                await ConvertAlgoritm();
                System.Threading.Thread.Sleep(period);
            }
        }

        private bool enableNetwork = false;
        async Task ConvertAlgoritm()
        {
            JsonOrderData jsonOrderData = null;
            string json = String.Empty;

            if (enableNetwork)
            {
                try
                {
                    Program.logger.Debug("Send OrderDataRequest...");
                    json = HttpManager.Instance.PostOrderDataRequest();
                    Program.logger.Debug("Response! json: " + json.Length);

                    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);
                    FileManager.Instance.WriteJson("orderData_", json);
                }
                catch (Exception e)
                {
                    Program.logger.Debug(e);
                }
            }


            try
            {

                if (jsonOrderData == null)
                {
                    jsonOrderData = await GetFromLocal();
                }

                if (jsonOrderData != null)
                {
                    string result = String.Empty;
                    if (enableNetwork)
                    {

                        Program.logger.Debug($"Send PostOrderInProductionRequest...");
                        result = HttpManager.Instance.PostOrderInProductionRequest(new OrderInProductionRequest() { orderId = jsonOrderData.orderId });
                        Program.logger.Debug($"result: {result}");
                    }

                    Program.logger.Debug("To Xml Converting...");
                    var xmlString = ConverterManager.Instance.ConvertOrderDataToXml(jsonOrderData);
                    Program.logger.Debug("Converted!");

                    Program.logger.Debug("FileManager Writing...");
                    FileManager.Instance.WriteXml(xmlString);
                    Program.logger.Debug("Writed!");

                    if (needSave)
                    {
                        Program.logger.Debug("LogDBManager send to save..");
                        LogDBManager.GetInstance().Save(new SessionInfo(DateTime.UtcNow, json, xmlString));
                        Program.logger.Debug("LogDBManager Saved!");
                    }

                    //TODO fil отправляем OrderExportData джисон из документации
                    //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
                    //var jsonFromFileObj = ConverterManager.Instance.GetJsonObject<JsonOrderExportData>(jsonFromFile);

                    JsonOrderExportData jsonOrderExportData = null;
                    if (enableNetwork)
                    {
                        jsonOrderExportData = await FileManager.Instance.GetOrderExportAsync(jsonOrderData);
                        result = HttpManager.Instance.PostOrderExportRequest(jsonOrderExportData);
                    }


                    //Console.WriteLine($"result: {JsonConvert.SerializeObject(jsonOrderExportData, Formatting.Indented)}");
                    Program.logger.Debug($"result: {result}");

                    if (jsonOrderExportData != null)
                    {
                        var jsonOrderExportDataToString = JsonConvert.SerializeObject(jsonOrderExportData);
                        FileManager.Instance.WriteJson("orderExportData_", jsonOrderExportDataToString);
                    }
                }
                else
                {
                    Program.logger.Debug("rootObj != null");
                }
            }
            catch (Exception e)
            {
                Program.logger.Debug(e);
                throw;
            }
        }

        async Task<JsonOrderData> GetFromLocal()
        {
            JsonOrderData result = null;

            var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-05-15T15_55_09.txt");
            var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
            //result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);

            if (result == null)
            {
                xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-05-15T15_55_04.json");

                result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(xmlOrderData);
            }

            return result;
        }
    }
}
