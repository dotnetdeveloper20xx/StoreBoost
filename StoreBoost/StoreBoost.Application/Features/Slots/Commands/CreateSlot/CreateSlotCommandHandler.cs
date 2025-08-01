using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CreateSlotCommandHandler> _logger;

        public CreateSlotCommandHandler(
            ISlotRepository repository,
            ILogger<CreateSlotCommandHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<Guid>> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateSlotCommand received with StartTime: {StartTime} and MaxBookings: {MaxBookings}",
                request.StartTime, request.MaxBookings);

            try
            {
                // Check for overlapping slot within 30-minute window
                var hasConflict = await _repository.IsOverlappingSlotExistsAsync(request.StartTime);
                if (hasConflict)
                {
                    _logger.LogWarning("Conflict detected: slot already exists around {StartTime}", request.StartTime);
                    return ApiResponse<Guid>.FailureResult("A slot already exists in this time window.");
                }

                // Create new appointment slot
                var newSlot = new AppointmentSlot(Guid.NewGuid(), request.StartTime, request.MaxBookings);
                await _repository.AddAsync(newSlot);

                _logger.LogInformation("Slot created successfully with ID: {SlotId}", newSlot.Id);
                return ApiResponse<Guid>.SuccessResult(newSlot.Id, "Slot created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a slot at {StartTime}", request.StartTime);
                return ApiResponse<Guid>.FailureResult("An unexpected error occurred while creating the slot.");
            }
        }
    }
}
