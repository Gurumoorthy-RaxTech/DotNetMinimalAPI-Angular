import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './pages/LoginPage';
import StudentList from './pages/StudentList';
import StudentForm from './pages/StudentForm';

// C# equivalent: Program.cs route configuration
export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Public */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected — any logged-in user */}
          <Route
            path="/students"
            element={
              <ProtectedRoute>
                <StudentList />
              </ProtectedRoute>
            }
          />

          {/* Admin only — Add / Edit */}
          <Route
            path="/students/add"
            element={
              <ProtectedRoute adminOnly>
                <StudentForm />
              </ProtectedRoute>
            }
          />
          <Route
            path="/students/edit/:id"
            element={
              <ProtectedRoute adminOnly>
                <StudentForm />
              </ProtectedRoute>
            }
          />

          {/* Default redirect */}
          <Route path="*" element={<Navigate to="/students" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
