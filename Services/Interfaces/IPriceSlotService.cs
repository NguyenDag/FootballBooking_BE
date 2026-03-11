using FootballBooking_BE.Models.DTOs;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IPriceSlotService
    {
        Task<IEnumerable<PriceSlotDTO>> GetSlotsByPitchIdAsync(int pitchId);
        Task<PriceSlotDTO?> GetSlotByIdAsync(int id);
        Task<PriceSlotDTO> CreateSlotAsync(int pitchId, PriceSlotRequest request);
        Task<bool> UpdateSlotAsync(int id, PriceSlotRequest request);
        Task<bool> DeleteSlotAsync(int id);
    }
}
