using OCore.Testing.Abstractions;

namespace OCore.Tests.Seeders.Authorization;

public class AuthorizationSeeder : ISeeder
{
    public string RootId { get; } = Guid.NewGuid().ToString();
    
    public Task Seed(IClusterClient clusterClient)
    {
        
        return Task.CompletedTask;
    }
}