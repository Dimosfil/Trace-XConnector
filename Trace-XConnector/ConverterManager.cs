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
        public static ConverterManager Instance { get; private set; }

        public static void Init()
        {
            if(Instance == null)
                Instance = new ConverterManager();
        }

        public T GetJsonObject<T>(string json)
        {
            var rootObj = JsonConvert.DeserializeObject<T>(json);
            return rootObj;
        }

        public string ConvertOrderDataToXml(JsonOrderData jsonOrderData)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(JsonOrderData));
            string xml;

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, jsonOrderData);
                    xml = sww.ToString(); // Your XML
                }
            }


            return xml;
        }

        public string ConvertOrderExportToXml(string json)
        {
            var rootObj = JsonConvert.DeserializeObject<JsonOrderExportData>(json);

            XmlSerializer xsSubmit = new XmlSerializer(typeof(JsonOrderExportData));
            string xml;

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

        public string ConvertXmlToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            return jsonText;
        }
    }
}