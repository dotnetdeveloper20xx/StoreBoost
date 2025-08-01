
namespace StoreBoost.Application.Interfaces
{
    public interface ISlotRepository
    {
        Task<IReadOnlyList<AppointmentSlot>> GetAllAsync();
        Task<AppointmentSlot?> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(AppointmentSlot slot);
        Task AddAsync(AppointmentSlot slot);
        Task<IReadOnlyList<AppointmentSlot>> GetAvailableAsync();
        Task<bool> IsOverlappingSlotExistsAsync(DateTime startTime);

    }

}
