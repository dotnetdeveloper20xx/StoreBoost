using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Application.Features.Slots.Commands.CreateSlot
{
    /// <summary>
    /// Handles creation of a new appointment slot, ensuring no overlap with existing slots.
    /// </summary>
    public sealed class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, ApiResponse<Guid>>
    {
        private readonly ISlotRepository _repository;

        public CreateSlotCommandHandler(ISlotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<Guid>> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            // Check for overlapping slot within 30-minute window
            var hasConflict = await _repository.IsOverlappingSlotExistsAsync(request.StartTime);
            if (hasConflict)
                return ApiResponse<Guid>.FailureResult("A slot already exists in this time window.");

            // Create new appointment slot
            var newSlot = new AppointmentSlot(Guid.NewGuid(), request.StartTime, request.MaxBookings);

            // Save to repository
            await _repository.AddAsync(newSlot);

            return ApiResponse<Guid>.SuccessResult(newSlot.Id, "Slot created successfully.");
        }
    }
}
