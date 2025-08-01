using FluentValidation;

namespace StoreBoost.Application.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Validates the BookSlotCommand request.
    /// </summary>
    public sealed class BookSlotCommandValidator : AbstractValidator<BookSlotCommand>
    {
        public BookSlotCommandValidator()
        {
            RuleFor(x => x.SlotId)
                .NotEqual(Guid.Empty)
                .WithMessage("Slot ID cannot be empty.");
        }
    }
}
