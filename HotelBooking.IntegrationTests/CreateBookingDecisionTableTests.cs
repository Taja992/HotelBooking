using System;
using System.Linq;
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
    /// <summary>
    /// Decision table TC1–TC10 against real repositories and a Sqlite
    /// in-memory database. DbInitializer books all three rooms from
    /// Today+4 to Today+18, so that is O here.
    /// </summary>
    public class CreateBookingDecisionTableTests : IDisposable
    {
        private const int SeedOccupiedStart = 4;
        private const int SeedOccupiedEnd = 18;

        private readonly SqliteConnection connection;
        private readonly BookingRepository bookingRepository;
        private readonly BookingManager bookingManager;

        public CreateBookingDecisionTableTests()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<HotelBookingContext>()
                            .UseSqlite(connection).Options;
            var dbContext = new HotelBookingContext(options);
            new DbInitializer().Initialize(dbContext);

            bookingRepository = new BookingRepository(dbContext);
            bookingManager = new BookingManager(bookingRepository, new RoomRepository(dbContext));
        }

        public void Dispose()
        {
            connection.Close();
        }

        [Theory]
        [MemberData(nameof(CreateBookingTestCases.DecisionTable),
            SeedOccupiedStart, SeedOccupiedEnd,
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_DecisionTableRule_ReturnsExpectedResult(
            int startOffset, int endOffset, bool expectedBooked)
        {
            // Act
            var booked = await bookingManager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset));

            // Assert
            Assert.Equal(expectedBooked, booked);
        }

        [Theory]
        [MemberData(nameof(CreateBookingTestCases.DecisionTable),
            SeedOccupiedStart, SeedOccupiedEnd,
            MemberType = typeof(CreateBookingTestCases))]
        public async Task CreateBooking_DecisionTableRule_PersistsOnlyWhenBooked(
            int startOffset, int endOffset, bool expectedBooked)
        {
            // Arrange
            var countBefore = (await bookingRepository.GetAllAsync()).Count();

            // Act
            await bookingManager.CreateBooking(
                CreateBookingTestCases.Request(startOffset, endOffset));

            // Assert
            var countAfter = (await bookingRepository.GetAllAsync()).Count();
            Assert.Equal(countBefore + (expectedBooked ? 1 : 0), countAfter);
        }
    }
}
