using System.Globalization;
using ReQuesty.Runtime.Abstractions.Serialization;
using ReQuesty.Runtime.Abstractions.Tests.Mocks;
using Moq;
using Xunit;

namespace ReQuesty.Runtime.Abstractions.Tests;

public class RequestInformationTests
{
    [Fact]
    public void SetUriCorrectlyEscapesDataString()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/repos/{owner}/{repo}/labels/{name}"
        };

        // Act
        testRequest.PathParameters.Add("owner", "me");
        testRequest.PathParameters.Add("repo", "test");
        testRequest.PathParameters.Add("name", "profane content 🤬");

        // Assert
        string actual = testRequest.URI.AbsoluteUri.ToString();
        Assert.Equal("http://localhost/repos/me/test/labels/profane%20content%20%F0%9F%A4%AC", actual);
        Assert.Empty(testRequest.QueryParameters);
    }

    [Fact]
    public void SetUriExtractsQueryParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/{path}/me?foo={foo}"
        };
        // Act
        testRequest.QueryParameters.Add("foo", "bar");
        testRequest.PathParameters.Add("path", "baz");
        // Assert
        Assert.Equal("http://localhost/baz/me?foo=bar", testRequest.URI.ToString());
        Assert.NotEmpty(testRequest.QueryParameters);
        Assert.Equal("foo", testRequest.QueryParameters.First().Key);
        Assert.Equal("bar", testRequest.QueryParameters.First().Value.ToString());
    }

    [Fact]
    public void AddsAndRemovesRequestOptions()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            URI = new Uri("http://localhost")
        };
        IRequestOption testRequestOption = new Mock<IRequestOption>().Object;
        Assert.Empty(testRequest.RequestOptions);
        // Act
        testRequest.AddRequestOptions(new IRequestOption[] { testRequestOption });
        // Assert
        Assert.NotEmpty(testRequest.RequestOptions);
        Assert.Equal(testRequestOption, testRequest.RequestOptions.First());

        // Act by removing the option
        testRequest.RemoveRequestOptions(testRequestOption);
        Assert.Empty(testRequest.RequestOptions);
    }

    [Fact]
    public void SetsSelectQueryParameters()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?%24select}"
        };
        Action<GetQueryParameters> q = x => x.Select = new[] { "id", "displayName" };
        GetQueryParameters qParams = new();
        q.Invoke(qParams);

        // Act
        requestInfo.AddQueryParameters(qParams);

        // Assert
        Assert.True(requestInfo.QueryParameters.ContainsKey("%24select"));
        Assert.False(requestInfo.QueryParameters.ContainsKey("select"));
        Assert.Equal("%24select", requestInfo.QueryParameters.First().Key);
    }

    [Fact]
    public void DoesNotSetEmptyStringQueryParameters()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?%24search}"
        };
        Action<GetQueryParameters> q = x => x.Search = "";//empty string
        GetQueryParameters qParams = new();
        q.Invoke(qParams);

        // Act
        requestInfo.AddQueryParameters(qParams);

        // Assert
        Assert.True(requestInfo.QueryParameters.ContainsKey($"%24search"));
        Assert.False(requestInfo.QueryParameters.ContainsKey("search"));
        Assert.Equal("http://localhost/me?%24search=", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void DoesNotSetEmptyCollectionQueryParameters()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?%24select}"
        };
        Action<GetQueryParameters> q = x => x.Select = []; //empty array
        GetQueryParameters qParams = new();
        q.Invoke(qParams);

        // Act
        requestInfo.AddQueryParameters(qParams);

        // Assert
        Assert.False(requestInfo.QueryParameters.ContainsKey($"%24select"));
        Assert.False(requestInfo.QueryParameters.ContainsKey("select"));
    }

    [Fact]
    public void DoesNotSetQueryParametersToLowerCaseFirstCharacter()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?%TenantId}"
        };
        Action<GetQueryParameters> q = x => x.TenantId = "Tenant1";
        GetQueryParameters qParams = new();
        q.Invoke(qParams);

        // Act
        requestInfo.AddQueryParameters(qParams);

        // Assert
        Assert.Contains("TenantId", requestInfo.QueryParameters.Keys);
        Assert.DoesNotContain("tenantId", requestInfo.QueryParameters.Keys);
    }

    [Fact]
    public void SetsPathParametersOfDateTimeOffsetType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/getDirectRoutingCalls(fromDateTime='{fromDateTime}',toDateTime='{toDateTime}')"
        };

        // Act
        DateTimeOffset fromDateTime = new(2022, 8, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset toDateTime = new(2022, 8, 2, 0, 0, 0, TimeSpan.Zero);
        Dictionary<string, object> pathParameters = new()
        {
            { "fromDateTime", fromDateTime },
            { "toDateTime", toDateTime }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains("fromDateTime='2022-08-01T00%3A00%3A00.0000000%2B00%3A00'", requestInfo.URI.OriginalString);
        Assert.Contains("toDateTime='2022-08-02T00%3A00%3A00.0000000%2B00%3A00'", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsPathParametersOfBooleanType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24count}"
        };

        // Act
        bool count = true;
        Dictionary<string, object> pathParameters = new()
        {
            { "%24count", count }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains("%24count=true", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsPathParametersOfGuidType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24requestId}"
        };

        // Act
        Guid guid = Guid.Parse("6d320a89-2d8f-4204-855d-b98a1bc176d4");
        Dictionary<string, object> pathParameters = new()
        {
            { "%24requestId", guid }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24requestId=6d320a89-2d8f-4204-855d-b98a1bc176d4", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsPathParametersOfDateType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24date}"
        };

        // Act
        Date date = new(2023, 10, 26);
        Dictionary<string, object> pathParameters = new()
        {
            { "%24date", date }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24date=2023-10-26", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsPathParametersOfTimeType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24time}"
        };

        // Act
        Time time = new(6, 0, 0);
        Dictionary<string, object> pathParameters = new()
        {
            { "%24time", time }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24time=06%3A00%3A00", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsPathParametersOfDateOnlyType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24date}"
        };

        // Act
        DateOnly date = new(2023, 10, 26);
        Dictionary<string, object> pathParameters = new()
        {
            { "%24date", date }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24date=2023-10-26", requestInfo.URI.OriginalString);
    }
    [Fact]
    public void SetsPathParametersOfTimeOnlyType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24time}"
        };

        // Act
        TimeOnly time = new(6, 0, 0);
        Dictionary<string, object> pathParameters = new()
        {
            { "%24time", time }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24time=06%3A00%3A00", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void CurrentCultureDoesNotAffectTimeSerialization()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/users{?%24time}"
        };

        CultureInfo currentCulture = CultureInfo.CurrentCulture;

        // Act
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");
        Time time = new(6, 0, 0);
        Dictionary<string, object> pathParameters = new()
        {
            { "%24time", time }
        };

        requestInfo.PathParameters = pathParameters;

        // Assert
        Assert.Contains($"%24time=06%3A00%3A00", requestInfo.URI.OriginalString);

        // Cleanup
        Thread.CurrentThread.CurrentCulture = currentCulture;
    }

    [Fact]
    public void ThrowsInvalidOperationExceptionWhenBaseUrlNotSet()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        // Assert
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => requestInfo.URI);
        Assert.Contains("baseurl", exception.Message);
    }

    [Fact]
    public void BuildsUrlOnProvidedBaseUrl()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        // Act
        requestInfo.PathParameters = new Dictionary<string, object>()
        {
            { "baseurl","http://localhost" }
        };

        // Assert
        Assert.Contains("http://localhost", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void InitializeWithProxyBaseUrl()
    {
        string proxyUrl = "https://proxy.apisandbox.msdn.microsoft.com/svc?url=https://graph.microsoft.com/beta";

        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        // Act
        requestInfo.PathParameters = new Dictionary<string, object>()
        {
            { "baseurl", proxyUrl },
            { "%24count", true }
        };

        // Assert we can build urls based on a Proxy based base url
        Assert.Equal("https://proxy.apisandbox.msdn.microsoft.com/svc?url=https://graph.microsoft.com/beta/users?%24count=true", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void GetsAndSetsResponseHandlerByType()
    {
        // Arrange as the request builders would
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        // Assert we have NO option
        Assert.Null(requestInfo.GetRequestOption<ResponseHandlerOption>());

        // Act
        requestInfo.PathParameters = new Dictionary<string, object>()
        {
            { "baseurl", "http://localhost" },
            { "%24count", true }
        };
        requestInfo.SetResponseHandler(new NativeResponseHandler());

        // Assert we now have an option
        Assert.NotNull(requestInfo.GetRequestOption<ResponseHandlerOption>());
    }

    [Fact]
    public void SetsObjectContent()
    {
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        requestInfo.SetContentFromParsable(requestAdapterMock.Object, "application/json", new TestEntity());

        // Assert we now have an option
        serializationWriterMock.Verify(x => x.WriteObjectValue(It.IsAny<string>(), It.IsAny<IParsable>()), Times.Once);
        serializationWriterMock.Verify(x => x.WriteCollectionOfObjectValues(It.IsAny<string>(), It.IsAny<IEnumerable<IParsable>>()), Times.Never);
    }

    [Fact]
    public void SetsObjectCollectionContentSingleElement()
    {
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        requestInfo.SetContentFromParsable(requestAdapterMock.Object, "application/json", new[] { new TestEntity() });

        // Assert we now have an option
        serializationWriterMock.Verify(x => x.WriteObjectValue(It.IsAny<string>(), It.IsAny<IParsable>()), Times.Never);
        serializationWriterMock.Verify(x => x.WriteCollectionOfObjectValues(It.IsAny<string>(), It.IsAny<IEnumerable<IParsable>>()), Times.Once);
    }

    [Fact]
    public void SetsScalarContent()
    {
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        requestInfo.SetContentFromScalar(requestAdapterMock.Object, "application/json", "foo");

        // Assert we now have an option
        serializationWriterMock.Verify(x => x.WriteStringValue(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        serializationWriterMock.Verify(x => x.WriteCollectionOfPrimitiveValues(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public void SetsEnumContent()
    {
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        requestInfo.SetContentFromEnum<TestEnum>(requestAdapterMock.Object, "application/json", TestEnum.First);

        // Assert we now have an option
        serializationWriterMock.Verify(x => x.WriteEnumValue<TestEnum>(It.IsAny<string>(), It.IsAny<TestEnum>()), Times.Once);
        serializationWriterMock.Verify(x => x.WriteCollectionOfEnumValues(It.IsAny<string>(), It.IsAny<IEnumerable<TestEnum?>>()), Times.Never);
    }

    [Fact]
    public void SetsScalarCollectionContent()
    {
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "{+baseurl}/users{?%24count}"
        };

        requestInfo.SetContentFromScalarCollection(requestAdapterMock.Object, "application/json", new[] { "foo" });

        // Assert we now have an option
        serializationWriterMock.Verify(x => x.WriteStringValue(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        serializationWriterMock.Verify(x => x.WriteCollectionOfPrimitiveValues(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public void GetUriResolvesParametersCaseSensitive()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/{URITemplate}/ParameterMapping?IsCaseSensitive={IsCaseSensitive}"
        };
        // Act
        testRequest.PathParameters.Add("URITemplate", "UriTemplate");
        testRequest.QueryParameters.Add("IsCaseSensitive", false);

        // Assert
        Assert.Equal("http://localhost/UriTemplate/ParameterMapping?IsCaseSensitive=false", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsBoundaryOnMultipartBody()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.POST,
            UrlTemplate = "http://localhost/{URITemplate}/ParameterMapping?IsCaseSensitive={IsCaseSensitive}"
        };
        Mock<IRequestAdapter> requestAdapterMock = new();
        Mock<ISerializationWriterFactory> serializationWriterFactoryMock = new();
        Mock<ISerializationWriter> serializationWriterMock = new();
        serializationWriterFactoryMock.Setup(x => x.GetSerializationWriter(It.IsAny<string>())).Returns(serializationWriterMock.Object);
        requestAdapterMock.SetupGet(x => x.SerializationWriterFactory).Returns(serializationWriterFactoryMock.Object);
        // Given
        MultipartBody multipartBody = new()
        {
            RequestAdapter = requestAdapterMock.Object
        };

        // When
        testRequest.SetContentFromParsable(requestAdapterMock.Object, "multipart/form-data", multipartBody);

        // Then
        Assert.NotNull(multipartBody.Boundary);
        Assert.True(testRequest.Headers.TryGetValue("Content-Type", out IEnumerable<string>? contentType));
        Assert.Single(contentType);
        Assert.Equal("multipart/form-data; boundary=" + multipartBody.Boundary, contentType.First());
    }

    [Fact]
    public void SetsEnumValueInQueryParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?dataset}"
        };
        // Act
        testRequest.AddQueryParameters(new GetQueryParameters { DataSet = TestEnum.First });
        // Assert
        Assert.Equal("http://localhost/me?dataset=Value_1", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsEnumValuesInQueryParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?datasets}"
        };
        // Act
        testRequest.AddQueryParameters(new GetQueryParameters { DataSets = new TestEnum[] { TestEnum.First, TestEnum.Second } });
        // Assert
        Assert.Equal("http://localhost/me?datasets=Value_1,Value_2", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsEnumValueInPathParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/{dataset}"
        };
        // Act
        testRequest.PathParameters.Add("dataset", TestEnum.First);
        // Assert
        Assert.Equal("http://localhost/Value_1", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsEnumValuesInPathParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/{dataset}"
        };
        // Act
        testRequest.PathParameters.Add("dataset", new TestEnum[] { TestEnum.First, TestEnum.Second });
        // Assert
        Assert.Equal("http://localhost/Value_1,Value_2", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsIntValueInQueryParameters()
    {
        // Arrange
        RequestInformation testRequest = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?item}"
        };
        // Act
        testRequest.AddQueryParameters(new GetQueryParameters { Item = 1 });
        // Assert
        Assert.Equal("http://localhost/me?item=1", testRequest.URI.ToString());
    }

    [Fact]
    public void SetsIntValuesInQueryParameters()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };
        // Act
        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { 1, 2 } });
        // Assert
        Assert.Equal("http://localhost/me?items=1,2", requestInfo.URI.ToString());
    }

    [Fact]
    public void SetsBooleanValuesInQueryParameters()
    {
        // Arrange
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };
        // Act
        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { true, false } });
        // Assert
        Assert.Equal("http://localhost/me?items=true,false", requestInfo.URI.ToString());
    }

    [Fact]
    public void SetsDateTimeOffsetValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        DateTimeOffset dateTime1 = new(2022, 8, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset dateTime2 = new(2022, 8, 2, 0, 0, 0, TimeSpan.Zero);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { dateTime1, dateTime2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=2022-08-01T00%3A00%3A00.0000000%2B00%3A00,2022-08-02T00%3A00%3A00.0000000%2B00%3A00", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsDateTimeValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        DateTime dateTime1 = new(2022, 8, 1, 0, 0, 0);
        DateTime dateTime2 = new(2022, 8, 2, 0, 0, 0);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { dateTime1, dateTime2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=2022-08-01T00%3A00%3A00.0000000,2022-08-02T00%3A00%3A00.0000000", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsDateValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        Date date1 = new(2022, 8, 1);
        Date date2 = new(2022, 8, 2);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { date1, date2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=2022-08-01,2022-08-02", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsTimeValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        Time time1 = new(10, 0, 0);
        Time time2 = new(11, 1, 1);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { time1, time2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=10%3A00%3A00,11%3A01%3A01", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsDateOnlyValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        Date date1 = new DateOnly(2022, 8, 1);
        DateOnly date2 = new Date(2022, 8, 2);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { date1, date2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=2022-08-01,2022-08-02", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsTimeOnlyValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        Time time1 = new TimeOnly(10, 0, 0);
        TimeOnly time2 = new Time(11, 1, 1);

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { time1, time2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=10%3A00%3A00,11%3A01%3A01", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void SetsGuidValuesInQueryParameters()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        Guid g1 = Guid.Parse("55331110-6817-4A9B-83B2-57617E3E08E5");
        Guid g2 = Guid.Parse("482DFF4F-63D6-47F4-A88B-5CAEC03180D4");

        requestInfo.AddQueryParameters(new GetQueryParameters { Items = new object[] { g1, g2 } });

        // Assert
        Assert.Equal("http://localhost/me?items=55331110-6817-4a9b-83b2-57617e3e08e5,482dff4f-63d6-47f4-a88b-5caec03180d4", requestInfo.URI.OriginalString);
    }

    [Fact]
    public void DoesNotExpandSecondLayerArrays()
    {
        RequestInformation requestInfo = new()
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/me{?items}"
        };

        // Act
        requestInfo.AddQueryParameters(new GetQueryParameters { Items = [new int[] { 1, 2, 3, 4 }] });
        // Assert
        Assert.Throws<ArgumentException>(() => requestInfo.URI.OriginalString);
    }
}

/// <summary>
///   The messages in a mailbox or folder. Read-only. Nullable.
/// </summary>
internal class GetQueryParameters
{
    /// <summary>
    ///   Select properties to be returned
    /// </summary>
    [QueryParameter("%24select")]
    public string[]? Select { get; set; }

    /// <summary>
    ///   Unique id of the request
    /// </summary>
    [QueryParameter("%24requestId")]
    public Guid RequestId { get; set; }

    /// <summary>
    ///   Include count of items
    /// </summary>
    [QueryParameter("%24count")]
    public bool? Count { get; set; }

    /// <summary>
    ///   Expand related entities
    /// </summary>
    [QueryParameter("%24filter")]
    public string? Filter { get; set; }

    /// <summary>
    ///   Order items by property values
    /// </summary>
    [QueryParameter("%24orderby")]
    public string[]? Orderby { get; set; }

    /// <summary>
    ///   Search items by search phrases
    /// </summary>
    [QueryParameter("%24search")]
    public string? Search { get; set; }

    /// <summary>
    ///   Restrict to TenantId
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    ///   Which Dataset to use
    /// </summary>
    [QueryParameter("dataset")]
    public TestEnum DataSet { get; set; }

    /// <summary>
    ///   Which Dataset to use
    /// </summary>
    [QueryParameter("datasets")]
    public TestEnum[]? DataSets { get; set; }

    [QueryParameter("item")]
    public object? Item { get; set; }

    [QueryParameter("items")]
    public object[]? Items { get; set; }
}
