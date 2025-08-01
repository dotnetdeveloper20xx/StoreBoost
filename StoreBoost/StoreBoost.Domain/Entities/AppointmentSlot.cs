/// <summary>
/// Represents a bookable time slot that can allow multiple bookings up to a defined limit.
/// </summary>
public class AppointmentSlot
{
    /// <summary>
    /// Unique identifier for the slot.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// The scheduled start time of the slot.
    /// </summary>
    public DateTime StartTime { get; }

    /// <summary>
    /// The maximum number of bookings allowed for this slot.
    /// </summary>
    public int MaxBookings { get; }

    /// <summary>
    /// The current number of successful bookings for this slot.
    /// </summary>
    public int CurrentBookings { get; private set; }

    /// <summary>
    /// Indicates whether the slot is fully booked.
    /// </summary>
    public bool IsBooked => CurrentBookings >= MaxBookings;

    /// <summary>
    /// Constructor to initialize a new appointment slot.
    /// </summary>
    /// <param name="id">The unique slot ID.</param>
    /// <param name="startTime">The start time of the slot.</param>
    /// <param name="maxBookings">Maximum number of allowed bookings (default is 1).</param>
    /// <exception cref="ArgumentException">Thrown if maxBookings is less than 1.</exception>
    public AppointmentSlot(Guid id, DateTime startTime, int maxBookings = 1)
    {
        if (maxBookings <= 0)
            throw new ArgumentException("MaxBookings must be at least 1.");

        Id = id;
        StartTime = startTime;
        MaxBookings = maxBookings;
        CurrentBookings = 0;
    }

    /// <summary>
    /// Books the slot for one user. Throws if the slot is already fully booked.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the slot is fully booked.</exception>
    public void Book()
    {
        if (IsBooked)
            throw new InvalidOperationException("Slot is fully booked.");

        CurrentBookings++;
    }
}
