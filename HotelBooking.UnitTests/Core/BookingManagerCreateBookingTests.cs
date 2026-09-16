using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestDoubles;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests.Core
{
    public class BookingManagerCreateBookingTests
    {
        private static Booking Request(int startOffset, int endOffset) => new Booking
        {
            CustomerId = 1,
            StartDate = DateTime.Today.AddDays(startOffset),
            EndDate = DateTime.Today.AddDays(endOffset)
        };

        [Fact]
        public async Task CreateBooking_RoomIsAvailable_ReturnsTrue()
        {
            // Arrange
            var bookingRepository = RepositoryMockFactory.Bookings();
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var created = await manager.CreateBooking(Request(1, 2));

            // Assert
            Assert.True(created);
        }

        [Fact]
        public async Task CreateBooking_RoomIsAvailable_SavesBookingExactlyOnce()
        {
            // Interaction test: persistence is part of the contract, and only a
            // mock can observe it.

            // Arrange
            var bookingRepository = RepositoryMockFactory.Bookings();
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            await manager.CreateBooking(Request(1, 2));

            // Assert
            bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Once);
        }

        [Fact]
        public async Task CreateBooking_RoomIsAvailable_SavesBookingAsActiveOnTheFreeRoom()
        {
            // Arrange: room 1 is taken for the requested period, room 2 is free
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

            // Act
            await manager.CreateBooking(Request(5, 7));

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(2, saved.RoomId);
            Assert.True(saved.IsActive);
            Assert.Equal(start, saved.StartDate);
            Assert.Equal(end, saved.EndDate);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_ReturnsFalse()
        {
            // Arrange: the only room is occupied for the requested period
            var start = DateTime.Today.AddDays(5);
            var end = DateTime.Today.AddDays(7);

            var manager = new BookingManager(
                RepositoryMockFactory.Bookings(
                    RepositoryMockFactory.ActiveBooking(1, start, end)).Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var created = await manager.CreateBooking(Request(5, 7));

            // Assert
            Assert.False(created);
        }

        [Fact]
        public async Task CreateBooking_NoRoomAvailable_DoesNotSaveBooking()
        {
            // Arrange
            var start = DateTime.Today.AddDays(5);
            var end = DateTime.Today.AddDays(7);

            var bookingRepository = RepositoryMockFactory.Bookings(
                RepositoryMockFactory.ActiveBooking(1, start, end));

            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            await manager.CreateBooking(Request(5, 7));

            // Assert
            bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Theory]
        [InlineData(0, 5)]   // start date is today
        [InlineData(-3, 5)]  // start date in the past
        [InlineData(9, 8)]   // start date after end date
        public async Task CreateBooking_InvalidDateRange_ThrowsArgumentExceptionAndDoesNotSave(
            int startOffset, int endOffset)
        {
            // Arrange
            var bookingRepository = RepositoryMockFactory.Bookings();
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            Task Act() => manager.CreateBooking(Request(startOffset, endOffset));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
            bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }
    }
}
