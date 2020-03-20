using System;
using System.Threading;

namespace Trace_XConnector
{
    class Program
    {
        private static bool needSave = true;
        private static bool runing = false;
        static void Main(string[] args)
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

            Console.WriteLine("Init FileManager!");
            FileManager.Init();
            Console.WriteLine("Inited!");

            runing = true;
            Thread thread = new Thread(Update);
            thread.Start();

            Console.WriteLine("Press enter to Stop !");

            Console.ReadLine();
            LogDBManager.GetInstance().Stop();
            runing = false;

            Console.WriteLine("Programm End!");
        }
        
        private static int period = 5000;
        static void Update()
        {
            Console.WriteLine("Start new thread Update");
            
            while (runing)
            {
                Console.WriteLine("Update timer");
                ConvertAlgoritm();
                System.Threading.Thread.Sleep(period);
            }
        }

        static void ConvertAlgoritm()
        {
            Console.WriteLine("SendRequest...");
            var json = HttpManager.Instance.GetJsonRequest();
            Console.WriteLine("Response! json: " + json.Length);

            Console.WriteLine("Converting...");
            var xmlString = ConverterManager.Instance.ConvertToXml(json);
            Console.WriteLine("Converted!");

            Console.WriteLine("FileManager Writing...");
            FileManager.Instance.WriteXml(xmlString);
            Console.WriteLine("Writed");

            if (needSave)
            {
                Console.WriteLine("LogDBManager send to save..");
                LogDBManager.GetInstance().Save(new SessionInfo(DateTime.UtcNow, json, xmlString));
                Console.WriteLine("LogDBManager Saved!");
            }
        }
    }
}
