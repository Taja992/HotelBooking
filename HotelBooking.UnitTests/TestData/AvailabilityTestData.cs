using System.Collections.Generic;

namespace HotelBooking.UnitTests.TestData
{
    /// <summary>
    /// Boundary / equivalence-class table for room availability.
    /// The single occupied booking spans Today+10 .. Today+20
    /// (see OccupiedStartOffset / OccupiedEndOffset).
    /// Each row is: startOffset, endOffset, expectedAvailable, description.
    /// Offsets are days relative to DateTime.Today, so the data never expires.
    /// </summary>
    public static class AvailabilityTestData
    {
        public const int OccupiedStartOffset = 10;
        public const int OccupiedEndOffset = 20;

        public static IEnumerable<object[]> Cases()
        {
            //                         start, end, expectedAvailable, description
            yield return new object[] {  1,  2, true,  "entirely before the occupied period" };
            yield return new object[] {  8,  9, true,  "ends the day before the occupied period" };
            yield return new object[] {  9, 10, false, "ends exactly on the first occupied day" };
            yield return new object[] {  5, 15, false, "overlaps the start of the occupied period" };
            yield return new object[] { 10, 20, false, "identical to the occupied period" };
            yield return new object[] { 12, 18, false, "entirely inside the occupied period" };
            yield return new object[] { 15, 25, false, "overlaps the end of the occupied period" };
            yield return new object[] { 20, 25, false, "starts exactly on the last occupied day" };
            yield return new object[] {  5, 25, false, "spans the whole occupied period" };
            yield return new object[] { 21, 22, true,  "starts the day after the occupied period" };
            yield return new object[] { 30, 40, true,  "entirely after the occupied period" };
        }
    }
}
