using OCore.Events;
using OCore.Services;

await OCore.Setup.Developer.LetsGo("Events");

[GenerateSerializer]
[Event("TestEvent")]
public class TestEvent
{
    [Id(0)]
    public string? Message { get; set; }
}

[Service("EventFirer")]
public interface IEventFirer : IService
{
    Task FireEvent();
} 

public class EventFirer : Service, IEventFirer
{
    public async Task FireEvent()
    {
        await GrainFactory.RaiseEvent(new TestEvent
        {
            Message = "TEST"
        });
    }
}

[EventHandler("TestEvent")]
public class TestEventHandler : OCore.Events.EventHandler<TestEvent>
{
    protected override Task HandleEvent(TestEvent @event)
    {
        Console.WriteLine(@event.Message);
        return base.HandleEvent(@event);
    }
}