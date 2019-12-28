using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlueToothServer
{
    public class ValidTT
    {
        int minTT, maxTT,baseNo,startTT,link;
        List<TravelTime> tt;
        int noofRejects = 0;
        int averjmp = 0;
        SqlCloudConsole sqlConsole;
        EmailConsole emailConsole;
        bool stateExtreme = false;
        int[] baseData=new int[2016];
        string[] state = { "No Issue", "Issue", "Moderate", "Heavy", "Extreme" };
        float[] increase = {1.5F,2.0F,3.0F,4.0F };
        float[] decrease = {1.0F, 1.1F, 1.1F, 1.5F, 2.0F };
        int level = 0;
        uint increaseTime;
 //       public ValidTT(int l,int [,] bd,int m, int mt,int b,int s)
        public ValidTT(SqlCloudConsole cc,int l, int[,] bd, int m, int mt, int b, int s)
        {
            tt=new List<TravelTime>();
            minTT = m;
            maxTT = mt;
            baseNo = b;
            link = l;
            if (l > 8) { l = l - 9; }
            startTT = s;
            sqlConsole = cc;
            if (baseNo != 0)
            {
                for (int i = 0; i < 2016; i++)
                {
                    baseData[i] = bd[l - 1, i];
                }
                for (int i = 0; i < 10; i++)
                {
                    tt.Add(new TravelTime(s));
                }
                emailConsole = sqlConsole.getEmailConsole();
            }
        }
        public int isValid(TravelTime t)
        {
            int aver = 0;
            if (t == null) return -1;
            if (t.getTT() < minTT) return -1;
            if (t.getTT() > maxTT) return -1;
            if (tt.Count < 10)
            {
                tt.Add(t);
                return t.getTT();
            }
            tt.Add(t);
            aver=getAverTest(tt);
            if (aver == -1) { tt.RemoveAt(10); return aver; } else { tt.RemoveAt(0); }
            if (baseNo != 0) { checkForCongestion(t, aver); }
/*            if (aver>(baseData[timeInterval]*1.85) && !stateExtreme)
            {
                stateExtreme=true;
                sqlConsole.WriteLine(6,link,"Congestion at "+t.getFt());

            }*/
            return aver;
        }
        private int getAverTest(List<TravelTime> tt)
        {
            int total = 0;
            int aver = 0;
            int i= 0;
            for (i=0;i<11;i++){
                total += tt[i].getTT();
            }
            aver= total/11;
            //if ((aver*1.2)<tt[10].getTT() && (aver+possDiff)<tt[10].getTT() && (tt[10].getSt()<tt[9].getSt())) {return -1;}
            if ((aver * 1.8) < tt[10].getTT()) {
                if (noofRejects != 10)
                {
                    noofRejects++;
                    averjmp = averjmp + tt[10].getTT();
                    return -1;
                }else{
                    aver = (int)(averjmp / 10);
                    for (int j = 0; j < 11; j++)
                    {
                        tt[j].setTT(aver);
                    }
                }
            }
           // if ((aver / 1.2) > tt[10].getTT() && (aver - possDiff) > tt[10].getTT()) { return -1; }
            noofRejects = 0;
            averjmp = 0;
            return aver;
        }
        private void checkForCongestion(TravelTime tt,int tTime)
        {   
            int day;
            int timeInterval;
            uint t = tt.getFt();
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(t + (10 * 60 * 60));
            if (baseNo == 1) { day = ((int)dateTime.DayOfWeek == 0) ? 7 : (int)dateTime.DayOfWeek; }
            else { 
                day = (int)dateTime.DayOfWeek+1;
                //if (day == 8) { day = 1; }
            }
            timeInterval = (((day - 1) * 288) + (dateTime.Hour * 12) + (dateTime.Minute / 5));
 //           if ((level != 4) && (tTime > (baseData[timeInterval]* increase[level]) ))
            if ((level != 4) && (checkEitherSide(timeInterval,tTime)))
            {
                level++;
                increaseTime = t;
                sqlConsole.WriteLine(10 + level, link, "increase at "+dateTime.ToString("dd/MM/yyyy HH:mm:ss.fff"));
                emailConsole.addEmailRequest(link, level + 10, dateTime.ToString("dd/MM/yyyy HH:mm:ss.fff"));
            }
            else
            {
                if ((level!=0) && (t>(increaseTime+3600)) && (tTime < (decrease[level]*baseData[timeInterval]))){
                    level--;
                    sqlConsole.WriteLine(10 + level, link, "decrease at " + dateTime.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                
                }
            }

            return ;
        }
        private bool checkEitherSide(int ti,int tt)
        {
            int from = ti - 6;
            int to = ti + 6;
            if (ti < 6 || ti > 2010)
            {
                return (tt > (baseData[ti] * increase[level]));
            }
            for (int i = from; i < to; i++)
            {
                if (!(tt> (baseData[i]*increase[level]))){
                    return false;
                }
            }
            return true;
        }
    }
}
