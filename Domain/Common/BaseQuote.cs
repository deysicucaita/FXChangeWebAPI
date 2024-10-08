using System.ComponentModel.DataAnnotations;

namespace FXChangeWebAPI.Domain.Common
{
    public abstract class BaseQuote
    {
        [Key]
        public int Id { get; set; }
        public DateTime CrntDate { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}
