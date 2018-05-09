using Npgsql;
using System;

namespace DataLoadReadReview.Library
{
    public class DataContext : IDisposable
    {
        public DataContext(String connString)
        {
            Connection = new NpgsqlConnection(connString);
        }

        public NpgsqlConnection Connection { get; }

        private Boolean tableExists(string dbName, string tableName)
        {
            int counter = 0;
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.CommandText = string.Format(

                    "SELECT COUNT(*) FROM information_schema.tables " +
                    "  WHERE table_schema=@dbName " +
                    "  AND " +
                    "  table_name=@tableName;",
                    dbName,
                    tableName);

                cmd.Connection = Connection;
                if (Connection != null && Connection.State == System.Data.ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }

                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@dbName", dbName);

                var result = cmd.ExecuteReader();

                if (result.Read())
                {
                    counter = result.GetInt16(0);
                }

                result.Close();
            }

            return counter > 0;
        }

        public NpgsqlDataReader ReadTable(string dbName, string tableName)
        {
            dbName = dbName.ToLower();
            tableName = tableName.ToLower();
            if (tableExists(dbName, tableName))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand())
                {
                    cmd.CommandText = string.Format(
                        "SELECT * FROM {0}.{1}; ",
                        dbName,
                        tableName);

                    cmd.Connection = Connection;
                    if (Connection != null && Connection.State == System.Data.ConnectionState.Closed)
                    {
                        cmd.Connection.Open();
                    }

                    cmd.Parameters.AddWithValue("@tableName", tableName);
                    cmd.Parameters.AddWithValue("@dbName", dbName);

                    return cmd.ExecuteReader();
                }
            }
            else
            {
                throw new Exception("Table Not Found!");
            }
        }

        public NpgsqlDataReader ListTables(string dbName)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.CommandText =
                    @"SELECT table_name FROM information_schema.tables WHERE table_schema=@dbName;";
                cmd.Connection = Connection;
                if (Connection != null && Connection.State == System.Data.ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }

                cmd.Parameters.AddWithValue("@dbName", dbName);

                return cmd.ExecuteReader();
            }
        }

        public NpgsqlDataReader ListSchema()
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand())
            {
                cmd.CommandText =
                    @"SELECT distinct table_schema FROM information_schema.tables;";
                cmd.Connection = Connection;
                if (Connection != null && Connection.State == System.Data.ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                }

                return cmd.ExecuteReader();
            }
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
