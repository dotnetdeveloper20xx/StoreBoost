using FluentValidation;

namespace StoreBoost.Application.Features.Slots.Commands.CreateSlot
{
    public sealed class CreateSlotCommandValidator : AbstractValidator<CreateSlotCommand>
    {
        public CreateSlotCommandValidator()
        {
            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Start time must be in the future.");

            RuleFor(x => x.MaxBookings)
                .GreaterThan(0)
                .WithMessage("Max bookings must be at least 1.");
        }
    }
}
