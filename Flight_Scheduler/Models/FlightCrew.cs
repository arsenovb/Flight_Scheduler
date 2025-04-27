namespace Flight_Scheduler.Models
{
    public class FlightCrew
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; } 
        public CrewPosition Position { get; set; }

        public enum CrewPosition 
        {
            Captain,  
            FirstOfficer,
            FlightEngineer,
            Purser,
            FlightAttendant
        }

        public virtual ICollection<Flight>? Flights { get; set; }
    }
}
