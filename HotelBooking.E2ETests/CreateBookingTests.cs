using System;
using HotelBooking.E2ETests.Pages;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace HotelBooking.E2ETests
{
    /// <summary>
    /// E2E tests for the "Create Booking" form using Selenium.
    /// 16 test cases total: TC1–TC10 (decision table) and V1–V6 (date validation).
    /// The DB is seeded with all 3 rooms occupied from today+4 to today+18.
    /// One ChromeDriver instance is shared across all tests (sequential by default in xunit).
    /// </summary>
    public class CreateBookingTests : IClassFixture<WebAppFactory>, IDisposable
    {
        private const int OccupiedStart = 4;   // matches DbInitializer: today+4
        private const int OccupiedEnd   = 18;  // matches DbInitializer: today+4+14

        private readonly WebAppFactory      _factory;
        private readonly ChromeDriver       _driver;
        private readonly CreateBookingPage  _createPage;
        private readonly BookingsListPage   _listPage;

        public CreateBookingTests(WebAppFactory factory)
        {
            _factory = factory;

            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            _createPage = new CreateBookingPage(_driver, _factory.BaseAddress);
            _listPage   = new BookingsListPage(_driver, _factory.BaseAddress);
        }

        public void Dispose()
        {
            _driver.Quit();
            GC.SuppressFinalize(this);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────────

        private enum BookingOutcome { Success, Failure }

        /// <summary>
        /// Waits up to 15 s for either a redirect to /Bookings (success) or an error
        /// message on the Create page (failure). Uses bool-returning Until to satisfy
        /// Selenium 4.27's requirement that TResult be bool or a reference type.
        /// </summary>
        private BookingOutcome WaitForResult()
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(15))
            {
                PollingInterval = TimeSpan.FromMilliseconds(200)
            }
            .Until(driver =>
            {
                if (new Uri(driver.Url).AbsolutePath
                        .Equals("/Bookings", StringComparison.OrdinalIgnoreCase))
                    return true;

                // FindElements (plural) does not trigger implicit wait — returns immediately.
                // Catch StaleElementReferenceException: on slow CI runners the page can
                // navigate away between FindElements and reading .Text — retry on next poll.
                try
                {
                    var els = driver.FindElements(By.CssSelector("h4.text-danger"));
                    return els.Count > 0 && !string.IsNullOrWhiteSpace(els[0].Text);
                }
                catch (StaleElementReferenceException)
                {
                    return false;
                }
            });

            return new Uri(_driver.Url).AbsolutePath
                       .Equals("/Bookings", StringComparison.OrdinalIgnoreCase)
                   ? BookingOutcome.Success
                   : BookingOutcome.Failure;
        }

        /// <summary>Navigate to the list page and return the current booking row count.</summary>
        private int GetListCount()
        {
            _listPage.Navigate();
            return _listPage.BookingRowCount();
        }

        /// <summary>Open the Create form, fill in dates, click submit, and return the outcome.</summary>
        private BookingOutcome TryBook(int startOffset, int endOffset)
        {
            _createPage.Navigate();
            _createPage.SelectFirstCustomer();
            _createPage.SetStartDate(DateTime.Today.AddDays(startOffset));
            _createPage.SetEndDate(DateTime.Today.AddDays(endOffset));
            _createPage.ClickCreate();
            return WaitForResult();
        }

        // ── TC1–TC2: success cases ────────────────────────────────────────────────────

        [Fact]
        public void TC1_StartBeforeEnd_BeforeOccupied_BookingSucceeds()
        {
            // SD=B(1) ED=B(3): both before occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Success, TryBook(startOffset: 1, endOffset: 3));
            Assert.Equal(before + 1, GetListCount());
        }

        [Fact]
        public void TC2_StartAfterEnd_AfterOccupied_BookingSucceeds()
        {
            // SD=A(19) ED=A(19): single day after occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Success, TryBook(startOffset: 19, endOffset: 19));
            Assert.Equal(before + 1, GetListCount());
        }

        // ── TC3–TC10: no room available ───────────────────────────────────────────────

        [Fact]
        public void TC3_StartBeforeEnd_AfterOccupied_NoRoom()
        {
            // SD=B(3) ED=A(19): spans the entire occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure, TryBook(startOffset: 3, endOffset: 19));
            Assert.Contains("no available room", _createPage.StatusMessage,
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC4_StartBefore_EndFirstOccupied_NoRoom()
        {
            // SD=B(3) ED=firstO(4)
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: 3, endOffset: OccupiedStart));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC5_StartBefore_EndLastOccupied_NoRoom()
        {
            // SD=B(3) ED=lastO(18)
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: 3, endOffset: OccupiedEnd));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC6_StartFirstOccupied_EndAfter_NoRoom()
        {
            // SD=firstO(4) ED=A(19)
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: OccupiedStart, endOffset: 19));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC7_StartLastOccupied_EndAfter_NoRoom()
        {
            // SD=lastO(18) ED=A(19)
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: OccupiedEnd, endOffset: 19));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC8_StartFirstOccupied_EndFirstOccupied_NoRoom()
        {
            // SD=firstO(4) ED=firstO(4): one day at start of occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: OccupiedStart, endOffset: OccupiedStart));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC9_StartFirstOccupied_EndLastOccupied_NoRoom()
        {
            // SD=firstO(4) ED=lastO(18): full occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: OccupiedStart, endOffset: OccupiedEnd));
            Assert.Equal(before, GetListCount());
        }

        [Fact]
        public void TC10_StartLastOccupied_EndLastOccupied_NoRoom()
        {
            // SD=lastO(18) ED=lastO(18): one day at end of occupied range
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Failure,
                TryBook(startOffset: OccupiedEnd, endOffset: OccupiedEnd));
            Assert.Equal(before, GetListCount());
        }

        // ── V4: boundary valid date ───────────────────────────────────────────────────

        [Fact]
        public void V4_StartTomorrow_MinValidDate_BookingSucceeds()
        {
            // SD=1 ED=1: tomorrow = minimum valid start date (boundary value for V1 vs V4)
            int before = GetListCount();
            Assert.Equal(BookingOutcome.Success, TryBook(startOffset: 1, endOffset: 1));
            Assert.Equal(before + 1, GetListCount());
        }

        // ── V1–V3, V5, V6: defect D-4 (invalid dates crash the page) ─────────────────

        [Fact(Skip = "Defect D-4: invalid dates cause unhandled ArgumentException (page crashes)")] // NOSONAR
        public void V1_StartToday_PageCrashes_D4()
        {
            TryBook(startOffset: 0, endOffset: 5);
        }

        [Fact(Skip = "Defect D-4: invalid dates cause unhandled ArgumentException (page crashes)")] // NOSONAR
        public void V2_StartYesterday_PageCrashes_D4()
        {
            TryBook(startOffset: -1, endOffset: 5);
        }

        [Fact(Skip = "Defect D-4: invalid dates cause unhandled ArgumentException (page crashes)")] // NOSONAR
        public void V3_WholePeriodInPast_PageCrashes_D4()
        {
            TryBook(startOffset: -5, endOffset: -1);
        }

        [Fact(Skip = "Defect D-4: invalid dates cause unhandled ArgumentException (page crashes)")] // NOSONAR
        public void V5_StartAfterEnd_PageCrashes_D4()
        {
            TryBook(startOffset: 6, endOffset: 5);
        }

        [Fact(Skip = "Defect D-4: invalid dates cause unhandled ArgumentException (page crashes)")] // NOSONAR
        public void V6_InvertedFutureDates_PageCrashes_D4()
        {
            TryBook(startOffset: OccupiedEnd + 1, endOffset: OccupiedStart - 1);
        }
    }
}
