import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { studentService } from '../services/studentService';
import { useAuth } from '../context/AuthContext';
import StudentCard from '../components/StudentCard';

export default function StudentList() {
  const navigate = useNavigate();
  const { user, logout, isAdmin } = useAuth();

  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  const fetchStudents = useCallback(async () => {
    try {
      setLoading(true);
      setError('');
      const res = await studentService.getAll();
      setStudents(res.data.data); // ApiResponse<List<Student>> → .data.data
    } catch {
      setError('Students load ஆகல. API running-ஆ check பண்ணுங்க.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchStudents();
  }, [fetchStudents]);

  const handleDelete = async (id) => {
    if (!window.confirm('Delete பண்ணணுமா?')) return;
    try {
      await studentService.delete(id);
      setStudents((prev) => prev.filter((s) => s.id !== id));
    } catch {
      alert('Delete ஆகல!');
    }
  };

  // Search filter — LINQ .Where() போல
  const filtered = students.filter(
    (s) =>
      s.name.toLowerCase().includes(search.toLowerCase()) ||
      s.course.toLowerCase().includes(search.toLowerCase()) ||
      s.email.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="page-wrapper">
      {/* Navbar */}
      <nav className="navbar">
        <div className="navbar-brand">
          <span className="brand-logo">ZC</span>
          <span>ZenCampus</span>
        </div>
        <div className="navbar-right">
          <span className="user-badge">{user?.role}</span>
          <span className="user-name">{user?.username}</span>
          <button className="btn-logout" onClick={logout}>Logout</button>
        </div>
      </nav>

      {/* Main content */}
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Students</h1>
            <p>{filtered.length} students found</p>
          </div>
          {isAdmin && (
            <button className="btn-primary" onClick={() => navigate('/students/add')}>
              + Add Student
            </button>
          )}
        </div>

        {/* Search */}
        <div className="search-bar">
          <input
            type="text"
            placeholder="Search by name, course or email..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
          {search && (
            <button className="clear-search" onClick={() => setSearch('')}>✕</button>
          )}
        </div>

        {/* States */}
        {loading && (
          <div className="state-box">
            <div className="spinner" />
            <p>Loading students...</p>
          </div>
        )}

        {!loading && error && (
          <div className="alert-error">
            {error}
            <button onClick={fetchStudents}>Retry</button>
          </div>
        )}

        {!loading && !error && filtered.length === 0 && (
          <div className="state-box">
            <p>No students found{search ? ` for "${search}"` : ''}.</p>
          </div>
        )}

        {/* Cards grid */}
        {!loading && !error && (
          <div className="cards-grid">
            {filtered.map((student) => (
              <StudentCard
                key={student.id}
                student={student}
                onDelete={handleDelete}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
