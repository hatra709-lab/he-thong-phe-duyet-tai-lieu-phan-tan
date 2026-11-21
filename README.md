
# Document Approval System

Hệ thống quản lý và phê duyệt tài liệu
nội bộ cho doanh nghiệp nhỏ.

## Công nghệ
- .NET 10
- ASP.NET Core Minimal API
- Blazor Web App
- PostgreSQL
- RabbitMQ
- Docker Compose

## Các project
- Document.Api
- Document.Web
- Notification.Service
- Document.Contracts

## Khởi chạy API tuần 1

Build Docker image:

docker build -t document-api-week1 -f src/Document.Api/Dockerfile .

Chạy container:

docker run --rm -p 5200:5200 document-api-week1

Kiểm tra:
http://localhost:5200/health

## Tài liệu
- docs/requirements.md
- docs/architecture.md
- docs/weekly-progress.md

## Tiến độ
Tuần 1: Khởi tạo hệ thống.

## Tác giả
Sinh viên thực hiện bài tập lớn
môn Phát triển ứng dụng phân tán.


## Tiến độ tuần 2

### Cơ sở dữ liệu
Sử dụng PostgreSQL 17 với 5 bảng:
- Departments
- Users
- Documents
- DocumentHistories
- Notifications

### API đã hoàn thành
- POST /documents
- GET /documents
- GET /documents/{id}

GET /documents hỗ trợ lọc:
- status
- createdById
- approverId

### Chạy hệ thống

Tạo file .env từ .env.example
và cấu hình thông tin PostgreSQL.

Chạy API và PostgreSQL:

```powershell
docker compose up -d --build api
```

Kiểm tra container:

```powershell
docker compose ps
```

Kiểm tra API:

http://localhost:5200/health

Xem danh sách tài liệu:

http://localhost:5200/documents

### Kiểm tra PostgreSQL

```powershell
docker compose exec postgres psql -U user -d document_approval_db
```

Bên trong PostgreSQL:

```sql
\dt
```

### Ghi chú
Cần áp dụng EF Core Migration trước khi
chạy API trên database mới.

Hiện tại hệ thống dùng người dùng mẫu,
chưa triển khai xác thực đăng nhập.
