using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modbus.Device;
using Modbus.Data;
using Modbus.IO;
using Modbus.Message;
using Modbus.Utility;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Diagnostics;



namespace BlueToothServer
{
    public class BTModBusSlave
    {
        TcpListener tcpListener;
        ModbusTcpSlave slave;
        MySqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;


        public BTModBusSlave()
        {

            Thread serverThread = new Thread(new ThreadStart(start)); // I listen in my own thread
            serverThread.Start();

        }
        public void start()
        {
            if (OpenStorage()) Console.WriteLine("Sql opened");
            tcpListener = new TcpListener(IPAddress.Any, 502);
            tcpListener.Start();
            slave = ModbusTcpSlave.CreateTcp(1, tcpListener);
            slave.Listen();
         //   Console.WriteLine("Passed slave.listen");
            slave.ModbusSlaveRequestReceived += new EventHandler<ModbusSlaveRequestEventArgs>(this.gotMasterMessage);
            slave.DataStore = Modbus.Data.DataStoreFactory.CreateDefaultDataStore();
        //    Console.WriteLine("Size of " + slave.DataStore.InputRegisters.Count());
            /*          slave.DataStore.InputRegisters[1] = 1;
           
                      slave.DataStore.InputRegisters[2] = 0;
                      slave.DataStore.InputRegisters[3] = 1;
                      slave.DataStore.InputRegisters[4] = 0;
                      slave.DataStore.InputRegisters[5] = 290;
                      slave.DataStore.InputRegisters[6] = 287;
                      slave.DataStore.InputRegisters[7] = 1402616377/65536;
                      slave.DataStore.InputRegisters[8] =(ushort) (1402616377-(slave.DataStore.InputRegisters[7]*65536));
                      slave.DataStore.InputRegisters[9] = 1402616307 / 65536;
                      slave.DataStore.InputRegisters[10] = (ushort)(1402616307 - (slave.DataStore.InputRegisters[5] * 65536));
                      slave.DataStore.InputRegisters[11] = 81;
                      slave.DataStore.InputRegisters[12] = 67;*/
            while (true)
            {
                getdata();
                Thread.Sleep(10000);
            }
        }
        public void getdata()
        {
            setDeviceHealth();
            getTTData();
            /*   slave.DataStore.InputRegisters[5] = 290;
               slave.DataStore.InputRegisters[6] = 287;
               slave.DataStore.InputRegisters[7] = 1402616377 / 65536;
               slave.DataStore.InputRegisters[8] = (ushort)(1402616377 - (slave.DataStore.InputRegisters[7] * 65536));
               slave.DataStore.InputRegisters[9] = 1402616307 / 65536;
               slave.DataStore.InputRegisters[10] = (ushort)(1402616307 - (slave.DataStore.InputRegisters[5] * 65536));
               slave.DataStore.InputRegisters[11] = 81;
               slave.DataStore.InputRegisters[12] = 67;*/
        }
        public void setDeviceHealth()
        {
            int deviceNo = 1;
            MySqlCommand command = con.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "Select * from devices";
            Reader = command.ExecuteReader();
            while (Reader.Read())
            {
                slave.DataStore.InputRegisters[deviceNo++] = Reader.GetUInt16(7);
            }
            Reader.Close();
            Reader.Dispose();
            command.Dispose();
        }
        public void getTTData()
        {
            int[] tt = new int[8];
            Int32[] ft = new Int32[8];
            int[] len = new int[8];
            int segLen = 0;
            Int32 valFin = 0;
            int thisSeg = 0;
            MySqlCommand command = con.CreateCommand();
            MySqlDataReader Reader;
            command.CommandText = "Select * from segments";
            Reader = command.ExecuteReader();
            while (Reader.Read() )
            {
                tt[thisSeg] = Reader.GetInt16(6);
                ft[thisSeg] = Reader.GetInt32(11);
                len[thisSeg++] = Reader.GetInt16(7);
                
            }
            Reader.Close();
            Reader.Dispose();
            command.Dispose();

            slave.DataStore.InputRegisters[11] = (ushort)(tt[0] + tt[1]);
            if (ft[0] > ft[1]) valFin = ft[1]; else valFin = ft[0];
            slave.DataStore.InputRegisters[12] = (ushort)(valFin / 65536);
            slave.DataStore.InputRegisters[13] = (ushort)(valFin - (slave.DataStore.InputRegisters[12] * 65536));
            slave.DataStore.InputRegisters[14] = (ushort)(tt[2] + tt[3]);
            if (ft[2] > ft[3]) valFin = ft[3]; else valFin = ft[2];
            slave.DataStore.InputRegisters[15] = (ushort)(valFin / 65536);
            slave.DataStore.InputRegisters[16] = (ushort)(valFin - (slave.DataStore.InputRegisters[15] * 65536));
            slave.DataStore.InputRegisters[17] = (ushort)(tt[4] + tt[5]);
            if (ft[0] > ft[1]) valFin = ft[5]; else valFin = ft[4];
            slave.DataStore.InputRegisters[18] = (ushort)(valFin / 65536);
            slave.DataStore.InputRegisters[19] = (ushort)(valFin - (slave.DataStore.InputRegisters[18] * 65536));
            slave.DataStore.InputRegisters[20] = (ushort)(tt[6] + tt[7]);
            if (ft[2] > ft[3]) valFin = ft[7]; else valFin = ft[6];
            slave.DataStore.InputRegisters[21] = (ushort)(valFin / 65536);
            slave.DataStore.InputRegisters[22] = (ushort)(valFin - (slave.DataStore.InputRegisters[21] * 65536));
            segLen = len[0] + len[1];
            slave.DataStore.InputRegisters[28] =  (ushort) segLen;
            if (slave.DataStore.InputRegisters[11] != 0) slave.DataStore.InputRegisters[28] = (ushort)(segLen / (slave.DataStore.InputRegisters[11] / 3.6)); else slave.DataStore.InputRegisters[28] = 0;
           segLen = len[2] + len[3];
            if (slave.DataStore.InputRegisters[14] != 0) slave.DataStore.InputRegisters[29] = (ushort)(segLen / (slave.DataStore.InputRegisters[14] / 3.6)); else slave.DataStore.InputRegisters[29] = 0;
            segLen = len[4] + len[5];
            if (slave.DataStore.InputRegisters[17] != 0) slave.DataStore.InputRegisters[30] = (ushort)(segLen / (slave.DataStore.InputRegisters[17] / 3.6)); else slave.DataStore.InputRegisters[30] = 0;
            segLen = len[6] + len[7];
            if (slave.DataStore.InputRegisters[20] != 0) slave.DataStore.InputRegisters[31] = (ushort)(segLen / (slave.DataStore.InputRegisters[20] / 3.6)); else slave.DataStore.InputRegisters[31] = 0;
         }

        public void gotMasterMessage(Object sender, EventArgs e)
        {

        }

        public bool OpenStorage()
        {
            try
            {
                con = new MySqlConnection(ConnectionString);
                con.Open();
                return (true);

            }
            catch (Exception ex)
            {
                string lSource = "TT Server";
                string lLog = "TT Server Log";
               EventLog.WriteEntry(lSource, "Error - "+ex);
                //Console.WriteLine(ex);
                return false;
            }
        }
    }
}


