using System.Security.Claims;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzCleaner.Func.Tests.Infrastructure;

public sealed class MockHttpRequestData : HttpRequestData
{
    private static readonly FunctionContext Context = Mock.Of<FunctionContext>();

    public MockHttpRequestData(string body) : base(Context)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        Body = new MemoryStream(bytes);
    }

    public override HttpResponseData CreateResponse() =>
        new MockHttpResponseData(Context);

    public override Stream Body { get; }

    public override HttpHeadersCollection Headers { get; }

    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }

    public override Uri Url { get; }

    public override IEnumerable<ClaimsIdentity> Identities { get; }

    public override string Method { get; }
}

