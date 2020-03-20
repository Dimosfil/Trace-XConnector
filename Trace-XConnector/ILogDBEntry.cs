using System.Data.SqlClient;

namespace UniverServer.GameLogic.Adapter
{
    public interface ILogDBEntry
    {
        string ProcedureName { get; }
        void FillCommand(SqlCommand command);
    }
}