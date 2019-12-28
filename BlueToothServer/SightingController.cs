using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data.SqlClient;
using System.Diagnostics;

namespace BlueToothServer
{
    public class SightingController
    {
        List<Sighting> sightings;
        List<Sighting> checkSightings;
        List<SegmentTTCalc> segTTCalcs;
        Controller controller;
        SqlCloudConsole sqlCloudConsole;
        int id;
        int[] TTto;
        int noTT;
        Thread SightingList;
        SightingController[] sightControllerFrom;
        int[] sightingControllerConnected;
        int[] sightingControllerIds;
        int noSightControllersTo,noSightControllersFrom,noSightControllersConnected;
        bool found;
 
        public SightingController(SqlCloudConsole cc,int i,Controller c)
        {
            int trys = 0;
            bool success;
            id = i;
            controller = c;
            sqlCloudConsole = cc;
            sightings=new List<Sighting>();
            checkSightings = new List<Sighting>();
            segTTCalcs = new List<SegmentTTCalc>();
            sightControllerFrom = new SightingController[100];
            sightingControllerIds=new int[100];
            sightingControllerConnected = new int[100];
            TTto = new int[100];
            noTT = 0;
            noSightControllersTo = 0;
            noSightControllersFrom = 0;
            noSightControllersConnected=0;
            success=this.getSegmentTT();
            while (!success && trys++<3) success = this.getSegmentTT();
            success=this.getFromControllers();
            trys = 0;
            while (!success && trys++ < 3) success = this.getFromControllers();
            SightingList = new Thread(new ThreadStart(run)); // i am in my own thread
            SightingList.Start(); 
        }
        private void run()
        {
            while (true)
            {
                while (sightings.Count > 1200) {
                     sightings.RemoveAt(0);
                }
                Thread.Sleep(50);
                if (noSightControllersTo != noSightControllersConnected) addFromControllers();
                processCheck(); 
             }
        }
        private void addFromControllers(){
            int i;
            SightingController s;
           
            for (i = 0; i < noSightControllersTo; i++)
            {
                if (sightingControllerConnected[i]==0)
                {
                    s = controller.getSightCont(sightingControllerIds[i]);
                    if (s != null)
                    {
                        s.addSendToControllers(this);
                        noSightControllersConnected++;
                        sightingControllerConnected[i] = 1;

                    }
                }
            }
        }
        public void addSighting(Sighting s)
        {
  /*          int count = sightings.Count-1;
            int i;
            for (i=count;i>=0 && (i>count-100);i--){
                 if (sightings[i].getId() == s.getId())
                {
                    sightings.RemoveAt(i);
                }
            }*/
            int i;
            sightings.Add(s);
            for (i = 0; i < noSightControllersFrom; i++)
            {
                sightControllerFrom[i].checkFrom(s);
            }
         }
        public int getId()
        {
            return id;
        }
        public bool addSegmentTTCalc(SegmentTTCalc sttc)
        {
            
            TTto[noTT++]=sttc.getToLoc();
            segTTCalcs.Add(sttc);
            //Console.WriteLine("I am " + id + " SegId " + sttc.getSegId()+" from "+sttc.getFromLoc()+" to "+sttc.getToLoc());
            string lSource = "TT Server";
            EventLog.WriteEntry(lSource, "I am " + id + " SegId " + sttc.getSegId() + " from " + sttc.getFromLoc() + " to " + sttc.getToLoc()+" poss Diff "+sttc.possDiff+" Min TT "+sttc.minTT);
            return true;
        }

        private void printTest()
        {
            int count = segTTCalcs.Count;
            int i;
            for (i = 0; i < count; i++)
            {
                segTTCalcs[i].PrintWhereIam(id);
            }
        }
        private bool getFromControllers()
        {
            SqlConnection con = null;
            string ConnectionString = Connection.GlobalConnectionStringCloud;
            con = new SqlConnection(ConnectionString);
            con.Open();
            SqlCommand command = con.CreateCommand();
            SqlDataReader Reader;
            command.CommandText = "Select * from Links";
            try
            {
                Reader = command.ExecuteReader();

                while (Reader.Read())
                {
                    if (Reader.GetInt16(2) == id) {
                        sightingControllerIds[noSightControllersTo++] = Reader.GetInt16(3);
                    }
                }
                Reader.Close();
                return true;
            }catch(Exception ex)
            {
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, "GetFromController10 "+ex.ToString());
                return false;

            }
        }
        private bool getSegmentTT()
        {
            SqlConnection con = null;
            int k = 0;
            string ConnectionString = Connection.GlobalConnectionStringCloud;
            con = new SqlConnection(ConnectionString);
            con.Open();
            SqlCommand command = con.CreateCommand();
            SqlDataReader Reader;
            command.CommandText = "Select * from Links";
            try
            {
                Reader = command.ExecuteReader();

            while (Reader.Read())
            {
                if (Reader.GetInt16(2) == id)
                {
                    addSegmentTTCalc(new SegmentTTCalc(sqlCloudConsole,Reader.GetInt32(0), Reader.GetInt16(2), Reader.GetInt16(3),Reader.GetByte(8),Reader.GetInt16(9), Reader.GetInt16(10),Reader.GetInt16(5)));
                }
            }
            Reader.Close();
                return true;
            }catch(Exception ex)
            {
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, "GetSegmentTT10 ID= " + id+" reader no = "+ (k++) + ex.ToString());
                return false;
            }
        }
        public void checkFrom(Sighting s)
        {
            lock (this)
            {
                checkSightings.Add(s);
            }
        }
        private void processCheck()
        {
            int count = sightings.Count-1;
            int i;
            if (checkSightings.Count > 10) { Console.WriteLine("my id = " + id + " count =" + checkSightings.Count); }
            while (checkSightings.Count > 0)
            {
             found = false;
             Sighting s=checkSightings[0];
             i = count;
             while (s!=null && i>=0 && !found)
             {
               if (sightings[i]!=null && s.getId() == sightings[i].getId()){
                sendToSegTTCalc(sightings[i], s);
                found = true;
                }
              i--;
              }
            checkSightings.RemoveAt(0);
                
            }
        }
        private void sendToSegTTCalc(Sighting f,Sighting t)
        {
            int i;
            int stt = -1;
            for (i = 0; i < noTT; i++)
            {
                if (t.getDeviceNo() == TTto[i]) { stt = i; }
            }
            if (stt!=-1) {
                segTTCalcs[stt].FoundTT(f, t);
                 
            }else {Console.WriteLine("From sc "+id+" t.deviceNo "+t.getDeviceNo()+" from device No "+f.getDeviceNo()+" TTto[0] "+ TTto[0]);}
        }
        public void addSendToControllers(SightingController s)
        {
            sightControllerFrom[noSightControllersFrom++]=s;
        }
        public void printTest(int j)
        {
            /*
            string lSource = "TT Server";
            string info = "";
            for (int i = 0; i < noSightControllersFrom; i++) {info=info+(sightControllerFrom[i].getId() + " "); }
            EventLog.WriteEntry(lSource, "I am SC " + id + " and device no " + j + " is connected to me " + " my froms are "+info);
            Console.WriteLine();
            */
        }
 
     }
}
