﻿namespace Flight_Scheduler.Models
{
    public class Flight
    {
        public int Id { get; set; }
        public int OriginId { get; set; }
        public virtual Airport? Origin { get; set; }

        public int DestinationId { get; set; }
        public virtual Airport? Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string? Gate { get; set; }

        public int AirlineId {  get; set; }
        public virtual Airline? Airlines { get; set; }

        public int AircraftId { get; set; }
        public virtual Aircraft? Aircraft { get; set; }

        public virtual ICollection<FlightCrew> FlightCrews { get; set; } = new List<FlightCrew>();
    }
}
