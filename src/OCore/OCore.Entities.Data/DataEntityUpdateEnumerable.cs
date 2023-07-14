using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Orleans;

namespace OCore.Entities.Data;

[GenerateSerializer]
public class DataEntityUpdateEnumerable<T> : IAsyncEnumerable<T>,
    IAsyncDisposable,
    IDataEntityUpdateEnumerable<T>
{
    private readonly IDataEntity<T> _dataEntity;
    private readonly UnboundedChannelOptions _channelOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = false };
    private readonly Channel<T> _stateUpdateChannel;

    public DataEntityUpdateEnumerable(IDataEntity<T> dataEntity)
    {
        _dataEntity = dataEntity;
        _channelOptions = new() { AllowSynchronousContinuations = false, SingleReader = false, SingleWriter = true, };
        _stateUpdateChannel = Channel.CreateUnbounded<T>(_channelOptions);
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

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        if (await _dataEntity.Exists() == true)
        {
            var state = await _dataEntity.Read();

            yield return state;
        }

        while (await _stateUpdateChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_stateUpdateChannel.Reader.TryRead(out var state))
            {
                yield return state;
            }
        }
    }

    public ValueTask DisposeAsync()
    {
        _dataEntity.Unsubscribe(this);
        return default;
    }
}