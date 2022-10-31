using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ApsDefence
{
    public static class LogonAttempts
    {
        static object _locker = new object();
        static List<LogonAttempt> _logonAttempts = new List<LogonAttempt>();
        private static Timer _expiredTimer = new Timer();

        static LogonAttempts()
        {
            // Start timer

            _expiredTimer.Elapsed += new ElapsedEventHandler(CheckForExpiredLogonAttempts);
            _expiredTimer.Interval = 60000;

            _expiredTimer.Start();

        }

        private static void CheckForExpiredLogonAttempts(object source, ElapsedEventArgs e)
        {
            _expiredTimer.Stop();

            Logger.Debug($"Checking for expired logon attempts (> {Helper.BlockPeriod.TotalMinutes} mins)");

            // Remove anything older than the period of interest
            lock (_locker)
            {
                _logonAttempts.RemoveAll(t => t.AttemptTime < DateTime.Now.Subtract(Helper.BlockPeriod));
            }

            _expiredTimer.Start();
        }

        public static void AddDetectedLogonAttempt(LogonAttempt logonAttempt)
        {
            bool blockIP = false;
            // Check if the attempt is an auto-add to firewall

            if (logonAttempt.IsUsernameOnBannedList)
            {
                blockIP = true;
            }

            lock (_locker)
            {
                _logonAttempts.Add(logonAttempt);
            }

            // Check if the IP Address has occurred too often

            lock (_locker)
            {
                if (_logonAttempts.Count(t => t.IPAddress == logonAttempt.IPAddress) >= Helper.MaxFailedLoginAttempts)
                {
                    blockIP = true;
                }
            }

            if (blockIP)
            {
                FirewallRules.AddInboundBlockRule(logonAttempt.IPAddress);
                RemoveIPFromCache(logonAttempt.IPAddress);
            }
        }

        public static void AddDetectedLogonAttempt(DateTime timeCreated, string ipAddress, string fullUsername)
        {
            AddDetectedLogonAttempt(new LogonAttempt(timeCreated, ipAddress, fullUsername));
        }

        public static void RemoveIPFromCache(string ipAddress)
        {
            lock (_locker)
            {
                _logonAttempts.RemoveAll(t => t.IPAddress == ipAddress);
            }
        }
    }
}
