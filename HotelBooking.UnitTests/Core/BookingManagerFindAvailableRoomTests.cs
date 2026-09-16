using System;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using HotelBooking.UnitTests.TestDoubles;
using Xunit;

namespace HotelBooking.UnitTests.Core
{
    public class BookingManagerFindAvailableRoomTests
    {
        // ---------- Invalid arguments: data-driven with [InlineData] ----------

        [Theory]
        [InlineData(0, 5)]    // start date is today  -> not in the future
        [InlineData(-1, 5)]   // start date is in the past
        [InlineData(-10, -5)] // both dates in the past
        [InlineData(5, 4)]    // start date after end date
        [InlineData(5, -1)]   // end date before today and before start
        public async Task FindAvailableRoom_InvalidDateRange_ThrowsArgumentException(
            int startOffset, int endOffset)
        {
            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms(1).Object);

            var start = DateTime.Today.AddDays(startOffset);
            var end = DateTime.Today.AddDays(endOffset);

            // Act
            Task Act() => manager.FindAvailableRoom(start, end);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
        }

        // ---------- Boundary table: data-driven with [MemberData] ----------

        [Theory]
        [MemberData(nameof(AvailabilityTestData.Cases), MemberType = typeof(AvailabilityTestData))]
        public async Task FindAvailableRoom_SingleRoomOccupied_ReturnsRoomOnlyWhenPeriodDoesNotOverlap(
            int startOffset, int endOffset, bool expectedAvailable, string because)
        {
            // Arrange: one room, occupied Today+10 .. Today+20
            var occupied = RepositoryMockFactory.ActiveBooking(
                roomId: 1,
                start: DateTime.Today.AddDays(AvailabilityTestData.OccupiedStartOffset),
                end: DateTime.Today.AddDays(AvailabilityTestData.OccupiedEndOffset));

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(occupied).Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var roomId = await manager.FindAvailableRoom(
                DateTime.Today.AddDays(startOffset),
                DateTime.Today.AddDays(endOffset));

            // Assert
            // The 'because' description is folded into the failure message so a red
            // row names the boundary class it belongs to, not just its offsets.
            var expectedRoomId = expectedAvailable ? 1 : -1;
            Assert.True(expectedRoomId == roomId,
                $"Period Today+{startOffset}..Today+{endOffset} is {because}, " +
                $"so expected room id {expectedRoomId} but got {roomId}.");
        }

        // ---------- Structural cases ----------

        [Fact]
        public async Task FindAvailableRoom_NoRoomsExist_ReturnsMinusOne()
        {
            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms().Object);

            // Act
            var roomId = await manager.FindAvailableRoom(
                DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_FirstRoomOccupied_ReturnsSecondRoom()
        {
            // Arrange: room 1 booked for the requested period, room 2 free
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(12);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, start, end)).Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var roomId = await manager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(2, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_AllRoomsOccupied_ReturnsMinusOne()
        {
            // Arrange
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(12);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, start, end),
                    RepositoryMockFactory.ActiveBooking(2, start, end)).Object,
                RepositoryMockFactory.Rooms(1, 2).Object);

            // Act
            var roomId = await manager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_OverlappingBookingIsInactive_RoomIsStillAvailable()
        {
            // Arrange: the only overlapping booking has been cancelled
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(12);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.InactiveBooking(1, start, end)).Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var roomId = await manager.FindAvailableRoom(start, end);

            // Assert
            Assert.Equal(1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnedRoomHasNoOverlappingActiveBooking()
        {
            // Strong assertion: don't just check "not -1" — check the returned room
            // really is free for the whole requested period.

            // Arrange
            var start = DateTime.Today.AddDays(10);
            var end = DateTime.Today.AddDays(12);

            var bookings = new[]
            {
                RepositoryMockFactory.ActiveBooking(1, start, end),
                RepositoryMockFactory.ActiveBooking(
                    2, DateTime.Today.AddDays(30), DateTime.Today.AddDays(40))
            };

            var bookingRepository = RepositoryMockFactory.Bookings(bookings);
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1, 2, 3).Object);

            // Act
            var roomId = await manager.FindAvailableRoom(start, end);

            // Assert
            var conflicting = (await bookingRepository.Object.GetAllAsync())
                .Where(b => b.IsActive
                            && b.RoomId == roomId
                            && b.StartDate <= end
                            && b.EndDate >= start);

            Assert.Empty(conflicting);
        }
    }
}
