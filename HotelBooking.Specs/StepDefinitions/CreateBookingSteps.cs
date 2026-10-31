using System;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.Specs.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions
{
    [Binding]
    public class CreateBookingSteps
    {
        private readonly HotelContext hotel;
        private int bookingsBefore;
        private Booking request;
        private bool booked;
        private Exception error;

        public CreateBookingSteps(HotelContext hotel)
        {
            this.hotel = hotel;
        }

        [When("I book a room from day {int} to day {int}")]
        public async Task WhenIBookARoom(int start, int end)
        {
            bookingsBefore = hotel.Bookings.Items.Count;
            request = new Booking
            {
                CustomerId = 1,
                StartDate = DateTime.Today.AddDays(start),
                EndDate = DateTime.Today.AddDays(end)
            };

            try
            {
                booked = await hotel.CreateManager().CreateBooking(request);
            }
            catch (Exception ex)
            {
                error = ex;
            }
        }

        [Then("the booking is created")]
        public void ThenTheBookingIsCreated()
        {
            Assert.Null(error);
            Assert.True(booked);
            Assert.Equal(bookingsBefore + 1, hotel.Bookings.Items.Count);
            Assert.True(request.IsActive);
            Assert.Contains(hotel.Rooms.Items, r => r.Id == request.RoomId);
        }

        [Then("the booking is rejected")]
        public void ThenTheBookingIsRejected()
        {
            Assert.Null(error);
            Assert.False(booked);
            Assert.Equal(bookingsBefore, hotel.Bookings.Items.Count);
        }

        [Then("the booking fails because the dates are invalid")]
        public void ThenTheBookingFailsBecauseTheDatesAreInvalid()
        {
            Assert.IsType<ArgumentException>(error);
            Assert.Equal(bookingsBefore, hotel.Bookings.Items.Count);
        }
    }
}
