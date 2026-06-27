import axios from 'axios';

const BASE_URL = 'http://localhost:5260/api/v1';

// Auth calls use plain axios (no JWT needed for login/refresh)
export const authService = {

  // POST /api/v1/auth/login
  login: (username, password) =>
    axios.post(`${BASE_URL}/auth/login`, { username, password }),

  // POST /api/v1/auth/refresh
  refresh: (accessToken, refreshToken) =>
    axios.post(`${BASE_URL}/auth/refresh`, { accessToken, refreshToken }),

  // POST /api/v1/auth/revoke (logout)
  revoke: (refreshToken) =>
    axios.post(
      `${BASE_URL}/auth/revoke`,
      {},
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem('accessToken')}`,
          'X-Refresh-Token': refreshToken,
        },
      }
    ),
};
