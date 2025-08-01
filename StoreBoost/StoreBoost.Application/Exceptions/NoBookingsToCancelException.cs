namespace StoreBoost.Application.Exceptions;

/// <summary>
/// Thrown when an attempt is made to cancel a booking on a slot that has no active bookings.
/// </summary>
public class NoBookingsToCancelException : Exception
{
    public NoBookingsToCancelException(Guid slotId)
        : base($"Slot with ID '{slotId}' has no bookings to cancel.") { }
}
