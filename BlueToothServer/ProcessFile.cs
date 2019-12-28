using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BlueToothServer
{
    class ProcessFile
    {
        DateTime start, finish,current;
        FileStream fs = null;//********************************<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        StreamReader sfr = null;
        string date = null;
        string data = null;
        List<FileData> FileDataList;
        SightingController[] sightingController;
        SightingStorage sightingRec;


        public ProcessFile(Controller c)
        {
            Controller controller= c;
            sightingController = new SightingController[6];
            sightingRec = new SightingStorage();
            sightingRec.OpenStorage(); 
            string[] devices = { "tsite01", "tsite02", "tsite03", "tsite04", "tsite05" };
            int i;
            int counter;
            int numberInList = 0;
            start = new DateTime(2014,01,12);
            finish = new DateTime(2014, 03, 08, 23, 59, 59);
            bool completed = false;
            Sighting s = null;
            current = start;
            completed = (current > finish);
            
            for (i = 1; i < 6; i++)
            {
                sightingController[i] = controller.getSightCont(i);
            }
            {

            }
                while (!completed)
                {
                    date = current.ToString("yyyyMMdd");
                    FileDataList = new List<FileData>(100000);
                    for (i = 0; i < 5; i++)
                    {
                        try
                        {
                            fs = new FileStream(@"c:\ian\" + devices[i] + date, FileMode.Open, FileAccess.Read, FileShare.Read);
                            sfr = new StreamReader(fs);
                            counter = 0;
                            while ((data = sfr.ReadLine()) != null)
                            {
                                Console.WriteLine(data);
                                FileDataList.Add(new FileData(i+1, data));
                                counter++;
                            }
                       /*       Console.WriteLine("Counter = "+date+"  "+devices[i]+" = " + counter);
                               Console.WriteLine("List = " + FileDataList.Count());
                               Thread.Sleep(2000);*/
                        }
                        catch (Exception e)
                        {

                        }


                    }
                    Console.WriteLine("Started sort");
                    FileDataList.Sort();
                    Console.WriteLine("Finished sort");
                    numberInList = FileDataList.Count();
                    
                    for (i = 0; i < numberInList; i++)
  
                   {
                        s = new Sighting(FileDataList.ElementAt(i).getData(), FileDataList.ElementAt(i).getDeviceNo());
                        sightingRec.WriteRecord(s);
                        sightingController[FileDataList.ElementAt(i).getDeviceNo()].addSighting(s);
                        if ((i % 100) == 0) { Thread.Sleep(200); Console.WriteLine(i); }
                    }
                     current = current.AddDays(1);
                    completed = (current > finish);

                }
            Console.WriteLine(completed);
        }
    }
 
}
   class FileData :IComparable
    {
       int deviceNo;
       string data;
        public FileData(int i,string s){
            deviceNo = i;
            data = s;

    }
       public int getDeviceNo(){
           return deviceNo;
       }
       public string getData()
       {
           return data;
       }
       public string getFinishtime()
       {
           return data.Substring(30);
       }
       public int CompareTo(object d){
           FileData otherData = (FileData)d;
           return this.getFinishtime().CompareTo(otherData.getFinishtime());
       }

}

