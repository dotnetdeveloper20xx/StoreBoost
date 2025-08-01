using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Application.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Handles the BookSlotCommand to book an available appointment slot.
    /// </summary>
    public sealed class BookSlotCommandHandler : IRequestHandler<BookSlotCommand, ApiResponse<bool>>
    {
        private readonly ISlotRepository _repository;

        public BookSlotCommandHandler(ISlotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
                slot.Book(); // Enforces domain rule internally

                var updated = await _repository.UpdateAsync(slot);
                if (!updated)
                    return ApiResponse<bool>.FailureResult("Failed to persist booking update.");

                return ApiResponse<bool>.SuccessResult(true, "Slot booked successfully.");
            }
            catch (Exception ex)
            {
                // Log the exception in a real system
                return ApiResponse<bool>.FailureResult($"Unexpected error: {ex.Message}");
            }
        }
    }
}
