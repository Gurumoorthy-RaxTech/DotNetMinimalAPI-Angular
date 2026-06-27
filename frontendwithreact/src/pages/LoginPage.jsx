import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({ username: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.username || !form.password) {
      setError('Username and password required');
      return;
    }
    try {
      setLoading(true);
      await login(form.username, form.password);
      navigate('/students');
    } catch {
      setError('Invalid username or password');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-wrapper">
      <div className="login-card">
        {/* Header */}
        <div className="login-header">
          <div className="login-logo">ZC</div>
          <h2>ZenCampus</h2>
          <p>Sign in to your account</p>
        </div>

        {/* Error */}
        {error && (
          <div className="alert-error">{error}</div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Username</label>
            <input
              name="username"
              type="text"
              placeholder="admin / student / guru"
              value={form.username}
              onChange={handleChange}
              autoFocus
            />
          </div>

          <div className="form-group">
            <label>Password</label>
            <input
              name="password"
              type="password"
              placeholder="Admin@123 / Student@123"
              value={form.password}
              onChange={handleChange}
            />
          </div>

          <button type="submit" className="btn-login" disabled={loading}>
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        {/* Demo credentials */}
        <div className="demo-creds">
          <p><strong>Demo Credentials:</strong></p>
          <p>Admin: <code>admin</code> / <code>Admin@123</code></p>
          <p>Student: <code>student</code> / <code>Student@123</code></p>
        </div>
      </div>
    </div>
  );
}
