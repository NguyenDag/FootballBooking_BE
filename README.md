# FootballBooking_BE

# Football Booking — EF Core Code First

## Cấu trúc thư mục

```
FootballBooking/
├── Entities/
│   ├── User.cs
│   ├── Pitch.cs
│   ├── StaffPitchAssignment.cs
│   ├── PriceSlot.cs
│   ├── Booking.cs
│   ├── BookingDetail.cs
│   ├── BookingStatusHistory.cs
│   ├── Wallet.cs
│   ├── Transaction.cs
│   ├── Payment.cs
│   ├── Refund.cs
│   ├── TopUpRequest.cs
│   └── RefundPolicy.cs
└── Data/
    └── AppDbContext.cs
```

---

## Cài đặt NuGet packages

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
```

---

## Đăng ký DbContext trong Program.cs

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=football_booking;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## Chạy Migration

```bash
# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate

# Áp dụng vào database
dotnet ef database update
```

---

## Lưu ý quan trọng

### DeleteBehavior được cấu hình:
| Quan hệ | Hành vi |
|---|---|
| Booking → BookingDetail | **Cascade** (xoá booking xoá luôn detail) |
| BookingDetail → StatusHistory | **Cascade** |
| User → Wallet | **Restrict** (không cho xoá user còn ví) |
| Payment → Transaction | **SetNull** |
| Refund → Transaction | **SetNull** |
| Staff → BookingDetail | **SetNull** (bỏ assign staff) |

### Enums — nên tạo constants class để tránh magic string:

```csharp
public static class UserRole
{
    public const string Admin    = "ADMIN";
    public const string Staff    = "STAFF";
    public const string Customer = "CUSTOMER";
}

public static class BookingStatus
{
    public const string Pending   = "PENDING";
    public const string Confirmed = "CONFIRMED";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";
    public const string Rejected  = "REJECTED";
}

public static class PaymentStatus
{
    public const string Unpaid   = "UNPAID";
    public const string Paid     = "PAID";
    public const string Refunded = "REFUNDED";
}

public static class TransactionDirection
{
    public const string Credit = "CREDIT";
    public const string Debit  = "DEBIT";
}
```