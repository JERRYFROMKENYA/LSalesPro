namespace SalesService.Infrastructure.Data;

public interface ISalesDbInitializer
{
    Task InitializeAsync();
    Task SeedDataAsync();
}
