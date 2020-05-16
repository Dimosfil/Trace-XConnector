using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnectorWeb.Trace_X
{
    public class Carton
    {
        public string serial { get; set; }
        public string f91 { get; set; }
        public string f92 { get; set; }
    }

    public class JsonOrderData
    {
        public int orderId { get; set; }
        public bool closed { get; set; }
        public string gtin { get; set; }
        public string series { get; set; }
        public string productionDate { get; set; }
        public string expiryDate { get; set; }
        public int amount { get; set; }
        public int wrapperCapacity { get; set; }
        public int caseCapacity { get; set; }
        public int palletCapacity { get; set; }
        public List<Carton> cartons { get; set; }
        public List<string> wrappers { get; set; }
        public List<string> cases { get; set; }
        public List<string> pallets { get; set; }
    }

    public class JsonOrderExportData
    {
        public int orderId { get; set; }
        public List<UpdatedData> data { get; set; } = new List<UpdatedData>();
    }

    public class UpdatedData
    {
        public string type { get; set; }
        public string serial { get; set; }
        public string status { get; set; }
        public string attachmentType { get; set; } = String.Empty;
        public List<UpdatedData> attachment { get; set; } = new List<UpdatedData>();
    }

    public class OrderInProductionRequest
    {
        public int orderId { get; set; }
    }
}
