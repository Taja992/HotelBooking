using System;
using HotelBooking.Core;
using HotelBooking.Specs.Support;
using Reqnroll;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>Given steps shared by all features.</summary>
    [Binding]
    public class HotelSteps
    {
        private readonly HotelContext hotel;

        public HotelSteps(HotelContext hotel)
        {
            this.hotel = hotel;
        }

        [Given("the hotel has the following rooms")]
        public void GivenTheHotelHasTheFollowingRooms(DataTable table)
        {
            foreach (var row in table.Rows)
            {
                hotel.Rooms.Items.Add(new Room
                {
                    Id = int.Parse(row["Id"]),
                    Description = row["Description"]
                });
            }
        }

        [Given("all rooms are booked from day {int} to day {int}")]
        public void GivenAllRoomsAreBooked(int start, int end)
        {
            foreach (var room in hotel.Rooms.Items)
                AddBooking(room.Id, start, end, true);
        }

        [Given("the following bookings")]
        public void GivenTheFollowingBookings(DataTable table)
        {
            foreach (var row in table.Rows)
            {
                AddBooking(int.Parse(row["Room"]), int.Parse(row["Start"]),
                    int.Parse(row["End"]), bool.Parse(row["Active"]));
            }
        }

        private void AddBooking(int roomId, int start, int end, bool isActive)
        {
            hotel.Bookings.Items.Add(new Booking
            {
                Id = hotel.Bookings.Items.Count + 1,
                CustomerId = 1,
                RoomId = roomId,
                StartDate = DateTime.Today.AddDays(start),
                EndDate = DateTime.Today.AddDays(end),
                IsActive = isActive
            });
        }
    }
}
