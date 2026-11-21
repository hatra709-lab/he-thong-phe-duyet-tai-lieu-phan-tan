
using Document.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Document.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        // 1. Thêm phòng ban mẫu
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Name = "Kinh doanh" },
                new Department { Name = "Kế toán" },
                new Department { Name = "Nhân sự" },
                new Department { Name = "Hành chính" }
            );

            await db.SaveChangesAsync();
        }

        // 2. Thêm nhân viên và trưởng phòng mẫu
        if (!await db.Users.AnyAsync())
        {
            var departments = await db.Departments
                .ToDictionaryAsync(d => d.Name, d => d.Id);

            db.Users.AddRange(
                // Phòng Kinh doanh
                new User
                {
                    FullName = "Trần Văn Bình",
                    Role = "Manager",
                    DepartmentId = departments["Kinh doanh"]
                },
                new User
                {
                    FullName = "Nguyễn Văn An",
                    Role = "Employee",
                    DepartmentId = departments["Kinh doanh"]
                },

                // Phòng Kế toán
                new User
                {
                    FullName = "Lê Thị Hương",
                    Role = "Manager",
                    DepartmentId = departments["Kế toán"]
                },
                new User
                {
                    FullName = "Phạm Văn Nam",
                    Role = "Employee",
                    DepartmentId = departments["Kế toán"]
                },

                // Phòng Nhân sự
                new User
                {
                    FullName = "Nguyễn Thị Mai",
                    Role = "Manager",
                    DepartmentId = departments["Nhân sự"]
                },
                new User
                {
                    FullName = "Trần Thị Lan",
                    Role = "Employee",
                    DepartmentId = departments["Nhân sự"]
                },

                // Phòng Hành chính
                new User
                {
                    FullName = "Phạm Văn Minh",
                    Role = "Manager",
                    DepartmentId = departments["Hành chính"]
                },
                new User
                {
                    FullName = "Lê Văn Hùng",
                    Role = "Employee",
                    DepartmentId = departments["Hành chính"]
                }
            );

            await db.SaveChangesAsync();
        }
    }
}
