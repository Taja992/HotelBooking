using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace HotelBooking.E2ETests.Pages
{
    /// <summary>Page object for /Bookings/Create.</summary>
    public class CreateBookingPage
    {
        private readonly IWebDriver driver;

        // Locators: how to find each element on the page
        private readonly By startDate = By.Id("StartDate");
        private readonly By endDate = By.Id("EndDate");
        private readonly By createButton = By.CssSelector("input[type=submit]");
        private readonly By statusMessage = By.CssSelector("h4.text-danger");

        public CreateBookingPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        public void Open()
        {
            driver.Navigate().GoToUrl(MvcAppFixture.BaseUrl + "/Bookings/Create");
        }

        public void BookRoom(DateTime start, DateTime end)
        {
            SetDate(startDate, start);
            SetDate(endDate, end);
            IWebElement button = driver.FindElement(createButton);
            button.Click();

            // Click() does not wait for the next page. Wait until the old
            // button is gone, which means the server's answer has loaded.
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(_ => IsGone(button));
        }

        public string StatusMessage => driver.FindElement(statusMessage).Text;

        // A date field shows its value in the computer's date format, so typing
        // "2026-11-02" only works on some machines. The field's value is always
        // yyyy-MM-dd, so we set it directly with JavaScript.
        private void SetDate(By field, DateTime date)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].value = arguments[1];",
                driver.FindElement(field), date.ToString("yyyy-MM-dd"));
        }

        private static bool IsGone(IWebElement element)
        {
            try
            {
                _ = element.Enabled;   // throws once the page has been replaced
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return true;
            }
        }
    }
}
