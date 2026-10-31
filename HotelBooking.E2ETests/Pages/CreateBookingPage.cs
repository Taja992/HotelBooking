using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace HotelBooking.E2ETests.Pages
{
    public class CreateBookingPage
    {
        private readonly IWebDriver _driver;
        private readonly Uri _baseAddress;

        public CreateBookingPage(IWebDriver driver, Uri baseAddress)
        {
            _driver = driver;
            _baseAddress = baseAddress;
        }

        public void Navigate() =>
            _driver.Navigate().GoToUrl(new Uri(_baseAddress, "Bookings/Create"));

        private void SetDate(string id, DateTime date)
        {
            // type="date" inputs require JS assignment; SendKeys is locale-dependent and unreliable
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "document.getElementById(arguments[0]).value = arguments[1];",
                id, date.ToString("yyyy-MM-dd"));
        }

        public void SetStartDate(DateTime d) => SetDate("StartDate", d);
        public void SetEndDate(DateTime d)   => SetDate("EndDate", d);

        public void SelectFirstCustomer() =>
            new SelectElement(_driver.FindElement(By.Id("CustomerId"))).SelectByIndex(0);

        public void ClickCreate() =>
            _driver.FindElement(By.CssSelector("input[type='submit'][value='Create']")).Click();

        public string StatusMessage =>
            _driver.FindElement(By.CssSelector("h4.text-danger")).Text;
    }
}
