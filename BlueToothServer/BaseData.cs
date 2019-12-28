using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace BlueToothServer
{
    public class BaseData
    {

        public BaseData()
        {
        }
    
    public int[][,] getBaseData(){
        int[][,] b = new int[3][,];
        SqlConnection con = null;
        con = new SqlConnection(Connection.GlobalConnectionStringCloud);
        con.Open();
        SqlCommand command = con.CreateCommand();
        SqlDataReader Reader;
        b[0] = null;
        b[1]= new int[8,2016];
        command.CommandText = "Select * from BaseData where BaseId=1 order by linkno,interval";
        Reader = command.ExecuteReader();

        while (Reader.Read())
        {
            b[1][Convert.ToInt32(Reader["LinkNo"]) - 1, Convert.ToInt32(Reader["Interval"]) - 1] = Convert.ToInt32(Reader["Seconds"]);
        }
        Reader.Close();

        b[2] = new int[16, 2016];
        command.CommandText = "Select * from BaseData where BaseId=2 order by linkno,interval";
        Reader = command.ExecuteReader();

        while (Reader.Read())
        {
            b[2][Convert.ToInt32(Reader["LinkNo"]) - 1, Convert.ToInt32(Reader["Interval"]) - 1] = Convert.ToInt32(Reader["Seconds"]);
        }
        Reader.Close();
        con.Close();

        return b;

    
       }
    }

}
