using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Trace_XConnectorWeb.Trace_X;

namespace Trace_XConnectorWeb.Controllers
{
    public class Prosalex : IDisposable
    {
        private static Prosalex _instance;
        public static Prosalex Instance => _instance;

        public static JsonOrderData JsonOrderData = null;
        public static EPCISDocument EPCISDocument = null;

        private static bool needSave = true;
        private static bool runing = false;
        private static bool inProductionStarted = false;

        private string pathXml;
        private string pathJson;

        public IConfiguration Configuration { get; }

        public Prosalex(IConfiguration configuration)
        {
            _instance = this;
            Configuration = configuration;
        }

        public void InitProcess()
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

        public string StartStopUpdate(bool isStart)
        {
            string str = "";
            runing = isStart;

            if (runing)
            {
                runing = true;
                period = 5000;

                str = "Сервис успешно стартовал";
                Program.logger.Debug(str);

                Task.Run(() => Process(true), CancellationToken.None);//.Wait();
            }
            else
            {
                str = "Сервис остановлен";
                Program.logger.Debug(str);

                Stop();
            }


            return str;
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
        //private bool enableReadFromFileJSON = false;
        //private bool enableReadFromFileXML = false;
        private bool enableInProduction = false;
        private bool enableOrderExportRequest = false;
        private bool enableCanDeleteOrderXml = false;
        //private bool enableOrderExportReading = false;
        void ConvertAlgoritm()
        {
            JsonOrderData jsonOrderData = null;
            string json = String.Empty;

            enableNetwork = Convert.ToBoolean(Configuration["enableNetwork"]);
            //enableReadFromFileJSON = Convert.ToBoolean(Configuration["enableReadFromFileJSON"]);
            //enableReadFromFileXML = Convert.ToBoolean(Configuration["enableReadFromFileXML"]);
            enableInProduction = Convert.ToBoolean(Configuration["enableInProduction"]);
            enableOrderExportRequest = Convert.ToBoolean(Configuration["enableOrderExportRequest"]);
            //enableOrderExportReading = Convert.ToBoolean(Configuration["enableOrderExportReading"]);

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
                    SendMailAsync($"Send OrderDataRequest... Exception {e.ToString()}");
                }
            }
            else
            {
                //if (enableReadFromFileJSON)
                {
                    Program.logger.Debug($"ConvertAlgoritm jsonOrderData == null... loading JSON from disc");
                    json = GetFromLocalJson();
                    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);
                }
            }

            try
            {

                
                //if (jsonOrderData == null && enableReadFromFileXML)
                //{
                //    Program.logger.Debug($"ConvertAlgoritm jsonOrderData == null... loading XML from disc");
                //    jsonOrderData = GetFromLocalXml();
                //}

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

                    Program.logger.Debug("FileManager Writing XML...");
                    var path = Configuration["pathWriteJson"];
                    FileManager.Instance.WriteXml(path, xmlString);
                    Program.logger.Debug("Writed!");

                    Program.logger.Debug("FileManager Writing JSON...");
                    pathJson = Configuration["pathWriteJson"];
                    FileManager.Instance.WriteJson(pathJson, "orderData_", json);
                    Program.logger.Debug("Writed!");

                    if (needSave)
                    {
                        Program.logger.Debug("LogDBManager send to save..");
                        LogDBManager.GetInstance().Save(new SessionInfo(DateTime.Now, json, xmlString));
                        Program.logger.Debug("LogDBManager Saved!");
                    }

                    JsonOrderData = jsonOrderData;
                    Helper.JsonOrderData = Helper.Clone(Prosalex.JsonOrderData);// await GetFromLocal();

                    Program.logger.Debug($"ConvertAlgoritm  -- Helper.JsonOrderData {Helper.JsonOrderData.series} JsonOrderData {JsonOrderData.series}");

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
                    Program.logger.Debug("rootObj == null");
                }
            }
            catch (Exception e)
            {
                Program.logger.Error(e, $"Task ConvertAlgoritm()");

                SendMailAsync($"Task ConvertAlgoritm() Exception {e.ToString()}");

                //Restart();
                throw;
            }
        }

        void PostOrderExport()//(JsonOrderData jsonOrderData, EPCISDocument orderExportXmlData)
        {
            var path = Configuration["pathWriteJson"];
            enableCanDeleteOrderXml = Convert.ToBoolean(Configuration["enableCanDeleteOrderXml"]);
            //TODO fil отправляем OrderExportData джисон из документации
            //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
            //var jsonFromFileObj = ConverterManager.Instance.GetJsonObject<JsonOrderExportData>(jsonFromFile);

            string result = String.Empty;
            JsonOrderExportData jsonOrderExportData = null;

            jsonOrderExportData = FileManager.Instance.GetOrderExport(JsonOrderData, EPCISDocument);

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

            if(enableCanDeleteOrderXml)
                System.IO.File.Delete(orderExportXmlFile.FullPath);

            orderExportXmlFile = null;

            inProductionStarted = false;
            runing = true;

            JsonOrderData = null;

            Helper.Release();

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
                watcher.Filter = "*.xml";

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
                    Program.logger.Debug($"Watching... ManagedThreadId {Thread.CurrentThread.ManagedThreadId} inProductionStarted {inProductionStarted}");

                    System.Threading.Thread.Sleep(period);
                }

                Program.logger.Debug($"EndWatching FileSystemWatcher ManagedThreadId {Thread.CurrentThread.ManagedThreadId} inProductionStarted {inProductionStarted}");
            }
        }

        static private OrderExportXmlFile orderExportXmlFile = null;

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            inProductionStarted = false;
            watcher = null;
            try
            {
                // Specify what is done when a file is changed, created, or deleted.
                Program.logger.Debug($"OnCreated File: {e.FullPath} {e.ChangeType}");

                orderExportXmlFile = new OrderExportXmlFile();
                orderExportXmlFile.FullPath = e.FullPath;
                orderExportXmlFile.Name = e.Name;

                if (!orderExportXmlFile.Name.Contains(JsonOrderData.series))
                {
                    Program.logger.Error($"OnCreated orderExportXmlFile not valid Name {orderExportXmlFile.Name} orderData series {JsonOrderData.series}");
                    return;
                }

                var xmlString = FileManager.Instance.ReadFile(orderExportXmlFile.FullPath);

                Program.logger.Debug($"OnCreated ReadedFile xml {xmlString.Length}");

                EPCISDocument result;

                Program.logger.Debug($"OnCreated Start serialize xml {xmlString.Length}");

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
            catch (Exception exception)
            {
                Program.logger.Error($"OnCreated Exception {exception}");

                SendMailAsync($"OnCreated Exception  {exception.ToString()}");

                //Restart();
            }
        }

        private void OnDeleted(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Program.logger.Debug($"OnDeleted File: {e.FullPath} {e.ChangeType}");
            if (orderExportXmlFile != null)
            {
                Program.logger.Debug($"orderExport OnDeleted orderExportXmlFile = null");

                orderExportXmlFile = null;
            }
            else
            {
                Program.logger.Error($"OnDeleted Error File: {e.FullPath} {e.ChangeType}");
            }
        }

        string GetFromLocalJson()
        {
            var json = String.Empty;
            json = FileManager.Instance.ReadFile(pathJson);

            Program.logger.Error($"GetFromLocal JsonOrderData Success Count {json.Length}");

            return json;
        }

        JsonOrderData GetFromLocalXml()
        {
            var xmlOrderData = FileManager.Instance.ReadFile(pathXml);
            var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);

            Program.logger.Debug("FileManager Writing JSON...");
            pathJson = Configuration["pathWriteJson"];
            FileManager.Instance.WriteJson(pathJson, "orderData_", orderDataJsonString);
            Program.logger.Debug("Writed!");

            JsonOrderData result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);
            return result;
        }

        public void Restart()
        {
            Stop();
            InitProcess();
            StartStopUpdate(true);
        }


        public void Stop()
        {
            Program.logger.Debug("EndProcesses...");

            EndProcesses();
            LogDBManager.GetInstance().Stop();
            Program.logger.Debug($"Programm End! JsonOrderData {JsonOrderData} Helper.JsonOrderData {Helper.JsonOrderData}");

            SendMailAsync($"Programm End! JsonOrderData {JsonOrderData} Helper.JsonOrderData {Helper.JsonOrderData}");
        }
        void EndProcesses()
        {
            runing = false;
            period = 10;
            inProductionStarted = false;
            JsonOrderData = null;
            orderExportXmlFile = null;
            Helper.Release();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public void SendMailAsync(string message)
        {
            var enableEmail = Convert.ToBoolean(Configuration["enableEmailSend"]);

            if (!enableEmail)
                return;

            if (string.IsNullOrEmpty(message))
            {
                message = "HttpManager.Instance.SendMailAsync string.IsNullOrEmpty(message)";
            }

            Program.logger.Debug($"SendMailAsync message {message}");
            HttpManager.Instance.SendMailAsync(message);
        }
    }
}
