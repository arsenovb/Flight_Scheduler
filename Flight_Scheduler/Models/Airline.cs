namespace Flight_Scheduler.Models
{
    public class Airline
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }

        public virtual ICollection<Flight>? Flights { get; set; }
    }
}
