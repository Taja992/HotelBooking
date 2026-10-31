using System;
using OpenQA.Selenium;

namespace HotelBooking.E2ETests.Pages
{
    public class BookingsListPage
    {
        private readonly IWebDriver _driver;
        private readonly Uri _baseAddress;

        public BookingsListPage(IWebDriver driver, Uri baseAddress)
        {
            _driver = driver;
            _baseAddress = baseAddress;
        }

        public void Navigate() =>
            _driver.Navigate().GoToUrl(new Uri(_baseAddress, "Bookings"));

        public bool IsCurrentPage()
        {
            var path = new Uri(_driver.Url).AbsolutePath;
            return path.Equals("/Bookings", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/Bookings/", StringComparison.OrdinalIgnoreCase);
        }

        // Index.cshtml has two tables: the calendar (no <thead>) and the bookings list (has <thead>).
        // ":has(thead)" selects only the bookings table; tbody tr gives only its body rows.
        public int BookingRowCount() =>
            _driver.FindElements(By.CssSelector("table:has(thead) tbody tr")).Count;
    }
}
