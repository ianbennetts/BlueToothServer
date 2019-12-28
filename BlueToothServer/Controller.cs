using System;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Collections.Generic;


namespace BlueToothServer
{
    public class Controller
    {
        private SqlCloudConsole sqlConsole;
        List<SightingController> sightingControllerList;

        public Controller(int i,SqlCloudConsole sql) // i control everything
        {
            int port = i;
            sqlConsole = sql;
            sightingControllerList = new List<SightingController>();
            ServerListener s = new ServerListener(port,this); // start the server

            //ProcessFile p = new ProcessFile(this);
            //CalcBaseLine c = new CalcBaseLine();
           // CalcActuals c = new CalcActuals();
        }

        public void WriteLine(int errorno, int deviceno, string s)
        {
            lock (this)
            {
                sqlConsole.WriteLine(errorno, deviceno, s);
            }
        }
 


        public SightingController getSightCont(int deviceNo){
            lock (this)
            {
                int noofControllers = sightingControllerList.Count;
                int i;
                for (i = 0; i < noofControllers; i++)
                {
                    if (sightingControllerList[i].getId() == deviceNo) return (sightingControllerList[i]);
                }
                sightingControllerList.Add(new SightingController(sqlConsole,deviceNo,this));
                return sightingControllerList[i];
            }
        }
        public void PassSegTTCalctoLocFrom(SegmentTTCalc sttc)
            {   
                lock(this){
                    int toL = sttc.getToLoc();    
                }
            }
 
        }

}
