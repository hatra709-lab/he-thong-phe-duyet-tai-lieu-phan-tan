
# PHÂN TÍCH YÊU CẦU HỆ THỐNG

## 1. Tên đề tài
Hệ thống quản lý và phê duyệt tài liệu nội bộ
cho doanh nghiệp nhỏ.

## 2. Bối cảnh
Hệ thống được xây dựng cho một doanh nghiệp
giả lập có khoảng 20-50 nhân viên,
gồm 4 phòng ban:
- Kinh doanh
- Kế toán
- Nhân sự
- Hành chính

## 3. Mục tiêu
Số hóa quá trình tạo, gửi, phê duyệt và
theo dõi tài liệu nội bộ.

Ứng dụng kiến trúc phân tán, REST API và
RabbitMQ để xử lý sự kiện, gửi thông báo.

## 4. Vai trò

### Nhân viên (Employee)
- Tạo tài liệu.
- Xem tài liệu của mình.
- Gửi tài liệu để phê duyệt.
- Theo dõi trạng thái và lịch sử.
- Nhận thông báo.

### Trưởng phòng (Manager)
- Xem tài liệu được giao xử lý.
- Phê duyệt tài liệu.
- Từ chối tài liệu, nhập lý do.
- Xem lịch sử và nhận thông báo.

## 5. Quy trình nghiệp vụ
Hệ thống áp dụng một cấp phê duyệt:

Nhân viên -> Trưởng phòng -> Kết quả

Các trạng thái:
- Draft: Bản nháp.
- Submitted: Đang chờ phê duyệt.
- Approved: Đã phê duyệt.
- Rejected: Đã từ chối.

Quy tắc:
- Chỉ tài liệu Draft mới được gửi.
- Chỉ tài liệu Submitted mới được xử lý.
- Người được giao phê duyệt là trưởng phòng.
- Từ chối phải có lý do.
- Mỗi lần xử lý đều lưu lịch sử.
- Approved và Rejected là trạng thái kết thúc.

## 6. Chức năng chính
1. Tạo tài liệu.
2. Gửi phê duyệt.
3. Phê duyệt hoặc từ chối.
4. Lưu lịch sử xử lý.
5. Phát sự kiện khi trạng thái thay đổi.
6. Gửi thông báo đến người liên quan.
7. Tra cứu theo trạng thái hoặc người xử lý.
8. Quản lý bốn trạng thái bắt buộc.

## 7. API tối thiểu
POST /documents
GET /documents
GET /documents/{id}
POST /documents/{id}/submit
POST /documents/{id}/approve
POST /documents/{id}/reject

## 8. Giới hạn phạm vi
- Một cấp phê duyệt.
- Sử dụng dữ liệu người dùng mẫu.
- Tài liệu nhập qua biểu mẫu điện tử.
- Không triển khai ký số.
- Không triển khai phê duyệt nhiều cấp.
- Không yêu cầu upload file ở phiên bản đầu.
