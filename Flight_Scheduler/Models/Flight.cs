namespace Flight_Scheduler.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Gate { get; set; }

        public int AirlineId {  get; set; }
        public virtual Airline? Airlines { get; set; }

        public int AircraftId { get; set; }
        public virtual Aircraft? Aircraft { get; set; }

        public int FlightCrewId { get; set; }
        public virtual FlightCrew? FlightCrew { get; set; }
    }
}
