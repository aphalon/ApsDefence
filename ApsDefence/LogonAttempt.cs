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

        public LogonAttempt()
        { }

        public LogonAttempt(DateTime attemptTime, string ipAddress, string username)
        {
            AttemptTime = attemptTime;
            IPAddress = ipAddress;
            Username = username;
        }

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

        public override string ToString()
        {
            return $"{AttemptTime.ToString("yyyy-MM-dd HH:mm:ss")} | {IPAddress} | {Username} | {IsUsernameOnBannedList}";
        }
    }
}
