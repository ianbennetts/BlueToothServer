using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace BlueToothServer
{
    public class SqlConsole
    {
        SqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
        public bool OpenStorage()
        {
             try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
                return (true);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public bool WriteLine(int errorno, int deviceno, string error)
        {
            try
            {
                SqlCommand command;
                String d,s;
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                d = dt.ToString("yyyy-MM-dd HH:mm:ss");
                if (error.Length > 40) { s = error.Substring(0, 39); } else s = error;  
                command = new SqlCommand("INSERT INTO errorlog (errorno, deviceNo, eventtime, log) VALUES (@e, @d, @t, @log) ", con);
                command.Parameters.AddWithValue("@e", errorno);
                command.Parameters.AddWithValue("@d", deviceno);
                command.Parameters.AddWithValue("@t", d);
                command.Parameters.AddWithValue("@log", s);
                command.ExecuteNonQuery();
                command.Dispose();
                return (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }

        }
    }
}