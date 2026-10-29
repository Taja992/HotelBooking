using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests.BlackBox
{
    /// <summary>
    /// Decision table TC1–TC10 at unit level. Both rooms are booked for the
    /// whole of O = Today+10 .. Today+20, i.e. O is fully occupied.
    /// </summary>
    public class CreateBookingDecisionTableTests
    {
        private static Mock<IRepository<Booking>> FullyOccupiedBookings()
        {
            var start = DateTime.Today.AddDays(CreateBookingTestCases.OccupiedStart);
            var end = DateTime.Today.AddDays(CreateBookingTestCases.OccupiedEnd);

            return RepositoryMockFactory.Bookings(
                RepositoryMockFactory.ActiveBooking(1, start, end),
                RepositoryMockFactory.ActiveBooking(2, start, end));
        }

        [Theory]
        [MemberData(nameof(CreateBookingTestCases.DecisionTable),
            CreateBookingTestCases.OccupiedStart, CreateBookingTestCases.OccupiedEnd,
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_DecisionTableRule_ReturnsExpectedResult(
            int startOffset, int endOffset, bool expectedBooked)
        {
            // Arrange
            var manager = new BookingManager(
                FullyOccupiedBookings().Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var booked = await manager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset));

            // Assert
            Assert.Equal(expectedBooked, booked);
        }

        [Theory]
        [MemberData(nameof(CreateBookingTestCases.DecisionTable),
            CreateBookingTestCases.OccupiedStart, CreateBookingTestCases.OccupiedEnd,
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_DecisionTableRule_SavesOnlyWhenBooked(
            int startOffset, int endOffset, bool expectedBooked)
        {
            // Arrange
            var bookingRepository = FullyOccupiedBookings();
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            await manager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset));

            // Assert
            bookingRepository.Verify(
                r => r.AddAsync(It.IsAny<Booking>()),
                Times.Exactly(expectedBooked ? 1 : 0));
        }
    }
}
