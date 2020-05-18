using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Trace_XConnectorWeb.Trace_X
{
    public class LogDBManager
    {
        private static LogDBManager _instance = null;
        public static LogDBManager GetInstance()
        {
            return _instance;
        }

        private string sLogDBConnectionString = null;
        private bool LogDBEnabled = false;

        //public static void Init(CompositeLogger logger)
        //{
        //    _instance = new LogDBManager();

        //    _instance._logger = logger;

        //    _instance.sLogDBConnectionString = SystemConfig.sLogDBConnectString;
        //    _instance.LogDBEnabled = SystemConfig.LogDbEnabled;

        //    _instance.Start();
        //}

        public static void Init()
        {
            if (_instance == null)
            {
                _instance = new LogDBManager();

                //_instance._logger = logger;

                _instance.sLogDBConnectionString = SystemConfig.sLogDBConnectString;
                _instance.LogDBEnabled = SystemConfig.LogDbEnabled;

                _instance.Start();
            }
        }

        //private CompositeLogger _logger = null;

        private void OnTimer()
        {
            while (!bStopping)
            {
                SaveData();
                System.Threading.Thread.Sleep(1000);
            }
        }

        //private RecursiveOptex optex = new RecursiveOptex(SystemConfig.iRecursiveOptexSpinCount);
        private object optex = new object();

        private void SaveData()
        {
            lock (optex)
            {
                ProcessSave();
            }

            //try
            //{
            //    optex.Enter();
            //    ProcessSave();
            //}
            //finally
            //{
            //    optex.Exit();
            //}
        }

        public void Save(ILogDBEntry data)
        {
            if (!LogDBEnabled)
                return;
            if (data == null)
                return;
            qSaveData.Enqueue(data);
        }

        private readonly ConcurrentQueue<ILogDBEntry> qSaveData = new ConcurrentQueue<ILogDBEntry>();
        private readonly LinkedList<ILogDBEntry> llSaveData = new LinkedList<ILogDBEntry>();
        void ProcessSave()
        {
            ILogDBEntry data = null;
            while (qSaveData.TryDequeue(out data))
            {
                llSaveData.AddLast(data);
            }

            //если мы не умеем сохранять, то и не будем этого делать тогда
            if (!LogDBEnabled)
            {
                llSaveData.Clear();
                return;
            }

            while (llSaveData.Count > 0)
            {
                ILogDBEntry entry = llSaveData.First.Value;

                try
                {
                    SaveLogData(entry);
                }
                catch (Exception ex)
                {
                    //if (_logger != null)
                    //    _logger.Error(ex.ToString());
                }

                llSaveData.RemoveFirst();
            }
        }

        private SqlConnection GetOpenConnection()
        {
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(sLogDBConnectionString);
                connection.Open();
            }
            catch (Exception ee)
            {
                if (ee.ToString().IndexOf("sp_sdidebug") < 0) //затычка от отладчика SQL
                {
                    //_logger.Error("Can't open sql connection", ee.ToString());
                    throw;
                }
            }
            return connection;
        }

        void SaveLogData(ILogDBEntry entry)
        {
            if (!LogDBEnabled)
            {
                //_logger.Info("LogDbManager->SaveLogData, db not enable");
                return;
            }

            string sql = entry.ProcedureName;

            using (var connection = GetOpenConnection())
            using (var command = connection.CreateCommand())
            {
                try
                {
                    command.CommandText = sql;
                    command.CommandType = CommandType.StoredProcedure;
                    entry.FillCommand(command);

                    command.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    //_logger.Error("Error save data, SaveLogData for  " + entry.ToString(), sqlException.ToString());
                }
                catch (Exception ee)
                {
                    //_logger.Error(ee.ToString());
                }
            }
            return;
        }

        private bool bEnabled = false;
        private bool bStopping = false;

        private void Start()
        {
            System.Threading.Thread thread = new System.Threading.Thread(OnTimer);
            thread.Start();
        }

        internal void Stop()
        {
            if (!bEnabled)
                return;

            bStopping = true;
            SaveData();
        }
    }
}
