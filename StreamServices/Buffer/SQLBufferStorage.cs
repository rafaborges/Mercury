using System;
using System.Collections.Generic;
using StreamServices.Services;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Configuration;

namespace StreamServices.Buffer
{
    class SQLBufferStorage : IBufferPersistence, IDisposable
    {
        /// <summary>
        /// Object containing the connection to the SQL server
        /// </summary>
        SqlConnection connection;

        // Information regaring the queries. It will be loaded from 
        // the assemvly .config file
        string insert, delete, getAll;
        string tsColName, idColName, valColName;
        string tsParamName, idParamName, valParamName;

        public SQLBufferStorage()
        {
            // Get configuration from the assembly config file
            insert = ConfigurationManager.AppSettings["BufferSQLInsert"];
            delete = ConfigurationManager.AppSettings["BufferSQLDelete"];
            getAll = ConfigurationManager.AppSettings["BufferSQLGetAll"];
            tsColName = ConfigurationManager.AppSettings["BufferSQLTimeStampColumn"];
            idColName = ConfigurationManager.AppSettings["BufferSQLIDColumn"];
            valColName = ConfigurationManager.AppSettings["BufferSQLValueColumn"];
            tsParamName = ConfigurationManager.AppSettings["BufferSQLTimeStampParam"];
            idParamName = ConfigurationManager.AppSettings["BufferSQLIDParam"];
            valParamName = ConfigurationManager.AppSettings["BufferSQLValueParam"];
            var connectionString = ConfigurationManager.AppSettings["BufferSQLConnectionString"]; 

            // Connecting to Database
            connection = new SqlConnection(connectionString);
        }
        public List<EventData> GetAllStoredValues(Guid id)
        {
            List<EventData> bufferedData = new List<EventData>();
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                SqlCommand command = new SqlCommand(getAll, connection);
                command.Parameters.AddWithValue(tsParamName, id);
                SqlDataReader reader = command.ExecuteReader();
                while(reader.Read())
                {
                    var timeStamp = (DateTime)reader[tsColName];
                    var value = reader[valColName];
                    bufferedData.Add(new EventData(id, timeStamp, value));
                }
            }

            return bufferedData;
        }

        public void RemoveData(EventData data)
        {
            InsertOrDelete(delete, data);
        }

        public void StoreData(EventData data)
        {
            InsertOrDelete(insert,data);
        }

        private void InsertOrDelete(string query, EventData data)
        {
            Task.Factory.StartNew(() => {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddRange(new SqlParameter[]{
                        new SqlParameter(idParamName,data.Source),
                        new SqlParameter(tsParamName,data.TimeStamp),
                        new SqlParameter(valParamName,data.Value)
                    });
                    command.ExecuteNonQuery();
                }
            });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    connection.Close();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
