using System.Threading.Channels;
using OCore.Entities.Data;
using OCore.Entities.Data.Extensions;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;
using Xunit.Abstractions;

namespace OCore.Tests.Cluster.DataEntities.StateStreaming;

[GenerateSerializer]
public class TestState
{
    [Id(0)] public int Value { get; set; }
}

[DataEntity("Producer")]
public interface IProducer : IDataEntity<TestState>
{
    Task Echo(string data);

    IAsyncEnumerable<int> Produce();

    IAsyncEnumerable<string> Feed();

    ValueTask Complete();
}

public class Producer : DataEntity<TestState>, IProducer
{
    private Channel<string> _updates = Channel.CreateUnbounded<string>();

    public Task Echo(string data)
    {
        _updates.Writer.TryWrite(data);
        return Task.FromResult(data);
    }

    public async IAsyncEnumerable<int> Produce()
    {
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(100);
            yield return i;
        }
    }

    public IAsyncEnumerable<string> Feed() => _updates.Reader.ReadAllAsync();

    public ValueTask Complete()
    {
        _updates.Writer.Complete();
        return default;
    }
}

public class DataEntityStreamingTests : FullHost
{
    private readonly ITestOutputHelper output;

    public DataEntityStreamingTests(ITestOutputHelper output, FullHostFixture fixture) : base(fixture)
    {
        this.output = output;
    }

    [Fact]
    public async Task TestJsonStream()
    {
        var entity = ClusterClient.GetDataEntity<IProducer>("Streamer");

        List<string> jsonLines1 = new();

        var runTask1 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(false))
            {
                output.WriteLine($"1: {json}");
                jsonLines1.Add(json);
                if (jsonLines1.Count == 2)
                    break;
            }
        });

        List<string> jsonLines2 = new();
        var runTask2 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(false))
            {
                output.WriteLine($"2: {json}");
                jsonLines2.Add(json);
                if (jsonLines2.Count == 2)
                    break;
            }
        });


        await Task.Delay(100);
        
        await entity.Create(new TestState()
        {
            Value = 69
        });
        
        await entity.Update(new TestState()
        {
            Value = 420
        });
        
        await Task.WhenAny(runTask1, runTask2, Task.Delay(200));
        await Task.WhenAll(runTask1, runTask2);
        
        Assert.True(runTask1.IsCompleted);
        Assert.True(runTask2.IsCompleted);
        Assert.Equal(2, jsonLines1.Count);
        Assert.Equal(2, jsonLines2.Count);
        Assert.Equal("{\"Value\":69}", jsonLines1[0]);
        Assert.Equal("{\"Value\":69}", jsonLines2[0]);
        Assert.Equal("{\"Value\":420}", jsonLines1[1]);
        Assert.Equal("{\"Value\":420}", jsonLines2[1]);
    }

    [Fact]
    public async Task TestJsonStreamWithObjectDeletion()
    {
        var entity = ClusterClient.GetDataEntity<IProducer>("JsonStreamerToBeDeleted");

        List<string> jsonLines1 = new();

        var runTask1 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(false))
            {
                output.WriteLine($"1: {json}");
                jsonLines1.Add(json);
            }
        });

        List<string> jsonLines2 = new();
        var runTask2 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(false))
            {
                output.WriteLine($"2: {json}");
                jsonLines2.Add(json);
            }
        });


        await Task.Delay(100);
        
        await entity.Create(new TestState()
        {
            Value = 69
        });
        
        await entity.Update(new TestState()
        {
            Value = 420
        });

        await entity.Delete();
        
        await Task.WhenAny(runTask1, runTask2, Task.Delay(200));
        await Task.WhenAll(runTask1, runTask2);
        
        Assert.Equal(2, jsonLines1.Count);
        Assert.Equal(2, jsonLines2.Count);
        Assert.Equal("{\"Value\":69}", jsonLines1[0]);
        Assert.Equal("{\"Value\":69}", jsonLines2[0]);
        Assert.Equal("{\"Value\":420}", jsonLines1[1]);
        Assert.Equal("{\"Value\":420}", jsonLines2[1]);
    }
    
    [Fact]
    public async Task TestJsonDiffStreamWithObjectDeletion()
    {
        var entity = ClusterClient.GetDataEntity<IProducer>("JsonDiffStreamerToBeDeleted");

        List<string> jsonLines1 = new();

        var runTask1 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(true))
            {
                output.WriteLine($"1: {json}");
                jsonLines1.Add(json);
            }
        });

        List<string> jsonLines2 = new();
        var runTask2 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetJsonUpdates(true))
            {
                output.WriteLine($"2: {json}");
                jsonLines2.Add(json);
            }
        });


        await Task.Delay(100);
        
        await entity.Create(new TestState()
        {
            Value = 69
        });
        
        await entity.Update(new TestState()
        {
            Value = 420
        });

        await entity.Delete();
        
        await Task.WhenAny(runTask1, runTask2, Task.Delay(200));
        await Task.WhenAll(runTask1, runTask2);
        
        Assert.Equal(2, jsonLines1.Count);
        Assert.Equal(2, jsonLines2.Count);
        Assert.Equal("{\"Value\":69}", jsonLines1[0]);
        Assert.Equal("{\"Value\":69}", jsonLines2[0]);
        Assert.Equal("[{\"path\":\"/Value\",\"op\":\"replace\",\"value\":420}]", jsonLines1[1]);
        Assert.Equal("[{\"path\":\"/Value\",\"op\":\"replace\",\"value\":420}]", jsonLines2[1]);
    }

    
    [Fact]
    public async Task TestStreamWithObjectDeletion()
    {
        var entity = ClusterClient.GetDataEntity<IProducer>("StreamerToBeDeleted");

        List<TestState> stateLines = new(); // We're crossing state lines!

        var runTask1 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetUpdates())
            {
                output.WriteLine($"1: {json}");
                stateLines.Add(json);
            }
        });

        List<TestState> jsonLines2 = new();
        var runTask2 = Task.Run(async () =>
        {
            await foreach (var json in entity.GetUpdates())
            {
                output.WriteLine($"2: {json}");
                jsonLines2.Add(json);
            }
        });


        await Task.Delay(100);
        
        await entity.Create(new TestState()
        {
            Value = 69
        });
        
        await entity.Update(new TestState()
        {
            Value = 420
        });

        await entity.Delete();
        
        await Task.WhenAny(runTask1, runTask2, Task.Delay(200));
        await Task.WhenAll(runTask1, runTask2);
        
        // I swear to God these magic numbers are coming from CoPilot, the
        // world is totally degenerating into a dystopian nightmare
        Assert.Equal(2, stateLines.Count);
        Assert.Equal(2, jsonLines2.Count);
        Assert.Equal(69, stateLines[0].Value);
        Assert.Equal(69, jsonLines2[0].Value);
        Assert.Equal(420, stateLines[1].Value);
        Assert.Equal(420, jsonLines2[1].Value);
    }

    [Fact]
    public Task TestDeletionAndRecreation()
    {
        throw new NotImplementedException("This should be tested");
    }
    
    [Fact]
    public async Task Test()
    {
        int sum = 0;
        await foreach (var item in ClusterClient.GetDataEntity<IProducer>("Test").Produce())
        {
            sum += item;
        }

        Assert.Equal(45, sum);
    }

    [Fact]
    public async Task TestSinglecast()
    {
        var producer = ClusterClient.GetDataEntity<IProducer>("SinglecastTest");

        var producerTask = Task.Run(async () =>
        {
            await Task.Delay(200);
            foreach (var value in Enumerable.Range(0, 10))
            {
                //await Task.Delay(100);
                await producer.Echo(value.ToString());
                output.WriteLine($"P: {value}");
            }

            await producer.Complete();
        });


        int sum = 0;

        var consumerTask1 = Task.Run(async () =>
        {
            await foreach (var item in producer.Feed())
            {
                sum += int.Parse(item);
                output.WriteLine($"C1: {item}");
            }
        });

        var consumerTask2 = Task.Run(async () =>
        {
            await foreach (var item in producer.Feed())
            {
                sum += int.Parse(item);
                output.WriteLine($"C2: {item}");
            }
        });

        await Task.WhenAll(producerTask, consumerTask1, consumerTask2);

        Assert.Equal(45, sum);
    }
}