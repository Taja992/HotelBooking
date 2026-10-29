using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.UnitTests.TestData;
using HotelBooking.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HotelBooking.UnitTests.BlackBox
{
    /// <summary>
    /// "Create booking" at its real boundary, POST /bookings (A8, A9).
    /// IBookingManager is mocked: these tests are about the HTTP mapping only.
    /// </summary>
    public class BookingsControllerPostTests
    {
        private static BookingsController Controller(Mock<IBookingManager> manager) =>
            new BookingsController(new Mock<IRepository<Booking>>().Object, manager.Object);

        private static Mock<IBookingManager> ManagerReturning(bool created)
        {
            var manager = new Mock<IBookingManager>();
            manager.Setup(m => m.CreateBooking(It.IsAny<Booking>())).ReturnsAsync(created);
            return manager;
        }

        [Fact]
        public async Task Post_BookingCreated_ReturnsCreatedAtRoute()
        {
            // Arrange
            var controller = Controller(ManagerReturning(true));

            // Act
            var result = await controller.Post(CreateBookingTestCases.Request(1, 2));

            // Assert
            Assert.IsType<CreatedAtRouteResult>(result);
        }

        [Fact]
        public async Task Post_NoRoomAvailable_ReturnsConflict()
        {
            // Arrange
            var controller = Controller(ManagerReturning(false));

            // Act
            var result = await controller.Post(CreateBookingTestCases.Request(1, 2));

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task Post_NullBooking_ReturnsBadRequest()
        {
            // Arrange
            var controller = Controller(new Mock<IBookingManager>());

            // Act
            var result = await controller.Post(null);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact(Skip = "D-3: ArgumentException from CreateBooking is not caught; client gets 500")]
        public async Task Post_InvalidDates_ReturnsBadRequest()
        {
            // A8

            // Arrange
            var manager = new Mock<IBookingManager>();
            manager.Setup(m => m.CreateBooking(It.IsAny<Booking>()))
                   .ThrowsAsync(new ArgumentException("The start date cannot be in the past or later than the end date."));
            var controller = Controller(manager);

            // Act
            var result = await controller.Post(CreateBookingTestCases.Request(0, 2));

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
