import axios from 'axios';

const BASE_URL = 'http://localhost:5260/api/v1';

// Central axios instance — like HttpClient with BaseAddress in C#
const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// Request interceptor — every request-la JWT token automatic-ஆ add பண்ணும்
// C# equivalent: DelegatingHandler / AuthorizationHeaderHandler
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor — 401 வந்தா refresh token try பண்ணும்
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config;

    // 401 + not already retried = token expired, try refresh
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true;
      try {
        const accessToken  = localStorage.getItem('accessToken');
        const refreshToken = localStorage.getItem('refreshToken');

        const res = await axios.post(`${BASE_URL}/auth/refresh`, {
          accessToken,
          refreshToken,
        });

        const newToken = res.data.data.token;
        localStorage.setItem('accessToken', newToken);
        original.headers.Authorization = `Bearer ${newToken}`;
        return api(original); // original request retry பண்ணும்
      } catch {
        // Refresh failed → logout
        localStorage.clear();
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
