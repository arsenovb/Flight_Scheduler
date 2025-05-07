using System.ComponentModel.DataAnnotations;

namespace Flight_Scheduler.Models
{
    public class Airport
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Flight> OriginatingFlights { get; set; } = new List<Flight>();
        public virtual ICollection<Flight> DestinationFlights { get; set; } = new List<Flight>();
    }
}
