using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;

namespace HotelBooking.Specs.Support
{
    /// <summary>Simple list-backed repository so the scenarios run without a database.</summary>
    public class InMemoryRepository<T> : IRepository<T>
    {
        private readonly Func<T, int> getId;

        public InMemoryRepository(Func<T, int> getId)
        {
            this.getId = getId;
        }

        public List<T> Items { get; } = new List<T>();

        public Task<IEnumerable<T>> GetAllAsync() => Task.FromResult<IEnumerable<T>>(Items.ToList());

        public Task<T> GetAsync(int id) => Task.FromResult(Items.FirstOrDefault(i => getId(i) == id));

        public Task AddAsync(T entity)
        {
            Items.Add(entity);
            return Task.CompletedTask;
        }

        public Task EditAsync(T entity)
        {
            Items[Items.FindIndex(i => getId(i) == getId(entity))] = entity;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(int id)
        {
            Items.RemoveAll(i => getId(i) == id);
            return Task.CompletedTask;
        }
    }
}
