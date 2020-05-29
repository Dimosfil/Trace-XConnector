using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
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
        public IConfiguration Configuration { get; }

        private string pathXml;
        private string pathJson;

        public UidRequestsController(IUserActionLogsSender userActionLogsSender, ILogger<UidRequestsController> logger, IConfiguration configuration)
        {
            //_userActionLogsSender = userActionLogsSender;
            //_logger = logger;
            Configuration = configuration;

            pathXml = Configuration["pathXml"];
            pathJson = Configuration["pathJson"];

        }

        /// GET api/EtalonByerFilials/5
        [HttpGet("{commandId}")]
        public IEnumerable<WeatherForecast> Get(bool isSync, string format)
        {
            var str = $"REQUEST Get isSync {isSync} format {format}";

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

        private JsonOrderData jsonOrderData;

        [HttpPost]
        public IActionResult PostAsync(bool isSync, string format, [FromBody] object bodyJson)
        {

            try
            {
                SGTINBody body = JsonConvert.DeserializeObject<SGTINBody>(bodyJson.ToString());

                var str = $"REQUEST PostAsync isSync {isSync} format {format} body {body.Name}";

                Program.logger.Debug(str);

                if (Helper.JsonOrderData == null)
                {
                    Helper.JsonOrderData = Helper.Clone(Prosalex.JsonOrderData);// await GetFromLocal();
                }

                jsonOrderData = Helper.JsonOrderData;


                string json = String.Empty;
                var shotKey = GetRequestTypeKeyShot(body.UIDRequestTypeKey);
                if (format.Contains("CRPT"))
                {
                    //ЗАПРОС SGTIN от LineMaster
                    var firstUrnPart = GetURNSGTIN(body.UIDRequestTypeKey);
                    json = GetSGTIN(jsonOrderData, firstUrnPart, shotKey, body.Quantity);

                    Program.logger.Debug($"UidRequestsController CRPT запрос SGTIN от LineMaster json {json}");
                    Program.logger.Debug($"UidRequestsController GetSGTIN cartons.Count left {jsonOrderData.cartons.Count}");
                }
                else if (format.Contains("DetailURI"))
                {
                    if (body.UIDRequestTypeKey.Contains("1+"))
                    {
                        //ЗАПРОС SSCC для коробок от LineMaster для коробок
                        var typeKey = body.UIDRequestTypeKey.Remove(0, 2);
                        var firstUrnPart = GetURNSSCC(typeKey);
                        json = GetSSCC(jsonOrderData.cases, firstUrnPart, typeKey, body.Quantity, 1);
                        Program.logger.Debug($"RESPONSE UidRequestsController DetailURI ЗАПРОС SSCC для коробок от LineMaster для коробок json {json}");
                        Program.logger.Debug($"UidRequestsController GetSSCC cases left {jsonOrderData.cases.Count} left");
                    }
                    else if (body.UIDRequestTypeKey.Contains("2+"))
                    {
                        //ЗАПРОС SSCC для коробок от LineMaster для палет
                        var typeKey = body.UIDRequestTypeKey.Remove(0, 2);
                        var firstUrnPart = GetURNSSCC(typeKey);
                        json = GetSSCC(jsonOrderData.pallets, firstUrnPart, typeKey, body.Quantity, 2);
                        Program.logger.Debug($"RESPONSE UidRequestsController ЗАПРОС SSCC для коробок от LineMaster для палет json {json}");
                        Program.logger.Debug($"UidRequestsController GetSSCC pallets left {jsonOrderData.pallets.Count} left");
                    }
                }

                var response = Content(json);
                response.ContentType = "application/json;charset=utf-8";
                response.StatusCode = (int)HttpStatusCode.OK;

                return response;

                //var mediaTypes = new MediaTypeCollection();
                //mediaTypes.Add("application/json");
                ////mediaTypes.Add("charset=utf-8");
                //var result = new ObjectResult(json)
                //{
                //    StatusCode = (int)HttpStatusCode.OK,
                //    ContentTypes = mediaTypes,                    

                //};

                //Response.Headers.Add("Content-type", "application/json;charset=utf-8");

                //Program.logger.Debug(JsonConvert.SerializeObject(result, Formatting.Indented));

                //return result;
            }
            catch (Exception e)
            {
                Program.logger.Error(e, "UidRequestsController PostAsync Exception ");

                var mediaTypes = new MediaTypeCollection();
                mediaTypes.Add("application/json");
                mediaTypes.Add("charset=utf-8");
                var result = new ObjectResult(e.ToString())
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    ContentTypes = mediaTypes
                };


                return result;// BadRequest($"UidRequestsController PostAsync Exception {e.ToString()}");
            }
        }

        string GetRequestTypeKeyShot(string requestTypeKey)
        {
            requestTypeKey = requestTypeKey.Replace("_CRPT", string.Empty);
            requestTypeKey = requestTypeKey.Remove(0, 1);
            requestTypeKey = requestTypeKey.Remove(7);
            requestTypeKey = requestTypeKey.Insert(7, ".");

            return requestTypeKey;
        }

        //"UIDRequestTypeKey": "04607035391972_CRPT",
        string GetURNSGTIN(string requestTypeKey)
        {
            requestTypeKey = requestTypeKey.Replace("_CRPT", string.Empty);
            requestTypeKey = requestTypeKey.Remove(0, 1);
            requestTypeKey = requestTypeKey.Insert(7, ".");

            var result = "urn:epc:id:sgtin:";
            result = result + requestTypeKey + ".";
            return result;
        }

        public string GetSGTIN(JsonOrderData jsonOrderData, string firstUrnPart, string urnKey, string bodyQuantity)
        {
            string result;
            var sgtin = new SGTIN();

            var currentQuantity = 0;
            var quantity = Convert.ToInt32(bodyQuantity);
            foreach (var carton in jsonOrderData.cartons)
            {
                if(currentQuantity >= quantity)
                    break;

                var urn = firstUrnPart + carton.serial;
                var item = new UID()
                {
                    Urn = urn,
                    CryptoKey = carton.f91,
                    CryptoCode = carton.f92
                };
                sgtin.UIDs.Add(item);

                currentQuantity++;
            }

            jsonOrderData.cartons.RemoveAll(c => sgtin.UIDs.Exists(u => u.CryptoCode == c.f92));

            sgtin.Location.References.Add(GetSGLN(urnKey));
            sgtin.Location.Name = Name;

            //sgtin.Quantity = jsonOrderData.cartons.Count.ToString();
            sgtin.Quantity = sgtin.UIDs.Count.ToString();

            var associatedProduct = new AssociatedProduct()
            {
                Type = "GTIN",
                Value = GetGTINValue(urnKey),
            };
            sgtin.AssociatedProducts.Add(associatedProduct);

            result = JsonConvert.SerializeObject(sgtin);
            return result;
        }

        private const string sglnConst = "39999.2";
        private const string sgtinConst = "391972";
        private const string Name = "NPO Petrovax";
        private const string CompanyPrefix = "0614141";
        string GetSGLN(string shortKey)
        {
            var result = "urn:epc:id:sgln:";
            result = result + shortKey + sglnConst;
            return result;
        }

        string GetGTINValue(string shortKey)
        {
            var result = "urn:epc:idpat:sgtin:";
            result = result + shortKey + sgtinConst + ".*";
            return result;
        }

        public string GetSSCC(List<string> items, string urnKey, string typeKey, string bodyQuantity, int digit)
        {
            string result;
            var currentQuantity = 0;
            var needQuantity = Convert.ToInt32(bodyQuantity);
            var itemsForDel = new List<string>();
            var sscc = new SSCC();
            foreach (var item in items)
            {
                if(currentQuantity >= needQuantity)
                    break;
                var urn = urnKey + GetSSCCItem(item, digit);// item.Remove(0, 8);
                sscc.UIDs.Add(urn);
                itemsForDel.Add(item);
                currentQuantity++;
            }

            items.RemoveAll(i => itemsForDel.Exists(d => d == i));

            sscc.Location.References.Add(GetLocationSGLN(typeKey));
            sscc.Location.Name = "NPO Petrovax";

            //sscc.Quantity = items.Count.ToString();
            sscc.Quantity = sscc.UIDs.Count.ToString();
            sscc.ExtensionDigit = digit.ToString();
            sscc.CompanyPrefix = CompanyPrefix;

            result = JsonConvert.SerializeObject(sscc);
            return result;
        }

        string GetSSCCItem(string item, int digit)
        {
            string result = String.Empty;
            item = item.Remove(0, 8);
            item = item.Remove(9);
            result = digit.ToString() + item;

            return result;
        }

        string GetLocationSGLN(string shortKey)
        {
            var result = "urn:epc:id:sgln:";
            result = result + shortKey + "." + sglnConst;
            return result;
        }

        string GetAssociatedProductsSGTIN(string shortKey)
        {
            var result = "urn:epc:idpat:sgtin:";
            result = result + shortKey + sgtinConst + ".*";
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
        public string LocationUri { get; set; }
        public string Quantity { get; set; }
        public List<BusinessTransaction> BusinessTransactions = new List<BusinessTransaction>();
        public string DeliveryName { get; set; }
    }

    public class BusinessTransaction
    {
        public string BizTransactionValue { get; set; }
        public string BizTransactionTypeName { get; set; }
    }

    public class SGTIN
    {
        public List<UID> UIDs { get; set; } = new List<UID>();
        public Location Location = new Location();
        public List<string> BusinessTransactions = new List<string>();
        public string UIDTypeName = null;

        public string Quantity { get; set; }

        public string CompanyPrefix = null;
        public string ExtensionDigit = null;

        public string LocationReferenceURI = null;

        public DateTime DateGenerated { get; set; } = DateTime.Now;
        public string CodeRequestKey { get; set; } = Guid.NewGuid().ToString();

        public List<AssociatedProduct> AssociatedProducts = new List<AssociatedProduct>();
    }

    public class AssociatedProduct
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class Location
    {
        public List<string> References { get; set; } = new List<string>();
        public string Name { get; set; } = "NPO Petrovax";
    }

    public class UID
    {
        public string Urn { get; set; } = String.Empty;
        public string CryptoKey { get; set; } = String.Empty;
        public string CryptoCode { get; set; } = String.Empty;

    }

    public class SSCC
    {
        public List<string> UIDs { get; set; } = new List<string>();
        public Location Location = new Location();

        public List<string> BusinessTransactions = new List<string>();
        public string UIDTypeName = null;

        public string Quantity { get; set; }

        public string CompanyPrefix = "0614141";
        public string ExtensionDigit = null;

        public string LocationReferenceURI = null;

        public DateTime DateGenerated { get; set; } = DateTime.Now;
        public string CodeRequestKey { get; set; } = Guid.NewGuid().ToString();

        public List<AssociatedProduct> AssociatedProducts = new List<AssociatedProduct>();

    }
}