using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnectorWeb.Trace_X
{
    public class SystemConfig
    {
        public static string sLogDBConnectString;
        public static bool LogDbEnabled { get; set; }

        static SystemConfig()
        {
            sLogDBConnectString = "server=localhost;Database=XConnector;user id=sa;password=1";
            LogDbEnabled = true;
        }
    }
}
