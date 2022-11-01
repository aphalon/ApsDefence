using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetFwTypeLib;
using System.Timers;

namespace ApsDefence
{
    public static class FirewallRules
    {
        private static object _locker = new object();
        private static List<FirewallRule> _firewallRules = new List<FirewallRule>();
        private static int _lastCacheReloadHour = -1;
        private static Timer _expiredTimer = new Timer();
        private static int _processingCount = 0;
        private static bool _startupComplete = false;

        /// <summary>
        /// Constructor for the static class
        /// </summary>
        static FirewallRules()
        {
            // Start timer

            _expiredTimer.Elapsed += new ElapsedEventHandler(CheckForExpiredRules);
            _expiredTimer.Interval = 60000;

            _expiredTimer.Start();
        }

        /// <summary>
        /// Check for rules (from cache) that have exceeded the BlockPeriod set in the configuration file
        /// </summary>
        /// <param name="source">Event source</param>
        /// <param name="e">Event parameters</param>
        public static void CheckForExpiredRules(object source, ElapsedEventArgs e)
        {
            try
            {
                _processingCount++;
                _expiredTimer.Stop();

                // Check if we need to do the initial load of the cache or reload to ensure it's in check
                if (!_startupComplete)
                {
                    Logger.Debug($"Loading rules cache");
                    BuildCache();
                    _startupComplete = true;
                }

                if (_lastCacheReloadHour != DateTime.Now.Hour)
                {
                    Logger.Debug($"Reloading rules cache");
                    BuildCache();
                }

                Logger.Debug($"Checking for expired rules (> {Helper.BlockPeriod.TotalMinutes} mins)");

                List<string> rulesToDelete = new List<string>();

                foreach (var rule in _firewallRules.Where(t => t.Created < DateTime.Now.Subtract(Helper.BlockPeriod)))
                {
                    rulesToDelete.Add(rule.RuleName);
                }

                foreach (string ruleName in rulesToDelete)
                {
                    DeleteInboundRule(ruleName);
                }

                _expiredTimer.Start();

            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking expired rules: {ex.Message}");
            }
            finally
            {
                _processingCount--;
            }
        }

        /// <summary>
        /// Prepare rules cache etc...
        /// </summary>
        public static void Startup()
        {
            // Fire the check code
            CheckForExpiredRules(null, null);
        }

        /// <summary>
        /// Stop Firewall processing ready for process shutdown
        /// </summary>
        public static void StopProcessing()
        {
            _expiredTimer.Stop();
            int maxWaits = 10;
            int waits = 0;

            Logger.Log("Stopping Firewall processing");
            
            while (_processingCount > 0)
            {
                System.Threading.Thread.Sleep(500);
                if (waits > maxWaits)
                {
                    Logger.Warning("Max wait for Firewall processing exceeded - terminating anyway");
                    break;
                }
                Logger.Log($"Waiting for Firewall processing jobs to complete - currently {_processingCount} active");
            }
        }

        /// <summary>
        /// Build cache from current Firewall rules
        /// </summary>
        private static void BuildCache()
        {
            _lastCacheReloadHour = DateTime.Now.Hour;

            try
            {
                _processingCount++;
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name.StartsWith("ApsDefence"))
                    {
                        FirewallRule fr = new FirewallRule(rule.Name);
                        AddToRuleCache(fr);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error listing Firewall rules - {ex.Message}");
            }
            finally
            {
                _processingCount--;
            }
        }

        /// <summary>
        /// Add cache entry
        /// </summary>
        /// <param name="rule">FirewallRule object to add</param>
        private static void AddToRuleCache(FirewallRule rule)
        {
            if (!_firewallRules.Any(t => t.RuleName == rule.RuleName))
            {
                lock (_locker)
                {
                    _firewallRules.Add(rule);
                    Logger.Log($"Added rule '{rule.RuleName}'");
                }
            }
        }

        /// <summary>
        /// Delete cache entry
        /// </summary>
        /// <param name="rule">FirewallRule object to remove</param>
        private static void DeleteFromRuleCache(FirewallRule rule)
        {
            lock (_locker)
            {
                _firewallRules.Remove(rule);
            }
        }

        /// <summary>
        /// Delete cache entry
        /// </summary>
        /// <param name="ruleName">Name of the rule to remove</param>
        private static void DeleteFromRulesCache(string ruleName)
        {
            FirewallRule rule = _firewallRules.Where(t => t.RuleName == ruleName).Single();
            DeleteFromRuleCache(rule);
        }

        /// <summary>
        /// Add Inbound Firewall rule to block all activity from the IP
        /// </summary>
        /// <param name="remoteIPAddr">Remote IP address to block</param>
        public static void AddInboundBlockRule(string remoteIPAddr)
        {
            try
            {
                _processingCount++;

                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);
                var currentProfiles = fwPolicy2.CurrentProfileTypes;

                // Let's create a new rule
                INetFwRule2 inboundRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                inboundRule.Enabled = true;
                inboundRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                inboundRule.RemoteAddresses = remoteIPAddr;
                inboundRule.Name = $"ApsDefence^{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}^{remoteIPAddr}";
                inboundRule.Profiles = currentProfiles;
                inboundRule.Grouping = "ApsDefence";

                // Now add the rule
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                firewallPolicy.Rules.Add(inboundRule);

                // Update the rules cache
                AddToRuleCache(new FirewallRule(inboundRule.Name));

            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to AddInboundRule: {ex.Message}");
            }
            finally
            {
                _processingCount--;
            }
        }

        /// <summary>
        /// Delete Firewall rule
        /// </summary>
        /// <param name="ruleName">Name of rule as seen in Windows Firewall</param>
        public static void DeleteInboundRule(string ruleName)
        {
            try
            {
                _processingCount++;
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name == ruleName)
                    {
                        // Remove a rule
                        INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                        firewallPolicy.Rules.Remove(rule.Name);
                        Logger.Log($"Deleted rule '{rule.Name}'");
                        // Update the rules cache
                        DeleteFromRulesCache(ruleName);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error deleting a Firewall rule '{ruleName}' - {ex.Message}");
            }
            finally
            {
                _processingCount--;
            }
        }

        /// <summary>
        /// Output rules cache to screen
        /// </summary>
        public static void DisplayRulesCache()
        {
            foreach (FirewallRule r in _firewallRules.OrderBy(t => t.Created))
            {
                Logger.Log(r.ToString());
            }
        }
    }
}
