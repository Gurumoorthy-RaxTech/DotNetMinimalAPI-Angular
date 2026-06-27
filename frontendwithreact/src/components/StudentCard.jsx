import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// Reusable card — C# partial view (@Html.Partial) போல
export default function StudentCard({ student, onDelete }) {
  const navigate = useNavigate();
  const { isAdmin } = useAuth();

  const courseColors = {
    'Computer Science': '#4f46e5',
    'Mathematics': '#0891b2',
    'Physics': '#059669',
    'Chemistry': '#d97706',
    'Biology': '#dc2626',
  };

  const color = courseColors[student.course] || '#6b7280';

  return (
    <div className="student-card">
      {/* Avatar */}
      <div className="student-avatar" style={{ backgroundColor: color }}>
        {student.name.charAt(0).toUpperCase()}
      </div>

      {/* Info */}
      <div className="student-info">
        <h3>{student.name}</h3>
        <p className="student-meta">ID: #{student.id} &nbsp;|&nbsp; Age: {student.age}</p>
        <span className="student-course" style={{ backgroundColor: `${color}20`, color }}>
          {student.course}
        </span>
        <p className="student-email">{student.email}</p>
      </div>

      {/* Actions — Admin மட்டும் Edit/Delete பண்ணலாம் */}
      {isAdmin && (
        <div className="student-actions">
          <button
            className="btn-edit"
            onClick={() => navigate(`/students/edit/${student.id}`)}
          >
            Edit
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(student.id)}
          >
            Delete
          </button>
        </div>
      )}
    </div>
  );
}
