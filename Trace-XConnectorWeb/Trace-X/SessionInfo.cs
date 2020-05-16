﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnectorWeb.Trace_X
{
    public class SessionInfo : ILogDBEntry
    {
        public DateTime WriteTime { get; set; }
        public string JsonString { get; set; }
        public string XmlString { get; set; }
        public SessionInfo()
        { }

        public SessionInfo(DateTime writeTime, string json, string xmlString)
        {
            // TODO: Complete member initialization

            WriteTime = writeTime;
            JsonString = json;
            XmlString = xmlString;
        }

        public string ProcedureName
        {
            get { return "SaveSession"; }
        }

        public void FillCommand(SqlCommand command)
        {
            command.Parameters.Add(new SqlParameter("@WriteTime", WriteTime));
            command.Parameters.Add(new SqlParameter("@JsonString", JsonString));
            command.Parameters.Add(new SqlParameter("@XmlString", XmlString));
            //command.Parameters.Add(new SqlParameter("@Length", Length));
            //command.Parameters.Add(new SqlParameter("@prevSession", prevSessionTime));
            //command.Parameters.Add(new SqlParameter("@device", device));
        }
    }
}
