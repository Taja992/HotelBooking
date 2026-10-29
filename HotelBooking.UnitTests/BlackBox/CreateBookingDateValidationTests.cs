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
    /// Equivalence classes EC1–EC4 and boundary values V1–V6. The booking
    /// repository is empty, so only date validation can decide the outcome.
    /// </summary>
    public class CreateBookingDateValidationTests
    {
        [Theory]
        [MemberData(nameof(CreateBookingTestCases.InvalidDates),
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_InvalidDates_ThrowsArgumentException(
            int startOffset, int endOffset)
        {
            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            Task Act() => manager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset));

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(Act);
        }

        [Theory]
        [MemberData(nameof(CreateBookingTestCases.InvalidDates),
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_InvalidDates_DoesNotSaveBooking(
            int startOffset, int endOffset)
        {
            // Arrange
            var bookingRepository = RepositoryMockFactory.Bookings();
            var manager = new BookingManager(
                bookingRepository.Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act: the exception is asserted by the test above
            await Record.ExceptionAsync(() => manager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset)));

            // Assert
            bookingRepository.Verify(r => r.AddAsync(It.IsAny<Booking>()), Times.Never);
        }

        [Fact]
        public async Task CreateBooking_StartTomorrowAndOneDayLong_ReturnsTrue()
        {
            // V4: SD just above the today boundary, and SD = ED (on the SD <= ED boundary)

            // Arrange
            var manager = new BookingManager(
                RepositoryMockFactory.Bookings().Object,
                RepositoryMockFactory.Rooms(1).Object);

            // Act
            var booked = await manager.CreateBooking(CreateBookingTestCases.Request(1, 1));

            // Assert
            Assert.True(booked);
        }
    }
}
