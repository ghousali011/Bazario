using MySql.Data.MySqlClient;
using System.Data;

namespace ECommerce.DL
{
    public static class DbHelper
    {
        private static string _connectionString = "";

        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public static DataTable ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            var dt = new DataTable();
            conn.Open();
            using var reader = cmd.ExecuteReader();
            dt.Load(reader);
            return dt;
        }

        public static int ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new MySqlCommand(query, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteScalar();
        }
    }
}
