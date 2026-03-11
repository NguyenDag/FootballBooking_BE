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
                // Explicitly remove price slots first
                var slots = _context.PriceSlots.Where(s => s.PitchId == id);
                _context.PriceSlots.RemoveRange(slots);
                
                _context.Pitches.Remove(pitch);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PitchExistsAsync(int id)
        {
            return await _context.Pitches.AnyAsync(p => p.PitchId == id);
        }
    }
}
