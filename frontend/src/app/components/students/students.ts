// ============================================================
// STUDENTS COMPONENT - CRUD operations with table
// Tanglish: Student list show pannuvom, add/edit/delete panlam
// ============================================================
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentService, StudentModel } from '../../services/student';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-students',
  imports: [CommonModule, FormsModule],
  templateUrl: './students.html',
  styleUrl: './students.scss'
})
export class Students implements OnInit {

  students: StudentModel[] = [];
  filteredStudents: StudentModel[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  courseFilter = '';
  searchText = '';

  // Form state
  showForm = false;
  isEditing = false;
  currentStudent: Partial<StudentModel> = {};

  isAdmin = false;

  constructor(
    private studentService: StudentService,
    private authService: AuthService
  ) {
    this.isAdmin = this.authService.isAdmin();
  }

  // OnInit lifecycle hook - component load aana udane call aagum
  ngOnInit(): void {
    this.loadStudents();
  }

  loadStudents(): void {
    this.isLoading = true;
    this.studentService.getAll(this.courseFilter || undefined).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.students = res.data;
          this.applySearch();
        }
      },
      error: () => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load students';
      }
    });
  }

  applySearch(): void {
    if (!this.searchText) {
      this.filteredStudents = [...this.students];
    } else {
      this.filteredStudents = this.students.filter(s =>
        s.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        s.email.toLowerCase().includes(this.searchText.toLowerCase())
      );
    }
  }

  openAddForm(): void {
    this.isEditing = false;
    this.currentStudent = { name: '', email: '', course: '', age: 0 };
    this.showForm = true;
  }

  openEditForm(student: StudentModel): void {
    this.isEditing = true;
    this.currentStudent = { ...student }; // spread operator - copy pannuvom
    this.showForm = true;
  }

  saveStudent(): void {
    if (this.isEditing && this.currentStudent.id) {
      this.studentService.update(this.currentStudent.id, this.currentStudent as StudentModel)
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.showSuccess('Student updated successfully');
              this.loadStudents();
              this.showForm = false;
            }
          },
          error: () => this.errorMessage = 'Update failed'
        });
    } else {
      this.studentService.create(this.currentStudent as Omit<StudentModel, 'id'>)
        .subscribe({
          next: (res) => {
            if (res.success) {
              this.showSuccess('Student added successfully');
              this.loadStudents();
              this.showForm = false;
            }
          },
          error: () => this.errorMessage = 'Create failed'
        });
    }
  }

  deleteStudent(id: number): void {
    if (!confirm('Are you sure you want to delete this student?')) return;

    this.studentService.delete(id).subscribe({
      next: (res) => {
        if (res.success) {
          this.showSuccess('Student deleted');
          this.students = this.students.filter(s => s.id !== id);
          this.applySearch();
        }
      },
      error: () => this.errorMessage = 'Delete failed'
    });
  }

  showSuccess(msg: string): void {
    this.successMessage = msg;
    setTimeout(() => this.successMessage = '', 3000);
  }
}
