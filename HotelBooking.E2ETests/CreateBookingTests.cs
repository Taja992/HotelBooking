using System;
using HotelBooking.E2ETests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace HotelBooking.E2ETests
{
    /// <summary>
    /// The Step 1 test cases for "Create booking", run in a real browser.
    /// Fixture = the app's seed: all 3 rooms booked day 4 – day 18
    /// (the same day offsets as the Postman tests in Step 3).
    /// </summary>
    public class CreateBookingTests : IClassFixture<MvcAppFixture>, IDisposable
    {
        private const string NoRoomMessage =
            "The booking could not be created. There were no available room.";

        private readonly IWebDriver driver;
        private readonly CreateBookingPage createPage;
        private readonly BookingsListPage listPage;

        // xUnit creates a new instance for every test, so every test gets its own browser
        public CreateBookingTests(MvcAppFixture app)
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");   // remove this line to watch the browser

            driver = new ChromeDriver(options);
            createPage = new CreateBookingPage(driver);
            listPage = new BookingsListPage(driver);
        }

        public void Dispose()
        {
            driver.Quit();
        }

        [Theory]
        [InlineData(1, 3, Label = "TC1")]
        [InlineData(19, 19, Label = "TC2")]
        [InlineData(1, 1, Label = "V4")]
        public void CreateBooking_FreePeriod_BookingIsConfirmed(int start, int end)
        {
            // Arrange
            int bookingsBefore = CountBookings();
            createPage.Open();

            // Act
            createPage.BookRoom(Day(start), Day(end));

            // Assert
            Assert.True(listPage.IsOpen, "Expected to be sent to the bookings list");
            Assert.Equal(bookingsBefore + 1, listPage.BookingCount);
        }

        [Theory]
        [InlineData(3, 19, Label = "TC3")]
        [InlineData(3, 4, Label = "TC4")]
        [InlineData(3, 18, Label = "TC5")]
        [InlineData(4, 19, Label = "TC6")]
        [InlineData(18, 19, Label = "TC7")]
        [InlineData(4, 4, Label = "TC8")]
        [InlineData(4, 18, Label = "TC9")]
        [InlineData(18, 18, Label = "TC10")]
        public void CreateBooking_OccupiedPeriod_BookingIsDeclined(int start, int end)
        {
            // Arrange
            int bookingsBefore = CountBookings();
            createPage.Open();

            // Act
            createPage.BookRoom(Day(start), Day(end));

            // Assert
            Assert.Equal(NoRoomMessage, createPage.StatusMessage);
            Assert.Equal(bookingsBefore, CountBookings());
        }

        // Known defect: invalid dates crash the page (500) instead of showing a message
        [Theory]
        [InlineData(0, 5, Label = "V1", Skip = "Defect D-4")]
        [InlineData(-1, 5, Label = "V2", Skip = "Defect D-4")]
        [InlineData(-5, -1, Label = "V3", Skip = "Defect D-4")]
        [InlineData(6, 5, Label = "V5", Skip = "Defect D-4")]
        [InlineData(19, 3, Label = "V6", Skip = "Defect D-4")]
        public void CreateBooking_InvalidDates_ShowsErrorMessage(int start, int end)
        {
            // Arrange
            int bookingsBefore = CountBookings();
            createPage.Open();

            // Act
            createPage.BookRoom(Day(start), Day(end));

            // Assert
            Assert.NotEmpty(createPage.StatusMessage);
            Assert.Equal(bookingsBefore, CountBookings());
        }

        private int CountBookings()
        {
            listPage.Open();
            return listPage.BookingCount;
        }

        private static DateTime Day(int offset) => DateTime.Today.AddDays(offset);
    }
}
