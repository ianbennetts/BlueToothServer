using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using System.Diagnostics;


namespace BlueToothServer
{
    public class SightingStorage // no comments straight forward
    {
        SqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
        SqlCommand command;

        public bool OpenStorage()
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
                command = new SqlCommand("INSERT INTO sighting (DeviceNo, SightingMacAddress, SightingArrive, SightingDepart) VALUES (@d, @m, @a, @dep) ", con);
                return (true);
            }
            catch (Exception ex)
            {
                string lSource = "TT Server";
                string lLog = "TT Server Log";
               EventLog.WriteEntry(lSource, "Open Storage not Working - SightingStorage "+ex.ToString());
                return false;
            }
        }

        public bool WriteRecord(Sighting s)
        {
            if (con.State != ConnectionState.Open) { Close(); OpenStorage(); } 
            try
            {
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@d", s.getDeviceNo());
                command.Parameters.AddWithValue("@m",(long) s.getId());
                command.Parameters.AddWithValue("@a",(int) s.getArrived());
                command.Parameters.AddWithValue("@dep", (int)s.getDeparted());
                command.ExecuteNonQuery();
                return (true);
            }
            catch (Exception ex)
            {
                string lSource = "TT Server";
                string lLog = "TT Server Log";
                EventLog.WriteEntry(lSource,ex.ToString());
 
                //Console.WriteLine("Error - " + ex.ToString() + " DeviceNo = " + s.getDeviceNo() + " Mac " + s.getId() + " arrived " + s.getArrived() + " departed at " + s.getDeparted());
                return false;
            }
        }
 
        public bool Close()
        {
            try
            {
                con.Close();
                return (true);
            }
            catch (Exception ex)
            {

                string lSource = "TT Server";
                string lLog = "TT Server Log";
                //EventLog.WriteEntry(lSource, "Sighting Storage Will not Close ") ;
//                Console.WriteLine("Yes " + ex);
                return false;
            }

        }

    }
}