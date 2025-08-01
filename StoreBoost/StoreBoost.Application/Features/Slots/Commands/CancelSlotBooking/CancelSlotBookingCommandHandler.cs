using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Commands.CancelSlotBooking
{
    /// <summary>
    /// Handles the cancellation of a booking on an appointment slot.
    /// </summary>
    public sealed class CancelSlotBookingCommandHandler
        : IRequestHandler<CancelSlotBookingCommand, ApiResponse<bool>>
    {
        private readonly ISlotRepository _repository;

        public CancelSlotBookingCommandHandler(ISlotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Attempts to cancel one booking from the specified slot.
        /// </summary>
        /// <param name="request">The command containing the Slot ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// An <see cref="ApiResponse{T}"/> indicating success or failure.
        /// </returns>
        public async Task<ApiResponse<bool>> Handle(CancelSlotBookingCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the slot from repository
            var slot = await _repository.GetByIdAsync(request.SlotId);
            if (slot == null)
                return ApiResponse<bool>.FailureResult("Slot not found.");

            // Check if there is anything to cancel
            if (slot.CurrentBookings <= 0)
                return ApiResponse<bool>.FailureResult("No bookings exist to cancel for this slot.");

            try
            {
                // Attempt cancellation using domain logic
                slot.Cancel();

                // Persist the updated state
                var updated = await _repository.UpdateAsync(slot);

                if (!updated)
                    return ApiResponse<bool>.FailureResult("Failed to persist slot cancellation.");

                return ApiResponse<bool>.SuccessResult(true, "Booking successfully cancelled.");
            }
            catch (Exception ex)
            {
                // Optional: log the exception here with ILogger
                return ApiResponse<bool>.FailureResult($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
