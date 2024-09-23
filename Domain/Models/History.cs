using FXChangeWebAPI.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FXChangeWebAPI.Domain.Models
{
    public class History : BaseHistory
    {
        public History(
            string currencyPair,
            DateTime startDate,
            DateTime endDate,
            int course) 
            : base()
        {
            CurrencyPair = currencyPair;
            StartDate = startDate;
            EndDate = endDate;
            Course = course;
        }

        [Column(TypeName = "varchar(6)")]
        public string CurrencyPair { get; set; }
    }
}
