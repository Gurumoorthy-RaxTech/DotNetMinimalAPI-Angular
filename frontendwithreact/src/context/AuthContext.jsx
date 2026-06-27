import { createContext, useContext, useState, useCallback } from 'react';
import { authService } from '../services/authService';

// C# equivalent: static SessionHelper or IHttpContextAccessor
const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    // Page refresh-லயும் login state maintain பண்ண localStorage படிக்கிறோம்
    const token = localStorage.getItem('accessToken');
    const username = localStorage.getItem('username');
    const role = localStorage.getItem('role');
    return token ? { username, role, token } : null;
  });

  const login = useCallback(async (username, password) => {
    const res = await authService.login(username, password);
    const { token, refreshToken, username: uname, role } = res.data.data;

    localStorage.setItem('accessToken', token);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('username', uname);
    localStorage.setItem('role', role);

    setUser({ username: uname, role, token });
  }, []);

  const logout = useCallback(async () => {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) await authService.revoke(refreshToken);
    } catch {
      // Revoke fail ஆனாலும் local logout பண்ணிடுவோம்
    } finally {
      localStorage.clear();
      setUser(null);
    }
  }, []);

  const isAdmin = user?.role === 'Admin';

  return (
    <AuthContext.Provider value={{ user, login, logout, isAdmin }}>
      {children}
    </AuthContext.Provider>
  );
}

// Custom hook — useContext(AuthContext) நேரடியா எழுதாம இதை use பண்ணலாம்
export function useAuth() {
  return useContext(AuthContext);
}
