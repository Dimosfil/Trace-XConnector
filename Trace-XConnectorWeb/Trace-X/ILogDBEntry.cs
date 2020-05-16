using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnectorWeb.Trace_X
{
    public interface ILogDBEntry
    {
        string ProcedureName { get; }
        void FillCommand(SqlCommand command);
    }
}
