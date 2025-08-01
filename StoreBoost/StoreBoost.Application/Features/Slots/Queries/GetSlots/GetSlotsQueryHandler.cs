using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;

namespace StoreBoost.Application.Features.Slots.Queries.GetSlots
{
    /// <summary>
    /// Handles the retrieval of all appointment slots, including their booking limits and current status.
    /// </summary>
    public sealed class GetSlotsQueryHandler : IRequestHandler<GetSlotsQuery, ApiResponse<IReadOnlyList<SlotDto>>>
    {
        private readonly ISlotRepository _repository;

        public GetSlotsQueryHandler(ISlotRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ApiResponse<IReadOnlyList<SlotDto>>> Handle(GetSlotsQuery request, CancellationToken cancellationToken)
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

            return ApiResponse<IReadOnlyList<SlotDto>>.SuccessResult(dtoList);
        }
    }
}
