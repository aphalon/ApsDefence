using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApsDefence
{
    public class FirewallRule
    {
        public DateTime Created;
        public string IPAddress;
        public string RuleName;

        /// <summary>
        /// Constructor for FirewallRule
        /// </summary>
        public FirewallRule()
        { }

        /// <summary>
        /// Constructor for FirewallRule
        /// </summary>
        /// <param name="ruleName">Windows Firewall rule in ApsDefence format</param>
        public FirewallRule(string ruleName)
        {
            string[] split = ruleName.Split('^');

            if (split.Length != 3)
            {
                throw new Exception($"Invalid rule format - count = {split.Length}");
            }
            else
            {
                Created = Convert.ToDateTime(split[1]);
                IPAddress = split[2];
                RuleName = ruleName;
            }

        }

        /// <summary>
        /// Friendly object string
        /// </summary>
        /// <returns>Formatted string</returns>
        public override string ToString()
        {
            return $"{Created.ToString("yyyy-MM-dd HH:mm:ss")} | {IPAddress}";
        }
    }
}
