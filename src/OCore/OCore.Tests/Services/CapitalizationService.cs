using OCore.Services;

namespace OCore.Tests.Services;

[Service("Capitalization")]
public interface ICapitalizationService : IService
{
    Task<string> Capitalize(string text);
}

public class CapitalizationService : Service, ICapitalizationService
{
    public Task<string> Capitalize(string text)
    {
        return Task.FromResult(text.ToUpper());
    }
}