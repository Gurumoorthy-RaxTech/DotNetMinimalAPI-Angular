import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

// C# equivalent: [Authorize] attribute on controller
export default function ProtectedRoute({ children, adminOnly = false }) {
  const { user, isAdmin } = useAuth();

  if (!user) return <Navigate to="/login" replace />;
  if (adminOnly && !isAdmin) return <Navigate to="/students" replace />;

  return children;
}
