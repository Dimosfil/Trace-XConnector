using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Trace_XConnectorWeb.Trace_X
{
    public class FileManager
    {
        public static FileManager Instance => instance;
        private static FileManager instance;
        public static void Init()
        {
            if (Instance == null)
                instance = new FileManager();
        }

        public void WriteJson(string path, string fileName, string text)
        {
            // создаем каталог для файла
            //path = @"C:\work\XConnectorXml";
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

        public void WriteXml(string path, string text)
        {
            // создаем каталог для файла
            //string path = @"C:\work\XConnectorXml";
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

        public string ReadFile(string fileName)
        {
            //string path = @"C:\work\Trace-XConnector\Trace-XConnector\OrderExport.txt";
            string path = fileName;

            string text = String.Empty;
            //try
            //{
                
            //}
            //catch (Exception e)
            //{
            //    Program.logger.Error(e.ToString());
            //    return text;
            //}

            bool isLocked = true;
            int rptCnt = 0;
            do
            {
                try
                {
                    //using (var file = new StreamReader(fileName, Encoding.GetEncoding(1251)))
                    //{
                        
                    //}

                    using (StreamReader sr = new StreamReader(path))
                    {
                        text = sr.ReadToEnd();
                        //Console.WriteLine();
                        //return text;
                    }

                    isLocked = false;
                }
                catch (IOException e)
                {
                    if (rptCnt >= 10)
                        throw;

                    var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                    isLocked = errorCode == 32 || errorCode == 33;
                    if (isLocked)
                    {
                        Trace.TraceError("Can't read file {0}. File is locked. Try #" + rptCnt, path);
                        Thread.Sleep(1000);
                        rptCnt++;
                    }
                    else
                        throw;
                }
            } while (isLocked);


            return text;
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
                    return text;
                }
            }
            catch (Exception e)
            {
                Program.logger.Error(e.ToString());
                return text;
            }
        }

        string[] statusStrings = new[] { "Rejected", "Printed" };

        public JsonOrderExportData GetOrderExportAsync(JsonOrderData orderData, EPCISDocument epcisDocument)
        {
            var orderExport = new JsonOrderExportData();

            orderExport.orderId = orderData.orderId;

            var objectEventList = new List<EPCISBodyEventListObjectEvent>();
            var aggregationEventList = new List<EPCISBodyEventListAggregationEvent>();

            bool isCorrectEPCISDocument = false;

            foreach (var item in epcisDocument.EPCISBody.EventList.Items)
            {
                if(item == null)
                    continue;

                if (item is EPCISBodyEventListObjectEvent objectEvent)
                {
                    objectEventList.Add(objectEvent);
                    continue;
                }

                if (item is EPCISBodyEventListAggregationEvent aggregationEvent)
                {
                    aggregationEventList.Add(aggregationEvent);
                    continue;
                }
            }

            var addedObjectEventList = objectEventList.FindAll(a =>
                a.action == "ADD");

            var rejectedObjectEventList = objectEventList.Find(a =>
                a.action == "DELETE");


            if (!CheckForCorrectEPCISDocument(addedObjectEventList, orderData))
            {
                return null;
            }

            var allAddedItemsUpdateData = new List<UpdatedData>();
            var allRejectedItemsUpdateData = new List<UpdatedData>();

            foreach (var pallet in orderData.pallets)
            {
                foreach (var objectEvent in addedObjectEventList)
                {
                    if (objectEvent.epcList == null)
                        continue;
                    
                    if (objectEvent.epcList.FirstOrDefault(e => e.Contains(pallet)) != null)
                    {
                        var updateData = new UpdatedData()
                        {
                            type = "Pallet",
                            serial = pallet,
                            attachmentType = "Case",
                            status = "Printed", //statusStrings[next],
                        };

                        allAddedItemsUpdateData.Add(updateData);
                    }
                }
            }

            foreach (var dataCase in orderData.cases)
            {
                foreach (var objectEvent in addedObjectEventList)
                {
                    if (objectEvent.epcList == null)
                        continue;

                    if (objectEvent.epcList.FirstOrDefault(e => e.Contains(dataCase)) != null)
                    {
                        var newCase = new UpdatedData()
                        {
                            type = "Case",
                            serial = dataCase,
                            attachmentType = "Carton",
                            status = "Printed",
                        };

                        allAddedItemsUpdateData.Add(newCase);
                    }
                }
            }

            foreach (var carton in orderData.cartons)
            {
                foreach (var objectEvent in addedObjectEventList)
                {
                    if (objectEvent.epcList == null)
                        continue;

                    if (objectEvent.epcList.FirstOrDefault(e => e.Contains(carton.serial)) != null)
                    {
                        var newCarton = new UpdatedData()
                        {
                            type = "Carton",
                            serial = carton.serial,
                            status = "Printed",
                        };

                        allAddedItemsUpdateData.Add(newCarton);
                    }
                }
            }

            foreach (var pallet in orderData.pallets)
            {
                if (rejectedObjectEventList.epcList?.FirstOrDefault(e => e.Contains(pallet)) != null)
                {
                    var updateData = new UpdatedData()
                    {
                        type = "Pallet",
                        serial = pallet,
                        attachmentType = "Case",
                        status = "Rejected", //statusStrings[next],
                    };

                    allRejectedItemsUpdateData.Add(updateData);
                }
            }

            foreach (var dataCase in orderData.cases)
            {
                if (rejectedObjectEventList.epcList?.FirstOrDefault(e => e.Contains(dataCase)) != null)
                {
                    var newCase = new UpdatedData()
                    {
                        type = "Case",
                        serial = dataCase,
                        attachmentType = "Carton",
                        status = "Rejected",
                    };

                    allRejectedItemsUpdateData.Add(newCase);
                }
            }

            foreach (var carton in orderData.cartons)
            {
                if (rejectedObjectEventList.epcList?.FirstOrDefault(e => e.Contains(carton.serial)) != null)
                {
                    var newCarton = new UpdatedData()
                    {
                        type = "Carton",
                        serial = carton.serial,
                        status = "Rejected",//statusStrings[next],
                    };

                    allRejectedItemsUpdateData.Add(newCarton);
                }
            }

            foreach (var aggregationEvent in aggregationEventList)
            {
                if (aggregationEvent.extension.name == "PACKAGING_LEVEL" && aggregationEvent.extension.Value == "4")
                {
                    var pallet = allAddedItemsUpdateData.FirstOrDefault(i => aggregationEvent.parentID.Contains(i.serial));

                    if (pallet == null)
                    {
                        Program.logger.Error(
                            $"pallet == null aggregationEvent {JsonConvert.SerializeObject(aggregationEvent)}");
                        continue;
                    }

                    orderExport.data.Add(pallet);
                    allAddedItemsUpdateData.Remove(pallet);

                    foreach (var childEpC in aggregationEvent.childEPCs)
                    {
                        var sscc = allAddedItemsUpdateData.FirstOrDefault(i => childEpC.Contains(i.serial));
                        pallet.attachment.Add(sscc);
                        allAddedItemsUpdateData.Remove(sscc);
                    }
                }
            }

            foreach (var aggregationEvent in aggregationEventList)
            {
                if (aggregationEvent.extension.name == "PACKAGING_LEVEL" && aggregationEvent.extension.Value == "3")
                {
                    var parentID = aggregationEvent.parentID;
                    var sscc = allAddedItemsUpdateData.FirstOrDefault(i => parentID.Contains(i.serial));
                    if (sscc == null)
                    {
                        foreach (var data in orderExport.data)
                        {
                            sscc = data.attachment.FirstOrDefault(i => parentID.Contains(i.serial));
                            if(sscc != null)
                                break;
                        }
                    }

                    if (sscc == null)
                    {
                        Program.logger.Error($"sscc == null aggregationEvent {JsonConvert.SerializeObject(aggregationEvent)}");
                        continue;
                    }
                    orderExport.data.Add(sscc);
                    allAddedItemsUpdateData.Remove(sscc);

                    foreach (var childEpC in aggregationEvent.childEPCs)
                    {
                        var sgtin = allAddedItemsUpdateData.FirstOrDefault(i => childEpC.Contains(i.serial));
                        sscc.attachment.Add(sgtin);
                        allAddedItemsUpdateData.Remove(sgtin);
                    }
                }
            }

            foreach (var updatedData in allAddedItemsUpdateData)
            {
                orderExport.data.Add(updatedData);
            }

            foreach (var updatedData in allRejectedItemsUpdateData)
            {
                orderExport.data.Add(updatedData);
            }

            return orderExport;
        }

        private bool CheckForCorrectEPCISDocument(List<EPCISBodyEventListObjectEvent> addedObjectEventList,
            JsonOrderData orderData)
        {
            Program.logger.Error($"CheckForCorrectEPCISDocument addedObjectEventList.Count {addedObjectEventList.Count}");

            foreach (var objectEvent in addedObjectEventList)
            {
                var extension = objectEvent.extension.FirstOrDefault(e => e.name == "PRODUCT_ID_LEVEL_1");
                if (extension != null && extension.Value == orderData.gtin)
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        //public async Task<JsonOrderExportData> Reject(JsonOrderData orderData)
        public JsonOrderExportData Reject(JsonOrderData orderData)
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
                    status = "Rejected",
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
                            status = "Rejected",
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
                                    //status = "Printed",//statusStrings[next],
                                    status = "Rejected",
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

        public XElement LoadXmlFromFile(string path)
        {
            //XElement booksFromFile = XElement.Load(@"books.xml");
            XElement booksFromFile = XElement.Load(path);

            return booksFromFile;
        }

    }
}
