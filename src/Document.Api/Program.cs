
using Document.Api.Models;
using Document.Api.Data;
using Microsoft.EntityFrameworkCore;
using DocumentEntity = global::Document.Api.Models.Document;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CẤU HÌNH KẾT NỐI POSTGRESQL
// ========================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing DefaultConnection configuration.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Thêm dữ liệu mẫu khi được bật
if (app.Configuration.GetValue<bool>("SeedDemoData"))
{
    await DbSeeder.SeedAsync(app.Services);
}

// ========================================
// 2. API KIỂM TRA HỆ THỐNG
// ========================================

app.MapGet("/", () => "Document API is running!");

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Service = "Document.Api",
        Status = "Healthy"
    });
});

// ========================================
// API 1: TẠO TÀI LIỆU
// POST /documents
// ========================================

app.MapPost("/documents", async (
    CreateDocumentRequest request,
    AppDbContext db) =>
{
    // Kiểm tra tiêu đề
    if (string.IsNullOrWhiteSpace(request.Title) ||
        request.Title.Length > 200)
    {
        return Results.BadRequest(new
        {
            Message = "Tiêu đề không hợp lệ (tối đa 200 ký tự)."
        });
    }

    // Kiểm tra loại tài liệu
    if (string.IsNullOrWhiteSpace(request.DocumentType) ||
        request.DocumentType.Length > 50)
    {
        return Results.BadRequest(new
        {
            Message = "Loại tài liệu không hợp lệ."
        });
    }

    // Kiểm tra nội dung
    if (string.IsNullOrWhiteSpace(request.Content))
    {
        return Results.BadRequest(new
        {
            Message = "Nội dung không được để trống."
        });
    }

    // Tìm nhân viên tạo tài liệu
    var employee = await db.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u =>
            u.Id == request.CreatedById);

    if (employee == null)
    {
        return Results.BadRequest(new
        {
            Message = "Không tìm thấy người tạo tài liệu."
        });
    }

    // Chỉ Employee được tạo tài liệu
    if (employee.Role != "Employee")
    {
        return Results.BadRequest(new
        {
            Message = "Người tạo phải có vai trò Employee."
        });
    }

    // Tìm trưởng phòng cùng phòng ban
    var manager = await db.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u =>
            u.DepartmentId == employee.DepartmentId &&
            u.Role == "Manager");

    if (manager == null)
    {
        return Results.Conflict(new
        {
            Message = "Phòng ban chưa có trưởng phòng."
        });
    }

    var now = DateTime.UtcNow;

    // Tạo tài liệu ở trạng thái Draft
    var document = new DocumentEntity
    {
        Title = request.Title.Trim(),
        DocumentType = request.DocumentType.Trim(),
        Content = request.Content.Trim(),
        CreatedById = employee.Id,
        ApproverId = manager.Id,
        Status = "Draft",
        CreatedAt = now,
        UpdatedAt = now
    };

    // Lưu tài liệu vào PostgreSQL
    db.Documents.Add(document);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/documents/{document.Id}",
        document);
});

// ========================================
// API 2: LẤY DANH SÁCH TÀI LIỆU
// GET /documents
// ========================================

app.MapGet("/documents", async (
    AppDbContext db,
    string? status,
    int? createdById,
    int? approverId) =>
{
    var query = db.Documents
        .AsNoTracking()
        .AsQueryable();

    // Lọc theo trạng thái
    if (!string.IsNullOrWhiteSpace(status))
    {
        query = query.Where(d =>
            d.Status == status);
    }

    // Lọc theo người tạo
    if (createdById.HasValue)
    {
        query = query.Where(d =>
            d.CreatedById == createdById.Value);
    }

    // Lọc theo người phê duyệt
    if (approverId.HasValue)
    {
        query = query.Where(d =>
            d.ApproverId == approverId.Value);
    }

    var documents = await query
        .OrderByDescending(d => d.CreatedAt)
        .ToListAsync();

    return Results.Ok(documents);
});

// ========================================
// API 3: XEM CHI TIẾT TÀI LIỆU
// GET /documents/{id}
// ========================================

app.MapGet("/documents/{id:int}", async (
    int id,
    AppDbContext db) =>
{
    var document = await db.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
    {
        return Results.NotFound(new
        {
            Message = "Không tìm thấy tài liệu."
        });
    }

    return Results.Ok(document);
});

// ========================================
// API 4: GỬI TÀI LIỆU PHÊ DUYỆT
// POST /documents/{id}/submit
// TUẦN 3
// ========================================

app.MapPost("/documents/{id:int}/submit", async (
    int id,
    SubmitDocumentRequest request,
    AppDbContext db) =>
{
    // Bước 1: Tìm tài liệu
    var document = await db.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
    {
        return Results.NotFound(new
        {
            Message = "Không tìm thấy tài liệu."
        });
    }

    // Bước 2: Kiểm tra người gửi
    if (document.CreatedById != request.ActorId)
    {
        return Results.Json(new
        {
            Message = "Chỉ người tạo mới được gửi tài liệu."
        }, statusCode: 403);
    }

    // Bước 3: Kiểm tra trạng thái
    if (document.Status != "Draft")
    {
        return Results.Conflict(new
        {
            Message = "Chỉ tài liệu Draft mới được gửi."
        });
    }

    var now = DateTime.UtcNow;

    // Bước 4: Bắt đầu transaction
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    // Bước 5: Chuyển Draft -> Submitted
    // Chỉ cập nhật khi trạng thái vẫn là Draft
    var updated = await db.Documents
        .Where(d =>
            d.Id == id &&
            d.Status == "Draft")
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(d => d.Status, "Submitted")
            .SetProperty(d => d.UpdatedAt, now));

    if (updated == 0)
    {
        return Results.Conflict(new
        {
            Message = "Trạng thái tài liệu đã thay đổi."
        });
    }

    // Bước 6: Ghi lịch sử xử lý
    var history = new DocumentHistory
    {
        DocumentId = id,
        PerformedById = request.ActorId,
        Action = "Submitted",
        Comment = "Gửi tài liệu phê duyệt",
        CreatedAt = now
    };

    db.DocumentHistories.Add(history);

    // Bước 7: Lưu lịch sử và hoàn tất transaction
    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    return Results.Ok(new
    {
        DocumentId = id,
        Status = "Submitted",
        Message = "Gửi tài liệu thành công."
    });
});

// ========================================
// API 5: XEM LỊCH SỬ XỬ LÝ TÀI LIỆU
// GET /documents/{id}/history
// TUẦN 3
// ========================================

app.MapGet("/documents/{id:int}/history", async (
    int id,
    AppDbContext db) =>
{
    // 1. Kiểm tra tài liệu có tồn tại
    var documentExists = await db.Documents
        .AnyAsync(d => d.Id == id);

    if (!documentExists)
    {
        return Results.NotFound(new
        {
            Message = "Không tìm thấy tài liệu."
        });
    }

    // 2. Lấy lịch sử xử lý theo thời gian
    var histories = await db.DocumentHistories
        .AsNoTracking()
        .Where(h => h.DocumentId == id)
        .OrderBy(h => h.CreatedAt)
        .ThenBy(h => h.Id)
        .ToListAsync();

    // 3. Trả về danh sách lịch sử
    return Results.Ok(histories);
});


 // ========================================
 // API 6: PHÊ DUYỆT TÀI LIỆU
 // POST /documents/{id}/approve
 // ========================================

app.MapPost("/documents/{id:int}/approve", async (
    int id,
    ApproveDocumentRequest request,
    AppDbContext db) =>
{
    // 1. Tìm tài liệu
    var document = await db.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
    {
        return Results.NotFound(new
        {
            Message = "Không tìm thấy tài liệu."
        });
    }

    // 2. Kiểm tra người được chỉ định phê duyệt
    if (document.ApproverId != request.ActorId)
    {
        return Results.Json(new
        {
            Message = "Bạn không phải người được chỉ định phê duyệt."
        }, statusCode: 403);
    }

    // 3. Kiểm tra vai trò trưởng phòng
    var isManager = await db.Users.AnyAsync(u =>
        u.Id == request.ActorId &&
        u.Role == "Manager");

    if (!isManager)
    {
        return Results.Json(new
        {
            Message = "Chỉ trưởng phòng mới được phê duyệt."
        }, statusCode: 403);
    }

    // 4. Kiểm tra trạng thái
    if (document.Status != "Submitted")
    {
        return Results.Conflict(new
        {
            Message = "Chỉ tài liệu Submitted mới được phê duyệt."
        });
    }

    var now = DateTime.UtcNow;

    // 5. Bắt đầu transaction
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    // 6. Chuyển Submitted -> Approved
    var updated = await db.Documents
        .Where(d =>
            d.Id == id &&
            d.Status == "Submitted" &&
            d.ApproverId == request.ActorId)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(d => d.Status, "Approved")
            .SetProperty(d => d.UpdatedAt, now));

    if (updated == 0)
    {
        return Results.Conflict(new
        {
            Message = "Trạng thái tài liệu đã thay đổi."
        });
    }

    // 7. Ghi lịch sử phê duyệt
    db.DocumentHistories.Add(new DocumentHistory
    {
        DocumentId = id,
        PerformedById = request.ActorId,
        Action = "Approved",
        Comment = "Trưởng phòng đã phê duyệt tài liệu",
        CreatedAt = now
    });

    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    return Results.Ok(new
    {
        DocumentId = id,
        Status = "Approved",
        Message = "Phê duyệt tài liệu thành công."
    });
});


// ========================================
// API 7: TỪ CHỐI TÀI LIỆU
// POST /documents/{id}/reject
// TUẦN 3
// ========================================

app.MapPost("/documents/{id:int}/reject", async (
    int id,
    RejectDocumentRequest request,
    AppDbContext db) =>
{
    // 1. Tìm tài liệu
    var document = await db.Documents
        .AsNoTracking()
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
    {
        return Results.NotFound(new
        {
            Message = "Không tìm thấy tài liệu."
        });
    }

    // 2. Kiểm tra người được chỉ định xử lý
    if (document.ApproverId != request.ActorId)
    {
        return Results.Json(new
        {
            Message = "Bạn không phải người được chỉ định xử lý."
        }, statusCode: 403);
    }

    // 3. Kiểm tra vai trò Manager
    var isManager = await db.Users.AnyAsync(u =>
        u.Id == request.ActorId &&
        u.Role == "Manager");

    if (!isManager)
    {
        return Results.Json(new
        {
            Message = "Chỉ trưởng phòng mới được từ chối tài liệu."
        }, statusCode: 403);
    }

    // 4. Chỉ tài liệu Submitted mới được từ chối
    if (document.Status != "Submitted")
    {
        return Results.Conflict(new
        {
            Message = "Chỉ tài liệu Submitted mới được từ chối."
        });
    }

    // 5. Bắt buộc nhập lý do từ chối
    if (string.IsNullOrWhiteSpace(request.Reason) ||
        request.Reason.Length > 200)
    {
        return Results.BadRequest(new
        {
            Message = "Lý do từ chối phải có nội dung và tối đa 200 ký tự."
        });
    }

    var now = DateTime.UtcNow;

    // 6. Bắt đầu transaction
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    // 7. Chuyển Submitted -> Rejected
    var updated = await db.Documents
        .Where(d =>
            d.Id == id &&
            d.Status == "Submitted" &&
            d.ApproverId == request.ActorId)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(d => d.Status, "Rejected")
            .SetProperty(d => d.UpdatedAt, now));

    if (updated == 0)
    {
        return Results.Conflict(new
        {
            Message = "Trạng thái tài liệu đã thay đổi."
        });
    }

    // 8. Ghi lịch sử từ chối
    db.DocumentHistories.Add(new DocumentHistory
    {
        DocumentId = id,
        PerformedById = request.ActorId,
        Action = "Rejected",
        Comment = request.Reason.Trim(),
        CreatedAt = now
    });

    // 9. Lưu lịch sử và hoàn tất transaction
    await db.SaveChangesAsync();
    await transaction.CommitAsync();

    return Results.Ok(new
    {
        DocumentId = id,
        Status = "Rejected",
        Reason = request.Reason.Trim(),
        Message = "Từ chối tài liệu thành công."
    });
});


// ========================================
// KHỞI CHẠY ỨNG DỤNG
// ========================================


app.Run();

// ========================================
// CÁC REQUEST DTO
// ========================================

// Request tạo tài liệu (tuần 2)
public record CreateDocumentRequest(
    string? Title,
    string? DocumentType,
    string? Content,
    int CreatedById
);

// Request gửi tài liệu phê duyệt (tuần 3)
public record SubmitDocumentRequest(
    int ActorId
);

public record ApproveDocumentRequest(
    int ActorId
);

// Request từ chối tài liệu
public record RejectDocumentRequest(
    int ActorId,
    string? Reason
);
