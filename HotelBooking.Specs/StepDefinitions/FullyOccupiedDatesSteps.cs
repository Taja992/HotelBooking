using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Specs.Support;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions
{
    /// <summary>When and Then steps for the optional "Fully occupied dates" feature.</summary>
    [Binding]
    public class FullyOccupiedDatesSteps
    {
        private readonly HotelFixture hotel;
        private List<DateTime> fullyOccupiedDates;

        public FullyOccupiedDatesSteps(HotelFixture hotel)
        {
            this.hotel = hotel;
        }

        [When("I ask for the fully occupied dates from day {int} to day {int}")]
        public async Task WhenIAskForTheFullyOccupiedDates(int startOffset, int endOffset)
        {
            fullyOccupiedDates = await hotel.Manager.GetFullyOccupiedDates(
                HotelFixture.Day(startOffset), HotelFixture.Day(endOffset));
        }

        [Then(@"the fully occupied dates are (.*)")]
        public void ThenTheFullyOccupiedDatesAre(List<DateTime> expected)
        {
            // Strong assertion: the exact dates, not just how many
            Assert.Equal(expected, fullyOccupiedDates);
        }

        [StepArgumentTransformation(@"day (-?\d+) to day (-?\d+)")]
        public List<DateTime> DayRange(int firstOffset, int lastOffset) =>
            Enumerable.Range(firstOffset, lastOffset - firstOffset + 1)
                      .Select(HotelFixture.Day)
                      .ToList();

        [StepArgumentTransformation("none")]
        public List<DateTime> NoDays() => new List<DateTime>();
    }
}
