using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace ApsDefence
{
    public class Config
    {
        [JsonProperty("blockPeriod")]
        public int BlockPeriod = 60;
        [JsonProperty("maxFailedLoginAttempts")]
        public int MaxFailedLoginAttempts = 3;
        [JsonProperty("autoBlockUsernames")]
        public List<string> AutoBlockUsernames = new List<string>();

        public Config() { }

        public Config(string configFile)
        {
            if (File.Exists(configFile))
            {
                // Load the config file into a temp object
                Config conf = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFile));

                // Copy the contents of the temp object to "this" one
                var fields = typeof(Config).GetFields().ToList();
                foreach (var field in fields)
                {
                    field.SetValue(this, field.GetValue(conf));
                }

                if (AutoBlockUsernames == null)
                {
                    AutoBlockUsernames = new List<string>();
                }
            }
            else
            {
                Logger.Warning($"Config file '{configFile}' does not exist - using default values");
            }
        }

    }
}
