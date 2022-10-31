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

        static FirewallRules()
        {
            BuildCache();

            // Start timer

            _expiredTimer.Elapsed += new ElapsedEventHandler(CheckForExpiredRules);
            _expiredTimer.Interval = 60000;

            _expiredTimer.Start();
        }


        public static void CheckForExpiredRules(object source, ElapsedEventArgs e)
        {
            _expiredTimer.Stop();

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

        private static void BuildCache()
        {
            _lastCacheReloadHour = DateTime.Now.Hour;

            try
            {
                Type tNetFwPolicy2 = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                INetFwPolicy2 fwPolicy2 = (INetFwPolicy2)Activator.CreateInstance(tNetFwPolicy2);

                foreach (INetFwRule rule in fwPolicy2.Rules)
                {
                    if (rule.Name.StartsWith("ApsDefence"))
                    {
                        FirewallRule fr = new FirewallRule(rule.Name);
                        AddToRulesCache(fr);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error listing Firewall rules - {ex.Message}");
            }
        }

        private static void AddToRulesCache(FirewallRule rule)
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

        private static void DeleteFromRulesCache(FirewallRule rule)
        {
            lock (_locker)
            {
                _firewallRules.Remove(rule);
            }
        }

        private static void DeleteFromRulesCache(string ruleName)
        {
            FirewallRule rule = _firewallRules.Where(t => t.RuleName == ruleName).Single();
            DeleteFromRulesCache(rule);
        }

        public static void AddInboundBlockRule(string remoteIPAddr)
        {

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
            AddToRulesCache(new FirewallRule(inboundRule.Name));
        }

        public static void DeleteInboundRule(string ruleName)
        {
            try
            {
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
        }

        public static void DisplayRulesCache()
        {
            foreach (FirewallRule r in _firewallRules.OrderBy(t => t.Created))
            {
                Logger.Log(r.ToString());
            }
        }
    }
}
