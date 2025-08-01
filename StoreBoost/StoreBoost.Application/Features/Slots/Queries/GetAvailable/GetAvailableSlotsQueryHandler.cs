using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Application.Features.Slots.Queries.GetAvailable
{
    /// <summary>
    /// Handles retrieval of available (unbooked) appointment slots.
    /// </summary>
    public sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, ApiResponse<IReadOnlyList<SlotDto>>>
    {
        private readonly ISlotRepository _repository;

        public GetAvailableSlotsQueryHandler(ISlotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<IReadOnlyList<SlotDto>>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            var availableSlots = await _repository.GetAvailableAsync();

            var dtoList = availableSlots
                .Select(slot => new SlotDto
                {
                    Id = slot.Id,
                    StartTime = slot.StartTime,
                    IsBooked = slot.IsBooked
                })
                .ToList();

            return ApiResponse<IReadOnlyList<SlotDto>>.SuccessResult(dtoList);
        }
    }
}
