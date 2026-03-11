using FootballBooking_BE.Common;
using FootballBooking_BE.Data.Entities;
using FootballBooking_BE.Models.DTOs;
using FootballBooking_BE.Repositories.Interfaces;
using FootballBooking_BE.Services.Interfaces;

namespace FootballBooking_BE.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IPriceSlotRepository _priceSlotRepository;
        private readonly IPitchRepository _pitchRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IPriceSlotRepository priceSlotRepository,
            IPitchRepository pitchRepository)
        {
            _bookingRepository = bookingRepository;
            _priceSlotRepository = priceSlotRepository;
            _pitchRepository = pitchRepository;
        }

        public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(int userId, BookingCreateRequest request)
        {
            // 1. Validate Start Time (:00 or :30)
            if (request.StartTime.Minutes != 0 && request.StartTime.Minutes != 30)
            {
                return ApiResponse<BookingResponse>.Fail("Booking chỉ được bắt đầu tại các mốc :00 hoặc :30.");
            }

            // 2. Validate Duration (60, 90, 120)
            if (request.DurationMinutes != 60 && request.DurationMinutes != 90 && request.DurationMinutes != 120)
            {
                return ApiResponse<BookingResponse>.Fail("Thời lượng đá chỉ được là 60, 90 hoặc 120 phút.");
            }

            // 3. Future Check
            DateTime playDateTime = request.PlayDate.ToDateTime(new TimeOnly(request.StartTime.Ticks));
            if (playDateTime < DateTime.Now)
            {
                return ApiResponse<BookingResponse>.Fail("Không thể đặt sân trong quá khứ.");
            }

            TimeSpan endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

            // 4. Check Pitch status
            var pitch = await _pitchRepository.GetPitchByIdAsync(request.PitchId);
            if (pitch == null || pitch.Status != "ACTIVE")
            {
                return ApiResponse<BookingResponse>.Fail("Sân không tồn tại hoặc đã ngừng hoạt động.");
            }

            // 5. Check operating hours and Price Calculation (Supports multiple slots)
            var priceSlots = await _priceSlotRepository.GetByPitchIdAsync(request.PitchId);
            string dayType = (request.PlayDate.DayOfWeek == DayOfWeek.Saturday || request.PlayDate.DayOfWeek == DayOfWeek.Sunday) ? "WEEKEND" : "WEEKDAY";
            var applicablePriceSlots = priceSlots.Where(s => s.ApplyOn == "ALL" || s.ApplyOn == dayType).ToList();

            decimal totalPrice = 0;
            // Break entire booking into 30-minute blocks
            for (int offset = 0; offset < request.DurationMinutes; offset += 30)
            {
                TimeSpan blockStart = request.StartTime.Add(TimeSpan.FromMinutes(offset));
                TimeSpan blockEnd = blockStart.Add(TimeSpan.FromMinutes(30));

                // Find a price slot that covers this 30-minute block
                var matchingSlot = applicablePriceSlots.FirstOrDefault(s => s.StartTime <= blockStart && s.EndTime >= blockEnd);
                if (matchingSlot == null)
                {
                    return ApiResponse<BookingResponse>.Fail($"Khung giờ {blockStart:hh\\:mm} - {blockEnd:hh\\:mm} không nằm trong giờ hoạt động của sân.");
                }
                totalPrice += matchingSlot.PricePerHour / 2; // Price for 30 mins
            }

            // 6. Check Overlap
            bool isOverlapped = await _bookingRepository.CheckOverlapAsync(request.PitchId, request.PlayDate, request.StartTime, endTime);
            if (isOverlapped)
            {
                return ApiResponse<BookingResponse>.Fail("Khung giờ này đã có người đặt.");
            }

            // 7. Create Booking
            var booking = new Booking
            {
                UserId = userId,
                TotalAmount = totalPrice,
                Status = "PENDING",
                PaymentStatus = "UNPAID",
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                BookingDetails = new List<BookingDetail>
                {
                    new BookingDetail
                    {
                        PitchId = request.PitchId,
                        PlayDate = request.PlayDate,
                        StartTime = request.StartTime,
                        EndTime = endTime,
                        DurationMinutes = request.DurationMinutes,
                        PriceAtBooking = totalPrice,
                        DetailStatus = "PENDING",
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            var createdBooking = await _bookingRepository.CreateBookingAsync(booking);
            return ApiResponse<BookingResponse>.Ok(MapToResponse(createdBooking));
        }

        public async Task<ApiResponse<IEnumerable<AvailabilitySlot>>> GetAvailableSlotsAsync(int pitchId, DateOnly playDate)
        {
            var priceSlots = await _priceSlotRepository.GetByPitchIdAsync(pitchId);
            if (!priceSlots.Any()) return ApiResponse<IEnumerable<AvailabilitySlot>>.Ok(new List<AvailabilitySlot>());

            string dayType = (playDate.DayOfWeek == DayOfWeek.Saturday || playDate.DayOfWeek == DayOfWeek.Sunday) ? "WEEKEND" : "WEEKDAY";
            var applicablePriceSlots = priceSlots.Where(s => s.ApplyOn == "ALL" || s.ApplyOn == dayType).ToList();

            if (!applicablePriceSlots.Any()) return ApiResponse<IEnumerable<AvailabilitySlot>>.Ok(new List<AvailabilitySlot>());

            // Get existing bookings for the day to mark as unavailable
            // Note: We need a new repository method or just use CheckOverlap for each slot (less efficient)
            // For now, let's assume we can get all active bookings for this pitch/date.
            // (Re-using a simplified overlap check logic here)
            
            var slots = new List<AvailabilitySlot>();
            
            // Operating range: min StartTime to max EndTime of all applicable price slots
            var minStart = applicablePriceSlots.Min(s => s.StartTime);
            var maxEnd = applicablePriceSlots.Max(s => s.EndTime);

            for (var time = minStart; time < maxEnd; time = time.Add(TimeSpan.FromMinutes(30)))
            {
                var blockStart = time;
                var blockEnd = time.Add(TimeSpan.FromMinutes(30));

                var priceSlot = applicablePriceSlots.FirstOrDefault(s => s.StartTime <= blockStart && s.EndTime >= blockEnd);
                if (priceSlot == null) continue; // Skip blocks outside operating hours

                bool isOccupied = await _bookingRepository.CheckOverlapAsync(pitchId, playDate, blockStart, blockEnd);

                slots.Add(new AvailabilitySlot
                {
                    StartTime = blockStart,
                    EndTime = blockEnd,
                    IsAvailable = !isOccupied && (playDate.ToDateTime(new TimeOnly(blockStart.Ticks)) > DateTime.Now),
                    Price = priceSlot.PricePerHour / 2
                });
            }

            return ApiResponse<IEnumerable<AvailabilitySlot>>.Ok(slots);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetMyBookingsAsync(int userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            return ApiResponse<IEnumerable<BookingResponse>>.Ok(bookings.Select(MapToResponse));
        }

        public async Task<ApiResponse<BookingResponse>> GetBookingByIdAsync(int userId, int bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            if (booking == null || booking.UserId != userId)
            {
                return ApiResponse<BookingResponse>.Fail("Không tìm thấy booking.");
            }
            return ApiResponse<BookingResponse>.Ok(MapToResponse(booking));
        }
        public async Task<ApiResponse<bool>> CancelBookingAsync(int userId, int detailId, CancelBookingRequest request) { 
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> StaffCancelBookingAsync(int staffId, int detailId, CancelBookingRequest request)
        {
            var detail = await _bookingRepository.GetBookingDetailByIdAsync(detailId);
            if (detail == null)
            {
                return ApiResponse<bool>.Fail("Không tìm thấy thông tin đặt sân.");
            }

            // Check if staff is assigned to this pitch
            bool isAssigned = await _bookingRepository.IsStaffAssignedToPitchAsync(staffId, detail.PitchId);
            if (!isAssigned)
            {
                return ApiResponse<bool>.Fail("Bạn không có quyền quản lý sân này.");
            }

            if (detail.DetailStatus == "CANCELLED" || detail.DetailStatus == "COMPLETED" || detail.DetailStatus == "REJECTED")
            {
                return ApiResponse<bool>.Fail("Không thể thực hiện trên booking ở trạng thái này.");
            }

            string oldStatus = detail.DetailStatus;
            detail.DetailStatus = "REJECTED"; // Staff rejects the booking
            detail.CancellationReason = request.Reason;

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingDetailId = detail.DetailId,
                OldStatus = oldStatus,
                NewStatus = "REJECTED",
                ChangedBy = staffId,
                ChangedAt = DateTime.UtcNow
            });

            var booking = await _bookingRepository.GetBookingByIdAsync(detail.BookingId);
            if (booking != null)
            {
                if (booking.BookingDetails.All(d => d.DetailStatus == "REJECTED" || d.DetailStatus == "CANCELLED"))
                {
                    booking.Status = "REJECTED";
                }
                await _bookingRepository.UpdateBookingAsync(booking);
            }

            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> BulkCancelByPitchAsync(int staffId, BulkCancelBookingRequest request)
        {
            // Check if staff is assigned to this pitch
            bool isAssigned = await _bookingRepository.IsStaffAssignedToPitchAsync(staffId, request.PitchId);
            if (!isAssigned)
            {
                return ApiResponse<bool>.Fail("Bạn không có quyền quản lý sân này.");
            }

            var activeDetails = await _bookingRepository.GetActiveBookingDetailsByPitchAsync(request.PitchId, request.FromDate);
            if (!activeDetails.Any())
            {
                return ApiResponse<bool>.Ok(true); // Nothing to cancel
            }

            foreach (var detail in activeDetails)
            {
                string oldStatus = detail.DetailStatus;
                detail.DetailStatus = "CANCELLED";
                detail.CancellationReason = $"[Hủy hàng loạt bởi Staff] {request.Reason}";

                await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
                {
                    BookingDetailId = detail.DetailId,
                    OldStatus = oldStatus,
                    NewStatus = "CANCELLED",
                    ChangedBy = staffId,
                    ChangedAt = DateTime.UtcNow
                });

                // Update parent booking if necessary
                var booking = await _bookingRepository.GetBookingByIdAsync(detail.BookingId);
                if (booking != null && booking.BookingDetails.All(d => d.DetailStatus == "CANCELLED" || d.DetailStatus == "REJECTED"))
                {
                    booking.Status = "CANCELLED";
                    await _bookingRepository.UpdateBookingAsync(booking);
                }
                else if (booking != null)
                {
                    await _bookingRepository.UpdateBookingAsync(booking);
                }
            }

            return ApiResponse<bool>.Ok(true);
        }

        private BookingResponse MapToResponse(Booking b)
        {
            return new BookingResponse
            {
                BookingId = b.BookingId,
                UserId = b.UserId,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                PaymentStatus = b.PaymentStatus,
                Notes = b.Notes,
                CreatedAt = b.CreatedAt,
                Details = b.BookingDetails.Select(d => new BookingDetailResponse
                {
                    DetailId = d.DetailId,
                    PitchId = d.PitchId,
                    PitchName = d.Pitch?.PitchName ?? "N/A",
                    PlayDate = d.PlayDate,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    DurationMinutes = d.DurationMinutes,
                    PriceAtBooking = d.PriceAtBooking,
                    Status = d.DetailStatus
                }).ToList()
            };
        }
    }
}
