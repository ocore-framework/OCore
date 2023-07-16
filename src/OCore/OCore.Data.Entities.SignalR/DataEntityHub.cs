using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using OCore.Entities.Data;

namespace OCore.Data.Entities.SignalR;

public class DataEntityHub : Hub
{
    private readonly ConcurrentDictionary<Type, DataEntityStreamer> _streamers = new();
    //private readonly ConcurrentDictionary<string, string> _connectionsToDataEntities = new();
    private readonly IClusterClient _clusterClient;
    
    public DataEntityHub(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }
    
    public async Task Subscribe(string dataEntityType, string dataEntityId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"{dataEntityType}:{dataEntityId}");
        var streamer = CreateIfNotExists(dataEntityType, dataEntityId);
        await streamer.Setup(dataEntityType, )
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
    }

    public async Task Unsubscribe(string dataEntityType, string dataEntityId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{dataEntityType}:{dataEntityId}");
    }
    
    public async Task Update(string dataEntityType, string dataEntityId, string dataEntityUpdate)
    {
        await Clients.Group($"{dataEntityType}:{dataEntityId}").SendAsync("Update", dataEntityUpdate);
    }

    public async Task Deleted(string dataEntityType, string dataEntityId)
    {
        await Clients.Group($"{dataEntityType}:{dataEntityId}").SendAsync("Deleted");
    }
    
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Context.ConnectionId
        return base.OnDisconnectedAsync(exception);
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
    
    public async Task Setup(Type type, string id)
    {
        var grain = (IDataEntity)_clusterClient.GetGrain(type, id);
        await foreach (var update in grain.GetJsonUpdates(_jsonDiff))
        {
            
        }
    }

    public Task Stop()
    {
        
    }
}