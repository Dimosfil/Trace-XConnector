using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnector
{
    public class FileManager
    {
        public static FileManager Instance => instance;
        private static FileManager instance;
        public static void Init()
        {
            instance = new FileManager();
        }

        public void WriteJson(string fileName, string text)
        {
            // создаем каталог для файла
            string path = @"C:\work\XConnectorXml";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            string name = fileName + DateTime.Now.ToString("s");

            name = name.Replace(':', '_');
            // запись в файл
            using (FileStream fstream = new FileStream($"{path}\\{name}.json", FileMode.OpenOrCreate))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine("Текст записан в файл");
            }
        }

        public void WriteXml(string text)
        {
            // создаем каталог для файла
            string path = @"C:\work\XConnectorXml";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            string name = "orderData_" + DateTime.Now.ToString("s");

            name = name.Replace(':', '_');
            // запись в файл
            using (FileStream fstream = new FileStream($"{path}\\{name}.txt", FileMode.OpenOrCreate))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine("Текст записан в файл");
            }
        }

        public async Task<string> ReadFileAsync(string fileName)
        {
            //string path = @"C:\work\Trace-XConnector\Trace-XConnector\OrderExport.txt";
            string path = fileName;

            string text = String.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    text = await sr.ReadToEndAsync();
                    //Console.WriteLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return text;//.Trim();
        }

        string[] statusStrings = new[] { "Rejected", "Printed" };

        public async Task<JsonOrderExportData> GetOrderExportAsync(JsonOrderData orderData)
        {
            var orderExport = new JsonOrderExportData();

            orderExport.orderId = orderData.orderId;

            var random = new Random();

            foreach (var pallet in orderData.pallets)
            {
                orderExport.data.Add(new UpdatedData()
                {
                    type = "Pallet",
                    serial = pallet,
                    attachmentType = "Case",
                    status = "Printed",//statusStrings[next],
                });
            }

            int caartonsCount = 0;
            int caseCount = 0;

            foreach (var currentPallet in orderExport.data)
            {
                if (currentPallet != null)
                {
                    for (var caseIndex = 0; caseIndex < orderData.palletCapacity; caseIndex++)
                    {
                        var @case = orderData.cases[caseCount];
                        var newCase = new UpdatedData()
                        {
                            type = "Case",
                            serial = @case,
                            attachmentType = "Carton",
                            status = "Printed",
                        };
                        currentPallet.attachment.Add(newCase);

                        if (caartonsCount < orderData.amount)
                        {
                            for (int i = 0; i < orderData.caseCapacity; i++)
                            {
                                var next = random.Next(0, statusStrings.Length);

                                var carton = orderData.cartons[caartonsCount];
                                newCase.attachment.Add(new UpdatedData()
                                {
                                    type = "Carton",
                                    serial = carton.serial,
                                    status = "Printed",//statusStrings[next],
                                });

                                caartonsCount++;
                            }
                        }

                        caseCount++;
                    }
                }
            }

            
            return orderExport;
        }

        public async Task<JsonOrderExportData> GetOrderExportAsync2(JsonOrderData orderData)
        {
            var orderExport = new JsonOrderExportData();

            orderExport.orderId = orderData.orderId;

            var random = new Random();
            
            foreach (var carton in orderData.cartons)
            {
                if (!orderExport.data.Exists(o => o.status == "Printed"))
                {
                    orderExport.data.Add(new UpdatedData()
                    {
                        type = "Carton",
                        serial = carton.serial,
                        status = "Printed",
                    });

                    continue;
                }

                var next = random.Next(0, statusStrings.Length);

                orderExport.data.Add(new UpdatedData()
                {
                    type = "Carton",
                    serial = carton.serial,
                    status = statusStrings[next],
                });
            }

            return orderExport;
        }

    }
}