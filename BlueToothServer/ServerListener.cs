using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Configuration;


namespace BlueToothServer
{
    public class ServerListener
    {
        TcpListener listener;
        int port;
        Controller controller;

     public ServerListener(int i,Controller c)
        {
            port = i;
            controller = c;
            controller.WriteLine(0, 99, "In ServerListener " + port);
            Thread serverThread = new Thread(new ThreadStart(Listen)); // I listen in my own thread
            serverThread.Start();
            controller.WriteLine(0, 99, "Started Thread " + port);
            Console.WriteLine("Started Thread ");
        }
     public void Listen()
     {
         listener = new TcpListener(IPAddress.Any, port);
         listener.Start(); 
         while (true)
         {
             controller.WriteLine(0,99,"Listenering on port "+port);
             Socket soc = listener.AcceptSocket(); // sit here awaiting connection
             DeviceConnectionXport devCon = new DeviceConnectionXport(soc,controller); // have a connection start a dedicated connection in its own thread. socket and controller ref
         }
     }
    }
}
