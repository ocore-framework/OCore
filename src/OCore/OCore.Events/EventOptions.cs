﻿using System;
using System.Collections.Generic;

namespace OCore.Events
{
    public class EventTypeOptions
    {
        public int WorkerInstances { get; set; } = 4;
        public bool Stable { get; set; } = true;
        public bool FireAndForget { get; set; } = true;
        public List<Guid> Destinations { get; set; }        
        public bool TrackAndKillPoisonEvents { get; set; }        
        public int PoisonLimit { get; set; } = 5;
        public string ProviderName { get; set; }
    }

    public class EventOptions
    {        
        public string DefaultProviderName { get; set; }

        public Dictionary<string, EventTypeOptions> EventTypes { get; set; }     
    }
}
