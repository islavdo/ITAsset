using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ITAssetAccounting.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EquipmentHealth_ShouldReturnOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        Assert.Contains("equipment", text);
    }
}
