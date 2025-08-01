using MediatR;
using StoreBoost.Application.Common.Models;

namespace StoreBoost.Application.Features.Slots.Queries.GetSlots
{
    /// <summary>
    /// Query to retrieve all appointment slots (DTO-based).
    /// </summary>
    public sealed class GetSlotsQuery : IRequest<ApiResponse<IReadOnlyList<SlotDto>>>;
}
