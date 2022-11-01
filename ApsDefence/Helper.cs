using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace ApsDefence
{
    public enum State { Pending, Running, Processing, ShutdownRequested, ShutdownComplete };

    /// <summary>
    /// ApsDefence static class available to all
    /// </summary>
    public static class Helper
    {
        private static Config _config = null;

        /// <summary>
        /// Username List<> as set in config file
        /// </summary>
        public static List<string> UsernamePatterns
        {
            get
            {
                if (_config == null)
                {
                    throw new ConfigFileNotLoadedException();
                }

                return _config.AutoBlockUsernames;
            }
        }

        /// <summary>
        /// Block Period as set in config file
        /// </summary>
        public static TimeSpan BlockPeriod
        {
            get
            {
                if (_config == null)
                {
                    throw new ConfigFileNotLoadedException();
                }

                return new TimeSpan(0, _config.BlockPeriod, 0);
            }
        }

        /// <summary>
        /// Max failed logon attempts within Block Period to trigger the IP block as set in config file
        /// </summary>
        public static int MaxFailedLoginAttempts
        {
            get
            {
                if (_config == null)
                {
                    throw new ConfigFileNotLoadedException();
                }

                return _config.MaxFailedLoginAttempts;
            }
        }

        /// <summary>
        /// Load the configuration json file
        /// </summary>
        /// <param name="configFile">Filename of the config file</param>
        public static void LoadConfig(string configFile)
        {
            _config = new Config(configFile);
        }

        /// <summary>
        /// Determine the executuion directory
        /// </summary>
        public static string ExecutionDirectory
        {
            get
            {
                return (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\");
            }
        }

    }
}
