using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;

namespace ApsDefence
{
    public class RDPDefender : IDisposable
    {
        private EventLogQuery _subscriptionQuery = null;
        private EventLogWatcher _watcher = null;

        public RDPDefender(string configFile)
        {
            Helper.LoadConfig(configFile);
            Init();
        }

        public void Dispose()
        {
            // Stop listening to events
            _watcher.Enabled = false;

            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        private void Init()
        {
            try
            {
                _subscriptionQuery = new EventLogQuery("Security", PathType.LogName, "*[System/EventID=4625]");

                _watcher = new EventLogWatcher(_subscriptionQuery);
                _watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(EventLogEventRead);

                // Activate the subscription
                _watcher.Enabled = true;
                Logger.Log("Listening on Security log for new Events");
            }
            catch (EventLogReadingException e)
            {
                Logger.Log($"Error reading the log: {e.Message}");
            }
        }

        private void EventLogEventRead(object obj, EventRecordWrittenEventArgs arg)
        {
            // Make sure there was no error reading the event.
            if (arg.EventRecord != null)
            {

                EventRecord eventRecord = ((EventRecordWrittenEventArgs)arg).EventRecord;

                Logger.Debug($"Received {eventRecord.Id}/{string.Join("|", eventRecord.KeywordsDisplayNames)}");

                if (eventRecord.KeywordsDisplayNames.Any(t => t == "Audit Failure"))
                {
                    string fullUsername = $"{eventRecord.Properties[6].Value}\\{eventRecord.Properties[5].Value}";
                    if (fullUsername.StartsWith("\\"))
                    {
                        fullUsername = "." + fullUsername;
                    }

                    LogonAttempts.AddDetectedLogonAttempt(new LogonAttempt(Convert.ToDateTime(eventRecord.TimeCreated), eventRecord.Properties[19].Value.ToString(), fullUsername));

                }
            }
            else
            {
                Logger.Log("The event instance was null.");
            }
        }

    }
}
