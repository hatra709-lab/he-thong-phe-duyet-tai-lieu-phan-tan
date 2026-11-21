
# NHẬT KÝ PHÁT TRIỂN DỰ ÁN

## Tuần 1 - Khởi tạo hệ thống

### Mục tiêu
Tạo bộ khung dự án .NET 10 và hoàn thành
phân tích yêu cầu, kiến trúc ban đầu.

### Công việc
- [ ] Tạo .NET Solution.
- [ ] Tạo Document.Api.
- [ ] Tạo Document.Web.
- [ ] Tạo Notification.Service.
- [ ] Tạo Document.Contracts.
- [ ] Thiết lập Project References.
- [ ] Xây dựng API /health.
- [ ] Chạy API bằng Docker.
- [ ] Build Solution thành công.
- [ ] Viết requirements.md.
- [ ] Viết architecture.md.
- [ ] Tạo README.md.
- [ ] Commit và push GitHub.

### Kết quả
Cập nhật sau khi hoàn thành tuần 1.

### Vấn đề gặp phải
Windows Application Control chặn
file .exe và .dll được build cục bộ.

Giải pháp: build và chạy ứng dụng
bên trong Linux container Docker.

### Tuần 2
Thiết kế PostgreSQL và xây dựng
API quản lý tài liệu cơ bản.


## Tuần 2 - PostgreSQL và Document API

### 1. Mục tiêu
Xây dựng cơ sở dữ liệu PostgreSQL và
3 API đầu tiên để quản lý tài liệu.

### 2. Công việc đã thực hiện
- [x] Chạy PostgreSQL 17 bằng Docker Compose.
- [x] Cấu hình kết nối database.
- [x] Thiết kế 5 Entity.
- [x] Xây dựng AppDbContext.
- [x] Cài đặt Entity Framework Core.
- [x] Tạo Migration InitialCreate.
- [x] Tạo 5 bảng trong PostgreSQL.
- [x] Thêm 4 phòng ban mẫu.
- [x] Thêm 8 người dùng mẫu.
- [x] Xây dựng POST /documents.
- [x] Xây dựng GET /documents.
- [x] Xây dựng GET /documents/{id}.
- [x] Kiểm tra lọc theo trạng thái và người tạo.
- [x] Kiểm tra lưu trữ dữ liệu trong PostgreSQL.
- [x] Kiểm tra dữ liệu sau khi restart API.

### 3. Kết quả
Hệ thống đã tạo thành công tài liệu:

- ID: 1
- Tiêu đề: Đề xuất mua máy tính
- Người tạo: Nguyễn Văn An (ID 2)
- Người duyệt: Trần Văn Bình (ID 1)
- Trạng thái: Draft

Dữ liệu được lưu trong PostgreSQL
và vẫn tồn tại sau khi khởi động lại API.

### 4. Công nghệ sử dụng
- .NET 10
- ASP.NET Core Minimal API
- Entity Framework Core 10
- PostgreSQL 17
- Docker Compose

### 5. Khó khăn và cách khắc phục
Khi tạo Migration trong Docker, xuất hiện lỗi
do đường dẫn NuGet được sinh trên Windows.

Giải pháp:
Tách thư mục bin/obj giữa Windows và
Linux container, sau đó restore package
trong Docker.

### 6. Kế hoạch tuần 3
- Xây dựng API gửi tài liệu.
- Xây dựng API phê duyệt.
- Xây dựng API từ chối.
- Kiểm tra quy tắc chuyển trạng thái.
- Lưu lịch sử xử lý tài liệu.
