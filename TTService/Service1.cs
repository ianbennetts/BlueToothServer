using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using BlueToothServer;
using System.Runtime.InteropServices;

namespace TTService
{
    public partial class Service1 : ServiceBase
    {
        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

 
        protected override void OnStart(string[] args)
        {
            string lSource = "TT Server";
            string lLog = "TT Server Log";

            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            if (!EventLog.SourceExists(lSource))
                EventLog.CreateEventSource(lSource, lLog);
            EventLog.WriteEntry(lSource, "About to Start ");
            int port = 32001;
            int port1 = 32002;
            SqlCloudConsole sqlConsole = new SqlCloudConsole();
            EventLog.WriteEntry(lSource, "Called Sql ");
            if (sqlConsole.OpenStorage())
            {
                EventLog.WriteEntry(lSource, "Opened SQL ok ");
                Controller s = new Controller(port, sqlConsole);
                Controller s1 = new Controller(port1, sqlConsole);
                sqlConsole.WriteLine(1, 0, "Server Up");
                // BTModBusSlave mbs = new BTModBusSlave();
                //Console.WriteLine("Modbus Running ");
                EventLog.WriteEntry(lSource, "Server Up ");
            }
            else
            {
                EventLog.WriteEntry(lSource, "Failed SQL ");
            }
            

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
        }
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

    }
}
