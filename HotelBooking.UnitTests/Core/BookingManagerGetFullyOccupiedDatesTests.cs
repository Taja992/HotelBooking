using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests.Core
{
    public class BookingManagerGetFullyOccupiedDatesTests
    {
        /// <summary>2 rooms, both booked solid from Today+10 to Today+20.</summary>
        private static BookingManager FullyBookedHotel()
        {
            var start = DateTime.Today.AddDays(OccupiedDatesTestData.OccupiedStartOffset);
            var end = DateTime.Today.AddDays(OccupiedDatesTestData.OccupiedEndOffset);

            return new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, start, end),
                    RepositoryMockFactory.ActiveBooking(2, start, end)).Object,
                RepositoryMockFactory.Rooms(1, 2).Object);
        }

        [Theory]
        [InlineData(5, 4)]     // start one day after end
        [InlineData(20, 10)]   // start well after end
        [InlineData(0, -1)]    // both in the past, still start > end
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException(
            int startOffset, int endOffset)
        {
            // Arrange
            var manager = FullyBookedHotel();

            // Act
            Task Act() => manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(startOffset),
                DateTime.Today.AddDays(endOffset));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
        }

        [Theory]
        [ClassData(typeof(OccupiedDatesTestData))]
        public async Task GetFullyOccupiedDates_HotelFullyBooked_ReturnsExpectedNumberOfDates(
            int startOffset, int endOffset, int expectedCount)
        {
            // Arrange
            var manager = FullyBookedHotel();

            // Act
            var dates = await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(startOffset),
                DateTime.Today.AddDays(endOffset));

            // Assert
            Assert.Equal(expectedCount, dates.Count);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_HotelFullyBooked_ReturnsEveryDayInTheOccupiedPeriod()
        {
            // Strong assertion: the returned dates are the *right* dates, not just
            // the right number of them.

            // Arrange
            var manager = FullyBookedHotel();
            var occupiedStart = DateTime.Today.AddDays(OccupiedDatesTestData.OccupiedStartOffset);
            var occupiedEnd = DateTime.Today.AddDays(OccupiedDatesTestData.OccupiedEndOffset);

            var expected = Enumerable
                .Range(0, (occupiedEnd - occupiedStart).Days + 1)
                .Select(offset => occupiedStart.AddDays(offset))
                .ToList();

            // Act
            var dates = await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(30));

            // Assert
            Assert.Equal(expected, dates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_NoBookings_ReturnsEmptyList()
        {
            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var dates = await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(30));

            // Assert
            Assert.Empty(dates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_OnlySomeRoomsBooked_ReturnsEmptyList()
        {
            // Arrange: 2 rooms but only room 1 is booked -> never fully occupied
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(20);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, start, end)).Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var dates = await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(30));

            // Assert
            Assert.Empty(dates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_BookingsAreInactive_ReturnsEmptyList()
        {
            // Arrange: cancelled bookings must not count towards occupancy
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(20);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.InactiveBooking(1, start, end),
                    RepositoryMockFactory.InactiveBooking(2, start, end)).Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var dates = await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(30));

            // Assert
            Assert.Empty(dates);
        }

        [Fact]
        public async Task GetFullyOccupiedDates_ReadsBothRepositoriesExactlyOnce()
        {
            // Guards against a regression where the repositories are queried inside
            // the day loop (an O(n) round-trip performance bug).

            // Arrange
            var bookingRepository = RepositoryMockFactory.Bookings(
                RepositoryMockFactory.ActiveBooking(
                    1, DateTime.Today.AddDays(10), DateTime.Today.AddDays(20)));
            var roomRepository = RepositoryMockFactory.Rooms(1, 2);

            var manager = new BookingManager(bookingRepository.Object, roomRepository.Object);

            // Act
            await manager.GetFullyOccupiedDates(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(30));

            // Assert
            bookingRepository.Verify(r => r.GetAllAsync(), Times.Once);
            roomRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }
    }
}
