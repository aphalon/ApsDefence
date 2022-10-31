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

        public FirewallRule()
        { }

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

        public override string ToString()
        {
            return $"{Created.ToString("yyyy-MM-dd HH:mm:ss")} | {IPAddress}";
        }
    }
}
