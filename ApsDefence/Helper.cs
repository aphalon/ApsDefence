using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace ApsDefence
{
    public static class Helper
    {
        private static Config _config = null;

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

        public static void LoadConfig(string configFile)
        {
            _config = new Config(configFile);
        }

        public static string ExecutionDirectory
        {
            get
            {
                return (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\");
            }
        }

    }
}
