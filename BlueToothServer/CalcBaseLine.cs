using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace BlueToothServer
{
    public class CalcBaseLine
    {
        DateTime start, finish, current,origin;
        double startUnix, finishUnix,from,to;
        int[,] baseLineTT;
        int[,] baseLineQty;
        int[] minTT;
        //string date = null;
       // string data = null;
        SqlConnection con = null;
        string ConnectionString = Connection.GlobalConnectionStringCloud;
        public CalcBaseLine()
        {
            baseLineTT = new int[20, 2016];
            baseLineQty = new int[8, 2016];
            minTT = new int[8];
            minTT[0] = 140; minTT[1] = 140; minTT[2] = 112; minTT[3] = 168; minTT[4] = 112; minTT[5] = 168; minTT[6] = 500; minTT[7] = 500;
            if (OpenStorage()){
                Calc();
                Console.WriteLine("Finished Calc ");
                summary();
                Console.WriteLine("Finished Summary ");
                calcRoutes();
                Console.WriteLine("Finished calcRoutes ");
                output();
                Console.WriteLine("Finished Output Now going to excel ");
                putInToExcel();
            }

            
        }
        public void putInToExcel()
        {
            // setup
            Excel.Application app;
            Excel.Workbook workbook15;
            Excel.Sheets worksheets;
            Excel.Worksheet[] worksheet;
            Excel.Worksheet summarySheet;
            worksheet = new Excel.Worksheet[7];
            app = new Excel.Application();
            workbook15 = app.Workbooks.Open(@"c:\Users\Ian Bennetts\Dropbox\Theiss\Data\Weekly\Template2.xlsx", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            worksheets = workbook15.Sheets;
            summarySheet = (Excel.Worksheet)worksheets.get_Item("7 Days");
            worksheet[0] = (Excel.Worksheet)worksheets.get_Item("Sunday");
            worksheet[1] = (Excel.Worksheet)worksheets.get_Item("Monday");
            worksheet[2] = (Excel.Worksheet)worksheets.get_Item("Tuesday");
            worksheet[3] = (Excel.Worksheet)worksheets.get_Item("Wednesday");
            worksheet[4] = (Excel.Worksheet)worksheets.get_Item("Thursday");
            worksheet[5] = (Excel.Worksheet)worksheets.get_Item("Friday");
            worksheet[6] = (Excel.Worksheet)worksheets.get_Item("Saturday");
            for (int route = 0; route < 20; route++)
            {
                Console.WriteLine("Route " + (route + 1));
                for (int day = 0; day < 7; day++)
                {
                   for (int t = 0; t < 288; t++)
                    {
                        worksheet[day].Cells[t+2, 3] = baseLineTT[route,t+(288*day)];
                    }

                }
                for (int i = 0; i < 672; i++) {
                    summarySheet.Cells[i + 2, 3] = (baseLineTT[route, i * 3] + baseLineTT[route, i * 3] + baseLineTT[route, (i * 3) + 1] + baseLineTT[route, (i * 3)+2])/3;
                }
                workbook15.SaveAs(@"c:\Users\Ian Bennetts\Dropbox\Theiss\Data\Weekly\Route "+(route+1) + ".xlsx");
            }
            workbook15.Close();
            
            
            //populate route
            // save route

        }
        public void calcRoutes()
        {
            for (int j = 0; j < 2016; j++)
            {
                baseLineTT[9-1, j] = baseLineTT[1-1, j] + baseLineTT[3-1, j];
                baseLineTT[10-1, j] = baseLineTT[1-1, j] + baseLineTT[4-1, j];
                baseLineTT[11-1, j] = baseLineTT[1-1, j] + baseLineTT[4-1, j] + baseLineTT[7-1, j];
                baseLineTT[12-1, j] = baseLineTT[5-1, j] + baseLineTT[2-1, j];
                baseLineTT[13-1, j] = baseLineTT[5-1, j] + baseLineTT[4-1, j];
                baseLineTT[14-1, j] = baseLineTT[5-1, j] + baseLineTT[4-1, j] + baseLineTT[7-1, j];
                baseLineTT[15-1, j] = baseLineTT[4-1, j] + baseLineTT[7-1, j];
                baseLineTT[16-1, j] = baseLineTT[6-1, j] + baseLineTT[2-1, j];
                baseLineTT[17-1, j] = baseLineTT[6-1, j] + baseLineTT[3-1, j];
                baseLineTT[18-1, j] = baseLineTT[8-1, j] + baseLineTT[6-1, j];
                baseLineTT[19-1, j] = baseLineTT[8-1, j] + baseLineTT[6-1, j] + baseLineTT[3-1, j]; 
                baseLineTT[20-1, j] = baseLineTT[8-1, j] + baseLineTT[6-1, j] + baseLineTT[2-1, j];
            }
        }
        public void output()
        {
            SqlCommand command;
            for (int i = 1; i < 21; i++)
            {
                for (int j = 0; j < 2016; j++)
                {
                    command = new SqlCommand("INSERT into baseline SET route=@r, Min5Interval=@t ,BaseTT=@tt", con);
                    command.Parameters.AddWithValue("@r", i);
                    command.Parameters.AddWithValue("@t", j);
                    command.Parameters.AddWithValue("@tt", baseLineTT[i - 1, j]);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void summary()
        {
            
            for (int i = 1; i < 9; i++)
            {
                for (int j = 0; j < 2016; j++)
                {
                    if (baseLineQty[i - 1, j] != 0)
                    {
                        baseLineTT[i - 1, j] = baseLineTT[i - 1, j] / baseLineQty[i - 1, j];                        
                    }
                    else {
                        baseLineTT[i - 1, j] = minTT[i - 1];
                    }
                    if (baseLineTT[i - 1, j] > 1000) baseLineTT[i - 1, j] = 1000;
                    if (j > 1)
                   {
                        baseLineTT[i - 1, j] = ((baseLineTT[i - 1, j] + baseLineTT[i - 1, j - 1] + baseLineTT[i - 1, j - 2])/3);
                    }
                }
             }

        }
        public void Calc()
        {
          int intervalNo;
          int sumTT, noTT,averTT;
          origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
          origin = origin.AddHours(10);
          start = new DateTime(2014, 01, 12);
          current = start;
          finish = new DateTime(2014, 03, 08, 23, 59, 59);
          startUnix = ((TimeSpan) (start - origin)).TotalSeconds;
          finishUnix = ((TimeSpan)(finish - origin)).TotalSeconds;
          SqlCommand command = con.CreateCommand();
          SqlDataReader Reader;
          for (double i = startUnix; i < finishUnix; i = i + 300)
          {
              from = i;
              to = i + 300;
              current = current.AddSeconds(300);
              intervalNo = ((((int)current.DayOfWeek)*24*60*60+current.Hour*60*60+current.Minute*60))/300;
              for (int seg = 1; seg < 9; seg++)
             {
                 sumTT = 0; noTT = 0;
                 command = new SqlCommand("select * from traveltimes where segmentno=@s and finishtime>@st and finishtime<@ft", con);
                 command.Parameters.AddWithValue("@s", seg);
                 command.Parameters.AddWithValue("@st", from);
                 command.Parameters.AddWithValue("@ft", to);
                 Reader=command.ExecuteReader();
                 while (Reader.Read())
                 {
                     sumTT = sumTT + Reader.GetInt16(5);
                     noTT++;
                 }
                 Reader.Close();
                 averTT = 0;
                 if (noTT != 0)
                 {
                     averTT = sumTT / noTT;
                     baseLineTT[seg-1, intervalNo] = baseLineTT[seg-1, intervalNo] + averTT;
                     baseLineQty[seg-1, intervalNo]++;
                    // Console.WriteLine("total = " + sumTT + " no = " + noTT + " averge = " + averTT);
                 }
            }
          }
        }
        private bool OpenStorage()
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
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
