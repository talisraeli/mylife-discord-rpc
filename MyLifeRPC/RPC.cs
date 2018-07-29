using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPC;
using RPC.Logging;

namespace MyLife
{
    [Serializable]
    public class RPC
    {
        public DiscordRpcClient client;

        public void Initialize(string clientID)
        {
            client = new DiscordRpcClient(clientID, false, -1)
            {
                //Set the logger
                Logger = new ConsoleLogger() { Level = LogLevel.Warning }
            };

            //Subscribe to events
            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };
        }

        public void SetPresence(RichPresence rp)
        {
            client?.SetPresence(rp);
        }
    }
}
