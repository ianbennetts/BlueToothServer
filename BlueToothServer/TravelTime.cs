using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueToothServer
{
    public class TravelTime
    {
        int from, to, travelTime;
        UInt64 macAddress;
        UInt32 startTime,finishTime;
        public TravelTime(int f, int t, int tt, UInt32 st, UInt32 ft, UInt64 ma)
        {
            from = f;
            to=t; 
            startTime=st;
            travelTime = tt;
            macAddress = ma;
            finishTime = ft;
        }
        public TravelTime(int tt)
        {
            travelTime = tt;
            from = 0;
            to = 0;
            macAddress = 0;
            startTime = 0;
            finishTime = 0;
        }
        public int getFrom()
        {
            return from;
        }
        public int getTo()
        {
            return to;
        }
        public int getTT()
        {
            return travelTime;
        }
        public UInt32 getFt()
        {
            return finishTime;
        }
        public UInt32 getSt()
        {
            return startTime;
        }

        public UInt64 getMA()
        {
            return macAddress;
        }
        public void setTT(int tt)
        {
            travelTime = tt;
        }

    }
}