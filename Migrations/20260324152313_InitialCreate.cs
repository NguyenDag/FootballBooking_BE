using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FootballBooking_BE.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pitches",
                columns: table => new
                {
                    PitchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PitchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PitchType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pitches", x => x.PitchId);
                    table.CheckConstraint("CK_Pitches_Status", "[Status] IN ('ACTIVE', 'MAINTENANCE', 'INACTIVE')");
                    table.CheckConstraint("CK_Pitches_Type", "[PitchType] IN ('5_PERSON', '7_PERSON', '11_PERSON')");
                });

            migrationBuilder.CreateTable(
                name: "RefundPolicies",
                columns: table => new
                {
                    PolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CancelBeforeHours = table.Column<int>(type: "int", nullable: false),
                    RefundPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundPolicies", x => x.PolicyId);
                    table.CheckConstraint("CK_RefundPolicy_Percentage", "[RefundPercentage] BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('ADMIN', 'STAFF', 'CUSTOMER')");
                });

            migrationBuilder.CreateTable(
                name: "PriceSlots",
                columns: table => new
                {
                    PriceSlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PitchId = table.Column<int>(type: "int", nullable: false),
                    PitchType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    PricePerHour = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ApplyOn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPeakHour = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSlots", x => x.PriceSlotId);
                    table.CheckConstraint("CK_PriceSlot_ApplyOn", "[ApplyOn] IN ('WEEKDAY', 'WEEKEND', 'ALL')");
                    table.CheckConstraint("CK_PriceSlot_Time", "[StartTime] < [EndTime]");
                    table.CheckConstraint("CK_PriceSlot_Type", "[PitchType] IN ('5_PERSON', '7_PERSON', '11_PERSON')");
                    table.ForeignKey(
                        name: "FK_PriceSlots_Pitches_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitches",
                        principalColumn: "PitchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.CheckConstraint("CK_Bookings_PaymentStatus", "[PaymentStatus] IN ('UNPAID', 'PAID', 'REFUNDED')");
                    table.CheckConstraint("CK_Bookings_Status", "[Status] IN ('PENDING', 'CONFIRMED', 'COMPLETED', 'CANCELLED', 'REJECTED')");
                    table.ForeignKey(
                        name: "FK_Bookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffPitchAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    PitchId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPitchAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffPitchAssignments_Pitches_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitches",
                        principalColumn: "PitchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffPitchAssignments_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffShifts",
                columns: table => new
                {
                    ShiftId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    PitchId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShifts", x => x.ShiftId);
                    table.CheckConstraint("CK_StaffShift_DayOfWeek", "[DayOfWeek] BETWEEN 1 AND 7");
                    table.CheckConstraint("CK_StaffShift_Time", "[StartTime] < [EndTime]");
                    table.ForeignKey(
                        name: "FK_StaffShifts_Pitches_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitches",
                        principalColumn: "PitchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StaffShifts_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    WalletId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                    table.CheckConstraint("CK_Wallets_Balance", "[Balance] >= 0");
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingDetails",
                columns: table => new
                {
                    DetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    PitchId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    PlayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    DetailStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingDetails", x => x.DetailId);
                    table.CheckConstraint("CK_BookingDetails_Duration", "[DurationMinutes] IN (60, 90, 120)");
                    table.CheckConstraint("CK_BookingDetails_Status", "[DetailStatus] IN ('PENDING', 'CONFIRMED', 'CANCELLED', 'COMPLETED', 'REJECTED')");
                    table.ForeignKey(
                        name: "FK_BookingDetails_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingDetails_Pitches_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitches",
                        principalColumn: "PitchId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookingDetails_Users_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.CheckConstraint("CK_Transactions_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_Transactions_Direction", "[Direction] IN ('CREDIT', 'DEBIT')");
                    table.CheckConstraint("CK_Transactions_Status", "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'REVERSED')");
                    table.CheckConstraint("CK_Transactions_Type", "[TransactionType] IN ('TOP_UP', 'BOOKING_PAYMENT', 'REFUND', 'WITHDRAWAL', 'ADJUSTMENT')");
                    table.ForeignKey(
                        name: "FK_Transactions_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Transactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingDetailId = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ChangedBy = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistory_BookingDetails_BookingDetailId",
                        column: x => x.BookingDetailId,
                        principalTable: "BookingDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingStatusHistory_Users_ChangedBy",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GatewayResponseData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.CheckConstraint("CK_Payments_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_Payments_Method", "[PaymentMethod] IN ('WALLET', 'CASH', 'BANK_TRANSFER', 'VNPAY', 'MOMO', 'ZALOPAY')");
                    table.CheckConstraint("CK_Payments_Status", "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'CANCELLED', 'REFUNDED', 'PARTIALLY_REFUNDED')");
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Payments_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TopUpRequests",
                columns: table => new
                {
                    TopupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProofImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    ConfirmedBy = table.Column<int>(type: "int", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopUpRequests", x => x.TopupId);
                    table.CheckConstraint("CK_TopUp_Amount", "[Amount] > 0");
                    table.CheckConstraint("CK_TopUp_Method", "[PaymentMethod] IN ('BANK_TRANSFER', 'VNPAY', 'MOMO', 'ZALOPAY')");
                    table.CheckConstraint("CK_TopUp_Status", "[Status] IN ('PENDING', 'SUCCESS', 'FAILED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "FK_TopUpRequests_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TopUpRequests_Users_ConfirmedBy",
                        column: x => x.ConfirmedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TopUpRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TopUpRequests_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    RefundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    BookingDetailId = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(15,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefundMethod = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedBy = table.Column<int>(type: "int", nullable: false),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayRefundId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.RefundId);
                    table.CheckConstraint("CK_Refunds_Amount", "[RefundAmount] > 0");
                    table.CheckConstraint("CK_Refunds_Method", "[RefundMethod] IS NULL OR [RefundMethod] IN ('WALLET', 'BANK_TRANSFER', 'ORIGINAL_METHOD')");
                    table.CheckConstraint("CK_Refunds_Status", "[Status] IN ('PENDING', 'APPROVED', 'PROCESSING', 'COMPLETED', 'REJECTED')");
                    table.ForeignKey(
                        name: "FK_Refunds_BookingDetails_BookingDetailId",
                        column: x => x.BookingDetailId,
                        principalTable: "BookingDetails",
                        principalColumn: "DetailId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Refunds_Users_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Users_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Pitches",
                columns: new[] { "PitchId", "CreatedAt", "Description", "ImageUrl", "PitchName", "PitchType", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sân 5 người cỏ nhân tạo mới thay", null, "Sân 5A (Cỏ mới)", "5_PERSON", "ACTIVE" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sân 5 người, đá không sợ mưa", null, "Sân 5B (Có mái che)", "5_PERSON", "ACTIVE" },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sân 7 người kích thước chuẩn", null, "Sân 7A (Tiêu chuẩn)", "7_PERSON", "ACTIVE" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sân đang thay lưới và đèn", null, "Sân 7B (Bảo trì)", "7_PERSON", "MAINTENANCE" },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sân 11 người phục vụ giải đấu", null, "Sân 11 Lớn", "11_PERSON", "ACTIVE" }
                });

            migrationBuilder.InsertData(
                table: "RefundPolicies",
                columns: new[] { "PolicyId", "CancelBeforeHours", "CreatedAt", "Description", "IsActive", "RefundPercentage" },
                values: new object[,]
                {
                    { 1, 72, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy trước 3 ngày (72h) - Hoàn 100%", true, 100.00m },
                    { 2, 48, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy trước 2 ngày (48h) - Hoàn 75%", true, 75.00m },
                    { 3, 24, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy trước 1 ngày (24h) - Hoàn 50%", true, 50.00m },
                    { 4, 12, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy trước 12 tiếng - Hoàn 20%", true, 20.00m },
                    { 5, 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hủy sát giờ (dưới 6 tiếng) - Không hoàn tiền", true, 0.00m }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "Password", "Phone", "Role" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@football.com", "Admin Quản Trị", true, "$2a$11$IcrPPnD8wX2UaRxXKadYfOUAo7t7snZV914CKUKx40QdEgO30H68W", "0901111111", "ADMIN" },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff1@football.com", "Nguyễn Văn A (Nhân viên 1)", true, "$2a$11$IcrPPnD8wX2UaRxXKadYfOUAo7t7snZV914CKUKx40QdEgO30H68W", "0902222222", "STAFF" },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "staff2@football.com", "Trần Thị B (Nhân viên 2)", true, "$2a$11$IcrPPnD8wX2UaRxXKadYfOUAo7t7snZV914CKUKx40QdEgO30H68W", "0903333333", "STAFF" },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer1@gmail.com", "Lê Khách C", true, "$2a$11$IcrPPnD8wX2UaRxXKadYfOUAo7t7snZV914CKUKx40QdEgO30H68W", "0904444444", "CUSTOMER" },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "customer2@gmail.com", "Phạm Khách D", true, "$2a$11$IcrPPnD8wX2UaRxXKadYfOUAo7t7snZV914CKUKx40QdEgO30H68W", "0905555555", "CUSTOMER" }
                });

            migrationBuilder.InsertData(
                table: "PriceSlots",
                columns: new[] { "PriceSlotId", "ApplyOn", "EndTime", "IsPeakHour", "PitchId", "PitchType", "PricePerHour", "StartTime" },
                values: new object[,]
                {
                    { 1, "WEEKDAY", new TimeSpan(0, 16, 0, 0, 0), false, 1, "5_PERSON", 200000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, "WEEKDAY", new TimeSpan(0, 22, 0, 0, 0), true, 1, "5_PERSON", 350000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 3, "WEEKEND", new TimeSpan(0, 16, 0, 0, 0), false, 1, "5_PERSON", 250000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 4, "WEEKEND", new TimeSpan(0, 22, 0, 0, 0), true, 1, "5_PERSON", 400000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 5, "WEEKDAY", new TimeSpan(0, 16, 0, 0, 0), false, 2, "5_PERSON", 220000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 6, "WEEKDAY", new TimeSpan(0, 22, 0, 0, 0), true, 2, "5_PERSON", 370000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 7, "WEEKEND", new TimeSpan(0, 16, 0, 0, 0), false, 2, "5_PERSON", 270000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 8, "WEEKEND", new TimeSpan(0, 22, 0, 0, 0), true, 2, "5_PERSON", 420000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 9, "WEEKDAY", new TimeSpan(0, 16, 0, 0, 0), false, 3, "7_PERSON", 400000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 10, "WEEKDAY", new TimeSpan(0, 22, 0, 0, 0), true, 3, "7_PERSON", 600000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 11, "WEEKEND", new TimeSpan(0, 16, 0, 0, 0), false, 3, "7_PERSON", 450000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 12, "WEEKEND", new TimeSpan(0, 22, 0, 0, 0), true, 3, "7_PERSON", 700000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 13, "WEEKDAY", new TimeSpan(0, 16, 0, 0, 0), false, 4, "7_PERSON", 380000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 14, "WEEKDAY", new TimeSpan(0, 22, 0, 0, 0), true, 4, "7_PERSON", 580000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 15, "WEEKEND", new TimeSpan(0, 16, 0, 0, 0), false, 4, "7_PERSON", 430000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 16, "WEEKEND", new TimeSpan(0, 22, 0, 0, 0), true, 4, "7_PERSON", 680000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 17, "WEEKDAY", new TimeSpan(0, 16, 0, 0, 0), false, 5, "11_PERSON", 800000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 18, "WEEKDAY", new TimeSpan(0, 22, 0, 0, 0), true, 5, "11_PERSON", 1200000m, new TimeSpan(0, 16, 0, 0, 0) },
                    { 19, "WEEKEND", new TimeSpan(0, 16, 0, 0, 0), false, 5, "11_PERSON", 900000m, new TimeSpan(0, 6, 0, 0, 0) },
                    { 20, "WEEKEND", new TimeSpan(0, 22, 0, 0, 0), true, 5, "11_PERSON", 1400000m, new TimeSpan(0, 16, 0, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "StaffShifts",
                columns: new[] { "ShiftId", "CreatedAt", "DayOfWeek", "EndTime", "IsActive", "PitchId", "StaffId", "StartTime" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, new TimeSpan(0, 14, 0, 0, 0), true, 1, 2, new TimeSpan(0, 6, 0, 0, 0) },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, new TimeSpan(0, 22, 0, 0, 0), true, 2, 2, new TimeSpan(0, 14, 0, 0, 0) },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, new TimeSpan(0, 14, 0, 0, 0), true, 3, 3, new TimeSpan(0, 6, 0, 0, 0) },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 7, new TimeSpan(0, 22, 0, 0, 0), true, 5, 3, new TimeSpan(0, 16, 0, 0, 0) },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, new TimeSpan(0, 16, 0, 0, 0), true, 1, 2, new TimeSpan(0, 8, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Booking_Conflict",
                table: "BookingDetails",
                columns: new[] { "PitchId", "PlayDate", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_BookingId",
                table: "BookingDetails",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingDetails_StaffId",
                table: "BookingDetails",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_UserId",
                table: "Bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistory_BookingDetailId",
                table: "BookingStatusHistory",
                column: "BookingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingStatusHistory_ChangedBy",
                table: "BookingStatusHistory",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_Token",
                table: "PasswordResetTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IDX_Payments_Booking",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IDX_Payments_Gateway",
                table: "Payments",
                column: "GatewayTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ConfirmedBy",
                table: "Payments",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
                table: "Payments",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSlots_PitchId",
                table: "PriceSlots",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IDX_Refunds_Payment",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IDX_Refunds_Status",
                table: "Refunds",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_BookingDetailId",
                table: "Refunds",
                column: "BookingDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_RequestedBy",
                table: "Refunds",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ReviewedBy",
                table: "Refunds",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_TransactionId",
                table: "Refunds",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPitchAssignments_PitchId",
                table: "StaffPitchAssignments",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPitchAssignments_StaffId_PitchId",
                table: "StaffPitchAssignments",
                columns: new[] { "StaffId", "PitchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IDX_StaffShift_Lookup",
                table: "StaffShifts",
                columns: new[] { "StaffId", "PitchId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffShifts_PitchId",
                table: "StaffShifts",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_TopUpRequests_ConfirmedBy",
                table: "TopUpRequests",
                column: "ConfirmedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TopUpRequests_TransactionId",
                table: "TopUpRequests",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TopUpRequests_UserId",
                table: "TopUpRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TopUpRequests_WalletId",
                table: "TopUpRequests",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IDX_Transactions_Booking",
                table: "Transactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IDX_Transactions_Ref",
                table: "Transactions",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IDX_Transactions_Wallet",
                table: "Transactions",
                columns: new[] { "WalletId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingStatusHistory");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PriceSlots");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RefundPolicies");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "StaffPitchAssignments");

            migrationBuilder.DropTable(
                name: "StaffShifts");

            migrationBuilder.DropTable(
                name: "TopUpRequests");

            migrationBuilder.DropTable(
                name: "BookingDetails");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Pitches");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
