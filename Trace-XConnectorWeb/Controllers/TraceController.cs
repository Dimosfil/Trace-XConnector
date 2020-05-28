using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Trace_XConnectorWeb.Trace_X;

namespace Trace_XConnectorWeb.Controllers
{
    public class TraceController : ControllerBase
    {
        private static bool needSave = true;
        private static bool runing = false;

        [HttpGet]
        public async Task<string> Get(int isStart)
        {
            try
            {
                //if (isStart == 0)
                //{
                //    runing = false;
                //}
                //else
                //{
                //    runing = true;
                //    await Process();
                //}

                var str = "Program.logger.Debug IEnumerable<WeatherForecast> Get() rng: STARTED!!!!";
                Program.logger.Debug(str);
                return str;
            }
            catch (Exception e)
            {
                return e.ToString();

            }

        }

        public async Task Process()
        {
            Program.logger.Debug("Hello World!");

            Program.logger.Debug("Init FileManager!");
            FileManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init Converter!");
            ConverterManager.Init();
            Program.logger.Debug("Inited");

            Program.logger.Debug("Init LogDBManager");
            //LogDBManager.Init();
            Program.logger.Debug("LogDBManager Inited!");

            Program.logger.Debug("Init HttpManager!");
            HttpManager.Init();
            Program.logger.Debug("Inited!");

            //var name = Enum.GetName(typeof(Finish), "Finish2");
            //var name = Enum.Parse<Finish>("Finish4");
            //var value = (int)name;
            //Console.WriteLine($"Finish name {name} value {value}");

            runing = true;
            //Thread thread = new Thread(Update);
            //thread.Start();

            await Update();

            //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
            //var xmlFromFile = ConverterManager.Instance.ConvertOrderExportToXml(jsonFromFile);

            Program.logger.Debug("Press enter to Stop !");

            Console.ReadLine();
            LogDBManager.GetInstance().Stop();
            runing = false;

            Program.logger.Debug("Programm End!");
        }

        private static int period = 5000;
        static async Task Update()
        {
            Program.logger.Debug("Start new thread Update");

            while (runing)
            {
                Program.logger.Debug("Update timer");
                await ConvertAlgoritm();
                System.Threading.Thread.Sleep(period);
            }
        }

        static async Task ConvertAlgoritm()
        {
            try
            {
                Program.logger.Debug("Send OrderDataRequest...");
                var json = HttpManager.Instance.PostOrderDataRequest();
                Program.logger.Debug("Response! json: " + json.Length);

                JsonOrderData jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);

                FileManager.Instance.WriteJson(String.Empty, "orderData_", json);

                //if (jsonOrderData == null)
                //{
                //    var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-04-14T14_39_28.txt");
                //    var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
                //    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);
                //}

                //if (jsonOrderData == null)
                //{
                //    var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-04-14T17_49_54.json");

                //    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(xmlOrderData);
                //}


                if (jsonOrderData != null)
                {
                    Program.logger.Debug($"Send PostOrderInProductionRequest...");
                    var result = HttpManager.Instance.PostOrderInProductionRequest(new OrderInProductionRequest() { orderId = jsonOrderData.orderId });
                    Program.logger.Debug($"result: {result}");

                    Program.logger.Debug("To Xml Converting...");
                    var xmlString = ConverterManager.Instance.ConvertOrderDataToXml(jsonOrderData);
                    Program.logger.Debug("Converted!");

                    Program.logger.Debug("FileManager Writing...");
                    FileManager.Instance.WriteXml(String.Empty, xmlString);
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

                    //var jsonOrderExportData = await FileManager.Instance.GetOrderExportAsync(jsonOrderData);
                    //result = HttpManager.Instance.PostOrderExportRequest(jsonOrderExportData);

                    ////Console.WriteLine($"result: {JsonConvert.SerializeObject(jsonOrderExportData, Formatting.Indented)}");
                    //Program.logger.Debug($"result: {result}");
                    //Console.ReadLine();

                    //var jsonOrderExportDataToString = JsonConvert.SerializeObject(jsonOrderExportData);
                    //FileManager.Instance.WriteJson(string.Empty, "orderExportData_", jsonOrderExportDataToString);

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
    }
}
