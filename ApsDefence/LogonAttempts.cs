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
        private static int _processingCount = 0;

        /// <summary>
        /// Static constructor 
        /// </summary>
        static LogonAttempts()
        {
            // Start timer

            _expiredTimer.Elapsed += new ElapsedEventHandler(CheckForExpiredLogonAttempts);
            _expiredTimer.Interval = 60000;

            _expiredTimer.Start();

        }

        /// <summary>
        /// Check cache for expired logon attempts and cleanup
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="e">Event parameters</param>
        private static void CheckForExpiredLogonAttempts(object source, ElapsedEventArgs e)
        {
            try
            {
                _processingCount++;
                _expiredTimer.Stop();

                Logger.Debug($"Checking for expired logon attempts (> {Helper.BlockPeriod.TotalMinutes} mins)");

                // Remove anything older than the period of interest
                lock (_locker)
                {
                    _logonAttempts.RemoveAll(t => t.AttemptTime < DateTime.Now.Subtract(Helper.BlockPeriod));
                }

                _expiredTimer.Start();

            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to check for expired logons: {ex.Message}");
            }
            finally
            {
                _processingCount--;
            }
        }

        /// <summary>
        /// Add detected logon attempt and block if needed
        /// </summary>
        /// <param name="logonAttempt">LogonAttempt object</param>
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

        /// <summary>
        /// Stop Logon processing ready for process shutdown
        /// </summary>
        public static void StopProcessing()
        {
            _expiredTimer.Stop();
            int maxWaits = 10;
            int waits = 0;

            Logger.Log("Stopping Logon processing");

            while (_processingCount > 0)
            {
                System.Threading.Thread.Sleep(500);
                if (waits > maxWaits)
                {
                    Logger.Warning("Max wait for Logon processing exceeded - terminating anyway");
                    break;
                }
                Logger.Log($"Waiting for Logon processing jobs to complete - currently {_processingCount} active");
            }
        }

        /// <summary>
        /// Add detected logon attempt and block if needed
        /// </summary>
        /// <param name="timeCreated">Time that the logon was received</param>
        /// <param name="ipAddress">Remote IP address</param>
        /// <param name="fullUsername">Username that the logon attempt used</param>
        public static void AddDetectedLogonAttempt(DateTime timeCreated, string ipAddress, string fullUsername)
        {
            AddDetectedLogonAttempt(new LogonAttempt(timeCreated, ipAddress, fullUsername));
        }

        /// <summary>
        /// Remove IP address from cache
        /// </summary>
        /// <param name="ipAddress">IP Address</param>
        private static void RemoveIPFromCache(string ipAddress)
        {
            lock (_locker)
            {
                _logonAttempts.RemoveAll(t => t.IPAddress == ipAddress);
            }
        }
    }
}
