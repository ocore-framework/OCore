using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OCore.Diagnostics.Abstractions;
using OCore.Diagnostics.Options;
using OCore.Entities.Data;

namespace OCore.Diagnostics.Entities;

public class CorrelationIdRegistry : DataEntity<CorrelationIdRegistryData>, ICorrelationIdRegistry
{
    private readonly IOptionsMonitor<DiagnosticsOptions> _diagnosticsOptionsMonitor;

    public CorrelationIdRegistry(IOptionsMonitor<DiagnosticsOptions> diagnosticsOptionsMonitorMonitor)
    {
        _diagnosticsOptionsMonitor = diagnosticsOptionsMonitorMonitor;
    }
    
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (_diagnosticsOptionsMonitor.CurrentValue.StoreCorrelationIdData == false
            && Created == false)
        {
            Created = true;
        }

        return base.OnActivateAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task Register(string correlationId)
    {
        // Make sure the id is in the back of the list.
        State.CorrelationIds.Remove(correlationId);
        State.CorrelationIds.Add(correlationId);
        Prune(State.CorrelationIds);
        return Task.CompletedTask;
    }

    private int pruneCount;

    private void Prune(List<string> stateCorrelationIds)
    {
        // Prune the number of correlation ids to the configured maximum.
        var maxCorrelationIds = _diagnosticsOptionsMonitor.CurrentValue.MaxRegistryStoredCorrelationIds;
        pruneCount++;

        if (pruneCount % 100 != 0
            && stateCorrelationIds.Count > maxCorrelationIds)
        {
            stateCorrelationIds.RemoveRange(stateCorrelationIds.Count - maxCorrelationIds, maxCorrelationIds);
        }
    }
    
}