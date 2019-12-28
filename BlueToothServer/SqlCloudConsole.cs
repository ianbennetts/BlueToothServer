using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace BlueToothServer
{
    public class SqlCloudConsole
    {
        SqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
        EmailConsole emailConsole;

        public SqlCloudConsole()
        {

            emailConsole = new EmailConsole(this);
 
        }
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
    
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, ex.ToString());
                return false;
            }
        }
        public bool WriteLine(int errorno, int deviceno, string error)
        {
            lock (this)
            {
                if (con.State != ConnectionState.Open) { OpenStorage(); }
                try
                {
                    SqlCommand command;
                    String d, s;
                    DateTime dt = new DateTime();
                    dt = DateTime.Now;
                    d = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    if (error.Length > 40) { s = error.Substring(0, 39); } else s = error;
                    command = new SqlCommand("INSERT INTO Log (ActionID, DeviceNo, EventTime, log) VALUES (@e, @d, @t, @log) ", con);
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
                    string lSource = "TT Server";

                    EventLog.WriteEntry(lSource, "ok this is it " + ex.ToString());
                    return false;
                }
            }

        }

        public EmailConsole getEmailConsole()
        {
            lock (this)
            {
                return emailConsole;
            }
        }

    }
}