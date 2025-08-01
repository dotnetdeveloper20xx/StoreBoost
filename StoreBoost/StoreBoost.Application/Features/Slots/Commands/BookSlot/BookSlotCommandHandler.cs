using MediatR;
using StoreBoost.Application.Common.Models;
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

        public BookSlotCommandHandler(ISlotRepository repository, INotificationService notifier)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        }

        public async Task<ApiResponse<bool>> Handle(BookSlotCommand request, CancellationToken cancellationToken)
        {
            var slot = await _repository.GetByIdAsync(request.SlotId);
            if (slot is null)
                return ApiResponse<bool>.FailureResult("Slot not found.");

            if (slot.IsBooked)
                return ApiResponse<bool>.FailureResult("Slot is already booked.");

            try
            {
                // Simulate current user ID (replace with real user context later)
                var userId = Guid.NewGuid();

                // Book the slot
                slot.Book();

                var updated = await _repository.UpdateAsync(slot);
                if (!updated)
                    return ApiResponse<bool>.FailureResult("Failed to persist booking update.");

                // Send user notification
                await _notifier.SendAsync(userId, $"Your booking for {slot.StartTime:t} is confirmed.");

                // Notify if this was the last available spot
                if (slot.IsBooked)
                {
                    await _notifier.SendAsync(userId, $"❗ Slot at {slot.StartTime:t} is now fully booked.");
                }

                return ApiResponse<bool>.SuccessResult(true, "Slot booked successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResult($"Unexpected error: {ex.Message}");
            }
        }
    }
}
