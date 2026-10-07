import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { env } from '../_env/env';
import { Component, OnInit, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { ActivatedRoute } from '@angular/router';
import { Teacher } from '../_models/teacher';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-update-teachers',
  imports: [MATERIAL_IMPORTS, FormsModule, FormsModule, CommonModule],
  templateUrl: './update-teachers.html',
  styleUrl: './update-teachers.scss',
})
export class UpdateTeachers implements OnInit {
  public teacher = signal<Teacher>(new Teacher());
  public isLoading = signal<boolean>(true);
  public deleteDisabled: boolean;

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private router: Router,
    private location: Location,
  ) {
    this.deleteDisabled = true;
  }

  ngOnInit(): void {
    this.route.params.subscribe((param) => {
      const teacherId = param['id'];

      this.http.get<Teacher>(`${env.teacherUri}/${teacherId}`).subscribe((resp) => {
        this.teacher.set(resp);
      });

      this.isLoading.set(false);
    });
  }

  updateTeacher(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http
      .put(`${env.teacherUri}/${this.teacher().id}`, this.teacher(), { headers: headers })
      .subscribe(
        (success) => {
          this.router.navigate(['/list-teachers']);

          this.snackBar.open('Update was successful!', 'Close', { duration: 5000 });
          console.log('::SUCCESS::', success);
        },
        (error) => {
          this.snackBar.open('Error occured, please try again.', 'Close', { duration: 5000 });
          console.log('::ERROR::', error);
        },
      );
  }

  deleteTeacher(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });
    this.http.delete(`${env.teacherUri}/${this.teacher().id}`, { headers: headers }).subscribe(
      (success) => {
        this.router.navigate(['/list-teachers']);

        this.snackBar.open('Delete was successful!', 'Close', { duration: 5000 });
        console.log('::SUCCESS::', success);
      },
      (error) => {
        this.snackBar.open('Error occured, please try again.', 'Close', { duration: 5000 });
        console.log('::ERROR::', error);
      },
    );
  }

  public goBack(): void {
    this.location.back();
  }
}
