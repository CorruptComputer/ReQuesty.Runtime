namespace ReQuesty.Runtime.Abstractions.Authentication;

/// <summary>
///   This authentication provider does not perform any authentication.
/// </summary>
public class AnonymousAuthenticationProvider : IAuthenticationProvider
{
    /// <inheritdoc />
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = default, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
