using Xunit;

namespace HotelBooking.UnitTests.TestData
{
    /// <summary>
    /// Strongly-typed rows for GetFullyOccupiedDates.
    /// Scenario for every row: 2 rooms, both fully booked from Today+10 to Today+20.
    /// Row = queryStartOffset, queryEndOffset, expectedNumberOfFullyOccupiedDates.
    /// </summary>
    public class OccupiedDatesTestData : TheoryData<int, int, int>
    {
        public const int OccupiedStartOffset = 10;
        public const int OccupiedEndOffset = 20;

        public OccupiedDatesTestData()
        {
            Add(1, 5, 0);    // entirely before the occupied period
            Add(1, 9, 0);    // ends the day before it starts
            Add(1, 10, 1);   // just reaches the first occupied day
            Add(10, 20, 11); // exactly the occupied period (inclusive on both ends)
            Add(12, 15, 4);  // strictly inside
            Add(20, 25, 1);  // starts on the last occupied day
            Add(21, 30, 0);  // entirely after
            Add(1, 30, 11);  // spans the whole occupied period
            Add(15, 15, 1);  // single-day query inside the period
            Add(5, 5, 0);    // single-day query outside the period
        }
    }
}
