using Microsoft.AspNetCore.Components;
using OCore.Events;
using OCore.Testing.Fixtures;
using OCore.Testing.Host;

namespace OCore.Tests.Events;

[GenerateSerializer]
[Event("TestEvent")]
public class TestEvent
{
    [Id(0)]
    public string Greeting { get; set; } = String.Empty;
}

[Handler("TestEvent")]
public class TestEventHandler : Handler<TestEvent>
{
    public static int Count { get; set; } = 0;

    protected override Task HandleEvent(TestEvent @event)
    {
        Count++;
        Console.WriteLine(@event.Greeting);
        return Task.CompletedTask;
    }
}

public class EventTests : FullHost
{
    public EventTests(FullHostFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task SimpleEvent()
    {  
        await ClusterClient.RaiseEvent(new TestEvent
        {
            Greeting = "Hello, OCore! I love you so much!"
        });

        await Task.Delay(2000);
        Assert.Equal(1, TestEventHandler.Count);
    }
}