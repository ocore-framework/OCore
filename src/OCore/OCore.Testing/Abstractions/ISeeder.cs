namespace OCore.Testing.Abstractions;

public interface ISeeder
{
    Task Seed(IClusterClient clusterClient);
}