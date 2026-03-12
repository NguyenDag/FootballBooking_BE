using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Repositories.Interfaces
{
    public interface IPriceSlotRepository
    {
        Task<IEnumerable<PriceSlot>> GetByPitchIdAsync(int pitchId);
        Task<PriceSlot?> GetByIdAsync(int id);
        Task CreateAsync(PriceSlot slot);
        Task UpdateAsync(PriceSlot slot);
        Task DeleteAsync(int id);
        Task DeleteRangeByPitchIdAsync(int pitchId);
    }
}
