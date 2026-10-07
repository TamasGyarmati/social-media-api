import { Location } from '@angular/common';
import { OnInit } from '@angular/core';
import { Teacher } from '../_models/teacher';
import { Component, signal } from '@angular/core';
import { env } from '../_env/env';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-teacher',
  imports: [MATERIAL_IMPORTS, FormsModule, CommonModule],
  templateUrl: './create-teacher.html',
  styleUrl: './create-teacher.scss',
})
export class CreateTeacher implements OnInit {
  public teacher: Teacher;
  public isLoading = signal<boolean>(true);

  constructor(
    private http: HttpClient,
    private route: Router,
    private matSnackBar: MatSnackBar,
    private location: Location,
  ) {
    this.teacher = new Teacher();
  }

  ngOnInit(): void {
    this.isLoading.set(false);
  }

  createTeacher(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http.post(env.teacherUri, this.teacher, { headers: headers }).subscribe(
      (success) => {
        this.route.navigate(['/list-teachers']);
        console.log('::SUCCESS::', success);
        this.matSnackBar.open('Created the teacher!', 'Close', { duration: 5000 });
      },
      (error) => {
        this.route.navigate(['/list-teachers']);
        console.log('::ERROR::', error);
        this.matSnackBar.open('Error happened!', 'Close', { duration: 5000 });
      },
    );
  }

  public goBack(): void {
    this.location.back();
  }
}
