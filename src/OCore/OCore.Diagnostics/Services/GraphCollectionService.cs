﻿using OCore.Diagnostics.Abstractions;
using OCore.Services;

namespace OCore.Diagnostics.Services
{
    /// <summary>
    /// This service will collect data from the GraphingSink and send the data on 
    /// a timer to a DataEntity
    /// </summary>
    public class GraphCollectionService : Service, IGraphCollectionService
    {
    }
}
