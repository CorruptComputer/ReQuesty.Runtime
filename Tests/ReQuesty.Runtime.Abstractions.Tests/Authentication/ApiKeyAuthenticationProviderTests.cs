using ReQuesty.Runtime.Abstractions.Authentication;
using Xunit;

namespace ReQuesty.Runtime.Abstractions.Tests;

public class ApiKeyAuthenticationProviderTests
{
    [Fact]
    public async Task DefensiveProgramming()
    {
        Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthenticationProvider(null!, "param", ApiKeyAuthenticationProvider.KeyLocation.Header));
        Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthenticationProvider("key", null!, ApiKeyAuthenticationProvider.KeyLocation.Header));

        ApiKeyAuthenticationProvider value = new("key", "param", ApiKeyAuthenticationProvider.KeyLocation.Header);
        await Assert.ThrowsAsync<ArgumentNullException>(() => value.AuthenticateRequestAsync(null!));
    }

    [Fact]
    public async Task AddsInHeader()
    {
        ApiKeyAuthenticationProvider value = new("key", "param", ApiKeyAuthenticationProvider.KeyLocation.Header);
        RequestInformation request = new()
        {
            UrlTemplate = "https://localhost{?param1}",
        };
        await value.AuthenticateRequestAsync(request);
        Assert.False(request.URI.ToString().EndsWith("param=key", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("param", request.Headers.Keys);
    }
    [Fact]
    public async Task AddsInQueryParameters()
    {
        ApiKeyAuthenticationProvider value = new("key", "param", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        RequestInformation request = new()
        {
            UrlTemplate = "https://localhost{?param1}",
        };
        await value.AuthenticateRequestAsync(request);
        Assert.EndsWith("?param=key", request.URI.ToString());
        Assert.DoesNotContain("param", request.Headers.Keys);
    }
    [Fact]
    public async Task AddsInQueryParametersWithOtherParameters()
    {
        ApiKeyAuthenticationProvider value = new("key", "param", ApiKeyAuthenticationProvider.KeyLocation.QueryParameter);
        RequestInformation request = new()
        {
            UrlTemplate = "https://localhost{?param1}",
        };
        request.QueryParameters.Add("param1", "value1");
        await value.AuthenticateRequestAsync(request);
        Assert.EndsWith("&param=key", request.URI.ToString());
        Assert.DoesNotContain("param", request.Headers.Keys);
    }
}