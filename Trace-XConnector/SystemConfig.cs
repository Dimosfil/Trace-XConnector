using System;
using System.Collections.Generic;
using System.Text;

namespace Trace_XConnector
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
