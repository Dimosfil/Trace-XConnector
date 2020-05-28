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
        public static JsonOrderData JsonOrderData = null;
        public static EPCISDocument EPCISDocument = null;

        private bool needSave = true;
        private bool runing = false;
        private static bool inProductionStarted = false;

        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly IUserActionLogsSender _userActionLogsSender;
        private readonly ILogger<ProsalexController> _logger;
        private Logger nlogger;

        public IConfiguration Configuration { get; }

        private string pathXml;
        private string pathJson;

        public ProsalexController(IUserActionLogsSender userActionLogsSender, ILogger<ProsalexController> logger, IConfiguration configuration)
        {
            _userActionLogsSender = userActionLogsSender;
            Configuration = configuration;
            //_logger = logger;

            InitProcess();
        }

        void InitProcess()
        {
            pathXml = Configuration["pathXml"];
            pathJson = Configuration["pathJson"];

            Program.logger.Debug("Init FileManager!");
            FileManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init Converter!");
            ConverterManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init LogDBManager");
            LogDBManager.Init(Configuration);
            Program.logger.Debug("LogDBManager Inited!");

            Program.logger.Debug("Init HttpManager!");
            HttpManager.Init();
            Program.logger.Debug("Inited!");
        }

        [HttpGet()]
        public IEnumerable<WeatherForecast> Get()
        {
            string str = String.Empty;
            if (JsonOrderData != null)
            {
                str = $" orderId {JsonOrderData.orderId} gtin {JsonOrderData.gtin} series {JsonOrderData.series}";
            }
            else
            {
                str = $"JsonOrderData == null";
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
            Program.logger.Debug($" commandId {commandId}");

            switch (commandId)
            {
                case -1: return RejectJsonOrderData();
                case 0: return StartStopUpdate(false);
                case 1: return StartStopUpdate(true);
                case 2:
                default: return StartOnce();
            }
        }

        //async Task<IEnumerable<WeatherForecast>> StartOnce()
        IEnumerable<WeatherForecast> StartOnce()
        {
            try
            {
                string str = "Task<IEnumerable<WeatherForecast>>";
                str = "StartOnce rng: STARTED!!!!";
                Program.logger.Debug(str);

                Process(false);

                var rng = new Random();
                //_userActionLogsSender.SendInfo(new LogSend() { LogMsg = "IEnumerable<WeatherForecast> Get() rng: " });

                Program.logger.Debug("Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: ");

                //nlogger.Debug("nlogger IEnumerable<WeatherForecast>");
                return Enumerable.Range(1, 1).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = 100,
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

        //async Task<IEnumerable<WeatherForecast>> RejectJsonOrderData()
        IEnumerable<WeatherForecast> RejectJsonOrderData()
        {
            string str = $"RejectJsonOrderData ";

            if (JsonOrderData != null)
            {
                var rejectedJsonOrderData = FileManager.Instance.Reject(JsonOrderData);

                str = $"RejectJsonOrderData rejectedJsonOrderData {rejectedJsonOrderData.data.Count}";
            }
            else
            {
                str = "RejectJsonOrderData JsonOrderData == null";
            }

            EndProcesses();

            return Enumerable.Range(1, 1).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = -1,
                Summary = str
            }).ToArray();
        }

        //async Task<IEnumerable<WeatherForecast>> StartStopUpdate(bool isStart)
        IEnumerable<WeatherForecast> StartStopUpdate(bool isStart)
        {
            try
            {
                string str = "";
                runing = isStart;

                if (runing)
                {
                    runing = true;
                    str = "Start Update STARTED!!!!";
                    Program.logger.Debug(str);

                    Process(true);
                }
                else
                {
                    str = "Stop Update";
                    Program.logger.Debug(str);

                    Stop();
                }

                var rng = new Random();
                //_userActionLogsSender.SendInfo(new LogSend() { LogMsg = "IEnumerable<WeatherForecast> Get() rng: " });

                //Program.logger.Debug("Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: ");

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

        public void Process(bool isUpdate)
        {
            Program.logger.Debug($"Process isUpdate {isUpdate}");

            if (isUpdate)
            {
                //UpdateCurrentThread?.Abort();
                //UpdateCurrentThread = null;
                Update();
            }
            else
            {
                ConvertAlgoritm();
            }
        }
        void EndProcesses()
        {
            runing = false;
            inProductionStarted = false;
            JsonOrderData = null;
        }

        void Stop()
        {
            Program.logger.Debug("EndProcesses...");

            EndProcesses();
            LogDBManager.GetInstance().Stop();
            Program.logger.Debug("Programm End!");
        }



        private int period = 5000;
        private Thread UpdateCurrentThread = null;
        void Update()
        {
            UpdateCurrentThread = System.Threading.Thread.CurrentThread;
            Program.logger.Debug($"Start new thread Update ManagedThreadId {UpdateCurrentThread.ManagedThreadId}");

            while (runing && !inProductionStarted)
            {
                Program.logger.Debug($"Update timer UpdateCurrentThread {UpdateCurrentThread.ManagedThreadId}");
                ConvertAlgoritm();

                System.Threading.Thread.Sleep(period);
            }
        }

        private bool enableNetwork = false;
        private bool enableReadFromFile = false;
        private bool enableInProduction = false;
        private bool enableOrderExportRequest = false;
        private bool enableOrderExportReading = false;
        void ConvertAlgoritm()
        {
            JsonOrderData jsonOrderData = null;
            string json = String.Empty;

            enableNetwork = Convert.ToBoolean(Configuration["enableNetwork"]);
            enableReadFromFile = Convert.ToBoolean(Configuration["enableReadFromFile"]);
            enableInProduction = Convert.ToBoolean(Configuration["enableInProduction"]);
            enableOrderExportRequest = Convert.ToBoolean(Configuration["enableOrderExportRequest"]);
            enableOrderExportReading = Convert.ToBoolean(Configuration["enableOrderExportReading"]);

            if (enableNetwork)
            {
                try
                {
                    Program.logger.Debug("Send OrderDataRequest...");
                    json = HttpManager.Instance.PostOrderDataRequest();
                    Program.logger.Debug("Response! json: " + json.Length);

                    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);

                    if (jsonOrderData == null)
                        return;

                    //var path = Configuration["pathWriteJson"];
                    //FileManager.Instance.WriteJson(path, "orderData_", json);
                }
                catch (Exception e)
                {
                    Program.logger.Error(e, "Send OrderDataRequest... Exception");
                }
            }

            try
            {

                if (jsonOrderData == null && enableReadFromFile)
                {
                    Program.logger.Debug($"ConvertAlgoritm jsonOrderData == null... loading from disc");
                    json = GetFromLocal();
                    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);
                }

                if (jsonOrderData != null)
                {
                    Program.logger.Debug($"ConvertAlgoritm jsonOrderData != null orderId: {jsonOrderData.orderId}");

                    string result = String.Empty;
                    if (enableInProduction)
                    {
                        Program.logger.Debug($"Send PostOrderInProductionRequest...");
                        result = HttpManager.Instance.PostOrderInProductionRequest(new OrderInProductionRequest() { orderId = jsonOrderData.orderId });
                        Program.logger.Debug($"result: {result}");
                    }

                    Program.logger.Debug("To Xml Converting...");
                    var xmlString = ConverterManager.Instance.ConvertOrderDataToXml(jsonOrderData);
                    Program.logger.Debug("Converted!");

                    Program.logger.Debug("FileManager Writing...");
                    var path = Configuration["pathWriteJson"];
                    FileManager.Instance.WriteXml(path, xmlString);
                    Program.logger.Debug("Writed!");

                    if (needSave)
                    {
                        Program.logger.Debug("LogDBManager send to save..");
                        LogDBManager.GetInstance().Save(new SessionInfo(DateTime.UtcNow, json, xmlString));
                        Program.logger.Debug("LogDBManager Saved!");
                    }

                    JsonOrderData = jsonOrderData;

                    inProductionStarted = true;
                    runing = false;

                    if (watcher != null)
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher = null;
                    }

                    StartWatching();
                }
                else
                {
                    Program.logger.Debug("rootObj != null");
                }
            }
            catch (Exception e)
            {
                Program.logger.Error(e, $"Task ConvertAlgoritm()");
                throw;
            }
        }
        void PostOrderExport()//(JsonOrderData jsonOrderData, EPCISDocument orderExportXmlData)
        {
            var path = Configuration["pathWriteJson"];
            //TODO fil отправляем OrderExportData джисон из документации
            //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
            //var jsonFromFileObj = ConverterManager.Instance.GetJsonObject<JsonOrderExportData>(jsonFromFile);

            string result = String.Empty;
            JsonOrderExportData jsonOrderExportData = null;

            jsonOrderExportData = FileManager.Instance.GetOrderExportAsync(JsonOrderData, EPCISDocument);

            if (jsonOrderExportData == null)
            {
                Program.logger.Error($"jsonOrderExportData == null");
                return;
            }

            var jsonOrderExportDataToString = JsonConvert.SerializeObject(jsonOrderExportData);
            FileManager.Instance.WriteJson(path, "orderExportData_", jsonOrderExportDataToString);

            if (enableOrderExportRequest)
            {
                result = HttpManager.Instance.PostOrderExportRequest(jsonOrderExportData);
                Program.logger.Debug($"PostOrderExport PostOrderExportRequest result: {result}");
            }

            Program.logger.Debug($"PostOrderExport  orderId: {jsonOrderExportData.orderId} jsonOrderExportData {jsonOrderExportData.data.Count}");

            //System.IO.File.Replace(orderExportXmlFile.FullPath, oldXmlPath, orderExportXmlFile.Name);

            orderExportXmlFile = null;

            inProductionStarted = false;
            runing = true;

            //StartStopUpdate(true);
        }

        static private FileSystemWatcher watcher;
        void StartWatching()
        {
            using (watcher = new FileSystemWatcher())
            {
                watcher.Path = Configuration["orderExportXmlPath"];

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                       | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName;

                // Only watch text files.
                //watcher.Filter = "*.txt";

                // Add event handlers.
                watcher.Created += OnCreated;
                //watcher. += OnCreated;
                watcher.Deleted += OnDeleted;
                //watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                Program.logger.Debug("StartWatching FileSystemWatcher Started");

                while (inProductionStarted)
                {
                    //Program.logger.Debug($"Watching... ManagedThreadId {Thread.CurrentThread.ManagedThreadId}");

                    System.Threading.Thread.Sleep(period);
                }
            }
        }

        private OrderExportXmlFile orderExportXmlFile = null;

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            inProductionStarted = false;
            watcher = null;
            try
            {
                // Specify what is done when a file is changed, created, or deleted.
                Program.logger.Debug($"OnCreated File: {e.FullPath} {e.ChangeType}");
                if (orderExportXmlFile == null)
                {
                    orderExportXmlFile = new OrderExportXmlFile();
                    orderExportXmlFile.FullPath = e.FullPath;
                    orderExportXmlFile.Name = e.Name;

                    var xmlString = FileManager.Instance.ReadFile(orderExportXmlFile.FullPath);

                    Program.logger.Debug($"OnCreated ReadedFile xml {xmlString.Length}");

                    EPCISDocument result;

                    var serializer = new XmlSerializer(typeof(EPCISDocument));

                    using (var stream = new StringReader(xmlString))
                    using (var reader = XmlReader.Create(stream))
                    {
                        result = (EPCISDocument)serializer.Deserialize(reader);
                        stream.Close();
                    }

                    Program.logger.Debug($"OnCreated orderExportXmlFile Deserialized result result.EPCISBody.EventList.Items.Length {result.EPCISBody.EventList.Items.Length}");

                    EPCISDocument = result;

                    PostOrderExport();
                }
                else
                {
                    Program.logger.Error($"OnCreated Error File: {e.FullPath} {e.ChangeType}");
                }
            }
            catch (Exception exception)
            {

                Program.logger.Error($"OnCreated Exception {exception}");
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Program.logger.Debug($"OnDeleted File: {e.FullPath} {e.ChangeType}");
            if (orderExportXmlFile != null)
            {
                orderExportXmlFile = null;
            }
            else
            {
                Program.logger.Error($"OnDeleted Error File: {e.FullPath} {e.ChangeType}");
            }
        }

        string GetFromLocal()
        {
            var json = String.Empty;
            //var xmlOrderData = await FileManager.Instance.ReadFileAsync(pathXml);
            //var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
            //result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);

            json = FileManager.Instance.ReadFile(pathJson);

            Program.logger.Error($"GetFromLocal JsonOrderData Success Count {json.Length}");

            return json;
        }

        //async Task<string> GetFromLocalAsync()
        //{
        //    var json = String.Empty;
        //    //var xmlOrderData = await FileManager.Instance.ReadFileAsync(pathXml);
        //    //var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
        //    //result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);

        //    json = await FileManager.Instance.ReadFileAsync(pathJson);

        //    Program.logger.Error($"GetFromLocal JsonOrderData Success Count {json.Length}");

        //    return json;
        //}


        public void Dispose()
        {
            Stop();

            GC.SuppressFinalize(this);
        }
    }

    public class OrderExportXmlFile
    {
        public string FullPath { get; set; }
        public string Name { get; set; }
    }
}
