using MediatR;
using StoreBoost.Application.Common.Models;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Application.Features.Slots.Commands.CreateSlot
{
    public sealed class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, ApiResponse<Guid>>
    {
        private readonly ISlotRepository _repository;

        public CreateSlotCommandHandler(ISlotRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<Guid>> Handle(CreateSlotCommand request, CancellationToken cancellationToken)
        {
            var newSlot = new AppointmentSlot(Guid.NewGuid(), request.StartTime, request.MaxBookings);

            await _repository.AddAsync(newSlot);

            return ApiResponse<Guid>.SuccessResult(newSlot.Id, "Slot created successfully.");
        }
    }
}
