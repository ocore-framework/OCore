using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Orleans;

namespace OCore.Entities.Data;

[GenerateSerializer]
public class DataEntityUpdateJsonEnumerable<T> : IAsyncEnumerable<string>, 
    IAsyncDisposable, 
    IDataEntityUpdateEnumerable<T>
{
    private readonly IDataEntity<T> _dataEntity;
    private readonly UnboundedChannelOptions _channelOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = false };
    private readonly Channel<T> _stateUpdateChannel;
    private readonly bool _jsonDiff;
    
    public DataEntityUpdateJsonEnumerable(IDataEntity<T> dataEntity, bool jsonDiff = true)
    {
        _dataEntity = dataEntity;
        _channelOptions = new() { AllowSynchronousContinuations = false, SingleReader = false, SingleWriter = true, };
        _stateUpdateChannel = Channel.CreateUnbounded<T>(_channelOptions);
        _jsonDiff = jsonDiff;
        dataEntity.Subscribe(this);
    }

    public async Task UpdateState(T newState)
    {
        await _stateUpdateChannel.Writer.WriteAsync(newState);
    }

    public void Complete()
    {
        _stateUpdateChannel.Writer.TryComplete();
    }

    private bool fullSent = false;
    
    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        if (await _dataEntity.Exists() == true)
        {
            var state = await _dataEntity.Read();
            string json = JsonSerializer.Serialize(state, _jsonSerializerOptions);
            yield return json;
            fullSent = true;
        }

        while (await _stateUpdateChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_stateUpdateChannel.Reader.TryRead(out var item))
            {
                if (fullSent == false || _jsonDiff == false)
                {
                    yield return JsonSerializer.Serialize(item, _jsonSerializerOptions);
                    fullSent = true;
                }
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _dataEntity.Unsubscribe(this);
        return default;
    }
}
