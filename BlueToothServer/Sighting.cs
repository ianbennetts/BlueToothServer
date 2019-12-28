using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueToothServer
{
    public class Sighting // no comments straight forward
    {
        private UInt32 arrived, departed;
        private UInt64 id;
        private string macAdd;
        private int deviceNo;
        public Sighting( string m,int d)
        {
            if (m.Length>26) {convert(m,d);} else {convertXport(m,d);}
        }
        private void convert(string sighting,int d)
        {
            string temp;
            deviceNo = d;
            temp = sighting.Substring(0, 10);
            arrived = Convert.ToUInt32(temp);
            temp = sighting.Substring(sighting.Length - 10, 10);
            departed = Convert.ToUInt32(temp);
            temp = sighting.Substring(11, 2) + sighting.Substring(14, 2) + sighting.Substring(17, 2) + sighting.Substring(20, 2)
                   + sighting.Substring(23, 2) + sighting.Substring(26, 2);
            macAdd = sighting.Substring(10, 18);
            id = Convert.ToUInt64(temp, 16);
        }

        private void convertXport(string sightingSent, int d)
        {
            string[] s;
            string sighting=sightingSent;
            DateTime time;
            long uTime;
            s = sightingSent.Split(' ');
            int dwell = 0;
            if (s.Length == 2)
            {
                dwell = Int32.Parse(s[1]);
                sighting = s[0];
            }
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            try
            {
                time = DateTime.UtcNow;
                uTime = (long)(time - sTime).TotalSeconds;
                string temp;
                deviceNo = d;
                departed = (UInt32)uTime;
                arrived = departed-(UInt32)dwell;

 
                temp = sighting.Substring(0, 2) + sighting.Substring(3, 2) + sighting.Substring(6, 2) + sighting.Substring(9, 2)
                       + sighting.Substring(12, 2) + sighting.Substring(15, 2);
                macAdd = sighting;
                id = Convert.ToUInt64(temp, 16);
            }
            catch (Exception ex) {Console.WriteLine(ex.ToString()); }
        }

        public int getDeviceNo()
        {
            return deviceNo;
        }
        public UInt32 getArrived()
        {
            return arrived;
        }
        public UInt64 getId()
        {
            return id;
        }

        public UInt32 getDeparted()
        {
            return departed;
        }
        public String getMacAdd()
        {
            return macAdd;
        }
        public bool isSame(UInt64 macNo)
        {
            return (id.Equals(macNo));
        }
        public void setID()
        {
            id = 0;
        }
    }
  }
