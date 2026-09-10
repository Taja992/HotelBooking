using System;
using System.Collections.Generic;
using System.Linq;
using HotelBooking.Core;
using Moq;

namespace HotelBooking.UnitTests.TestDoubles
{
    /// <summary>
    /// Centralises Moq setup for the two repositories BookingManager depends on.
    /// Keeps the Arrange section of each test to one or two readable lines.
    /// </summary>
    public static class RepositoryMockFactory
    {
        /// <summary>A room repository containing <paramref name="roomIds"/>.</summary>
        public static Mock<IRepository<Room>> Rooms(params int[] roomIds)
        {
            var rooms = roomIds
                .Select(id => new Room { Id = id, Description = $"Room {id}" })
                .ToList();

            var mock = new Mock<IRepository<Room>>();
            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            return mock;
        }

        /// <summary>A booking repository containing exactly <paramref name="bookings"/>.</summary>
        public static Mock<IRepository<Booking>> Bookings(params Booking[] bookings)
        {
            var mock = new Mock<IRepository<Booking>>();
            mock.Setup(r => r.GetAllAsync()).ReturnsAsync(bookings.ToList());
            return mock;
        }

        /// <summary>Convenience factory for an active booking on a given room.</summary>
        public static Booking ActiveBooking(int roomId, DateTime start, DateTime end) =>
            new Booking
            {
                Id = roomId,
                RoomId = roomId,
                CustomerId = 1,
                StartDate = start,
                EndDate = end,
                IsActive = true
            };

        /// <summary>Same, but cancelled — must be ignored by the availability logic.</summary>
        public static Booking InactiveBooking(int roomId, DateTime start, DateTime end)
        {
            var booking = ActiveBooking(roomId, start, end);
            booking.IsActive = false;
            return booking;
        }
    }
}
