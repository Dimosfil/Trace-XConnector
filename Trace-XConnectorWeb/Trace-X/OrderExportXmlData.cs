using System.Collections.Generic;
using System.Xml.Serialization;

namespace Trace_XConnectorWeb.Trace_X
{
    // Примечание. Для запуска созданного кода может потребоваться NET Framework версии 4.5 или более поздней версии и .NET Core или Standard версии 2.0 или более поздней.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:epcglobal:epcis:xsd:1")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:epcglobal:epcis:xsd:1", IsNullable = false)]
    public partial class EPCISDocument
    {

        private EPCISBody ePCISBodyField;

        private System.DateTime creationDateField;

        private decimal schemaVersionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public EPCISBody EPCISBody
        {
            get
            {
                return this.ePCISBodyField;
            }
            set
            {
                this.ePCISBodyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime creationDate
        {
            get
            {
                return this.creationDateField;
            }
            set
            {
                this.creationDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal schemaVersion
        {
            get
            {
                return this.schemaVersionField;
            }
            set
            {
                this.schemaVersionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EPCISBody
    {

        private EPCISBodyEventList eventListField;

        /// <remarks/>
        public EPCISBodyEventList EventList
        {
            get
            {
                return this.eventListField;
            }
            set
            {
                this.eventListField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventList
    {

        private object[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("AggregationEvent", typeof(EPCISBodyEventListAggregationEvent))]
        [System.Xml.Serialization.XmlElementAttribute("ObjectEvent", typeof(EPCISBodyEventListObjectEvent))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListAggregationEvent
    {

        private System.DateTime eventTimeField;

        private string eventTimeZoneOffsetField;

        private string parentIDField;

        private string[] childEPCsField;

        private string actionField;

        private string bizStepField;

        private EPCISBodyEventListAggregationEventReadPoint readPointField;

        private EPCISBodyEventListAggregationEventBizLocation bizLocationField;

        private extension extensionField;

        /// <remarks/>
        public System.DateTime eventTime
        {
            get
            {
                return this.eventTimeField;
            }
            set
            {
                this.eventTimeField = value;
            }
        }

        /// <remarks/>
        public string eventTimeZoneOffset
        {
            get
            {
                return this.eventTimeZoneOffsetField;
            }
            set
            {
                this.eventTimeZoneOffsetField = value;
            }
        }

        /// <remarks/>
        public string parentID
        {
            get
            {
                return this.parentIDField;
            }
            set
            {
                this.parentIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("epc", IsNullable = false)]
        public string[] childEPCs
        {
            get
            {
                return this.childEPCsField;
            }
            set
            {
                this.childEPCsField = value;
            }
        }

        /// <remarks/>
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        /// <remarks/>
        public string bizStep
        {
            get
            {
                return this.bizStepField;
            }
            set
            {
                this.bizStepField = value;
            }
        }

        /// <remarks/>
        public EPCISBodyEventListAggregationEventReadPoint readPoint
        {
            get
            {
                return this.readPointField;
            }
            set
            {
                this.readPointField = value;
            }
        }

        /// <remarks/>
        public EPCISBodyEventListAggregationEventBizLocation bizLocation
        {
            get
            {
                return this.bizLocationField;
            }
            set
            {
                this.bizLocationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "optelvision.extension.epcis")]
        public extension extension
        {
            get
            {
                return this.extensionField;
            }
            set
            {
                this.extensionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListAggregationEventReadPoint
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListAggregationEventBizLocation
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "optelvision.extension.epcis")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "optelvision.extension.epcis", IsNullable = false)]
    public partial class extension
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListObjectEvent
    {

        private System.DateTime eventTimeField;

        private string eventTimeZoneOffsetField;

        private string[] epcListField;

        private string actionField;

        private string bizStepField;

        private string dispositionField;

        private EPCISBodyEventListObjectEventReadPoint readPointField;

        private EPCISBodyEventListObjectEventBizLocation bizLocationField;

        private extension[] extensionField;

        /// <remarks/>
        public System.DateTime eventTime
        {
            get
            {
                return this.eventTimeField;
            }
            set
            {
                this.eventTimeField = value;
            }
        }

        /// <remarks/>
        public string eventTimeZoneOffset
        {
            get
            {
                return this.eventTimeZoneOffsetField;
            }
            set
            {
                this.eventTimeZoneOffsetField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("epc", IsNullable = false)]
        public string[] epcList
        {
            get
            {
                return this.epcListField;
            }
            set
            {
                this.epcListField = value;
            }
        }

        /// <remarks/>
        public string action
        {
            get
            {
                return this.actionField;
            }
            set
            {
                this.actionField = value;
            }
        }

        /// <remarks/>
        public string bizStep
        {
            get
            {
                return this.bizStepField;
            }
            set
            {
                this.bizStepField = value;
            }
        }

        /// <remarks/>
        public string disposition
        {
            get
            {
                return this.dispositionField;
            }
            set
            {
                this.dispositionField = value;
            }
        }

        /// <remarks/>
        public EPCISBodyEventListObjectEventReadPoint readPoint
        {
            get
            {
                return this.readPointField;
            }
            set
            {
                this.readPointField = value;
            }
        }

        /// <remarks/>
        public EPCISBodyEventListObjectEventBizLocation bizLocation
        {
            get
            {
                return this.bizLocationField;
            }
            set
            {
                this.bizLocationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("extension", Namespace = "optelvision.extension.epcis")]
        public extension[] extension
        {
            get
            {
                return this.extensionField;
            }
            set
            {
                this.extensionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListObjectEventReadPoint
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class EPCISBodyEventListObjectEventBizLocation
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }
}