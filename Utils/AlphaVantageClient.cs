using System;
using System.Net.Http;
using System.Threading.Tasks;
using FXChangeWebAPI.Data_Transfer;
using Newtonsoft.Json.Linq;

public class AlphaVantageClient
{
    private static readonly HttpClient client = new HttpClient();
    private const string apiKey = "W9ORZD6PWGK18YFM";  

    public async Task<FXRateDto> GetFXRatesAsync(string fromCurrency, string toCurrency)
    {
        var url = $"https://www.alphavantage.co/query?function=FX_DAILY&from_symbol={fromCurrency}&to_symbol={toCurrency}&apikey={apiKey}";
        var response = await client.GetStringAsync(url);

        var jsonData = JObject.Parse(response);
        var timeSeries = jsonData["Time Series FX (Daily)"];

        if (timeSeries == null)
        {
            throw new Exception("No se encontraron datos.");
        }

        // Obtener la fecha más reciente
        var latestDate = timeSeries.First.Path;
        var latestData = timeSeries[latestDate];

        // Crear un objeto FXRateDto con los datos obtenidos
        var fxRate = new FXRateDto
        {
            CrntDate = DateTime.Parse(latestDate),
            Open = decimal.Parse(latestData["1. open"].ToString()),
            High = decimal.Parse(latestData["2. high"].ToString()),
            Low = decimal.Parse(latestData["3. low"].ToString()),
            Close = decimal.Parse(latestData["4. close"].ToString())
        };

        return fxRate;
    }
}
