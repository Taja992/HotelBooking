using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests.BlackBox
{
    /// <summary>
    /// Experience-based tests (A3, A5–A7) that break the assumptions behind
    /// the decision table. See docs/01-step-1-create-booking-black-box.md §11.
    /// </summary>
    public class CreateBookingAugmentedTests
    {
        private static Booking Request(DateTime start, DateTime end) => new Booking
        {
            CustomerId = 1,
            StartDate = start,
            EndDate = end
        };

        [Theory]
        [InlineData(15, 15, true)]   // exactly the gap
        [InlineData(14, 15, false)]  // gap plus the last day of the first booking
        [InlineData(15, 16, false)]  // gap plus the first day of the second booking
        public async Task CreateBooking_OneDayGapBetweenTwoBookings_BooksOnlyTheGap(
            int startOffset, int endOffset, bool expectedBooked)
        {
            // A3: one room, booked +10..+14 and +16..+20 -> only +15 is free

            // Arrange
            var today = DateTime.Today;
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, today.AddDays(10), today.AddDays(14)),
                    RepositoryMockFactory.ActiveBooking(1, today.AddDays(16), today.AddDays(20))).Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var booked = await manager.CreateBooking(
                Request(today.AddDays(startOffset), today.AddDays(endOffset)));

            // Assert
            Assert.Equal(expectedBooked, booked);
        }

        [Fact]
        public async Task CreateBooking_CallerSuppliesOccupiedRoomId_BookingGoesToFreeRoom()
        {
            // A5: the client asks for room 1, which is occupied; room 2 is free

            // Arrange
            var start = DateTime.Today.AddDays(5);
            var end = DateTime.Today.AddDays(7);

            var bookingRepository = RepositoryMockFactory.Bookings(
                RepositoryMockFactory.ActiveBooking(1, start, end));

            Booking saved = null;
            bookingRepository
                .Setup(r => r.AddAsync(It.IsAny<Booking>()))
                .Callback<Booking>(b => saved = b)
                .Returns(Task.CompletedTask);

            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            var request = Request(start, end);
            request.RoomId = 1;

            // Act
            await manager.CreateBooking(request);

            // Assert
            Assert.Equal(2, saved.RoomId);
        }

        [Fact(Skip = "D-1: start date with a time of day passes the 'not today' check")]
        public async Task CreateBooking_StartIsTodayWithTimeOfDay_ThrowsArgumentException()
        {
            // A6: a picker sends 00:00, but the API accepts any DateTime

            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            Task Act() => manager.CreateBooking(
                Request(DateTime.Today.AddSeconds(1), DateTime.Today.AddDays(2)));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
        }

        [Fact(Skip = "D-2: start at noon on the last occupied day double-books the room")]
        public async Task CreateBooking_StartsAtNoonOnLastOccupiedDay_ReturnsFalse()
        {
            // A7: the only room is occupied +10..+20 (midnight-to-midnight dates)

            // Arrange
            var occupiedStart = DateTime.Today.AddDays(10);
            var occupiedEnd = DateTime.Today.AddDays(20);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, occupiedStart, occupiedEnd)).Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var booked = await manager.CreateBooking(
                Request(occupiedEnd.AddHours(12), occupiedEnd.AddDays(1)));

            // Assert
            Assert.False(booked);
        }
    }
}
