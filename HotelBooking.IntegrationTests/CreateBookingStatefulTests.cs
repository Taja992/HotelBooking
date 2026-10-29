using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Repositories;
using HotelBooking.UnitTests.TestData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelBooking.IntegrationTests
{
    /// <summary>A10: bookings accumulate. Seed has 3 rooms, all free on Today+1..+2.</summary>
    public class CreateBookingStatefulTests : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly BookingManager bookingManager;

        public CreateBookingStatefulTests()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<HotelBookingContext>()
                            .UseSqlite(connection).Options;
            var dbContext = new HotelBookingContext(options);
            new DbInitializer().Initialize(dbContext);

            bookingManager = new BookingManager(
                new BookingRepository(dbContext), new RoomRepository(dbContext));
        }

        public void Dispose()
        {
            connection.Close();
        }

        [Fact]
        public async Task CreateBooking_SamePeriodOnceMoreThanThereAreRooms_OnlyTheLastIsRejected()
        {
            // Act
            var results = new List<bool>();
            for (var i = 0; i < 4; i++)
            {
                results.Add(await bookingManager.CreateBooking(
                    CreateBookingTestCases.Request(1, 2)));
            }

            // Assert: one assertion on the whole sequence, so it can't pass for the wrong reason
            Assert.Equal(new[] { true, true, true, false }, results);
        }
    }
}
