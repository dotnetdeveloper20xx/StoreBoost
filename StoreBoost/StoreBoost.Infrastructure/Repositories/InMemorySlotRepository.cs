using StoreBoost.Application.Interfaces;
using StoreBoost.Domain.Entities;

namespace StoreBoost.Infrastructure.Repositories
{
    /// <summary>
    /// In-memory implementation of ISlotRepository for testing/demo purposes.
    /// </summary>
    public class InMemorySlotRepository : ISlotRepository
    {
        private readonly List<AppointmentSlot> _slots = new();

        /// <summary>
        /// Initializes with sample data: 10 slots from 9:00 AM, 30-min apart. Some pre-booked.
        /// </summary>
        public InMemorySlotRepository()
        {
            SeedInitialData();
        }

        /// <summary>
        /// Generates 10 slots from 9:00 AM, some of which are marked as booked.
        /// </summary>
        private void SeedInitialData()
        {
            var baseTime = DateTime.Today.AddHours(9);

            for (int i = 0; i < 10; i++)
            {
                var slot = new AppointmentSlot(Guid.NewGuid(), baseTime.AddMinutes(30 * i));

                // Book slot 2 and 5 as defaults
                if (i == 2 || i == 5)
                {
                    slot.Book();
                }

                _slots.Add(slot);
            }
        }

        /// <summary>
        /// Returns all slots (available and booked).
        /// </summary>
        public Task<IReadOnlyList<AppointmentSlot>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<AppointmentSlot>>(_slots);

        /// <summary>
        /// Returns a slot by its ID.
        /// </summary>
        public Task<AppointmentSlot?> GetByIdAsync(Guid id) =>
            Task.FromResult(_slots.FirstOrDefault(s => s.Id == id));

        /// <summary>
        /// Adds a new slot, throws if duplicate ID exists.
        /// </summary>
        public Task AddAsync(AppointmentSlot slot)
        {
            if (_slots.Any(s => s.Id == slot.Id))
                throw new InvalidOperationException($"Slot with ID {slot.Id} already exists.");

            _slots.Add(slot);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates an existing slot if found.
        /// </summary>
        public Task<bool> UpdateAsync(AppointmentSlot slot)
        {
            var index = _slots.FindIndex(s => s.Id == slot.Id);
            if (index == -1)
                return Task.FromResult(false);

            _slots[index] = slot;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Returns only the slots that are still available.
        /// </summary>
        public Task<IReadOnlyList<AppointmentSlot>> GetAvailableAsync()
        {
            var available = _slots.Where(s => !s.IsBooked).ToList();
            return Task.FromResult<IReadOnlyList<AppointmentSlot>>(available);
        }

        /// <summary>
        /// Resets and regenerates the in-memory slot list.
        /// Useful for test cases or re-initializing demo data.
        /// </summary>
        public void Reset()
        {
            _slots.Clear();
            SeedInitialData();
        }

        /// <summary>
        /// Manually mark a slot as booked by ID (demo/testing only).
        /// </summary>
        public bool BookSlotById(Guid id)
        {
            var slot = _slots.FirstOrDefault(s => s.Id == id);
            if (slot == null || slot.IsBooked)
                return false;

            slot.Book();
            return true;
        }
    }
}
