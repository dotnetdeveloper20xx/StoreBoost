using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Commands.CancelSlotBooking
{
    /// <summary>
    /// Handles the cancellation of a booking on an appointment slot.
    /// Sends a notification to the user upon successful cancellation.
    /// </summary>
    public sealed class CancelSlotBookingCommandHandler
        : IRequestHandler<CancelSlotBookingCommand, ApiResponse<bool>>
    {
        private readonly ISlotRepository _repository;
        private readonly INotificationService _notifier;

        public CancelSlotBookingCommandHandler(ISlotRepository repository, INotificationService notifier)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        /// <summary>
        /// Attempts to cancel one booking from the specified slot and notifies the user.
        /// </summary>
        public async Task<ApiResponse<bool>> Handle(CancelSlotBookingCommand request, CancellationToken cancellationToken)
        {
            var slot = await _repository.GetByIdAsync(request.SlotId);
            if (slot == null)
                return ApiResponse<bool>.FailureResult("Slot not found.");

            if (slot.CurrentBookings <= 0)
                return ApiResponse<bool>.FailureResult("No bookings exist to cancel for this slot.");

            try
            {
                var userId = Guid.NewGuid(); // Replace with real user ID in production

                slot.Cancel();

                var updated = await _repository.UpdateAsync(slot);
                if (!updated)
                    return ApiResponse<bool>.FailureResult("Failed to persist slot cancellation.");

                await _notifier.SendAsync(userId, $"Your booking for {slot.StartTime:t} was cancelled.");

                return ApiResponse<bool>.SuccessResult(true, "Booking successfully cancelled.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
