using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using ApsDefence;
using System.IO;

namespace ApsDefenceService
{
    public partial class TheService : ServiceBase
    {
        RDPDefender _defender = null;
        public TheService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Startup code
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {

            Logger.Log($"Execution direction = {Helper.ExecutionDirectory}");
            Directory.SetCurrentDirectory(Helper.ExecutionDirectory);

            _defender = new RDPDefender(Path.Combine(Helper.ExecutionDirectory, "ApsDefence.conf"));

        }

        /// <summary>
        /// Shutdown code
        /// </summary>
        protected override void OnStop()
        {
            Logger.Log("Service Stop requested...");
            if (_defender != null)
            {
                _defender.StopProcessing();
            }
            Logger.Close();
        }
    }
}
