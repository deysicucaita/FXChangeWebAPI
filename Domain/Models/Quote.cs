using FXChangeWebAPI.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FXChangeWebAPI.Domain.Models;

public class Quote : BaseQuote
{
    public Quote(
        string currencyPair,
        DateTime crntDate,
        decimal open,
        decimal high,
        decimal low,
        decimal close)
    {
        CurrencyPair = currencyPair;
        CrntDate = crntDate;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }

    [Column(TypeName = "varchar(6)")]
    public string CurrencyPair { get; set; }
}
