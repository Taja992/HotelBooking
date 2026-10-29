using System.Linq;
using HotelBooking.Core;
using HotelBooking.Specs.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>Given steps: build the rooms and bookings a scenario starts from.</summary>
    [Binding]
    public class HotelSteps
    {
        private readonly HotelFixture hotel;

        public HotelSteps(HotelFixture hotel)
        {
            this.hotel = hotel;
        }

        [Given("the hotel has {int} room(s)")]
        public void GivenTheHotelHasRooms(int count)
        {
            for (var i = 1; i <= count; i++)
            {
                hotel.Db.Room.Add(new Room { Description = $"Room {i}" });
            }
            hotel.Db.SaveChanges();
        }

        [Given("there are no bookings")]
        public void GivenThereAreNoBookings()
        {
            // Not a no-op: it checks the precondition the scenario relies on
            Assert.Equal(0, hotel.BookingCount);
        }

        [Given("all rooms are booked from day {int} to day {int}")]
        public void GivenAllRoomsAreBooked(int startOffset, int endOffset)
        {
            foreach (var room in hotel.Db.Room.ToList())
            {
                AddBooking(room.Id, startOffset, endOffset, isActive: true);
            }
        }

        [Given("room {int} is booked from day {int} to day {int}")]
        public void GivenRoomIsBooked(int roomId, int startOffset, int endOffset)
        {
            AddBooking(roomId, startOffset, endOffset, isActive: true);
        }

        [Given("room {int} has a cancelled booking from day {int} to day {int}")]
        public void GivenRoomHasACancelledBooking(int roomId, int startOffset, int endOffset)
        {
            AddBooking(roomId, startOffset, endOffset, isActive: false);
        }

        private void AddBooking(int roomId, int startOffset, int endOffset, bool isActive)
        {
            hotel.Db.Booking.Add(new Booking
            {
                RoomId = roomId,
                CustomerId = hotel.CustomerId,
                StartDate = HotelFixture.Day(startOffset),
                EndDate = HotelFixture.Day(endOffset),
                IsActive = isActive
            });
            hotel.Db.SaveChanges();
        }
    }
}
