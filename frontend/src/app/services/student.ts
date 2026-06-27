// ============================================================
// STUDENT SERVICE - CRUD API calls
// Tanglish: Backend API call pannuvom - GET, POST, PUT, DELETE
// ============================================================
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from './auth';

export interface StudentModel {
  id: number;
  name: string;
  email: string;
  course: string;
  age: number;
}

@Injectable({
  providedIn: 'root'
})
export class StudentService {

  // V2 API use pannuvom - course filter feature irukku
  private apiUrl = 'http://localhost:5260/api/v2/students';

  constructor(private http: HttpClient) {}

  // GET /api/v2/students?course=optional
  getAll(course?: string): Observable<ApiResponse<StudentModel[]>> {
    let params = new HttpParams();
    if (course) {
      params = params.set('course', course);
    }
    return this.http.get<ApiResponse<StudentModel[]>>(this.apiUrl, { params });
  }

  // GET /api/v2/students/:id
  getById(id: number): Observable<ApiResponse<StudentModel>> {
    return this.http.get<ApiResponse<StudentModel>>(`${this.apiUrl}/${id}`);
  }

  // POST /api/v2/students
  create(student: Omit<StudentModel, 'id'>): Observable<ApiResponse<StudentModel>> {
    return this.http.post<ApiResponse<StudentModel>>(this.apiUrl, student);
  }

  // PUT /api/v2/students/:id
  update(id: number, student: StudentModel): Observable<ApiResponse<StudentModel>> {
    return this.http.put<ApiResponse<StudentModel>>(`${this.apiUrl}/${id}`, student);
  }

  // DELETE /api/v2/students/:id
  delete(id: number): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }
}
