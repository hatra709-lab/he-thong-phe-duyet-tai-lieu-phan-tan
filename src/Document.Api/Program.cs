
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Trang kiểm tra API
app.MapGet("/", () => "Document API is running!");

// Endpoint kiểm tra trạng thái service
app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Service = "Document.Api",
        Status = "Healthy",
        Message = "Document Approval System API is running"
    });
});

app.Run();
