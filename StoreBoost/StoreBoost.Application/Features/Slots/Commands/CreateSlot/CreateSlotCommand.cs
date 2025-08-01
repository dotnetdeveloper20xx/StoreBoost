using MediatR;
using StoreBoost.Application.Common.Models;

namespace StoreBoost.Application.Features.Slots.Commands.CreateSlot
{
    public sealed record CreateSlotCommand(DateTime StartTime, int MaxBookings = 1)
        : IRequest<ApiResponse<Guid>>;
}
