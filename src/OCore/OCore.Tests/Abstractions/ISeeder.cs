namespace OCore.Tests.Abstractions;

public interface ISeeder
{
    Task Seed(IClusterClient clusterClient);
}