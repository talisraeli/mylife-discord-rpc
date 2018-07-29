using RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLife
{
    [Serializable]
    public class Config
    {
        public bool IsOn { get; set; }
        [NonSerialized]
        public RPC Client;
        public string ClientId { get; set; }
        public Dictionary<string, RichPresence> Templates { get; set; }

        public Config()
        {
            IsOn = false;
            Client = new RPC();
            Templates = new Dictionary<string, RichPresence>(StringComparer.CurrentCulture);
            Templates.Add("Custom", new RichPresence()
            {
                Details = "I'm using MyLife RP!",
                State = "Made by KeshetBehanan#6796",
                Timestamps = new Timestamps()
                {
                    Start = DateTime.UtcNow
                },
                Assets = new Assets(),
                Party = new Party()
            });
        }
    }
}