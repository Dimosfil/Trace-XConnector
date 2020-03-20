using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Trace_XConnector
{
    public class ConverterManager
    {
        public static ConverterManager Instance => instance;
        private static ConverterManager instance;

        public static void Init()
        {
            instance = new ConverterManager();
        }

        public string ConvertToXml(string json)
        {
            var obj = JsonConvert.DeserializeObject(json);

            var rootObj = JsonConvert.DeserializeObject<JsonData>(json);

            // To convert JSON text contained in string json into an XML node
            //var doc = JsonConvert.SerializeObject(obj);


            XmlSerializer xsSubmit = new XmlSerializer(typeof(JsonData));
            //var subReq = new JsonData();
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, rootObj);
                    xml = sww.ToString(); // Your XML
                }
            }


            return xml;
        }

        public string ConvertToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            return jsonText;
        }
    }
    public class RootObject
    {
        //public Xml __invalid_name__?xml { get; set; }
        public EpcisEPCISDocument EPCISDocument { get; set; }
    }

    public class EpcisEPCISDocument
    {
        //public DateTime __invalid_name__@creationDate { get; set; }
        //public string __invalid_name__@schemaVersion { get; set; }
        //public string __invalid_name__@xmlns:epcis { get; set; }
        //public string __invalid_name__@xmlns:gs1ushc { get; set; }
        //public string __invalid_name__@xmlns:optelvision { get; set; }
        public EPCISBody EPCISBody { get; set; }
    }

    public class Xml
    {
        //public string __invalid_name__@version { get; set; }
    }

    public class EpcList
    {
        public List<string> epc { get; set; }
    }

    public class ReadPoint
    {
        public string id { get; set; }
    }

    public class BizLocation
    {
        public string id { get; set; }
    }

    public class ObjectEvent
    {
        public DateTime eventTime { get; set; }
        public string eventTimeZoneOffset { get; set; }
        public EpcList epcList { get; set; }
        public string action { get; set; }
        public string disposition { get; set; }
        public ReadPoint readPoint { get; set; }
        public BizLocation bizLocation { get; set; }
    }

    public class EventList
    {
        public ObjectEvent ObjectEvent { get; set; }
    }

    public class EPCISBody
    {
        public EventList EventList { get; set; }
    }
}