using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;

namespace StoreBoost.Application.Features.Slots.Queries.GetAvailable
{
    /// <summary>
    /// Query to retrieve only available (unbooked) appointment slots.
    /// </summary>
    public sealed class GetAvailableSlotsQuery : IRequest<ApiResponse<IReadOnlyList<SlotDto>>>;
}
