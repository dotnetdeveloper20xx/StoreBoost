using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Exceptions;
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
        private readonly ILogger<CancelSlotBookingCommandHandler> _logger;

        public CancelSlotBookingCommandHandler(
            ISlotRepository repository,
            INotificationService notifier,
            ILogger<CancelSlotBookingCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Attempts to cancel one booking from the specified slot and notifies the user.
        /// </summary>
        public async Task<ApiResponse<bool>> Handle(CancelSlotBookingCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancel booking requested for SlotId: {SlotId}", request.SlotId);

            var slot = await _repository.GetByIdAsync(request.SlotId);
            if (slot == null)
            {
                _logger.LogWarning("Slot with ID {SlotId} not found", request.SlotId);
                throw new SlotNotFoundException(request.SlotId);
            }

            if (slot.CurrentBookings <= 0)
            {
                _logger.LogWarning("Attempt to cancel booking on SlotId {SlotId} which has no bookings", slot.Id);
                throw new NoBookingsToCancelException(slot.Id);
            }

            var userId = Guid.NewGuid(); // Replace with real user context later

            slot.Cancel();

            var updated = await _repository.UpdateAsync(slot);
            if (!updated)
            {
                _logger.LogError("Failed to persist cancellation for SlotId {SlotId}", slot.Id);
                return ApiResponse<bool>.FailureResult("Failed to persist slot cancellation.");
            }

            _logger.LogInformation("Booking successfully cancelled for SlotId {SlotId}", slot.Id);
            await _notifier.SendAsync(userId, $"Your booking for {slot.StartTime:t} was cancelled.");

            return ApiResponse<bool>.SuccessResult(true, "Booking successfully cancelled.");
        }
    }
}
