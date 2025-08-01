using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Exceptions;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Handles the BookSlotCommand to book an available appointment slot.
    /// Sends notifications on success and when slot is fully booked.
    /// </summary>
    public sealed class BookSlotCommandHandler : IRequestHandler<BookSlotCommand, ApiResponse<bool>>
    {
        private readonly ISlotRepository _repository;
        private readonly INotificationService _notifier;
        private readonly ILogger<BookSlotCommandHandler> _logger;

        public BookSlotCommandHandler(
            ISlotRepository repository,
            INotificationService notifier,
            ILogger<BookSlotCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<bool>> Handle(BookSlotCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Booking request received for SlotId: {SlotId}", request.SlotId);

            var slot = await _repository.GetByIdAsync(request.SlotId);
            if (slot is null)
            {
                _logger.LogWarning("Slot with ID {SlotId} not found", request.SlotId);
                throw new SlotNotFoundException(request.SlotId);
            }

            if (slot.IsBooked)
            {
                _logger.LogWarning("Slot with ID {SlotId} is already fully booked", slot.Id);
                throw new SlotAlreadyBookedException(slot.Id);
            }

            // Simulate current user ID (replace with real user context later)
            var userId = Guid.NewGuid();

            // Book the slot
            slot.Book();

            var updated = await _repository.UpdateAsync(slot);
            if (!updated)
            {
                _logger.LogError("Failed to persist booking for SlotId {SlotId}", slot.Id);
                return ApiResponse<bool>.FailureResult("Failed to persist booking update.");
            }

            _logger.LogInformation("Slot {SlotId} successfully booked", slot.Id);
            await _notifier.SendAsync(userId, $"Your booking for {slot.StartTime:t} is confirmed.");

            if (slot.IsBooked)
            {
                _logger.LogInformation("Slot {SlotId} is now fully booked", slot.Id);
                await _notifier.SendAsync(userId, $"Slot at {slot.StartTime:t} is now fully booked.");
            }

            return ApiResponse<bool>.SuccessResult(true, "Slot booked successfully.");
        }
    }
}
