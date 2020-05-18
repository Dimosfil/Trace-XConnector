using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Trace_XConnectorWeb.Trace_X;

namespace Trace_XConnectorWeb.Controllers
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class UidRequestsController : ControllerBase
    {

        public UidRequestsController(IUserActionLogsSender userActionLogsSender, ILogger<UidRequestsController> logger)
        {
            //_userActionLogsSender = userActionLogsSender;
            //_logger = logger;
        }

        /// GET api/EtalonByerFilials/5
        [HttpGet("{commandId}")]
        public IEnumerable<WeatherForecast> Get(bool isSync, string format)
        {
            var str = $" isSync {isSync} format {format}";

            Program.logger.Debug(str);

            //switch (commandId)
            //{
            //    case 0: return await StartStopUpdate(false);
            //    case 1: return await StartStopUpdate(true);
            //    case 2:
            //    default: return await StartOnce();
            //}

            return Enumerable.Range(1, 1).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = 0,
                Summary = str
            })
                .ToArray();
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(bool isSync, string format, [FromBody] SGTINBody body)
        {
            try
            {
                var str = $" isSync {isSync} format {format} body {body.Name}";

                Program.logger.Debug(str);

                JsonOrderData jsonOrderData = await GetFromLocal();
                string json = String.Empty;
                if (format.Contains("CRPT"))
                {
                    //ЗАПРОС SGTIN от LineMaster
                    var firstUrnPart = GetURNSGTIN(body.UIDRequestTypeKey);
                    json = GetSGTIN(jsonOrderData, firstUrnPart);
                }
                else if (format.Contains("DetailURI"))
                {
                    if (body.UIDRequestTypeKey.Contains("1+"))
                    {
                        //ЗАПРОС SSCC для коробок от LineMaster для коробок
                        var typeKey = body.UIDRequestTypeKey.Remove(0, 2);
                        var firstUrnPart = GetURNSSCC(typeKey);
                        json = GetSSCC(jsonOrderData.cases, firstUrnPart);

                    }
                    else if(body.UIDRequestTypeKey.Contains("2+"))
                    {
                        //ЗАПРОС SSCC для коробок от LineMaster для палет
                        var typeKey = body.UIDRequestTypeKey.Remove(0, 2);
                        var firstUrnPart = GetURNSSCC(typeKey);
                        json = GetSSCC(jsonOrderData.pallets, firstUrnPart);
                    }
                }

                var mediaTypes = new MediaTypeCollection();
                mediaTypes.Add("application/json");
                mediaTypes.Add("charset=utf-8");
                var result = new ObjectResult(json)
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ContentTypes = mediaTypes
                };

                //Response.Headers.Add("Content-type", "application/json;charset=utf-8");

                //Program.logger.Debug(JsonConvert.SerializeObject(result, Formatting.Indented));

                return result;
            }
            catch (Exception e)
            {
                Program.logger.Error(e.ToString);

                return BadRequest($"UidRequestsController PostAsync Exception {e.ToString()}");
            }
        }

        async Task<JsonOrderData> GetFromLocal()
        {
            JsonOrderData result = null;

            //var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-05-15T15_55_09.txt");
            //var orderDataJsonString = ConverterManager.Instance.ConvertXmlToJson(xmlOrderData);
            //result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(orderDataJsonString);

            if (result == null)
            {
                var xmlOrderData = await FileManager.Instance.ReadFileAsync("c:\\work\\XConnectorXml\\orderData_2020-05-15T15_55_04.json");

                result = ConverterManager.Instance.GetJsonObject<JsonOrderData>(xmlOrderData);
            }

            return result;
        }


        public string GetSGTIN(JsonOrderData jsonOrderData, string urnKey)
        {
            string result;
            var sgtin = new SGTIN();
            foreach (var carton in jsonOrderData.cartons)
            {
                var urn = urnKey + carton.serial;
                var item = new UID()
                {
                    URN = urn,
                    CryptoKey = carton.f91,
                    CryptoCode = carton.f92
                };
                sgtin.UIDs.Add(item);
            }

            sgtin.Quantity = jsonOrderData.cartons.Count.ToString();

            result = JsonConvert.SerializeObject(sgtin);
            return result;
        }
        //"UIDRequestTypeKey": "04607035391972_CRPT",
        string GetURNSGTIN(string requestTypeKey)
        {
            var result = "urn:epc:id:sgtin:";

            requestTypeKey = requestTypeKey.Replace("_CRPT", string.Empty);
            requestTypeKey = requestTypeKey.Remove(0, 1);
            requestTypeKey = requestTypeKey.Insert(7, ".");

            result = result + requestTypeKey + ".";

            return result;
        }

        public string GetSSCC(List<string> items, string urnKey)
        {
            string result;
            var sscc = new SSCC();
            foreach (var item in items)
            {
                var urn = urnKey + item.Remove(0, 8);
                sscc.UIDs.Add(urn);
            }

            sscc.Quantity = items.Count.ToString();

            result = JsonConvert.SerializeObject(sscc);
            return result;
        }
        
        string GetURNSSCC(string requestTypeKey)
        {
            var result = "urn:epc:id:sscc:";
            //requestTypeKey = requestTypeKey.Insert(7, ".");
            result = result + requestTypeKey + ".";

            return result;
        }
    }

    public class SGTINBody
    {
        public string UIDRequestTypeKey { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
    }

    public class SGTIN
    {
        public List<UID> UIDs { get; set; } = new List<UID>();
        public Location Location = new Location();
        public string Quantity { get; set; }
        public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
        public string CodeRequestKey { get; set; }
    }

    public class Location
    {
        public List<string> References { get; set; } = new List<string>();
        public string Name { get; set; } = "NPO Petrovax";
    }

    public class UID
    {
        public string URN { get; set; } = String.Empty;
        public string CryptoKey { get; set; } = String.Empty;
        public string CryptoCode { get; set; } = String.Empty;

    }

    public class SSCC
    {
        public List<string> UIDs { get; set; } = new List<string>();
        public Location Location = new Location();
        public string Quantity { get; set; }
        public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
        public string CodeRequestKey { get; set; }
    }
}