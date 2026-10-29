using OpenQA.Selenium;

namespace HotelBooking.E2ETests.Pages
{
    /// <summary>Page object for /Bookings (the calendar and the list of bookings).</summary>
    public class BookingsListPage
    {
        private readonly IWebDriver driver;

        // The page has two tables: the calendar first, the list of bookings second
        private readonly By bookingRows = By.CssSelector("table.table:nth-of-type(2) tbody tr");

        public BookingsListPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void Open()
        {
            driver.Navigate().GoToUrl(MvcAppFixture.BaseUrl + "/Bookings");
        }

        public bool IsOpen => driver.Url.EndsWith("/Bookings");

        public int BookingCount => driver.FindElements(bookingRows).Count;
    }
}
