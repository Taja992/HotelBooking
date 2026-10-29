using System;
using HotelBooking.Core;
using Xunit;

namespace HotelBooking.UnitTests.TestData
{
    /// <summary>
    /// Black-box test cases for "Create booking", transcribed from
    /// docs/01-step-1-create-booking-black-box.md (§2 and §3.6).
    /// All values are day offsets from DateTime.Today.
    /// </summary>
    public static class CreateBookingTestCases
    {
        /// <summary>Fully occupied range O used by the unit tests.</summary>
        public const int OccupiedStart = 10;
        public const int OccupiedEnd = 20;

        /// <summary>
        /// Extended-entry decision table (TC1–TC10). Classes are relative to the
        /// fully occupied range O: B = before O, A = after O.
        /// Each row: startOffset, endOffset, expectedBooked.
        /// </summary>
        public static TheoryData<int, int, bool> DecisionTable(int occupiedStart, int occupiedEnd)
        {
            int firstB = 1, lastB = occupiedStart - 1;
            int firstO = occupiedStart, lastO = occupiedEnd;
            int firstA = occupiedEnd + 1;

            return new TheoryData<int, int, bool>
            {
                Row("TC1 SD=B ED=B",  firstB, lastB,  true),
                Row("TC2 SD=A ED=A",  firstA, firstA, true),
                Row("TC3 SD=B ED=A",  lastB,  firstA, false),
                Row("TC4 SD=B ED=O",  lastB,  firstO, false),
                Row("TC5 SD=B ED=O",  lastB,  lastO,  false),
                Row("TC6 SD=O ED=A",  firstO, firstA, false),
                Row("TC7 SD=O ED=A",  lastO,  firstA, false),
                Row("TC8 SD=O ED=O",  firstO, firstO, false),
                Row("TC9 SD=O ED=O",  firstO, lastO,  false),
                Row("TC10 SD=O ED=O", lastO,  lastO,  false),
            };
        }

        /// <summary>ECT/BVT invalid inputs (V1–V3, V5–V6). Each breaks exactly one rule.</summary>
        public static TheoryData<int, int> InvalidDates => new()
        {
            Row("V1 SD = today",                0, 5),
            Row("V2 SD = yesterday",           -1, 5),
            Row("V3 whole period in the past", -5, -1),
            Row("V5 SD = ED + 1",               6, 5),
            Row("V6 SD in A, ED in B",          OccupiedEnd + 1, OccupiedStart - 1),
        };

        public static Booking Request(int startOffset, int endOffset) => new Booking
        {
            CustomerId = 1,
            StartDate = DateTime.Today.AddDays(startOffset),
            EndDate = DateTime.Today.AddDays(endOffset)
        };

        private static TheoryDataRow<int, int, bool> Row(string id, int start, int end, bool expected) =>
            new(start, end, expected) { Label = $"{id} ({Offset(start)}..{Offset(end)})" };

        private static TheoryDataRow<int, int> Row(string id, int start, int end) =>
            new(start, end) { Label = $"{id} ({Offset(start)}..{Offset(end)})" };

        private static string Offset(int days) => days.ToString("+0;-0;0");
    }
}
