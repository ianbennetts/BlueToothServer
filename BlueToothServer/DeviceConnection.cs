using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Diagnostics;

namespace BlueToothServer
{
    public class DeviceConnection
    {
        Socket soc;
        Controller controller;
        SightingController sightingController;
        int deviceNo;
        public DeviceConnection(Socket s,Controller c) // listener has connection and created socket and me
        {
            soc = s;
            controller = c;
            Thread serverThread = new Thread(new ThreadStart(startConnection)); // i am in my own thread
            serverThread.Start(); 
        }
        private void startConnection(){
        string site = null; 
        Sighting sighting;
        DeviceStorage device = new DeviceStorage();
        SightingStorage sightingRec = new SightingStorage();
        bool wroteOK;
        FileStream fs = null;//********************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        StreamWriter sfw = null;//*****************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        try {
                
            Stream s = new NetworkStream(soc);
            s.ReadTimeout = 30000; //if nothing from sensor for 11 secs close
            StreamReader sr = new StreamReader(s);
            StreamWriter sw = new StreamWriter(s);
            sw.AutoFlush = true; // enable automatic flushing
            String data; //data from sensor
            bool valid=false;
            site = sr.ReadLine(); // read first data from sensor should be device name 
            DateTime curDate = DateTime.Now;//****************************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            int day = curDate.Day;//**************************************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            int newDay = 0;//********************************************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            String date = curDate.ToString("yyyyMMdd");//********************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            fs = new FileStream(@"c:\BTData\" + site + date, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);//***************<<<<<<<<<<<<<<<
            sfw = new StreamWriter(fs);//***********************<<<<<<<<<<<<<<<<<<<<<<<<
            sfw.AutoFlush = true;//*****************************<<<<<<<<<<<<<<<<<<<<<
            device.OpenStorage(); // open sql storage to read device information
            deviceNo = device.IsDevice(site); // is this sensor device in the system or is it a fake
            if (deviceNo!=-1) //if not fake 
            {
                device.DeviceUp(site); // tell sql that sensor up
                device.Close();
                valid = true;
                sightingRec.OpenStorage(); // open sql storeage of sightings from this sensor
                sightingController=controller.getSightCont(deviceNo);
                Console.WriteLine("device No " + deviceNo + " site " + site + "Sighting Controller id " + sightingController.getId());
             } else valid=false;
            while (valid) //wont became invalid
            {
                data = sr.ReadLine();
                newDay = DateTime.Now.Day;//******************>>>>>>>>>>>>>>>>>>>
                if (newDay != day)//************************>>>>>>>>>>>>>>>>>>>>>>>>>>>
                {
                    // close file
                    Console.WriteLine("days dont match " + day + newDay);
                    if (sfw != null) sfw.Close();
                    if (fs != null) fs.Close();
                    //open new file
                    day = newDay;
                    curDate = DateTime.Now;
                    date = curDate.ToString("yyyyMMdd");
                    fs = new FileStream(@"c:\BTData\" + site + date, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    sfw = new StreamWriter(fs);
                    sfw.AutoFlush = true;
                  
                }
                if (data != "Done") 
                {
                    sfw.WriteLine(data);
                    sighting = new Sighting(data,deviceNo);
                    wroteOK = sightingRec.WriteRecord(sighting);
                    sightingController.addSighting(sighting);
                    //sfw.WriteLine(data);
                    //sightingController.printTest(deviceNo);
                 }
                if (data == "Done") { sw.WriteLine(data); } // this is heart beat from sensor then send heart beat to sensor
                if (data == "" || data == null) break; //should never receive null
            }
            s.Close();
        }catch (Exception ex){
                string lSource = "TT Server";
                EventLog.WriteEntry(lSource, "Failed in DevceConnection Try");
                soc.Close();
            }
      
        device.OpenStorage(); // tell sql device down
        device.DeviceDown(site);
        device.Close();
        sightingRec.Close();
        controller.WriteLine(1, deviceNo, "Disconnected = "+site); 
        soc.Close();
        if (sfw != null) sfw.Close();
        if (fs != null) fs.Close();
        }
    }
}
