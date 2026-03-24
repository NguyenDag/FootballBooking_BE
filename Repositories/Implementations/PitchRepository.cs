using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Repositories.Implementations
{
    public class PitchRepository : IPitchRepository
    {
        private readonly AppDbContext _context;

        public PitchRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Pitch>> GetAllPitchesAsync()
        {
            return await _context.Pitches
                .Include(p => p.PriceSlots)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Pitch>> GetPitchesByStaffIdAsync(int staffId)
        {
            return await _context.Pitches
                .Include(p => p.PriceSlots)
                .Where(p => p.StaffPitchAssignments.Any(a => a.StaffId == staffId))
                .ToListAsync();
        }

        public async Task<Pitch?> GetPitchByIdAsync(int id)
        {
            return await _context.Pitches
                .Include(p => p.PriceSlots)
                .FirstOrDefaultAsync(p => p.PitchId == id);
        }

        public async Task<Pitch> CreatePitchAsync(Pitch pitch)
        {
            _context.Pitches.Add(pitch);
            await _context.SaveChangesAsync();
            return pitch;
        }

        public async Task UpdatePitchAsync(Pitch pitch)
        {
            _context.Pitches.Update(pitch);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePitchAsync(int id)
        {
            var pitch = await _context.Pitches.FindAsync(id);
            if (pitch != null)
            {
                pitch.Status = "INACTIVE";

                var assignments = _context.StaffPitchAssignments.Where(a => a.PitchId == id);
                _context.StaffPitchAssignments.RemoveRange(assignments);

                var shifts = _context.StaffShifts.Where(s => s.PitchId == id);
                _context.StaffShifts.RemoveRange(shifts);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PitchExistsAsync(int id)
        {
            return await _context.Pitches.AnyAsync(p => p.PitchId == id);
        }
    }
}
