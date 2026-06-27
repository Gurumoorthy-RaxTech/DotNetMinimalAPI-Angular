# .NET Minimal API + Angular — SmartSchool

**Interview Preparation Project | Gurumoorthy M**

Full-stack project with JWT Authentication, API Versioning, and Angular UI.

---

## Project Structure

```
.NetWithAngular/
├── backend/          ← .NET 9 Minimal API
│   ├── Program.cs        (Every line commented)
│   ├── Models/
│   ├── Services/
│   └── Endpoints/
└── frontend/         ← Angular 21
    └── src/app/
        ├── services/     (AuthService, StudentService)
        ├── components/   (Login, Dashboard, Students)
        ├── guards/       (AuthGuard)
        └── interceptors/ (JWT Interceptor)
```

---

## Backend — .NET 9 Minimal API

### Features
- JWT Authentication (Bearer token)
- API Versioning (v1 and v2)
- RBAC (Admin / Student roles)
- Swagger with JWT support
- CORS for Angular

### Run Backend
```bash
cd backend
dotnet run
# Swagger: https://localhost:7001/swagger
```

### API Endpoints

| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| POST | /api/v1/auth/login | None | Get JWT token |
| GET | /api/v1/students | Any role | List all students |
| GET | /api/v2/students?course=CS | Any role | List with filter (V2) |
| POST | /api/v1/students | Admin only | Add student |
| PUT | /api/v1/students/{id} | Admin only | Update student |
| DELETE | /api/v1/students/{id} | Admin only | Delete student |

### Demo Credentials
| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Admin (full access) |
| student | Student@123 | Student (read only) |
| guru | Guru@123 | Admin |

---

## Frontend — Angular 21

### Features
- Login page with JWT token storage
- Auth Guard (protected routes)
- JWT Interceptor (auto-attach token)
- Student CRUD (Admin only)
- API v2 with course filter
- Reactive forms + HttpClient

### Run Frontend
```bash
cd frontend
npm install
ng serve
# App: http://localhost:4200
```

---

## Key Concepts (Interview Topics)

### Backend
- **Minimal API** — No controllers, endpoints mapped directly in Program.cs
- **JWT** — Header.Payload.Signature, stateless authentication
- **API Versioning** — URL segment (/api/v1/, /api/v2/)
- **CORS** — Allows Angular (4200) to call API
- **Middleware order** — CORS → Authentication → Authorization

### Angular
- **Component** — UI building block with HTML, TS, SCSS
- **Service** — Business logic, API calls, shared state
- **Interceptor** — Middleware for HTTP (auto-add JWT token)
- **Guard** — Route protection (canActivate)
- **BehaviorSubject** — Reactive state management
- **Standalone Components** — No NgModule needed (Angular 14+)

---

## Interview Q&A

See `Interview_QA.html` in the ZenCampusWeb project for 100 Q&A.
