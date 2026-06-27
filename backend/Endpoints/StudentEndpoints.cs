using Microsoft.AspNetCore.Mvc;
using MinimalApiJwt.Models;
using MinimalApiJwt.Services;

namespace MinimalApiJwt.Endpoints;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        // ==================== VERSION 1 ====================
        var v1 = app.NewVersionedApi("Students");

        var studentsV1 = v1.MapGroup("/api/v{version:apiVersion}/students")
                           .HasApiVersion(1.0)
                           .WithTags("Students V1")
                           .RequireAuthorization(); // JWT token mandatory

        // GET /api/v1/students - All students return pannuvom
        studentsV1.MapGet("/", (IStudentService service) =>
        {
            var students = service.GetAll();
            return Results.Ok(ApiResponse<List<Student>>.Ok(students, $"{students.Count} students found"));
        })
        .WithName("GetAllStudentsV1")
        .WithSummary("Get all students (V1)");

        // GET /api/v1/students/{id} - Single student
        studentsV1.MapGet("/{id:int}", (int id, IStudentService service) =>
        {
            var student = service.GetById(id);
            if (student == null)
                return Results.NotFound(ApiResponse<Student>.Fail($"Student with ID {id} not found"));

            return Results.Ok(ApiResponse<Student>.Ok(student));
        })
        .WithName("GetStudentByIdV1")
        .WithSummary("Get student by ID (V1)");

        // POST /api/v1/students - New student add pannuvom
        studentsV1.MapPost("/", (Student student, IStudentService service) =>
        {
            var created = service.Add(student);
            // 201 Created with location header
            return Results.Created($"/api/v1/students/{created.Id}",
                ApiResponse<Student>.Ok(created, "Student created successfully"));
        })
        .WithName("CreateStudentV1")
        .WithSummary("Create new student (V1)")
        .RequireAuthorization("AdminOnly"); // Only Admin create panlam

        // PUT /api/v1/students/{id} - Update student
        studentsV1.MapPut("/{id:int}", (int id, Student student, IStudentService service) =>
        {
            var updated = service.Update(id, student);
            if (updated == null)
                return Results.NotFound(ApiResponse<Student>.Fail($"Student with ID {id} not found"));

            return Results.Ok(ApiResponse<Student>.Ok(updated, "Student updated successfully"));
        })
        .WithName("UpdateStudentV1")
        .WithSummary("Update student (V1)")
        .RequireAuthorization("AdminOnly");

        // DELETE /api/v1/students/{id} - Delete student
        studentsV1.MapDelete("/{id:int}", (int id, IStudentService service) =>
        {
            var deleted = service.Delete(id);
            if (!deleted)
                return Results.NotFound(ApiResponse<object>.Fail($"Student with ID {id} not found"));

            return Results.Ok(ApiResponse<object>.Ok(new { }, "Student deleted successfully"));
        })
        .WithName("DeleteStudentV1")
        .WithSummary("Delete student (V1)")
        .RequireAuthorization("AdminOnly");

        // ==================== VERSION 2 ====================
        // V2 la additional search feature add pannuvom
        var studentsV2 = v1.MapGroup("/api/v{version:apiVersion}/students")
                           .HasApiVersion(2.0)
                           .WithTags("Students V2")
                           .RequireAuthorization();

        // GET /api/v2/students - V2 la course filter feature add pannuvom
        studentsV2.MapGet("/", ([FromQuery] string? course, IStudentService service) =>
        {
            var students = service.GetAll();

            // V2 feature: course filter
            if (!string.IsNullOrEmpty(course))
                students = students.Where(s =>
                    s.Course.Contains(course, StringComparison.OrdinalIgnoreCase)).ToList();

            return Results.Ok(ApiResponse<List<Student>>.Ok(students,
                $"{students.Count} students found (V2 - with filter support)"));
        })
        .WithName("GetAllStudentsV2")
        .WithSummary("Get all students with optional course filter (V2)");

        // V2 - same endpoints with same behavior as V1 plus new features
        studentsV2.MapGet("/{id:int}", (int id, IStudentService service) =>
        {
            var student = service.GetById(id);
            if (student == null)
                return Results.NotFound(ApiResponse<Student>.Fail($"Student with ID {id} not found"));

            return Results.Ok(ApiResponse<Student>.Ok(student));
        }).WithName("GetStudentByIdV2");

        studentsV2.MapPost("/", (Student student, IStudentService service) =>
        {
            var created = service.Add(student);
            return Results.Created($"/api/v2/students/{created.Id}",
                ApiResponse<Student>.Ok(created, "Student created (V2)"));
        }).WithName("CreateStudentV2").RequireAuthorization("AdminOnly");

        studentsV2.MapPut("/{id:int}", (int id, Student student, IStudentService service) =>
        {
            var updated = service.Update(id, student);
            if (updated == null)
                return Results.NotFound(ApiResponse<Student>.Fail("Not found"));
            return Results.Ok(ApiResponse<Student>.Ok(updated, "Updated (V2)"));
        }).WithName("UpdateStudentV2").RequireAuthorization("AdminOnly");

        studentsV2.MapDelete("/{id:int}", (int id, IStudentService service) =>
        {
            var deleted = service.Delete(id);
            if (!deleted) return Results.NotFound(ApiResponse<object>.Fail("Not found"));
            return Results.Ok(ApiResponse<object>.Ok(new { }, "Deleted (V2)"));
        }).WithName("DeleteStudentV2").RequireAuthorization("AdminOnly");
    }
}
