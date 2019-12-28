using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;

namespace BlueToothServer
{
    public class SegmentTTCalc
    {
        Thread TTCalc;
        int segId;
        int fromLoc;
        int toLoc;
        public int minTT;
        public  int [,] baseData;
        public int possDiff;
        public int baseNo;
        public int startTT;
        List<TravelTime> TravelTimeList;
        SqlConnection con = null;
        SqlCloudConsole sqlConsole;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
        ValidTT validTT;
        public SegmentTTCalc(SqlCloudConsole cc,int i, int j, int k,byte b,int l,int m,int stt)
        {
            segId = i;
            fromLoc = j;
            toLoc = k;
            baseNo = (int)b;
            minTT = l;
            possDiff = m;
            startTT = stt;
            sqlConsole = cc;
            TravelTimeList = new List<TravelTime>();
            OpenStorage();
            if (baseNo!=0){
                baseData=Connection.db[baseNo];
            }else{
                baseData = null;
            }
  //          while (!getConfig());

  //          validTT = new ValidTT(segId, baseData, minTT, possDiff, baseNo, startTT); 
            validTT = new ValidTT(sqlConsole,segId,baseData,minTT, possDiff, baseNo, startTT);

            TTCalc = new Thread(new ThreadStart(run)); // i am in my own thread
            TTCalc.Start();
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
                Console.WriteLine(ex); string lSource = "TT Server";
                string lLog = "TT Server Log";
                EventLog.WriteEntry(lSource, "Segment TT Failed to Open " + segId);
                return false;
            }
        }

        private void run()
        {
            int i = 0;
             while (true)
            {

                Thread.Sleep(100);
                processTT();

             }
        }
        private bool getConfig()
        {
            SqlCommand command = con.CreateCommand();
            SqlDataReader Reader;
            command.CommandText = "Select * from Links";
            try {
                Reader = command.ExecuteReader();
                while (Reader.Read())
                {
                    if (Reader.GetInt32(0) == segId)
                    {
                        minTT = (int)Reader.GetInt16(9);
                        possDiff = (int)Reader.GetInt16(10);
                        Reader.Close();
                        //                   Console.WriteLine("I am Seg no " + segId + "my mintt = " + minTT + " my possible diff is " + possDiff);
                        return true;
                    }
                    return true;
                }
                //            Console.WriteLine("I am seg no " + segId + " and I have no config data");
                Reader.Close();
            }catch(SqlException ex)
            {
                return false;
            }
            return true;
        }
        public int getSegId()
        {
            return segId;
        }
        public int getFromLoc()
        {
            return fromLoc;
        }
        public int getToLoc()
        {
            return toLoc;
        }
        public void FoundTT(Sighting f, Sighting t)
        {
            lock (this)
            {
                TravelTimeList.Add(new TravelTime(f.getDeviceNo(), t.getDeviceNo(), (int)(t.getDeparted() - f.getDeparted()), f.getDeparted(), t.getDeparted(), t.getId()));
            }
        }
        private void processTT()
        {
            int noToProcess = TravelTimeList.Count - 1;
            int i,aver;
            TravelTime tt;
            for (i = 0; i < noToProcess; i++)
            {
                tt = TravelTimeList[0];
                TravelTimeList.RemoveAt(0);
              //  noToProcess--;
                aver=validTT.isValid(tt);
                if (tt == null)
                {
                    Console.WriteLine("I am segTT " + segId + " I have null ");
                }
                else if (aver != -1)
                {
                    WriteRecord(tt, aver);
                }
            }
        }
        private bool valid(TravelTime t)
        {
            if (t.getTT() < minTT) return false;

            return false;
        }
        public void PrintWhereIam(int i)
        {
            Console.WriteLine("This is segmentTTCalc " + segId + " I am attached to " + i+ " I am from "+getFromLoc()+" to "+getToLoc());
        }
        public bool WriteRecord(TravelTime t, int aver)
        {
            if (con.State != ConnectionState.Open) {  OpenStorage(); }
            try
            {
                SqlCommand command;
                command = new SqlCommand("INSERT INTO traveltimes (LinkNo, macAddNo, FinishTime, ttime,averageTT,baseTT) VALUES (@s, @ma, @ft, @tt,@att,@btt) ", con);
                command.Parameters.AddWithValue("@s", (short) segId);
                command.Parameters.AddWithValue("@ma",(long) t.getMA());
                command.Parameters.AddWithValue("@ft", (int)t.getFt());
                command.Parameters.AddWithValue("@tt", (short)t.getTT());
                command.Parameters.AddWithValue("@att",(short) t.getTT()); 
                command.Parameters.AddWithValue("@btt", (short)aver); 
                command.ExecuteNonQuery();
                command.Dispose();

                //"UPDATE devices SET connected=1, timeof=@t WHERE deviceName=@dev", con);
                command = new SqlCommand("UPDATE Links SET currentTT=@tt,CurrentAverageTT=@att,currentBaseTT=@btt,timearrived=@ta WHERE LinkID=@s", con);
                command.Parameters.AddWithValue("@s", (int)segId);
                command.Parameters.AddWithValue("@tt", (short)t.getTT());
                command.Parameters.AddWithValue("@att",(short) aver);
                command.Parameters.AddWithValue("@btt", (short)aver);
                command.Parameters.AddWithValue("@ta", (int)t.getFt());
                command.ExecuteNonQuery();
                command.Dispose();
                return (true);
            }
            catch (Exception ex)
            {
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, ex.ToString()+"Segment TT Not writing to DB - Seg No "+segId);
                return false;
            }

        }
       
    }
}
