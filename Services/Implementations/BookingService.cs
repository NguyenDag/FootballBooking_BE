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

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetBookingHistoryAsync(int userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now.TimeOfDay;

            var historyBookings = bookings.Where(b => 
                b.Status.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                b.Status.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                b.Status.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                // Check if ALL details are in the past
                b.BookingDetails.All(d => d.PlayDate < today || (d.PlayDate == today && d.StartTime < now))
            );

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(historyBookings.Select(MapToResponse));
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetUpcomingBookingsAsync(int userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now.TimeOfDay;

            var upcomingBookings = bookings.Where(b => 
                (b.Status.Equals(BookingStatus.Pending, StringComparison.OrdinalIgnoreCase) ||
                 b.Status.Equals(BookingStatus.Confirmed, StringComparison.OrdinalIgnoreCase)) &&
                // Check if ANY detail is in the future
                b.BookingDetails.Any(d => d.PlayDate > today || (d.PlayDate == today && d.StartTime >= now))
            );

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(upcomingBookings.Select(MapToResponse));
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
        public async Task<ApiResponse<bool>> CancelBookingAsync(int userId, int detailId, CancelBookingRequest request)
        {
            var detail = await _bookingRepository.GetBookingDetailByIdAsync(detailId);
            if (detail == null || detail.Booking.UserId != userId)
            {
                return ApiResponse<bool>.Fail("Không tìm thấy thông tin đặt sân hoặc bạn không có quyền hủy.");
            }

            if (detail.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) || 
                detail.DetailStatus.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                detail.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<bool>.Fail("Không thể hủy booking ở trạng thái này.");
            }

            // 6-hour notice check
            var bookingDate = detail.PlayDate.ToDateTime(new TimeOnly(detail.StartTime.Ticks));
            if ((bookingDate - DateTime.Now).TotalHours < 6)
            {
                return ApiResponse<bool>.Fail("Chỉ có thể hủy lịch trước giờ đá tối thiểu 6 tiếng.");
            }

            string oldStatus = detail.DetailStatus;
            detail.DetailStatus = BookingStatus.Cancelled;
            detail.CancellationReason = request.Reason;

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingDetailId = detail.DetailId,
                OldStatus = oldStatus,
                NewStatus = BookingStatus.Cancelled,
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow
            });

            await _bookingRepository.UpdateBookingAsync(detail.Booking); // Sync changes to detail and history through navigation or repo update

            // Sync parent Booking status if all details are cancelled or rejected
            var booking = await _bookingRepository.GetBookingByIdAsync(detail.BookingId);
            if (booking != null && booking.BookingDetails.All(d => 
                d.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) || 
                d.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase)))
            {
                booking.Status = BookingStatus.Cancelled;
                await _bookingRepository.UpdateBookingAsync(booking);
            }

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
            detail.StaffId = staffId;

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

        public async Task<ApiResponse<bool>> StaffConfirmBookingAsync(int staffId, int detailId)
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

            if (detail.DetailStatus != BookingStatus.Pending)
            {
                return ApiResponse<bool>.Fail("Chỉ có thể xác nhận booking ở trạng thái PENDING.");
            }

            string oldStatus = detail.DetailStatus;
            detail.DetailStatus = BookingStatus.Confirmed;
            detail.StaffId = staffId;

            await _bookingRepository.AddStatusHistoryAsync(new BookingStatusHistory
            {
                BookingDetailId = detail.DetailId,
                OldStatus = oldStatus,
                NewStatus = BookingStatus.Confirmed,
                ChangedBy = staffId,
                ChangedAt = DateTime.UtcNow
            });

            var booking = await _bookingRepository.GetBookingByIdAsync(detail.BookingId);
            if (booking != null)
            {
                // If any detail is confirmed, parent booking becomes CONFIRMED
                booking.Status = BookingStatus.Confirmed;
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
                detail.StaffId = staffId;

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

        public async Task<ApiResponse<Models.DTOs.Dashboard.DashboardStatsResponse>> GetDashboardStatsAsync(int userId, string role)
        {
            var allDetails = await _bookingRepository.GetAllBookingDetailsAsync();
            
            // Filter details based on user role
            IEnumerable<BookingDetail> userDetails;

            int managedPitches = 0;
            if (role.Equals(UserRole.Customer, StringComparison.OrdinalIgnoreCase))
            {
                userDetails = allDetails.Where(d => d.Booking.UserId == userId);
            }
            else if (role.Equals(UserRole.Staff, StringComparison.OrdinalIgnoreCase))
            {
                var staffPitchIds = await _bookingRepository.GetStaffAssignedPitchIdsAsync(userId);
                managedPitches = staffPitchIds.Count;
                userDetails = allDetails.Where(d => staffPitchIds.Contains(d.PitchId));
            }
            else // ADMIN
            {
                userDetails = allDetails;
            }

            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = DateTime.Now.TimeOfDay;

            var stats = new Models.DTOs.Dashboard.DashboardStatsResponse
            {
                // Total bookings = Count all regardless of status
                TotalBookingsCount = userDetails.Count(),
                
                // Upcoming Confirmed = CONFIRMED and in the future
                UpcomingConfirmedCount = userDetails.Count(d => 
                    d.DetailStatus.Equals(BookingStatus.Confirmed, StringComparison.OrdinalIgnoreCase) && 
                    (d.PlayDate > today || (d.PlayDate == today && d.StartTime > now))),
                
                CompletedBookingsCount = userDetails.Count(d => 
                    d.DetailStatus.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase)),
                
                RejectedBookingsCount = userDetails.Count(d => 
                    d.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase) ||
                    d.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase)),
                
                PendingBookingsCount = userDetails.Count(d => 
                    d.DetailStatus.Equals(BookingStatus.Pending, StringComparison.OrdinalIgnoreCase)),
                    
                TotalManagedPitches = managedPitches,
                
                UpcomingBookings = userDetails
                    .Where(d => (d.DetailStatus.Equals(BookingStatus.Confirmed, StringComparison.OrdinalIgnoreCase) || 
                                 d.DetailStatus.Equals(BookingStatus.Pending, StringComparison.OrdinalIgnoreCase)) && 
                                (d.PlayDate > today || (d.PlayDate == today && d.StartTime > now)))
                    .OrderBy(d => d.PlayDate)
                    .ThenBy(d => d.StartTime)
                    .Take(10)
                    .Select(d => new Models.DTOs.Dashboard.UpcomingBookingDto
                    {
                        DetailId = d.DetailId,
                        PitchName = d.Pitch?.PitchName ?? "N/A",
                        PlayDate = d.PlayDate,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Price = d.PriceAtBooking,
                        Status = d.DetailStatus,
                        CustomerName = d.Booking.User?.FullName,
                        CustomerPhone = d.Booking.User?.Phone
                    })
                    .ToList(),
                    
                PendingBookings = userDetails
                    .Where(d => d.DetailStatus.Equals(BookingStatus.Pending, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(d => d.PlayDate)
                    .ThenBy(d => d.StartTime)
                    .Select(d => new Models.DTOs.Dashboard.UpcomingBookingDto
                    {
                        DetailId = d.DetailId,
                        PitchName = d.Pitch?.PitchName ?? "N/A",
                        PlayDate = d.PlayDate,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        Price = d.PriceAtBooking,
                        Status = d.DetailStatus,
                        CustomerName = d.Booking.User?.FullName,
                        CustomerPhone = d.Booking.User?.Phone
                    })
                    .ToList()
            };

            return ApiResponse<Models.DTOs.Dashboard.DashboardStatsResponse>.Ok(stats);
        }

        public async Task<ApiResponse<Models.DTOs.Dashboard.AdminAdvancedStatsResponse>> GetAdminAdvancedStatsAsync(DateOnly fromDate, DateOnly toDate)
        {
            var allDetails = await _bookingRepository.GetAllBookingDetailsAsync();

            // Lọc theo khoảng thời gian PlayDate
            var periodDetails = allDetails.Where(d => 
                d.PlayDate >= fromDate && d.PlayDate <= toDate).ToList();

            var totalBookings = periodDetails.Count;

            // Tính Revenue: Chỉ tính các BookingDetail có trạng thái COMPLETED
            var revenueDetails = periodDetails.Where(d => 
                d.DetailStatus.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                (d.Booking != null && d.Booking.PaymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))
            ).ToList();

            decimal totalRevenue = revenueDetails.Sum(d => d.PriceAtBooking);

            // Tỉ lệ huỷ / từ chối
            var cancelledOrRejectedCount = periodDetails.Count(d => 
                d.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) ||
                d.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase));

            double cancellationRate = totalBookings > 0 
                ? Math.Round((double)cancelledOrRejectedCount / totalBookings * 100, 2) 
                : 0;

            // Nhóm theo ngày (BookingsByDate)
            var bookingsGroupedByDate = periodDetails
                .GroupBy(d => d.PlayDate)
                .Select(g => new Models.DTOs.Dashboard.BookingsByDateDto
                {
                    DateLabel = g.Key.ToString("yyyy-MM-dd"),
                    BookingsCount = g.Count()
                })
                .OrderBy(x => x.DateLabel)
                .ToList();

            // Nhóm doanh thu theo sân (RevenueByPitch)
            var revenueGroupedByPitch = revenueDetails
                .GroupBy(d => new { d.PitchId, PitchName = d.Pitch?.PitchName ?? "Unknown" })
                .Select(g => new Models.DTOs.Dashboard.RevenueByPitchDto
                {
                    PitchId = g.Key.PitchId,
                    PitchName = g.Key.PitchName,
                    TotalRevenue = g.Sum(d => d.PriceAtBooking)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList();

            // Nhóm doanh thu theo tháng (Last 6 months)
            var sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6));
            var monthlyRevenue = allDetails
                .Where(d => d.PlayDate >= sixMonthsAgo && 
                            (d.DetailStatus.Equals(BookingStatus.Completed, StringComparison.OrdinalIgnoreCase) ||
                             (d.Booking != null && d.Booking.PaymentStatus.Equals("PAID", StringComparison.OrdinalIgnoreCase))))
                .GroupBy(d => new { d.PlayDate.Year, d.PlayDate.Month })
                .Select(g => new Models.DTOs.Dashboard.MonthlyRevenueDto
                {
                    Month = $"T{g.Key.Month}/{g.Key.Year}",
                    Revenue = g.Sum(d => d.PriceAtBooking),
                    Bookings = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Thống kê giờ cao điểm (PeakHours)
            var peakHours = new List<Models.DTOs.Dashboard.PeakHourDto>();
            string[] ranges = { "06:00-09:00", "09:00-12:00", "12:00-15:00", "15:00-18:00", "18:00-21:00", "21:00-24:00" };
            
            foreach (var range in ranges)
            {
                var times = range.Split('-');
                var start = TimeSpan.Parse(times[0]);
                var end = times[1] == "24:00" ? TimeSpan.FromHours(24) : TimeSpan.Parse(times[1]);

                int count = periodDetails.Count(d => d.StartTime >= start && d.StartTime < end);
                peakHours.Add(new Models.DTOs.Dashboard.PeakHourDto
                {
                    HourRange = range,
                    BookingsCount = count
                });
            }

            var response = new Models.DTOs.Dashboard.AdminAdvancedStatsResponse
            {
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                CancellationRate = cancellationRate,
                BookingsByDate = bookingsGroupedByDate,
                RevenueByPitch = revenueGroupedByPitch,
                MonthlyRevenue = monthlyRevenue,
                PeakHours = peakHours
            };

            return ApiResponse<Models.DTOs.Dashboard.AdminAdvancedStatsResponse>.Ok(response);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetPitchBookingsByDateAsync(int pitchId, DateOnly date)
        {
            var allDetails = await _bookingRepository.GetAllBookingDetailsAsync();
            var filteredDetails = allDetails.Where(d => 
                d.PitchId == pitchId && 
                d.PlayDate == date && 
                !d.DetailStatus.Equals(BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase) &&
                !d.DetailStatus.Equals(BookingStatus.Rejected, StringComparison.OrdinalIgnoreCase));

            var bookings = filteredDetails
                .GroupBy(d => d.BookingId)
                .Select(g => MapToResponseFromDetails(g.Key, g.ToList()))
                .ToList();

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(bookings);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffBookingsByDateAsync(int staffId, DateOnly date)
        {
            var staffDetails = await _bookingRepository.GetBookingDetailsByStaffAndDateAsync(staffId, date);

            var bookings = staffDetails
                .GroupBy(d => d.BookingId)
                .Select(g => MapToResponseFromDetails(g.Key, g.ToList()))
                .ToList();

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(bookings);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffAllBookingsAsync(int staffId, DateOnly? date = null)
        {
            IEnumerable<BookingDetail> staffDetails;
            if (date.HasValue)
            {
                staffDetails = await _bookingRepository.GetBookingDetailsByStaffAndDateAsync(staffId, date.Value);
            }
            else
            {
                staffDetails = await _bookingRepository.GetBookingDetailsByStaffAsync(staffId);
            }

            var bookings = staffDetails
                .GroupBy(d => d.BookingId)
                .Select(g => MapToResponseFromDetails(g.Key, g.ToList()))
                .ToList();

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(bookings);
        }

        public async Task<ApiResponse<IEnumerable<BookingResponse>>> GetStaffPendingBookingsAsync(int staffId)
        {
            var pendingDetails = await _bookingRepository.GetPendingBookingDetailsByStaffAsync(staffId);

            var bookings = pendingDetails
                .GroupBy(d => d.BookingId)
                .Select(g => MapToResponseFromDetails(g.Key, g.ToList()))
                .ToList();

            return ApiResponse<IEnumerable<BookingResponse>>.Ok(bookings);
        }

        private async Task<List<int>> GetStaffAssignedPitchIdsAsync(int staffId)
        {
            return await _bookingRepository.GetStaffAssignedPitchIdsAsync(staffId);
        }

        private BookingResponse MapToResponseFromDetails(int bookingId, List<BookingDetail> details)
        {
            var first = details.First();
            var booking = first.Booking;

            if (booking == null)
            {
                // Fallback if booking is not loaded/missing
                return new BookingResponse
                {
                    BookingId = bookingId,
                    Status = "UNKNOWN",
                    Details = details.Select(d => new BookingDetailResponse
                    {
                        DetailId = d.DetailId,
                        PitchId = d.PitchId,
                        PitchName = d.Pitch?.PitchName ?? "N/A",
                        PlayDate = d.PlayDate,
                        StartTime = d.StartTime,
                        EndTime = d.EndTime,
                        DurationMinutes = d.DurationMinutes,
                        PriceAtBooking = d.PriceAtBooking,
                        Status = d.DetailStatus,
                        CancellationReason = d.CancellationReason
                    }).ToList()
                };
            }

            return new BookingResponse
            {
                BookingId = bookingId,
                UserId = booking.UserId,
                CustomerName = booking.User?.FullName,
                CustomerPhone = booking.User?.Phone,
                TotalAmount = booking.TotalAmount,
                Status = booking.Status,
                PaymentStatus = booking.PaymentStatus,
                Notes = booking.Notes,
                CreatedAt = booking.CreatedAt,
                Details = details.Select(d => new BookingDetailResponse
                {
                    DetailId = d.DetailId,
                    PitchId = d.PitchId,
                    PitchName = d.Pitch?.PitchName ?? "N/A",
                    PlayDate = d.PlayDate,
                    StartTime = d.StartTime,
                    EndTime = d.EndTime,
                    DurationMinutes = d.DurationMinutes,
                    PriceAtBooking = d.PriceAtBooking,
                    Status = d.DetailStatus,
                    CancellationReason = d.CancellationReason
                }).ToList()
            };
        }

        private BookingResponse MapToResponse(Booking b)
        {
            return new BookingResponse
            {
                BookingId = b.BookingId,
                UserId = b.UserId,
                CustomerName = b.User?.FullName,
                CustomerPhone = b.User?.Phone,
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
                    Status = d.DetailStatus, // Use detail status instead of parent for more accuracy
                    CancellationReason = d.CancellationReason
                }).ToList()
            };
        }
    }
}
