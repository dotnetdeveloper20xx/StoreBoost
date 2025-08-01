using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Queries.GetAvailable
{
    /// <summary>
    /// Handles retrieval of available (not fully booked) appointment slots.
    /// </summary>
    public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, ApiResponse<IReadOnlyList<SlotDto>>>
    {
        private readonly ISlotRepository _repository;
        private readonly ILogger<GetAvailableSlotsQueryHandler> _logger;

        public GetAvailableSlotsQueryHandler(
            ISlotRepository repository,
            ILogger<GetAvailableSlotsQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<IReadOnlyList<SlotDto>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching available slots...");

            try
            {
                var availableSlots = await _repository.GetAvailableAsync();

                var dtoList = availableSlots
                    .Select(slot => new SlotDto
                    {
                        Id = slot.Id,
                        StartTime = slot.StartTime,
                        MaxBookings = slot.MaxBookings,
                        CurrentBookings = slot.CurrentBookings,
                        IsBooked = slot.IsBooked
                    })
                    .ToList();

                _logger.LogInformation("Found {Count} available slots", dtoList.Count);

                return ApiResponse<IReadOnlyList<SlotDto>>.SuccessResult(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available slots.");
                return ApiResponse<IReadOnlyList<SlotDto>>.FailureResult("An unexpected error occurred while retrieving available slots.");
            }
        }
    }
}
