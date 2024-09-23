using System;
using System.Net.Http;
using System.Threading.Tasks;
using FXChangeWebAPI.Abstractions;
using FXChangeWebAPI.Domain.Common;
using System.Net.Http.Json;

namespace FXChangeWebAPI.Utils
{
    public sealed class FXRatesApiClient : IFXRatesApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FXRatesApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<FResult<TEntity>> GetEntityAsync<TEntity>(string url)
        {
            using (var client = _httpClientFactory.CreateClient("ForeignExchangeAPI"))
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<TEntity>();
                    return new FResult<TEntity>(content);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return new FResult<TEntity>(new Error("00", errorMsg));
                }
            }
        }
    }
}
