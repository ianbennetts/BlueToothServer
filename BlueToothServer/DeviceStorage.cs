using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;


namespace BlueToothServer
{
    public class DeviceStorage // no need for comments as straight forward
    {
        SqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
 
        public bool OpenStorage()
        {
         try {
             con = new SqlConnection(ConnectionString);
             
             con.Open();
             return (true);
            
            }catch (Exception ex){
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, ex.ToString());
                return false;
            }
        }
        public int IsDevice(String s){
            int deviceNo;
            SqlCommand command = con.CreateCommand();
            SqlDataReader Reader;
            
            command.CommandText = "Select * from devices";
            try {
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    if (Reader.GetValue(1).ToString().CompareTo(s) == 0) { deviceNo = Reader.GetInt32(0); Reader.Close(); return deviceNo; }
                }
                Reader.Close();
                return -1;
            }catch(Exception ex)
            {
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, ex.ToString());
                return 0;

            }
        }
        public bool DeviceUp(String s)
        {
            try{
                String d;
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                d = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SqlCommand command ;
                command = new SqlCommand("UPDATE devices SET connected=1, timeof=@t WHERE deviceSerialNo=@dev", con);
                command.Parameters.AddWithValue("@dev", s);
                command.Parameters.AddWithValue("@t", d);
                command.ExecuteNonQuery();
                return (true);
            }catch (Exception ex){
                Console.WriteLine(ex);
                return false;
            }
                              
        }
        public bool DeviceDown(String s)
        {
            try
            {
                String d;
                DateTime dt = new DateTime();
                dt = DateTime.Now;
                d = dt.ToString("yyyy-MM-dd HH:mm:ss");
                SqlCommand command;
                command = new SqlCommand("UPDATE devices SET connected=0, timeof=@t WHERE deviceSerialNo=@dev ", con);
                command.Parameters.AddWithValue("@dev", s);
                command.Parameters.AddWithValue("@t", d);
                command.ExecuteNonQuery();
                return (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                Console.WriteLine(ex);
                return false;
            }

        }
         
    }
}