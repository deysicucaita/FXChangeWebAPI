using System.ComponentModel.DataAnnotations;

namespace FXChangeWebAPI.Domain.Common
{
    public class BaseHistory
    {
        [Key]
        public int Id { get; set; } 
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 
        public int Course { get; set; } 

    }
}
