using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ApsDefence;

namespace ApsDefenceService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            Logger.Log("Entered Main");
            ServicesToRun = new ServiceBase[]
            {
                new TheService()
            };
            Logger.Log("Starting service thread");
            ServiceBase.Run(ServicesToRun);
            Logger.Log("Started");
        }
    }
}
