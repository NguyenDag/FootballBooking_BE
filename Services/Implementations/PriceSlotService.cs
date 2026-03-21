using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Repositories.Interfaces;
using FootballBooking_BE.Services.Interfaces;

namespace FootballBooking_BE.Services.Implementations
{
    public class PriceSlotService : IPriceSlotService
    {
        private readonly IPriceSlotRepository _slotRepo;
        private readonly IPitchRepository _pitchRepo;

        public PriceSlotService(IPriceSlotRepository slotRepo, IPitchRepository pitchRepo)
        {
            _slotRepo = slotRepo;
            _pitchRepo = pitchRepo;
        }

        public async Task<IEnumerable<PriceSlotDTO>> GetSlotsByPitchIdAsync(int pitchId)
        {
            var slots = await _slotRepo.GetByPitchIdAsync(pitchId);
            return slots.Select(MapToDTO);
        }

        public async Task<PriceSlotDTO?> GetSlotByIdAsync(int id)
        {
            var slot = await _slotRepo.GetByIdAsync(id);
            return slot != null ? MapToDTO(slot) : null;
        }

        public async Task<PriceSlotDTO> CreateSlotAsync(int pitchId, PriceSlotRequest request)
        {
            var pitch = await _pitchRepo.GetPitchByIdAsync(pitchId);
            if (pitch == null) throw new KeyNotFoundException("Không tìm thấy sân bóng");

            // Kiểm tra trùng lặp khung giờ
            var existingSlots = await _slotRepo.GetByPitchIdAsync(pitchId);
            var isConflict = existingSlots.Any(s =>
                ((s.ApplyOn == "ALL" || request.ApplyOn == "ALL") || (s.ApplyOn == request.ApplyOn)) &&
                (request.StartTime < s.EndTime && request.EndTime > s.StartTime)
            );

            if (isConflict)
            {
                var conflictSlot = existingSlots.First(s =>
                    ((s.ApplyOn == "ALL" || request.ApplyOn == "ALL") || (s.ApplyOn == request.ApplyOn)) &&
                    (request.StartTime < s.EndTime && request.EndTime > s.StartTime)
                );
                throw new InvalidOperationException($"Khung giờ mới bị trùng với khung giờ {conflictSlot.StartTime:hh\\:mm}-{conflictSlot.EndTime:hh\\:mm} ({conflictSlot.ApplyOn}) đã tồn tại.");
            }

            var slot = new PriceSlot
            {
                PitchId = pitchId,
                PitchType = pitch.PitchType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PricePerHour = request.PricePerHour,
                ApplyOn = request.ApplyOn,
                IsPeakHour = request.IsPeakHour
            };

            await _slotRepo.CreateAsync(slot);
            return MapToDTO(slot);
        }

        public async Task<bool> UpdateSlotAsync(int id, PriceSlotRequest request)
        {
            var slot = await _slotRepo.GetByIdAsync(id);
            if (slot == null) return false;

            // Kiểm tra trùng lặp (trừ chính nó)
            var existingSlots = await _slotRepo.GetByPitchIdAsync(slot.PitchId);
            var isConflict = existingSlots.Any(s =>
                s.PriceSlotId != id &&
                ((s.ApplyOn == "ALL" || request.ApplyOn == "ALL") || (s.ApplyOn == request.ApplyOn)) &&
                (request.StartTime < s.EndTime && request.EndTime > s.StartTime)
            );

            if (isConflict)
            {
                 var conflictSlot = existingSlots.First(s =>
                    s.PriceSlotId != id &&
                    ((s.ApplyOn == "ALL" || request.ApplyOn == "ALL") || (s.ApplyOn == request.ApplyOn)) &&
                    (request.StartTime < s.EndTime && request.EndTime > s.StartTime)
                );
                throw new InvalidOperationException($"Khung giờ cập nhật bị trùng với khung giờ {conflictSlot.StartTime:hh\\:mm}-{conflictSlot.EndTime:hh\\:mm} ({conflictSlot.ApplyOn}) đã tồn tại.");
            }

            slot.StartTime = request.StartTime;
            slot.EndTime = request.EndTime;
            slot.PricePerHour = request.PricePerHour;
            slot.ApplyOn = request.ApplyOn;
            slot.IsPeakHour = request.IsPeakHour;

            await _slotRepo.UpdateAsync(slot);
            return true;
        }

        public async Task<bool> DeleteSlotAsync(int id)
        {
            var slot = await _slotRepo.GetByIdAsync(id);
            if (slot == null) return false;

            await _slotRepo.DeleteAsync(id);
            return true;
        }

        private PriceSlotDTO MapToDTO(PriceSlot s) => new()
        {
            PriceSlotId = s.PriceSlotId,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            PricePerHour = s.PricePerHour,
            ApplyOn = s.ApplyOn,
            IsPeakHour = s.IsPeakHour
        };
    }
}
