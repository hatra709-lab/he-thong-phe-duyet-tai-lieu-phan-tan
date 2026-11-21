
using Document.Api.Data;
using Microsoft.EntityFrameworkCore;
using DocumentEntity = global::Document.Api.Models.Document;

var builder = WebApplication.CreateBuilder(args);

// Kết nối PostgreSQL
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

// API kiểm tra hoạt động
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
    // Kiểm tra dữ liệu đầu vào
    if (string.IsNullOrWhiteSpace(request.Title) ||
        request.Title.Length > 200)
    {
        return Results.BadRequest(new
        {
            Message = "Tiêu đề không hợp lệ (tối đa 200 ký tự)."
        });
    }

    if (string.IsNullOrWhiteSpace(request.DocumentType) ||
        request.DocumentType.Length > 50)
    {
        return Results.BadRequest(new
        {
            Message = "Loại tài liệu không hợp lệ."
        });
    }

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

    // Lưu vào PostgreSQL
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
        query = query.Where(d => d.Status == status);
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

app.Run();

// Dữ liệu client gửi đến khi tạo tài liệu
public record CreateDocumentRequest(
    string? Title,
    string? DocumentType,
    string? Content,
    int CreatedById
);
