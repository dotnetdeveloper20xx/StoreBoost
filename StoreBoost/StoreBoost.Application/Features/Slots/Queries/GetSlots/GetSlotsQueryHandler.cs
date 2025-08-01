using MediatR;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Queries.GetSlots
{
    /// <summary>
    /// Handles the retrieval of all appointment slots, including their booking limits and current status.
    /// </summary>
    public sealed class GetSlotsQueryHandler : IRequestHandler<GetSlotsQuery, ApiResponse<IReadOnlyList<SlotDto>>>
    {
        private readonly ISlotRepository _repository;
        private readonly ILogger<GetSlotsQueryHandler> _logger;

        public GetSlotsQueryHandler(ISlotRepository repository, ILogger<GetSlotsQueryHandler> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<IReadOnlyList<SlotDto>>> Handle(GetSlotsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving all slots...");

            try
            {
                var slots = await _repository.GetAllAsync();

                var dtoList = slots
                    .Select(slot => new SlotDto
                    {
                        Id = slot.Id,
                        StartTime = slot.StartTime,
                        MaxBookings = slot.MaxBookings,
                        CurrentBookings = slot.CurrentBookings,
                        IsBooked = slot.IsBooked
                    })
                    .ToList();

                _logger.LogInformation("Successfully retrieved {Count} slots.", dtoList.Count);

                return ApiResponse<IReadOnlyList<SlotDto>>.SuccessResult(dtoList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all slots.");
                return ApiResponse<IReadOnlyList<SlotDto>>.FailureResult("An unexpected error occurred while retrieving slots.");
            }
        }
    }
}
