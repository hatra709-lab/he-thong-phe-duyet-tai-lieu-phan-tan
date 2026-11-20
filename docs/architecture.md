
# KIẾN TRÚC HỆ THỐNG

## 1. Công nghệ
- C# và .NET 10
- ASP.NET Core Minimal API
- Blazor Web App
- PostgreSQL
- RabbitMQ
- .NET Worker Service
- Docker và Docker Compose

## 2. Các thành phần

### Document.Web
Giao diện người dùng dành cho nhân viên
và trưởng phòng.

Giao tiếp với Document.Api bằng HTTP REST.

### Document.Api
Xử lý nghiệp vụ tạo tài liệu,
gửi phê duyệt, phê duyệt và từ chối.

Lưu dữ liệu, lịch sử và phát sự kiện
khi trạng thái thay đổi.

### PostgreSQL
Lưu trữ:
- Departments
- Users
- Documents
- DocumentHistories
- Notifications

### RabbitMQ
Message Broker truyền sự kiện bất đồng bộ
từ Document.Api đến Notification.Service.

### Notification.Service
Nhận các sự kiện từ RabbitMQ và tạo
thông báo cho người dùng liên quan.

### Document.Contracts
Class Library chứa các định nghĩa
sự kiện dùng chung.

## 3. Luồng xử lý

1. Người dùng thao tác trên Document.Web.
2. Web gọi API bằng HTTP REST.
3. Document.Api xử lý nghiệp vụ.
4. Trạng thái và lịch sử lưu vào PostgreSQL.
5. API phát sự kiện đến RabbitMQ.
6. Notification.Service nhận sự kiện.
7. Notification.Service lưu thông báo.
8. Web lấy thông báo qua Document.Api.

## 4. Triển khai
Mỗi service .NET có Dockerfile riêng.
Docker Compose khởi chạy toàn hệ thống.

Các container giao tiếp bằng Docker network
và service name.

Cấu hình môi trường được tách khỏi mã nguồn.

## 5. Khả năng phục hồi
Sử dụng durable queue, persistent messages
và acknowledgement phù hợp với RabbitMQ.

Thử nghiệm phục hồi khi Notification.Service
tạm thời dừng hoạt động.
