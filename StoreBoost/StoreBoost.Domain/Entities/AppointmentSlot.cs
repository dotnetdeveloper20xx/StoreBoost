
namespace StoreBoost.Domain.Entities
{
    public class AppointmentSlot
    {
        public Guid Id { get; }
        public DateTime StartTime { get; }
        public bool IsBooked { get; private set; }

        public AppointmentSlot(Guid id, DateTime startTime)
        {
            Id = id;
            StartTime = startTime;
            IsBooked = false;
        }

        public void Book()
        {
            if (IsBooked)
                throw new InvalidOperationException("Slot is already booked.");
            IsBooked = true;
        }
    }

}
