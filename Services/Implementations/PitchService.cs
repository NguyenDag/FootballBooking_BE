using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Repositories.Interfaces;
using FootballBooking_BE.Services.Interfaces;

namespace FootballBooking_BE.Services.Implementations
{
    public class PitchService : IPitchService
    {
        private readonly IPitchRepository _pitchRepo;

        public PitchService(IPitchRepository pitchRepo)
        {
            _pitchRepo = pitchRepo;
        }

        public async Task<IEnumerable<PitchDTO>> GetAllPitchesAsync()
        {
            var pitches = await _pitchRepo.GetAllPitchesAsync();
            return pitches.Select(MapToDTO);
        }

        public async Task<PitchDTO?> GetPitchByIdAsync(int id)
        {
            var pitch = await _pitchRepo.GetPitchByIdAsync(id);
            return pitch != null ? MapToDTO(pitch) : null;
        }

        public async Task<PitchDTO> CreatePitchAsync(CreatePitchRequest request)
        {
            var pitch = new Pitch
            {
                PitchName = request.PitchName,
                PitchType = request.PitchType,
                Status = request.Status,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            var createdPitch = await _pitchRepo.CreatePitchAsync(pitch);

            return MapToDTO(createdPitch);
        }

        public async Task<bool> UpdatePitchAsync(int id, UpdatePitchRequest request)
        {
            var pitch = await _pitchRepo.GetPitchByIdAsync(id);
            if (pitch == null) return false;

            if (!string.IsNullOrEmpty(request.PitchName)) pitch.PitchName = request.PitchName;
            if (!string.IsNullOrEmpty(request.PitchType)) pitch.PitchType = request.PitchType;
            if (!string.IsNullOrEmpty(request.Status)) pitch.Status = request.Status;
            if (request.Description != null) pitch.Description = request.Description;

            await _pitchRepo.UpdatePitchAsync(pitch);
            return true;
        }

        public async Task<bool> DeletePitchAsync(int id)
        {
            if (!await _pitchRepo.PitchExistsAsync(id)) return false;
            await _pitchRepo.DeletePitchAsync(id);
            return true;
        }


        private PitchDTO MapToDTO(Pitch pitch)
        {
            return new PitchDTO
            {
                PitchId = pitch.PitchId,
                PitchName = pitch.PitchName,
                PitchType = pitch.PitchType,
                Status = pitch.Status ?? "ACTIVE",
                Description = pitch.Description,
                CreatedAt = pitch.CreatedAt,
                PriceSlots = pitch.PriceSlots.Select(s => new PriceSlotDTO
                {
                    PriceSlotId = s.PriceSlotId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    PricePerHour = s.PricePerHour,
                    ApplyOn = s.ApplyOn,
                    IsPeakHour = s.IsPeakHour
                }).ToList()
            };
        }
    }
}
