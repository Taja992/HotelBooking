using System;
using System.Linq;
using HotelBooking.Core;
using HotelBooking.Infrastructure;
using HotelBooking.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Specs.Support
{
    /// <summary>
    /// One empty hotel per scenario: a Sqlite in-memory database with real
    /// repositories and one customer, but no rooms and no bookings. The Given
    /// steps build the rest. Reqnroll creates one instance per scenario,
    /// shares it with every step class that asks for it, and disposes it when
    /// the scenario ends.
    /// </summary>
    public sealed class HotelFixture : IDisposable
    {
        private readonly SqliteConnection connection;

        public HotelFixture()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<HotelBookingContext>()
                            .UseSqlite(connection).Options;
            Db = new HotelBookingContext(options);
            Db.Database.EnsureCreated();

            // Every booking needs a customer (foreign key), so add one up front
            var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
            Db.Customer.Add(customer);
            Db.SaveChanges();
            CustomerId = customer.Id;

            Manager = new BookingManager(new BookingRepository(Db), new RoomRepository(Db));
        }

        public HotelBookingContext Db { get; }
        public BookingManager Manager { get; }
        public int CustomerId { get; }

        public int BookingCount => Db.Booking.Count();

        /// <summary>"day 10" in a feature file means Today + 10 days.</summary>
        public static DateTime Day(int offset) => DateTime.Today.AddDays(offset);

        public void Dispose()
        {
            Db.Dispose();
            connection.Close();
        }
    }
}
