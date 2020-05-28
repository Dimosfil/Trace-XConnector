using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Trace_XConnectorWeb.Trace_X
{
    public class OrderExportXmlDataOld
    {
        [XmlRoot(ElementName = "epcList")]
        public class EpcList
        {
            [XmlElement(ElementName = "epc")]
            public List<string> Epc { get; set; }
        }

        [XmlRoot(ElementName = "readPoint")]
        public class ReadPoint
        {
            [XmlElement(ElementName = "id")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "bizLocation")]
        public class BizLocation
        {
            [XmlElement(ElementName = "id")]
            public string Id { get; set; }
        }

        [XmlRoot(ElementName = "extension", Namespace = "optelvision.extension.epcis")]
        public class Extension
        {
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }
            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "ObjectEvent")]
        public class ObjectEvent
        {
            [XmlElement(ElementName = "eventTime")]
            public string EventTime { get; set; }
            [XmlElement(ElementName = "eventTimeZoneOffset")]
            public string EventTimeZoneOffset { get; set; }
            [XmlElement(ElementName = "epcList")]
            public EpcList EpcList { get; set; }
            [XmlElement(ElementName = "action")]
            public string Action { get; set; }
            [XmlElement(ElementName = "bizStep")]
            public string BizStep { get; set; }
            [XmlElement(ElementName = "disposition")]
            public string Disposition { get; set; }
            [XmlElement(ElementName = "readPoint")]
            public ReadPoint ReadPoint { get; set; }
            [XmlElement(ElementName = "bizLocation")]
            public BizLocation BizLocation { get; set; }
            [XmlElement(ElementName = "extension", Namespace = "optelvision.extension.epcis")]
            public List<Extension> Extension { get; set; }
        }

        [XmlRoot(ElementName = "childEPCs")]
        public class ChildEPCs
        {
            [XmlElement(ElementName = "epc")]
            public List<string> Epc { get; set; }
        }

        [XmlRoot(ElementName = "AggregationEvent")]
        public class AggregationEvent
        {
            [XmlElement(ElementName = "eventTime")]
            public string EventTime { get; set; }
            [XmlElement(ElementName = "eventTimeZoneOffset")]
            public string EventTimeZoneOffset { get; set; }
            [XmlElement(ElementName = "parentID")]
            public string ParentID { get; set; }
            [XmlElement(ElementName = "childEPCs")]
            public ChildEPCs ChildEPCs { get; set; }
            [XmlElement(ElementName = "action")]
            public string Action { get; set; }
            [XmlElement(ElementName = "bizStep")]
            public string BizStep { get; set; }
            [XmlElement(ElementName = "readPoint")]
            public ReadPoint ReadPoint { get; set; }
            [XmlElement(ElementName = "bizLocation")]
            public BizLocation BizLocation { get; set; }
            [XmlElement(ElementName = "extension", Namespace = "optelvision.extension.epcis")]
            public Extension Extension { get; set; }
        }

        [XmlRoot(ElementName = "EventList")]
        public class EventList
        {
            [XmlElement(ElementName = "ObjectEvent")]
            public List<ObjectEvent> ObjectEvent { get; set; }
            [XmlElement(ElementName = "AggregationEvent")]
            public List<AggregationEvent> AggregationEvent { get; set; }
        }

        [XmlRoot(ElementName = "EPCISBody")]
        public class EPCISBody
        {
            [XmlElement(ElementName = "EventList")]
            public EventList EventList { get; set; }
        }

        [XmlRoot(ElementName = "EPCISDocument", Namespace = "urn:epcglobal:epcis:xsd:1")]
        public class EPCISDocument
        {
            [XmlElement(ElementName = "EPCISBody")]
            public EPCISBody EPCISBody { get; set; }
            [XmlAttribute(AttributeName = "creationDate")]
            public string CreationDate { get; set; }
            [XmlAttribute(AttributeName = "schemaVersion")]
            public string SchemaVersion { get; set; }
            [XmlAttribute(AttributeName = "epcis", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Epcis { get; set; }
            [XmlAttribute(AttributeName = "gs1ushc", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Gs1ushc { get; set; }
            [XmlAttribute(AttributeName = "optelvision", Namespace = "http://www.w3.org/2000/xmlns/")]
            public string Optelvision { get; set; }
        }
    }


}
