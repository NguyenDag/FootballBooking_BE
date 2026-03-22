using System.Collections.Generic;
using System.Threading.Tasks;
using FootballBooking_BE.Data.Entities;

namespace FootballBooking_BE.Repositories.Interfaces
{
    public interface IPitchRepository
    {
        Task<IEnumerable<Pitch>> GetAllPitchesAsync();
        Task<IEnumerable<Pitch>> GetPitchesByStaffIdAsync(int staffId);
        Task<Pitch?> GetPitchByIdAsync(int id);
        Task<Pitch> CreatePitchAsync(Pitch pitch);
        Task UpdatePitchAsync(Pitch pitch);
        Task DeletePitchAsync(int id);
        Task<bool> PitchExistsAsync(int id);
    }
}
