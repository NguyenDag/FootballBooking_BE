# FootballBooking_BE (Backend System)

## Giới thiệu chung
**FootballBooking_BE** là hệ thống Backend cung cấp các API quản lý và đặt lịch sân bóng đá nhân tạo. Hệ thống được xây dựng dựa trên kiến trúc phân tầng chuẩn mực, cung cấp giải pháp toàn diện cho:
- **Khách hàng (Customer)**: Đăng ký, đăng nhập, tìm kiếm sân, đặt lịch và thanh toán.
- **Nhân viên (Staff)**: Được phân công quản lý sân, quản lý ca làm việc, xử lý (Xác nhận/Từ chối) các yêu cầu đặt sân.
- **Quản trị viên (Admin)**: Quản lý toàn bộ nhân viên, sân bóng, phân quyền, phân công ca làm và xem thống kê.

Dự án được phát triển trên nền tảng **.NET Core (C#)** kết hợp **Entity Framework Core (Code-First)** và cơ sở dữ liệu **SQL Server**.

---

## Kiến trúc & Công nghệ sử dụng
- **Framework**: .NET Core (ASP.NET Core Web API)
- **Database ORM**: Entity Framework Core
- **Database**: SQL Server
- **Authentication/Authorization**: 
  - JWT (JSON Web Token) kết hợp Refresh Token (Rotation mechanism).
  - Phân quyền theo Role-based Access Control (RBAC): `ADMIN`, `STAFF`, `CUSTOMER`.
- **Bảo mật**: Sử dụng `BCrypt` để hash mật khẩu.
- **Tài liệu API**: Tích hợp Swagger / OpenAPI.

---

## Cấu trúc thư mục lõi
Dự án áp dụng chặt chẽ kiến trúc **N-Tier (N-Layers)** để đảm bảo tính dễ bảo trì và mở rộng:

```
FootballBooking_BE/
├── Controllers/       # HTTP API Endpoints (Routing, Request/Response handling)
├── Services/          # Lớp Business Logic (Xử lý nghiệp vụ lõi)
│   ├── Interfaces/
│   └── Implementations/
├── Repositories/      # Lớp Data Access (Tương tác trực tiếp với Database)
│   ├── Interfaces/
│   └── Implementations/
├── Models/            # Data Transfer Objects (DTOs) & ViewModels
├── Data/              # DbContext & Entities (Domain Models)
├── Middleware/        # Global Exception Handling & Custom Middlewares
└── Common/            # Tiện ích dùng chung (Constants, Enums, ApiResponse structure)
```

---

## Các tính năng nổi bật & Cải tiến gần đây

### 1. Hệ thống xác thực bảo mật cao (Auth System)
- Đăng nhập/Đăng ký tài khoản với tính năng kiểm tra chuẩn hoá email (chống phân biệt hoa/thường sai lệch).
- **Refresh Token Rotation**: Chống đánh cắp token. Hệ thống sẽ cấp lại Refresh Token mới mỗi khi xin lại Access Token và đồng thời xoá Token cũ.
- So khớp xác thực chéo `UserId` giữa AccessToken và RefreshToken để ngăn chặn lỗ hổng giả mạo Token.
- Hỗ trợ thu hồi Token của tất cả các thiết bị khi người dùng đổi mật khẩu.

### 2. Quản lý Nhân sự & Ca làm việc (Staff Management)
- **Soft Delete**: Vô hiệu hoá nhân viên thay vì xoá cứng để giữ lại lịch sử công việc.
- **Thuật toán xếp ca**: Ngăn chặn tuyệt đối việc xếp trùng ca làm việc cho cùng một khung giờ/sân bóng đối với mọi nhân viên (Anti-Overlap Shift Algorithm).
- **Phân tách quyền hạn**: Nhân viên chỉ được phép Xác nhận (Confirm) hoặc Bỏ qua/Từ chối (Reject) những Booking nằm đúng trong ca làm việc và sân mà họ được phân công.

### 3. Quy trình Đặt sân (Booking Flow)
- Liên kết huỷ tự động: Khi tât cả các chi tiết của một Booking bị từ chối, hệ thống tự động đồng bộ trạng thái đơn hàng tổng (Parent Booking) về `CANCELLED`.
- Cấu hình **Cascade Delete** thông minh, tránh lỗi rác dữ liệu:
  - Xoá Booking -> Xoá BookingDetail -> Xoá StatusHistory.
  - Xoá Nhân viên -> Đặt `Null` cho StaffId trong Booking thay vì xoá Booking.

---

## Hướng dẫn cài đặt và khởi chạy (Local)

### Yêu cầu hệ thống:
- .NET SDK (phiên bản tương ứng với project)
- SQL Server (Local or Docker)

### Cấu hình Database (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "MyCnn": "Server=localhost;Database=football_booking;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "YOUR_SUPER_SECRET_KEY_HERE",
    "Issuer": "FootballBookingApp",
    "Audience": "FootballBookingUsers",
    "AccessTokenExpiryMinutes": "60",
    "RefreshTokenExpiryDays": "7"
  }
}
```

### Chạy Migration & Update CSDL:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Chạy dự án:
Mở Terminal tại thư mục `FootballBooking_BE` và chạy lệnh:
```bash
dotnet run
```
Truy cập giao diện Swagger để kiểm thử API tại: `https://localhost:<port>/swagger`