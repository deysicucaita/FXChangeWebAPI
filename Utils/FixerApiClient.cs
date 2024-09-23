using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class FixerApiClient
{
    private readonly HttpClient _httpClient;
    private const string ApiKey = "64e29da5c7f6d930d48661249d914020";  

    public FixerApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JObject> GetLatestRatesAsync(string baseCurrency, string symbols)
    {
        var url = $"http://data.fixer.io/api/latest?access_key={ApiKey}&base={baseCurrency}&symbols={symbols}";
        var response = await _httpClient.GetStringAsync(url);
        return JObject.Parse(response);
    }
}
