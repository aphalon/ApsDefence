using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApsDefence
{
    public class LogonAttempt
    {
        public DateTime AttemptTime;
        public string IPAddress;
        public string Username;

        /// <summary>
        /// Constructor
        /// </summary>
        public LogonAttempt()
        { }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="attemptTime">Time that the logon was received</param>
        /// <param name="ipAddress">Remote IP address</param>
        /// <param name="username">Username that the logon attempt used</param>
        public LogonAttempt(DateTime attemptTime, string ipAddress, string username)
        {
            AttemptTime = attemptTime;
            IPAddress = ipAddress;
            Username = username;
        }

        /// <summary>
        /// Check whether the username included is on the list of auto-ban users
        /// </summary>
        public bool IsUsernameOnBannedList
        {
            get
            {
                string[] split = Username.Split('\\');
                foreach (string pat in Helper.UsernamePatterns)
                {
                    Match m = Regex.Match(split[1], pat, RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Friendly object string
        /// </summary>
        /// <returns>Formatted string</returns>
        public override string ToString()
        {
            return $"{AttemptTime.ToString("yyyy-MM-dd HH:mm:ss")} | {IPAddress} | {Username} | {IsUsernameOnBannedList}";
        }
    }
}
