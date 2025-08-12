using System.IO.Compression;
using System.Net;

namespace ReQuesty.Runtime.Http.Tests.Mocks
{
    public class MockCompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;

        public MockCompressedContent(HttpContent httpContent)
        {
            _originalContent = httpContent;
            foreach (KeyValuePair<string, IEnumerable<string>> header in _originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            Stream compressedStream = new GZipStream(stream, CompressionMode.Compress, true);
            await _originalContent.CopyToAsync(compressedStream);
            compressedStream.Dispose();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
