using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime;
namespace BlueToothServer
{
    public static class Connection
    {
       // public static string GlobalConnectionString = @"Server=localhost;Database=scadatt;Uid=root;" + "Pwd=seahorse99";
    //  public static string GlobalConnectionStringCloud = @"Data Source=tcp:sic-australia.database.windows.net;Database=ScadaTT_Staging;User ID = admin-xw7@sic-australia;Password=L]Z2JMh^UTM7;Trusted_Connection=False;Encrypt=True";
//        public static string GlobalConnectionStringCloud = @"Server = ALIENWARE-PC\SQLEXPRESS; Database = ScadaTT_Staging; Trusted_Connection = True";
        public static string GlobalConnectionStringCloud = @"Server =MONITORTT; Database = ScadaTT_Staging; Trusted_Connection = True;Max Pool Size=500; Connection Timeout=60";
        public static int[][,] db = new BaseData().getBaseData();

    }

    
    class Program
    {

        static void Main(string[] args)
        {
            int port = 32001;
            int port1 = 32002;
            Console.WriteLine("Started");
            SqlCloudConsole sqlConsole=new SqlCloudConsole();
            if (sqlConsole.OpenStorage()){
                Controller s = new Controller(port,sqlConsole);
                Controller s1 = new Controller(port1, sqlConsole);
                sqlConsole.WriteLine(0, 100, "Server Up");
  //              BTModBusSlave mbs = new BTModBusSlave();
  //              Console.WriteLine("Modbus Running ");
                
            }
         }
 
    }
}