using System.Text.Json.Serialization;

namespace FXChangeWebAPI.Data_Transfer;

public class FXRateDto
{
    public DateTime CrntDate { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }

    /// <summary>
    /// Variables de api Exchange generates
    /// </summary>
    public bool success { get; set; }
    //public DateTime timestamp { get; set; }
    public string Base { get; set; }
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }

}

