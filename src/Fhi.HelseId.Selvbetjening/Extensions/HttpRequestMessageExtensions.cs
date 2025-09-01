using Duende.IdentityModel.Client;

namespace Fhi.HelseIdSelvbetjening.Extensions
{
    internal static class HttpRequestMessageExtensions
    {
        public static HttpRequestMessage WithDpop(this HttpRequestMessage httpRequest, string uri, string httpMethod, string privateJwk, string privateJwkAlg, string accessToken)
        {
            var dpopProof = DPoPProofGenerator.CreateDPoPProof(
                uri,
                httpMethod,
                privateJwk,
                privateJwkAlg,
                accessToken: accessToken);

            httpRequest.SetDPoPToken(accessToken, dpopProof);
            return httpRequest;
        }

        public static HttpRequestMessage WithContent(this HttpRequestMessage httpRequest, HttpContent content)
        {
            httpRequest.Content = content;
            return httpRequest;
        }
        public static HttpRequestMessage WithHeader(this HttpRequestMessage httpRequest, string name, string value)
        {
            httpRequest.Headers.Add(name, value);
            return httpRequest;
        }
    }
}
