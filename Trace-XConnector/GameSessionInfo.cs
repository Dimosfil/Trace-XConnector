using System;
using System.Data.SqlClient;
using UniverServer.GameLogic.Adapter;

namespace Trace_XConnector
{
    public class GameSessionInfo : ILogDBEntry
    {
        public DateTime WriteTime { get; set; }
        public string JsonString { get; set; }
        public string XmlString { get; set; }
        public GameSessionInfo()
        { }

        public GameSessionInfo(DateTime writeTime, string json, string xmlString)
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