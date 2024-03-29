﻿using System.Collections.Generic;
using System.Threading.Tasks;
using OCore.Entities.Data;
using Orleans;

namespace OCore.Diagnostics.Abstractions;

[GenerateSerializer]
public class CorrelationIdRegistryData
{
    [Id(0)] public List<string> CorrelationIds { get; set; } = new();
}

[DataEntity("OCore.CorrelationIdRegistry", dataEntityMethods: DataEntityMethods.Read)]
public interface ICorrelationIdRegistry : IDataEntity<CorrelationIdRegistryData>
{
    /// <summary>
    /// To be able to monitor the system, we need to know which correlation ids are active.
    /// </summary>
    /// <param name="correlationId">The correlation id to store.</param>
    Task Register(string correlationId);
}