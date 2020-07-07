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
                        sr.Close();
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
                        Program.logger.Debug("Can't read file {0}. File is locked. Try #" + rptCnt, path);
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

        public JsonOrderExportData GetOrderExport(JsonOrderData orderData, EPCISDocument epcisDocument)
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

            Program.logger.Debug($"addedObjectEventList orderData.pallets {orderData.pallets.Count}");

            foreach (var pallet in orderData.pallets)
            {
                foreach (var objectEvent in addedObjectEventList)
                {
                    if (objectEvent.epcList == null)
                        continue;

                    var sscc = objectEvent.epcList.FirstOrDefault(e =>
                    {
                        if (!e.Contains("urn:epc:id:sscc:"))
                            return false;

                        //Program.logger.Debug($"Add Pallet SSCCForOrderDataFormat {ssccForOrderDataFormat} pallet {pallet} e {e}");
                        var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(e);
                        return pallet.Contains(ssccForOrderDataFormat);
                    });

                    if (sscc != null)
                    {
                        var updateData = new UpdatedData()
                        {
                            type = "Pallet",
                            serial = pallet,
                            attachmentType = "Case",
                            status = "Printed", //statusStrings[next],
                        };

                        allAddedItemsUpdateData.Add(updateData);

                        Program.logger.Debug($"Added Pallets  SSCCForOrderDataFormat {JsonConvert.SerializeObject(updateData)}");
                    }
                }
            }

            //Program.logger.Debug($"addedObjectEventList orderData.cases {orderData.cases.Count}");

            foreach (var dataCase in orderData.cases)
            {
                foreach (var objectEvent in addedObjectEventList)
                {
                    if (objectEvent.epcList == null)
                        continue;

                    var sscc = objectEvent.epcList.FirstOrDefault(e =>
                    {
                        if (!e.Contains("urn:epc:id:sscc:"))
                            return false;

                        var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(e);
                        //Program.logger.Debug($"Add Cases SSCCForOrderDataFormat {ssccForOrderDataFormat} Case {dataCase} e {e}");

                        return dataCase.Contains(ssccForOrderDataFormat);
                    });

                    if (sscc != null)
                    {
                        var newCase = new UpdatedData()
                        {
                            type = "Case",
                            serial = dataCase,
                            attachmentType = "Carton",
                            status = "Printed",
                        };

                        allAddedItemsUpdateData.Add(newCase);
                        Program.logger.Debug($"Added Cases Case {JsonConvert.SerializeObject(newCase)}");
                    }
                }
            }

            //Program.logger.Debug($"addedObjectEventList orderData.cartons {orderData.cartons.Count}");
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

            Program.logger.Debug($"addedObjectEventList For Printed {allAddedItemsUpdateData.Count}");

            foreach (var pallet in orderData.pallets)
            {
                var sscc = rejectedObjectEventList.epcList?.FirstOrDefault(e =>
                {
                    if (e.Contains("urn:epc:id:sscc:"))
                    {
                        var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(e);
                        //Program.logger.Debug($"Rejected Pallets SSCCForOrderDataFormat {ssccForOrderDataFormat} pallet {pallet} e {e}");

                        return pallet.Contains(ssccForOrderDataFormat);
                    }
                    return false;
                });

                if (sscc != null)
                {
                    var updateData = new UpdatedData()
                    {
                        type = "Pallet",
                        serial = pallet,
                        attachmentType = "Case",
                        status = "Rejected", //statusStrings[next],
                    };

                    allRejectedItemsUpdateData.Add(updateData);

                    Program.logger.Debug($"Rejected Pallets pallet {JsonConvert.SerializeObject(updateData)} pallet");
                }
            }

            foreach (var dataCase in orderData.cases)
            {
                var sscc = rejectedObjectEventList.epcList?.FirstOrDefault(e =>
                {
                    if (e.Contains("urn:epc:id:sscc:"))
                    {
                        var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(e);
                        //Program.logger.Debug($"Rejected Cases SSCCForOrderDataFormat {ssccForOrderDataFormat} Case {dataCase} e {e}");

                        return dataCase.Contains(ssccForOrderDataFormat);
                    }
                        
                    return false;
                });

                if (sscc != null)
                {
                    var newCase = new UpdatedData()
                    {
                        type = "Case",
                        serial = dataCase,
                        attachmentType = "Carton",
                        status = "Rejected",
                    };

                    allRejectedItemsUpdateData.Add(newCase);

                    Program.logger.Debug($"Rejected Cases Case {JsonConvert.SerializeObject(newCase)}");
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

            Program.logger.Debug($" Rejected allRejectedItemsUpdateData Printed {allRejectedItemsUpdateData.Count}");

            foreach (var aggregationEvent in aggregationEventList)
            {
                if (aggregationEvent.extension.name == "PACKAGING_LEVEL" && aggregationEvent.extension.Value == "4")
                {
                    var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(aggregationEvent.parentID);
                    var pallet = allAddedItemsUpdateData.FirstOrDefault(i =>
                    {
                        if (i.type == "Pallet")
                        {
                            Program.logger.Debug($"Pallet SSCCForOrderDataFormat {ssccForOrderDataFormat}, i.serial {i.serial}");
                            return i.serial.Contains(ssccForOrderDataFormat);
                        }
                        return false;
                    });

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
                        var SSCCForOrderDataFormat = GetSSCCForOrderDataFormat(childEpC);
                        var sscc = allAddedItemsUpdateData.FirstOrDefault(i =>
                        {
                            return i.serial.Contains(SSCCForOrderDataFormat);
                        });
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
                    var sscc = allAddedItemsUpdateData.FirstOrDefault(i =>
                    {
                        var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(aggregationEvent.parentID);
                        if (i.type == "Case")
                        {
                            Program.logger.Debug($"Case SSCCForOrderDataFormat {ssccForOrderDataFormat}, i.serial {i.serial}");
                            return i.serial.Contains(ssccForOrderDataFormat);
                        }

                        return false;
                    });
                    if (sscc == null)
                    {
                        foreach (var data in orderExport.data)
                        {
                            sscc = data.attachment.FirstOrDefault(i =>
                            {
                                var ssccForOrderDataFormat = GetSSCCForOrderDataFormat(parentID);
                                Program.logger.Debug($"Case SSCCForOrderDataFormat {ssccForOrderDataFormat}, i.serial {i.serial}");

                                return i.serial.Contains(ssccForOrderDataFormat);
                            });
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

        //коробка из ордерДаты - 146070353974584071
        //отправка лайн мастер -  4607035.1397458407
        //получаю в ордер хмл -   4607035.1397458407
        //конвертирую для ордерЭкспортДжисон   14607035397458407 + надо вычислить 1 символ

        private string GetSSCCForOrderDataFormat(string sscc)
        {
            string result = String.Empty;
            if (!sscc.Contains("urn:epc:id:sscc:"))
                return result;

            result = sscc.Replace("urn:epc:id:sscc:", String.Empty);
            var ids = result.Split('.');
            var digit = ids[1][0];
            ids[1] = ids[1].Remove(0, 1);
            result = digit + ids[0] + ids[1];

            //Program.logger.Debug($"GetSSCCForOrderDataFormat sscc {sscc} result {result}");

            return result;
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

            foreach (var @case in orderData.cases)
            {
                orderExport.data.Add(new UpdatedData()
                {
                    type = "Case",
                    serial = @case,
                    attachmentType = "Carton",
                    status = "Rejected",
                });
            }

            foreach (var carton in orderData.cartons)
            {
                orderExport.data.Add(new UpdatedData()
                {
                    type = "Carton",
                    serial = carton.serial,
                    //status = "Printed",//statusStrings[next],
                    status = "Rejected",
                });
            }


            //int caartonsCount = 0;
            //int caseCount = 0;

            //foreach (var currentPallet in orderExport.data)
            //{
            //    if (currentPallet != null)
            //    {
            //        for (var caseIndex = 0; caseIndex < orderData.palletCapacity; caseIndex++)
            //        {
            //            var @case = orderData.cases[caseCount];
            //            var newCase = new UpdatedData()
            //            {
            //                type = "Case",
            //                serial = @case,
            //                attachmentType = "Carton",
            //                status = "Rejected",
            //            };
            //            currentPallet.attachment.Add(newCase);

            //            if (caartonsCount < orderData.amount)
            //            {
            //                for (int i = 0; i < orderData.caseCapacity; i++)
            //                {
            //                    var next = random.Next(0, statusStrings.Length);

            //                    var carton = orderData.cartons[caartonsCount];
            //                    newCase.attachment.Add(new UpdatedData()
            //                    {
            //                        type = "Carton",
            //                        serial = carton.serial,
            //                        //status = "Printed",//statusStrings[next],
            //                        status = "Rejected",
            //                    });

            //                    caartonsCount++;
            //                }
            //            }

            //            caseCount++;
            //        }
            //    }
            //}

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
