using MinimalApiJwt.Models;

namespace MinimalApiJwt.Services;

public interface IStudentService
{
    List<Student> GetAll();
    Student? GetById(int id);
    Student Add(Student student);
    Student? Update(int id, Student student);
    bool Delete(int id);
}

// In-Memory implementation - real project la database use pannuvom
// Interview preparation ku in-memory use pannuvom
public class StudentService : IStudentService
{
    // Static list - application running varaikum data irukum
    private static readonly List<Student> _students = new()
    {
        new Student { Id = 1, Name = "Gurumoorthy", Email = "guru@school.com", Course = "Computer Science", Age = 22 },
        new Student { Id = 2, Name = "Basha", Email = "basha@school.com", Course = "Electronics", Age = 21 },
        new Student { Id = 3, Name = "Sridhar", Email = "sridhar@school.com", Course = "Mechanical", Age = 23 }
    };

    private static int _nextId = 4;

    // GET ALL students
    public List<Student> GetAll() => _students;

    // GET student by ID - null return pannuvom if not found
    public Student? GetById(int id) => _students.FirstOrDefault(s => s.Id == id);

    // ADD new student
    public Student Add(Student student)
    {
        student.Id = _nextId++;
        _students.Add(student);
        return student;
    }

    // UPDATE existing student
    public Student? Update(int id, Student updatedStudent)
    {
        var existing = _students.FirstOrDefault(s => s.Id == id);
        if (existing == null) return null;

        existing.Name = updatedStudent.Name;
        existing.Email = updatedStudent.Email;
        existing.Course = updatedStudent.Course;
        existing.Age = updatedStudent.Age;

        return existing;
    }

    // DELETE student
    public bool Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student == null) return false;

        _students.Remove(student);
        return true;
    }
}
