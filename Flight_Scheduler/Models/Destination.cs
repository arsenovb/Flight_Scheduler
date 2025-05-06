using System.ComponentModel.DataAnnotations;

namespace Flight_Scheduler.Models
{
    public class Destination
    {
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; }
            public virtual ICollection<Flight> Flights { get; set; }
    }
}
