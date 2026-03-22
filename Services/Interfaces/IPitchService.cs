using FootballBooking_BE.Models.DTOs;

namespace FootballBooking_BE.Services.Interfaces
{
    public interface IPitchService
    {
        Task<IEnumerable<PitchDTO>> GetAllPitchesAsync();
        Task<IEnumerable<PitchDTO>> GetPitchesByStaffIdAsync(int staffId);
        Task<PitchDTO?> GetPitchByIdAsync(int id);
        Task<PitchDTO> CreatePitchAsync(CreatePitchRequest request);
        Task<bool> UpdatePitchAsync(int id, UpdatePitchRequest request);
        Task<bool> DeletePitchAsync(int id);
    }
}
