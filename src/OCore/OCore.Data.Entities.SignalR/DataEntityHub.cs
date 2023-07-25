using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.SignalR;
using OCore.Entities.Data;

namespace OCore.Data.Entities.SignalR;

public class DataEntityHub : Hub
{
    private readonly ConcurrentDictionary<Type, DataEntityStreamer> _streamers = new();

    private readonly ConcurrentDictionary<string, List<string>> _groups = new();

    //private readonly ConcurrentDictionary<string, string> _connectionsToDataEntities = new();
    private readonly IClusterClient _clusterClient;

    public DataEntityHub(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }

    public async Task Subscribe(string dataEntityTypeString, string dataEntityId)
    {
        var groupName = $"{dataEntityTypeString}:{dataEntityId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _groups.AddOrUpdate(Context.ConnectionId,
            (_) => { return new List<string> { groupName }; },
            (_, l) =>
            {
                l.Add(groupName);
                return l;
            });
        var streamer = CreateIfNotExists(dataEntityTypeString, dataEntityId);
        var dataEntityTypeParam = Type.GetType(dataEntityTypeString);
        if (dataEntityTypeParam is null)
        {
            return;
        }
        var dataEntityType = typeof(IDataEntity<>).MakeGenericType(dataEntityTypeParam);
        var _ = streamer.Stream(dataEntityType, dataEntityId);
    }

    private DataEntityStreamer CreateIfNotExists(string dataEntityType, string dataEntityId)
    {
        var type = Type.GetType(dataEntityType);
        if (type == null)
        {
            throw new ArgumentException($"Type not found");
        }

        if (_streamers.TryGetValue(type, out var streamer) == false)
        {
            streamer = new DataEntityStreamer(this, _clusterClient);
            _streamers.TryAdd(type, streamer);
            return streamer;
        }
        else
        {
            return streamer;
        }
    }

    public async Task Unsubscribe(string dataEntityType, string dataEntityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{dataEntityType}:{dataEntityId}");
        // TODO: Should be removed from _groups and the streamer should be stopped if no more connections
    }

    public async Task Update(string dataEntityType, string dataEntityId, string dataEntityUpdate)
    {
        await Clients.Group($"{dataEntityType}:{dataEntityId}").SendAsync("Update", dataEntityUpdate);
    }

    public async Task Deleted(string dataEntityType, string dataEntityId)
    {
        await Clients.Group($"{dataEntityType}:{dataEntityId}").SendAsync("Deleted");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_groups.TryGetValue(Context.ConnectionId, out var groups))
        {
            foreach (var group in groups)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}

internal class DataEntityStreamer
{
    private readonly DataEntityHub _hub;
    private readonly bool _jsonDiff;
    private readonly HashSet<string> _connections = new();
    private readonly IClusterClient _clusterClient;

    public DataEntityStreamer(DataEntityHub hub,
        IClusterClient clusterClient,
        bool jsonDiff = true)
    {
        _hub = hub;
        _clusterClient = clusterClient;
        _jsonDiff = jsonDiff;
    }

    public async Task Stream(Type type, string id)
    {
        var grain = (IDataEntity)_clusterClient.GetGrain(type, id);
        var typeString = type.ToString();
        await foreach (var update in grain.GetJsonUpdates(_jsonDiff))
        {
            await _hub.Update(typeString, id, update);
        }

        await _hub.Deleted(typeString, id);
    }

    public Task Stop()
    {
        // TODO: Implement
        return Task.CompletedTask;
    }
}