
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
