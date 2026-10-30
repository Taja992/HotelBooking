using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Specs.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions
{
    [Binding]
    public class FullyOccupiedDatesSteps
    {
        private readonly HotelContext hotel;
        private List<DateTime> fullyOccupiedDates;

        public FullyOccupiedDatesSteps(HotelContext hotel)
        {
            this.hotel = hotel;
        }

        [When("I ask for the fully occupied dates from day {int} to day {int}")]
        public async Task WhenIAskForTheFullyOccupiedDates(int start, int end)
        {
            fullyOccupiedDates = await hotel.CreateManager().GetFullyOccupiedDates(
                DateTime.Today.AddDays(start), DateTime.Today.AddDays(end));
        }

        [Then("the fully occupied dates are day {int} to day {int}")]
        public void ThenTheFullyOccupiedDatesAre(int start, int end)
        {
            var expected = Enumerable.Range(start, end - start + 1)
                .Select(d => DateTime.Today.AddDays(d));

            Assert.Equal(expected, fullyOccupiedDates);
        }

        [Then("there are no fully occupied dates")]
        public void ThenThereAreNoFullyOccupiedDates()
        {
            Assert.Empty(fullyOccupiedDates);
        }
    }
}
