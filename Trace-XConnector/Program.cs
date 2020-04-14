using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Trace_XConnector
{
    class Program
    {
        private static bool needSave = true;
        private static bool runing = false;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Console.WriteLine("Init FileManager!");
            FileManager.Init();
            Console.WriteLine("Inited");

            Console.WriteLine("Init Converter!");
            ConverterManager.Init();
            Console.WriteLine("Inited");

            Console.WriteLine("Init LogDBManager");
            LogDBManager.Init();
            Console.WriteLine("LogDBManager Inited!");

            Console.WriteLine("Init HttpManager!");
            HttpManager.Init();
            Console.WriteLine("Inited!");

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

            Console.WriteLine("Press enter to Stop !");

            Console.ReadLine();
            LogDBManager.GetInstance().Stop();
            runing = false;

            Console.WriteLine("Programm End!");
        }

        private static int period = 5000;
        static async Task Update()
        {
            Console.WriteLine("Start new thread Update");

            while (runing)
            {
                Console.WriteLine("Update timer");
                await ConvertAlgoritm();
                System.Threading.Thread.Sleep(period);
            }
        }

        static async Task ConvertAlgoritm()
        {
            try
            {
                Console.WriteLine("Send OrderDataRequest...");
                var json = HttpManager.Instance.PostOrderDataRequest();
                Console.WriteLine("Response! json: " + json.Length);

                JsonOrderData jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(json);

                //if (jsonOrderData == null)
                //{
                //    var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-04-14T14_39_28.txt");
                //    var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
                //    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);
                //}

                if (jsonOrderData == null)
                {
                    var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-04-14T14_39_28.json");
                    
                    jsonOrderData = ConverterManager.Instance.GetJsonObject<JsonOrderData>(xmlOrderData);
                }


                if (jsonOrderData != null)
                {
                    Console.WriteLine($"Send PostOrderInProductionRequest...");
                    var result = HttpManager.Instance.PostOrderInProductionRequest(new OrderInProductionRequest() { orderId = jsonOrderData.orderId });
                    Console.WriteLine($"result: {result}");

                    Console.WriteLine("To Xml Converting...");
                    var xmlString = ConverterManager.Instance.ConvertOrderDataToXml(jsonOrderData);
                    Console.WriteLine("Converted!");

                    Console.WriteLine("FileManager Writing...");
                    FileManager.Instance.WriteXml(xmlString);
                    Console.WriteLine("Writed!");

                    if (needSave)
                    {
                        Console.WriteLine("LogDBManager send to save..");
                        LogDBManager.GetInstance().Save(new SessionInfo(DateTime.UtcNow, json, xmlString));
                        Console.WriteLine("LogDBManager Saved!");
                    }

                    //TODO fil отправляем OrderExportData джисон из документации
                    //var jsonFromFile = await FileManager.Instance.ReadFileAsync();
                    //var jsonFromFileObj = ConverterManager.Instance.GetJsonObject<JsonOrderExportData>(jsonFromFile);

                    var jsonOrderExportData = await FileManager.Instance.GetOrderExportAsync(jsonOrderData);
                    //result = HttpManager.Instance.PostOrderExportRequest(jsonOrderExportData);
                    
                    Console.WriteLine($"result: {JsonConvert.SerializeObject(jsonOrderExportData, Formatting.Indented)}");
                    //Console.WriteLine($"result: {result}");
                    Console.ReadLine();

                    var jsonOrderExportDataToString = JsonConvert.SerializeObject(jsonOrderExportData);
                    FileManager.Instance.WriteJson(jsonOrderExportDataToString);

                }
                else
                {
                    Console.WriteLine("rootObj != null");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
