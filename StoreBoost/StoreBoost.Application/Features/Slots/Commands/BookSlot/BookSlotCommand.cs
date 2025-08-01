using MediatR;
using StoreBoost.Application.Common.Models;

namespace StoreBoost.Application.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Command representing a user's intent to book a specific appointment slot.
    /// </summary>
    public sealed record BookSlotCommand(Guid SlotId) : IRequest<ApiResponse<bool>>;
}
