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
            var rootObj = JsonConvert.DeserializeObject<JsonData>(json);

            XmlSerializer xsSubmit = new XmlSerializer(typeof(JsonData));
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

        public string ConvertToJson(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            return jsonText;
        }
    }
}