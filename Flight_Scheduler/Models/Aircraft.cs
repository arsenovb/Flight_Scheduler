namespace Flight_Scheduler.Models
{
    public class Aircraft
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public int CrewCapacity { get; set; }
        public int PassengerCapacity { get; set; }
        public int FuelTankCapacity { get; set; }

        public virtual ICollection<Flight>? Flights { get; set; }
    }
}
