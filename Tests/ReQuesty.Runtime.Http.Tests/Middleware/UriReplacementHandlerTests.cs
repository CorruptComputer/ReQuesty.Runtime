using ReQuesty.Runtime.Abstractions;
using ReQuesty.Runtime.Http.Middleware;
using ReQuesty.Runtime.Http.Middleware.Options;
using Moq;
using Xunit;

namespace ReQuesty.Runtime.Http.Tests.Middleware;

public class UriReplacementOptionTests
{
    [Fact]
    public void Does_Nothing_When_Url_Replacement_Is_Disabled()
    {
        Uri uri = new("http://localhost/test");
        UriReplacementHandlerOption disabled = new(false, new Dictionary<string, string>());

        Assert.False(disabled.IsEnabled());
        Assert.Equal(uri, disabled.Replace(uri));

        disabled = new UriReplacementHandlerOption(false, new Dictionary<string, string>{
            {"test", ""}
        });

        Assert.Equal(uri, disabled.Replace(uri));
    }

    [Fact]
    public void Returns_Null_When_Url_Provided_Is_Null()
    {
        UriReplacementHandlerOption disabled = new(false, new Dictionary<string, string>());

        Assert.False(disabled.IsEnabled());
        Assert.Null(disabled.Replace(null));
    }

    [Fact]
    public void Replaces_Key_In_Path_With_Value()
    {
        Uri uri = new("http://localhost/test");
        UriReplacementHandlerOption option = new(true, new Dictionary<string, string> { { "test", "" } });

        Assert.True(option.IsEnabled());
        Assert.Equal("http://localhost/", option.Replace(uri)!.ToString());
    }
}

public class UriReplacementHandlerTests
{
    [Fact]
    public async Task Calls_Uri_ReplacementAsync()
    {
        Mock<IUriReplacementHandlerOption> mockReplacement = new();
        mockReplacement.Setup(static x => x.IsEnabled()).Returns(true);
        mockReplacement.Setup(static x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        UriReplacementHandler<IUriReplacementHandlerOption> handler = new(mockReplacement.Object)
        {
            InnerHandler = new FakeSuccessHandler()
        };
        HttpRequestMessage msg = new(HttpMethod.Get, "http://localhost");
        HttpClient client = new(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(static x => x.Replace(It.IsAny<Uri>()), Times.Once());
    }

    [Fact]
    public async Task Calls_Uri_Replacement_From_Request_OptionsAsync()
    {
        Mock<IUriReplacementHandlerOption> mockReplacement = new();
        mockReplacement.Setup(static x => x.IsEnabled()).Returns(true);
        mockReplacement.Setup(static x => x.Replace(It.IsAny<Uri>())).Returns(new Uri("http://changed"));

        UriReplacementHandler<IUriReplacementHandlerOption> handler = new()
        {
            InnerHandler = new FakeSuccessHandler()
        };
        HttpRequestMessage msg = new(HttpMethod.Get, "http://localhost");
        SetRequestOption(msg, mockReplacement.Object);
        HttpClient client = new(handler);
        await client.SendAsync(msg);

        mockReplacement.Verify(static x => x.Replace(It.IsAny<Uri>()), Times.Once());
    }

    /// <summary>
    ///   Sets a <see cref="IRequestOption"/> in <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
    /// <param name="option">The request option.</param>
    private static void SetRequestOption<T>(HttpRequestMessage httpRequestMessage, T option) where T : IRequestOption
    {
        httpRequestMessage.Options.Set(new HttpRequestOptionsKey<T>(typeof(T).FullName!), option);
    }
}
