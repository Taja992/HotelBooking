using HotelBooking.Core;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// Shared state for one scenario. Reqnroll creates a new instance per scenario
    /// and injects it into every step definition class.
    /// </summary>
    public class HotelContext
    {
        public InMemoryRepository<Room> Rooms { get; } = new InMemoryRepository<Room>(r => r.Id);

        public InMemoryRepository<Booking> Bookings { get; } = new InMemoryRepository<Booking>(b => b.Id);

        public BookingManager CreateManager() => new BookingManager(Bookings, Rooms);
    }
}
