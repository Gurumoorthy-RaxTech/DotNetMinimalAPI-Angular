import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { studentService } from '../services/studentService';

const COURSES = ['Computer Science', 'Mathematics', 'Physics', 'Chemistry', 'Biology'];

const EMPTY_FORM = { name: '', email: '', course: '', age: '' };

export default function StudentForm() {
  const { id } = useParams();        // Edit mode: URL → /students/edit/5
  const navigate = useNavigate();
  const isEdit = !!id;

  const [form, setForm] = useState(EMPTY_FORM);
  const [errors, setErrors] = useState({});
  const [saving, setSaving] = useState(false);
  const [loadError, setLoadError] = useState('');

  // Edit mode — existing data load பண்ணு
  useEffect(() => {
    if (!isEdit) return;
    studentService
      .getById(id)
      .then((res) => setForm(res.data.data))
      .catch(() => setLoadError('Student data load ஆகல'));
  }, [id, isEdit]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: '' }));
  };

  // Client-side validation — C# ModelState போல
  const validate = () => {
    const e = {};
    if (!form.name.trim())         e.name   = 'Name is required';
    if (!form.email.trim())        e.email  = 'Email is required';
    else if (!/\S+@\S+\.\S+/.test(form.email)) e.email = 'Invalid email format';
    if (!form.course)              e.course = 'Course is required';
    if (!form.age)                 e.age    = 'Age is required';
    else if (form.age < 5 || form.age > 100) e.age = 'Age must be between 5 and 100';
    return e;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    try {
      setSaving(true);
      const payload = { ...form, age: Number(form.age) };
      if (isEdit) {
        await studentService.update(id, payload);
      } else {
        await studentService.create(payload);
      }
      navigate('/students');
    } catch {
      setErrors({ submit: 'Save ஆகல. API error.' });
    } finally {
      setSaving(false);
    }
  };

  if (loadError) {
    return (
      <div className="page-wrapper">
        <div className="content">
          <div className="alert-error">{loadError}</div>
          <button onClick={() => navigate('/students')}>Back</button>
        </div>
      </div>
    );
  }

  return (
    <div className="page-wrapper">
      <div className="content">
        <div className="form-card">
          {/* Header */}
          <div className="form-card-header">
            <button className="btn-back" onClick={() => navigate('/students')}>← Back</button>
            <h2>{isEdit ? 'Edit Student' : 'Add New Student'}</h2>
          </div>

          {errors.submit && <div className="alert-error">{errors.submit}</div>}

          <form onSubmit={handleSubmit} noValidate>
            {/* Name */}
            <div className="form-group">
              <label>Full Name <span className="required">*</span></label>
              <input
                name="name"
                value={form.name}
                onChange={handleChange}
                placeholder="Enter student name"
                className={errors.name ? 'input-error' : ''}
              />
              {errors.name && <span className="error-msg">{errors.name}</span>}
            </div>

            {/* Email */}
            <div className="form-group">
              <label>Email <span className="required">*</span></label>
              <input
                name="email"
                type="email"
                value={form.email}
                onChange={handleChange}
                placeholder="student@example.com"
                className={errors.email ? 'input-error' : ''}
              />
              {errors.email && <span className="error-msg">{errors.email}</span>}
            </div>

            {/* Course + Age — 2 columns */}
            <div className="form-row">
              <div className="form-group">
                <label>Course <span className="required">*</span></label>
                <select
                  name="course"
                  value={form.course}
                  onChange={handleChange}
                  className={errors.course ? 'input-error' : ''}
                >
                  <option value="">Select course</option>
                  {COURSES.map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
                {errors.course && <span className="error-msg">{errors.course}</span>}
              </div>

              <div className="form-group">
                <label>Age <span className="required">*</span></label>
                <input
                  name="age"
                  type="number"
                  value={form.age}
                  onChange={handleChange}
                  placeholder="18"
                  min="5"
                  max="100"
                  className={errors.age ? 'input-error' : ''}
                />
                {errors.age && <span className="error-msg">{errors.age}</span>}
              </div>
            </div>

            {/* Buttons */}
            <div className="form-actions">
              <button
                type="button"
                className="btn-secondary"
                onClick={() => navigate('/students')}
              >
                Cancel
              </button>
              <button type="submit" className="btn-primary" disabled={saving}>
                {saving ? 'Saving...' : isEdit ? 'Update Student' : 'Add Student'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
