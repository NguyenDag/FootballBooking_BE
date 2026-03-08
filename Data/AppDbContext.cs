using FootballBooking_BE.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FootballBooking_BE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Pitch> Pitches => Set<Pitch>();
        public DbSet<StaffPitchAssignment> StaffPitchAssignments => Set<StaffPitchAssignment>();
        public DbSet<PriceSlot> PriceSlots => Set<PriceSlot>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingDetail> BookingDetails => Set<BookingDetail>();
        public DbSet<BookingStatusHistory> BookingStatusHistories => Set<BookingStatusHistory>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<TopUpRequest> TopUpRequests => Set<TopUpRequest>();
        public DbSet<RefundPolicy> RefundPolicies => Set<RefundPolicy>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================================
            // Users
            // ============================================================
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();

                e.Property(u => u.Role)
                 .HasMaxLength(20);

                e.HasCheckConstraint("CK_Users_Role",
                    "[Role] IN ('ADMIN', 'STAFF', 'CUSTOMER')");
            });

            // ============================================================
            // Pitches
            // ============================================================
            modelBuilder.Entity<Pitch>(e =>
            {
                e.HasCheckConstraint("CK_Pitches_Type",
                    "[PitchType] IN ('5_PERSON', '7_PERSON', '11_PERSON')");

                e.HasCheckConstraint("CK_Pitches_Status",
                    "[Status] IN ('ACTIVE', 'MAINTENANCE', 'INACTIVE')");
            });

            // ============================================================
            // StaffPitchAssignments
            // ============================================================
            modelBuilder.Entity<StaffPitchAssignment>(e =>
            {
                e.HasIndex(s => new { s.StaffId, s.PitchId }).IsUnique();

                e.HasOne(s => s.Staff)
                 .WithMany(u => u.StaffPitchAssignments)
                 .HasForeignKey(s => s.StaffId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(s => s.Pitch)
                 .WithMany(p => p.StaffPitchAssignments)
                 .HasForeignKey(s => s.PitchId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================================
            // PriceSlots
            // ============================================================
            modelBuilder.Entity<PriceSlot>(e =>
            {
                e.HasCheckConstraint("CK_PriceSlot_Type",
                    "[PitchType] IN ('5_PERSON', '7_PERSON', '11_PERSON')");

                e.HasCheckConstraint("CK_PriceSlot_ApplyOn",
                    "[ApplyOn] IN ('WEEKDAY', 'WEEKEND', 'ALL')");

                e.HasCheckConstraint("CK_PriceSlot_Time",
                    "[StartTime] < [EndTime]");

                e.HasOne(ps => ps.Pitch)
                 .WithMany(p => p.PriceSlots)
                 .HasForeignKey(ps => ps.PitchId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================================
            // Bookings
            // ============================================================
            modelBuilder.Entity<Booking>(e =>
            {
                e.HasCheckConstraint("CK_Bookings_Status",
                    "[Status] IN ('PENDING', 'CONFIRMED', 'COMPLETED', 'CANCELLED', 'REJECTED')");

                e.HasCheckConstraint("CK_Bookings_PaymentStatus",
                    "[PaymentStatus] IN ('UNPAID', 'PAID', 'REFUNDED')");

                e.HasOne(b => b.User)
                 .WithMany(u => u.Bookings)
                 .HasForeignKey(b => b.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================================
            // BookingDetails
            // ============================================================
            modelBuilder.Entity<BookingDetail>(e =>
            {
                e.HasCheckConstraint("CK_BookingDetails_Duration",
                    "[DurationMinutes] IN (60, 90, 120)");

                e.HasCheckConstraint("CK_BookingDetails_Status",
                    "[DetailStatus] IN ('PENDING', 'CONFIRMED', 'CANCELLED', 'COMPLETED')");

                // Index chống trùng slot
                e.HasIndex(d => new { d.PitchId, d.PlayDate, d.StartTime, d.EndTime })
                 .HasDatabaseName("IDX_Booking_Conflict");

                e.HasOne(d => d.Booking)
                 .WithMany(b => b.BookingDetails)
                 .HasForeignKey(d => d.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(d => d.Pitch)
                 .WithMany(p => p.BookingDetails)
                 .HasForeignKey(d => d.PitchId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(d => d.Staff)
                 .WithMany(u => u.AssignedBookingDetails)
                 .HasForeignKey(d => d.StaffId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // BookingStatusHistory
            // ============================================================
            modelBuilder.Entity<BookingStatusHistory>(e =>
            {
                e.HasOne(h => h.BookingDetail)
                 .WithMany(d => d.StatusHistories)
                 .HasForeignKey(h => h.BookingDetailId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(h => h.ChangedByUser)
                 .WithMany(u => u.BookingStatusHistories)
                 .HasForeignKey(h => h.ChangedBy)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // Wallets
            // ============================================================
            modelBuilder.Entity<Wallet>(e =>
            {
                e.HasIndex(w => w.UserId).IsUnique();

                e.HasCheckConstraint("CK_Wallets_Balance", "[Balance] >= 0");

                e.HasOne(w => w.User)
                 .WithOne(u => u.Wallet)
                 .HasForeignKey<Wallet>(w => w.UserId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ============================================================
            // Transactions
            // ============================================================
            modelBuilder.Entity<Transaction>(e =>
            {
                e.HasCheckConstraint("CK_Transactions_Type",
                    "[TransactionType] IN ('TOP_UP', 'BOOKING_PAYMENT', 'REFUND', 'WITHDRAWAL', 'ADJUSTMENT')");

                e.HasCheckConstraint("CK_Transactions_Direction",
                    "[Direction] IN ('CREDIT', 'DEBIT')");

                e.HasCheckConstraint("CK_Transactions_Status",
                    "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'REVERSED')");

                e.HasCheckConstraint("CK_Transactions_Amount",
                    "[Amount] > 0");

                e.HasIndex(t => new { t.WalletId, t.CreatedAt })
                 .HasDatabaseName("IDX_Transactions_Wallet");

                e.HasIndex(t => t.BookingId)
                 .HasDatabaseName("IDX_Transactions_Booking");

                e.HasIndex(t => t.ReferenceId)
                 .HasDatabaseName("IDX_Transactions_Ref");

                e.HasOne(t => t.Wallet)
                 .WithMany(w => w.Transactions)
                 .HasForeignKey(t => t.WalletId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Booking)
                 .WithMany(b => b.Transactions)
                 .HasForeignKey(t => t.BookingId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // Payments
            // ============================================================
            modelBuilder.Entity<Payment>(e =>
            {
                e.HasCheckConstraint("CK_Payments_Method",
                    "[PaymentMethod] IN ('WALLET', 'CASH', 'BANK_TRANSFER', 'VNPAY', 'MOMO', 'ZALOPAY')");

                e.HasCheckConstraint("CK_Payments_Status",
                    "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'CANCELLED', 'REFUNDED', 'PARTIALLY_REFUNDED')");

                e.HasCheckConstraint("CK_Payments_Amount", "[Amount] > 0");

                e.HasIndex(p => p.BookingId)
                 .HasDatabaseName("IDX_Payments_Booking");

                e.HasIndex(p => p.GatewayTransactionId)
                 .HasDatabaseName("IDX_Payments_Gateway");

                e.HasOne(p => p.Booking)
                 .WithMany(b => b.Payments)
                 .HasForeignKey(p => p.BookingId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.Transaction)
                 .WithOne(t => t.Payment)
                 .HasForeignKey<Payment>(p => p.TransactionId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(p => p.ConfirmedByUser)
                 .WithMany(u => u.ConfirmedPayments)
                 .HasForeignKey(p => p.ConfirmedBy)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // Refunds
            // ============================================================
            modelBuilder.Entity<Refund>(e =>
            {
                e.HasCheckConstraint("CK_Refunds_Status",
                    "[Status] IN ('PENDING', 'APPROVED', 'PROCESSING', 'COMPLETED', 'REJECTED')");

                e.HasCheckConstraint("CK_Refunds_Method",
                    "[RefundMethod] IS NULL OR [RefundMethod] IN ('WALLET', 'BANK_TRANSFER', 'ORIGINAL_METHOD')");

                e.HasCheckConstraint("CK_Refunds_Amount", "[RefundAmount] > 0");

                e.HasIndex(r => r.PaymentId)
                 .HasDatabaseName("IDX_Refunds_Payment");

                e.HasIndex(r => new { r.Status, r.CreatedAt })
                 .HasDatabaseName("IDX_Refunds_Status");

                e.HasOne(r => r.Payment)
                 .WithMany(p => p.Refunds)
                 .HasForeignKey(r => r.PaymentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.BookingDetail)
                 .WithMany(d => d.Refunds)
                 .HasForeignKey(r => r.BookingDetailId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(r => r.Transaction)
                 .WithOne(t => t.Refund)
                 .HasForeignKey<Refund>(r => r.TransactionId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(r => r.RequestedByUser)
                 .WithMany(u => u.RequestedRefunds)
                 .HasForeignKey(r => r.RequestedBy)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.ReviewedByUser)
                 .WithMany(u => u.ReviewedRefunds)
                 .HasForeignKey(r => r.ReviewedBy)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ============================================================
            // TopUpRequests
            // ============================================================
            modelBuilder.Entity<TopUpRequest>(e =>
            {
                e.HasCheckConstraint("CK_TopUp_Method",
                    "[PaymentMethod] IN ('BANK_TRANSFER', 'VNPAY', 'MOMO', 'ZALOPAY')");

                e.HasCheckConstraint("CK_TopUp_Status",
                    "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'CANCELLED')");

                e.HasCheckConstraint("CK_TopUp_Amount", "[Amount] > 0");

                e.HasOne(t => t.User)
                 .WithMany(u => u.TopUpRequests)
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Wallet)
                 .WithMany(w => w.TopUpRequests)
                 .HasForeignKey(t => t.WalletId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Transaction)
                 .WithOne(tr => tr.TopUpRequest)
                 .HasForeignKey<TopUpRequest>(t => t.TransactionId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(t => t.ConfirmedByUser)
                 .WithMany(u => u.ConfirmedTopUps)
                 .HasForeignKey(t => t.ConfirmedBy)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.HasIndex(rt => rt.Token).IsUnique();
                e.HasOne(rt => rt.User)
                 .WithMany()
                 .HasForeignKey(rt => rt.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PasswordResetToken>(e =>
            {
                e.HasIndex(t => t.Token).IsUnique();
                e.HasOne(t => t.User)
                 .WithMany()
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ============================================================
            // RefundPolicies
            // ============================================================
            modelBuilder.Entity<RefundPolicy>(e =>
            {
                e.HasCheckConstraint("CK_RefundPolicy_Percentage",
                    "[RefundPercentage] BETWEEN 0 AND 100");

                // Seed data
                e.HasData(
                    new RefundPolicy
                    {
                        PolicyId = 1,
                        CancelBeforeHours = 48,
                        RefundPercentage = 100.00m,
                        Description = "Huỷ trước 48 giờ — hoàn 100%",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)  // ← cố định
                    },
                    new RefundPolicy
                    {
                        PolicyId = 2,
                        CancelBeforeHours = 24,
                        RefundPercentage = 70.00m,
                        Description = "Huỷ trước 24 giờ — hoàn 70%",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new RefundPolicy
                    {
                        PolicyId = 3,
                        CancelBeforeHours = 12,
                        RefundPercentage = 50.00m,
                        Description = "Huỷ trước 12 giờ — hoàn 50%",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new RefundPolicy
                    {
                        PolicyId = 4,
                        CancelBeforeHours = 6,
                        RefundPercentage = 0.00m,
                        Description = "Huỷ trong vòng 6 giờ — không hoàn tiền",
                        IsActive = true,
                        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    }
                );
            });
        }
    }
}
