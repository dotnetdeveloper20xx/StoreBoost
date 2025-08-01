using MediatR;
using StoreBoost.Application.Common.Models;

namespace StoreBoost.Application.Features.Slots.Commands.CancelSlotBooking
{
    public sealed record CancelSlotBookingCommand(Guid SlotId) : IRequest<ApiResponse<bool>>;
}
