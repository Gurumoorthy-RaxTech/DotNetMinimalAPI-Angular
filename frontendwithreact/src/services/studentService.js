import api from './api';

// Student CRUD — all calls use the central api instance (JWT auto-attached)
export const studentService = {

  // GET /api/v1/students
  getAll: () => api.get('/students'),

  // GET /api/v1/students/{id}
  getById: (id) => api.get(`/students/${id}`),

  // POST /api/v1/students  (Admin only)
  create: (data) => api.post('/students', data),

  // PUT /api/v1/students/{id}  (Admin only)
  update: (id, data) => api.put(`/students/${id}`, data),

  // DELETE /api/v1/students/{id}  (Admin only)
  delete: (id) => api.delete(`/students/${id}`),
};
