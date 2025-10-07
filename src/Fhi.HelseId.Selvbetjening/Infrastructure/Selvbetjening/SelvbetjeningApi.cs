using System.Net.Http.Json;
using Fhi.HelseIdSelvbetjening.Extensions;
using Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening.Dtos;

namespace Fhi.HelseIdSelvbetjening.Infrastructure.Selvbetjening
{
    internal interface ISelvbetjeningApi
    {
        Task<(ClientSecretUpdateResult? ClientSecretUpdate, ProblemDetail? ProblemDetail)> UpdateClientSecretsAsync(string baseAddress, string dPoPKey, string accessToken, string newPublicJwk);
        Task<(IEnumerable<ClientSecret>? ClientSecrets, ProblemDetail? ProblemDetail)> GetClientSecretsAsync(string baseAddress, string dPoPKey, string accessToken);
    }

    internal class SelvbetjeningApi : ISelvbetjeningApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _clientSecretEndpoint = "/v1/client-secret";

        public SelvbetjeningApi(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(IEnumerable<ClientSecret>? ClientSecrets, ProblemDetail? ProblemDetail)> GetClientSecretsAsync(string baseAddress, string dPoPKey, string accessToken)
        {
            var uri = new Uri(new Uri(baseAddress), _clientSecretEndpoint);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
                .WithDpop(uri.ToString(), HttpMethod.Get.ToString(), dPoPKey, "PS256", accessToken)
                .WithHeader("Accept", "application/json");

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(requestMessage);

            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return (content.Deserialize<ClientSecret[]>(), null);
            }

            var problemDetail = new ProblemDetail(
                response.ReasonPhrase ?? "Error",
                response.ReasonPhrase ?? "Error",
                (int)response.StatusCode,
                content,
                null
            );

            return (null, problemDetail);
        }

        public async Task<(ClientSecretUpdateResult? ClientSecretUpdate, ProblemDetail? ProblemDetail)> UpdateClientSecretsAsync(string baseAddress, string dPoPKey, string accessToken, string newPublicJwk)
        {
            var uri = new Uri(new Uri(baseAddress), _clientSecretEndpoint);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
                .WithDpop(uri.ToString(), HttpMethod.Post.ToString(), dPoPKey, "PS256", accessToken)
                .WithContent(JsonContent.Create(newPublicJwk, options: JsonSerializerExtensions.Options))
                .WithHeader("Accept", "application/json");
            var client = _httpClientFactory.CreateClient();

            var clientSecretUpdateResponse = await client.SendAsync(requestMessage);

            var content = await clientSecretUpdateResponse.Content.ReadAsStringAsync();
            if (clientSecretUpdateResponse.IsSuccessStatusCode)
            {
                return (content.Deserialize<ClientSecretUpdateResult>(), null);
            }

            var problemDetail = new ProblemDetail(
                clientSecretUpdateResponse.ReasonPhrase ?? "Error",
                clientSecretUpdateResponse.ReasonPhrase ?? "Error",
                (int)clientSecretUpdateResponse.StatusCode,
                content,
                null
            );

            return (null, problemDetail);
        }
    }
}
