using System.Collections.Generic;

namespace Trace_XConnector
{
    public class Carton
    {
        public string serial { get; set; }
        public string f91 { get; set; }
        public string f92 { get; set; }
    }

    public class JsonData
    {
        public int orderId { get; set; }
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
        public List<string> @case { get; set; }
        public List<string> pallet { get; set; }
    }
}