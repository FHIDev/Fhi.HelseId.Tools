using System.Net;
using System.Text;

namespace Fhi.HelseId.ClientSecret.App.Tests.Fhi.HelseIdSelvbetjening.Tests
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> _responses = new();

        public void AddResponse(HttpMethod method, string url, HttpStatusCode statusCode, string content, string contentType = "application/json")
        {
            var key = $"{method}:{url}";
            _responses[key] = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var key = $"{request.Method}:{request.RequestUri}";
            
            if (_responses.TryGetValue(key, out HttpResponseMessage? response))
            {
                return Task.FromResult(response);
            }
            
            // Return not found if no matching response was configured
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                RequestMessage = request,
                Content = new StringContent($"No response configured for {request.Method} {request.RequestUri}")
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var response in _responses.Values)
                {
                    response.Dispose();
                }
                _responses.Clear();
            }
            
            base.Dispose(disposing);
        }
    }
}