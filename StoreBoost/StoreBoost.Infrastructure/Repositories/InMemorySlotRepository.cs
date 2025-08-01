using StoreBoost.Application.Interfaces;

namespace StoreBoost.Infrastructure.Repositories
{
    /// <summary>
    /// In-memory implementation of ISlotRepository for testing/demo purposes.
    /// Supports multiple bookings per slot.
    /// </summary>
    public class InMemorySlotRepository : ISlotRepository
    {
        private readonly List<AppointmentSlot> _slots = new();

        /// <summary>
        /// Initializes with seeded slot data.
        /// </summary>
        public InMemorySlotRepository()
        {
            SeedInitialData();
        }

        /// <summary>
        /// Generates 30 demo slots starting from 9:00 AM at 30-minute intervals.
        /// Each slot has a randomized max bookings limit (1–5),
        /// and some are partially pre-booked to simulate real usage.
        /// </summary>
        private void SeedInitialData()
        {
            var baseTime = DateTime.Today.AddHours(9);
            var random = new Random();

            for (int i = 0; i < 30; i++)
            {
                var id = Guid.NewGuid();
                var startTime = baseTime.AddMinutes(i * 30);
                var maxBookings = random.Next(1, 6); // Between 1 and 5

                var slot = new AppointmentSlot(id, startTime, maxBookings);

                // Pre-book 0–3 random bookings to simulate usage
                int preBooked = random.Next(0, Math.Min(4, maxBookings + 1));
                for (int j = 0; j < preBooked; j++)
                {
                    try { slot.Book(); } catch { /* Ignore if max reached */ }
                }

                _slots.Add(slot);
            }
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<AppointmentSlot>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<AppointmentSlot>>(_slots);

        /// <inheritdoc />
        public Task<AppointmentSlot?> GetByIdAsync(Guid id) =>
            Task.FromResult(_slots.FirstOrDefault(s => s.Id == id));

        /// <inheritdoc />
        public Task AddAsync(AppointmentSlot slot)
        {
            if (_slots.Any(s => s.Id == slot.Id))
                throw new InvalidOperationException($"Slot with ID {slot.Id} already exists.");

            _slots.Add(slot);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> UpdateAsync(AppointmentSlot slot)
        {
            var index = _slots.FindIndex(s => s.Id == slot.Id);
            if (index == -1)
                return Task.FromResult(false);

            _slots[index] = slot;
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<AppointmentSlot>> GetAvailableAsync()
        {
            var available = _slots.Where(s => !s.IsBooked).ToList();
            return Task.FromResult<IReadOnlyList<AppointmentSlot>>(available);
        }

        /// <summary>
        /// Clears and re-seeds the in-memory slot list.
        /// Useful for testing.
        /// </summary>
        public void Reset()
        {
            _slots.Clear();
            SeedInitialData();
        }

        /// <summary>
        /// Manually books a slot by ID (demo/test only).
        /// </summary>
        public bool BookSlotById(Guid id)
        {
            var slot = _slots.FirstOrDefault(s => s.Id == id);
            if (slot == null || slot.IsBooked)
                return false;

            slot.Book();
            return true;
        }

        /// <summary>
        /// Checks if there is already a slot that overlaps with the proposed start time.
        /// Assumes all slots are 30 minutes long.
        /// </summary>
        /// <param name="startTime">The proposed start time for the new slot.</param>
        /// <returns>True if a conflict exists; otherwise, false.</returns>
        public Task<bool> IsOverlappingSlotExistsAsync(DateTime startTime)
        {
            const int slotDurationMinutes = 30;

            // Calculate proposed slot's end time
            var proposedEndTime = startTime.AddMinutes(slotDurationMinutes);

            // Check for overlap with existing slots
            var conflict = _slots.Any(existing =>
            {
                var existingEnd = existing.StartTime.AddMinutes(slotDurationMinutes);

                // Overlap occurs if start < existingEnd AND existingStart < proposedEnd
                return startTime < existingEnd && existing.StartTime < proposedEndTime;
            });

            return Task.FromResult(conflict);
        }


    }
}
