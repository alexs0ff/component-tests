namespace Shop.ComponentTests.Configurations;

public sealed class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public string TablesToClean { get; set; } = string.Empty;
}