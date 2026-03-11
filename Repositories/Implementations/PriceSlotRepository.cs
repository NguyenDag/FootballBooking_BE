using FootballBooking_BE.Data;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Repositories.Implementations
{
    public class PriceSlotRepository : IPriceSlotRepository
    {
        private readonly AppDbContext _context;

        public PriceSlotRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PriceSlot>> GetByPitchIdAsync(int pitchId)
        {
            return await _context.PriceSlots
                .Where(s => s.PitchId == pitchId)
                .ToListAsync();
        }

        public async Task<PriceSlot?> GetByIdAsync(int id)
        {
            return await _context.PriceSlots.FindAsync(id);
        }

        public async Task CreateAsync(PriceSlot slot)
        {
            await _context.PriceSlots.AddAsync(slot);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PriceSlot slot)
        {
            _context.PriceSlots.Update(slot);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var slot = await _context.PriceSlots.FindAsync(id);
            if (slot != null)
            {
                _context.PriceSlots.Remove(slot);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteRangeByPitchIdAsync(int pitchId)
        {
            var slots = await _context.PriceSlots.Where(s => s.PitchId == pitchId).ToListAsync();
            if (slots.Any())
            {
                _context.PriceSlots.RemoveRange(slots);
                await _context.SaveChangesAsync();
            }
        }
    }
}
