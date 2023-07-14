using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using JsonDiffPatchDotNet;
using JsonDiffPatchDotNet.Formatters.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OCore.Entities.Data;

[GenerateSerializer]
public class DataEntityUpdateJsonEnumerable<T> : IAsyncEnumerable<string>,
    IAsyncDisposable,
    IDataEntityUpdateEnumerable<T>
{
    private readonly IDataEntity<T> _dataEntity;
    private readonly UnboundedChannelOptions _channelOptions;

    private JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = false,
    };

    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly Channel<T> _stateUpdateChannel;
    private readonly bool _jsonDiff;

    public DataEntityUpdateJsonEnumerable(IDataEntity<T> dataEntity, bool jsonDiff = true)
    {
        _dataEntity = dataEntity;
        _channelOptions = new() { AllowSynchronousContinuations = false, SingleReader = false, SingleWriter = true, };
        _stateUpdateChannel = Channel.CreateUnbounded<T>(_channelOptions);
        _jsonDiff = jsonDiff;
        if (_jsonDiff == true)
        {
            _jsonDiffPatch = new();
            _jsonDeltaFormatter = new();
        }

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

    private JToken _previousState = null;
    private JsonDiffPatch _jsonDiffPatch = null;
    private JsonDeltaFormatter _jsonDeltaFormatter = null;

    public async IAsyncEnumerator<string> GetAsyncEnumerator(
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (await _dataEntity.Exists() == true)
        {
            var state = await _dataEntity.Read();
            string json = JsonSerializer.Serialize(state, _jsonSerializerOptions);
            yield return json;
            fullSent = true;
            if (_jsonDiff == true)
            {
                _previousState = JToken.Parse(json);
            }
        }

        while (await _stateUpdateChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_stateUpdateChannel.Reader.TryRead(out var item))
            {
                var json = JsonSerializer.Serialize(item, _jsonSerializerOptions);
                if (fullSent == false || _jsonDiff == false)
                {
                    yield return json;
                    fullSent = true;
                    if (_jsonDiff == true)
                    {
                        _previousState = JToken.Parse(json);
                        _jsonSerializerOptions = new()
                        {
                            WriteIndented = false,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };
                    }
                }
                else if (_jsonDiff == true)
                {
                    var currentState = JToken.Parse(json);
                    JToken patch = _jsonDiffPatch.Diff(_previousState, currentState);
                    var deltaFormat = _jsonDeltaFormatter.Format(patch);
                    var deltaJson = JsonConvert.SerializeObject(deltaFormat, Formatting.None, _jsonSerializerSettings);
                    yield return deltaJson;
                    _previousState = currentState;
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