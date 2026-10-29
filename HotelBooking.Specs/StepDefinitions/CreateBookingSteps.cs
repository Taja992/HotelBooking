using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.Specs.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>When and Then steps for the "Create booking" feature.</summary>
    [Binding]
    public class CreateBookingSteps
    {
        private readonly HotelFixture hotel;

        private bool booked;
        private ArgumentException rejection;
        private int bookingsBefore;

        public CreateBookingSteps(HotelFixture hotel)
        {
            this.hotel = hotel;
        }

        [When("I book a room from day {int} to day {int}")]
        public Task WhenIBookARoom(int startOffset, int endOffset) =>
            Book(HotelFixture.Day(startOffset), HotelFixture.Day(endOffset));

        [When("I book a room from day {int} at {string} to day {int}")]
        public Task WhenIBookARoomAtATime(int startOffset, string time, int endOffset) =>
            Book(HotelFixture.Day(startOffset).Add(TimeSpan.Parse(time)), HotelFixture.Day(endOffset));

        [Then("the booking is confirmed")]
        public void ThenTheBookingIsConfirmed()
        {
            Assert.Null(rejection);
            Assert.True(booked);
        }

        [Then("the booking is declined")]
        public void ThenTheBookingIsDeclined()
        {
            Assert.Null(rejection);
            Assert.False(booked);
        }

        [Then("the dates are rejected as invalid")]
        public void ThenTheDatesAreRejectedAsInvalid()
        {
            Assert.NotNull(rejection);
        }

        [Then("the number of new bookings is {int}")]
        public void ThenTheNumberOfNewBookingsIs(int expected)
        {
            Assert.Equal(bookingsBefore + expected, hotel.BookingCount);
        }

        private async Task Book(DateTime start, DateTime end)
        {
            bookingsBefore = hotel.BookingCount;
            rejection = null;

            var request = new Booking
            {
                CustomerId = hotel.CustomerId,
                StartDate = start,
                EndDate = end
            };

            try
            {
                booked = await hotel.Manager.CreateBooking(request);
            }
            catch (ArgumentException ex)
            {
                // Kept for the Then step; any other exception fails the scenario here
                rejection = ex;
            }
        }
    }
}
