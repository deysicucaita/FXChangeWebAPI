using Newtonsoft.Json;

public class AlphaVantageClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AlphaVantageClient(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient("ForeignExchangeAPI");
        _apiKey = configuration["AlphaVantage:ApiKey"];
    }

    public async Task<T> GetExchangeRateAsync<T>(string function, string symbol)
    {
        string url = $"query?function={function}&symbol={symbol}&apikey={_apiKey}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(jsonResponse);
    }


    public class AlphaVantageResponseDto
    {
        [JsonProperty("Time Series (Daily)")]
        public Dictionary<string, Dictionary<string, string>> TimeSeries { get; set; }
    }
}
