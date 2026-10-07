import { Location } from '@angular/common';
import { HttpHeaders } from '@angular/common/http';
import { env } from '../_env/env';
import { OnInit } from '@angular/core';
import { signal } from '@angular/core';
import { Subject } from '../_models/subject';
import { Teacher } from '../_models/teacher';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-connect-teacher-to-subject',
  imports: [MATERIAL_IMPORTS, CommonModule, FormsModule],
  templateUrl: './connect-teacher-to-subject.html',
  styleUrl: './connect-teacher-to-subject.scss',
})
export class ConnectTeacherToSubject implements OnInit {
  public subjects = signal<Subject[]>([]);
  public teachers = signal<Teacher[]>([]);
  public isLoading = signal<boolean>(true);
  public selectedTeacherIdToRemove: string = '';
  public selectedSubjectIdsToRemove: Array<string> = [];
  public selectedTeacherIdToAdd: string = '';
  public selectedSubjectIdsToAdd: Array<string> = [];

  constructor(
    private http: HttpClient,
    private matSnackBar: MatSnackBar,
    private location: Location,
  ) {}

  ngOnInit(): void {
    this.http.get<Teacher[]>(`${env.teacherUri}`).subscribe((resp) => {
      this.teachers.set(
        resp.map((x) => {
          let t = new Teacher();

          t.id = x.id;
          t.name = x.name;
          t.neptun = x.neptun;
          t.birthYear = x.birthYear;
          t.image = x.image;
          t.creatorName = x.creatorName;
          t.createSubjects(x.teachedSubjects);

          return t;
        }),
      );
    });

    this.http.get<Subject[]>(`${env.subjectUri}`).subscribe((resp) => {
      this.subjects.set(
        resp.map((x) => {
          let s = new Subject();

          s.id = x.id;
          s.name = x.name;
          s.neptun = x.neptun;
          s.credit = x.credit;
          s.exam = x.exam;
          s.image = x.image;
          s.creatorName = x.creatorName;
          s.registeredStudents = x.registeredStudents;

          return s;
        }),
      );
    });

    this.isLoading.set(false);
  }

  public goBack(): void {
    this.location.back();
  }

  public addConnection(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http
      .post(
        env.schoolUri,
        { teacherId: this.selectedTeacherIdToAdd, subjectId: this.selectedSubjectIdsToAdd },
        { headers: headers },
      )
      .subscribe(
        (success) => {
          console.log('::SUCCESS::', success);
          this.matSnackBar.open('Successfully added connection', 'Close', { duration: 5000 });
        },
        (error) => {
          console.log(this.selectedSubjectIdsToAdd);
          console.log('::ERROR::', error);
          if (error.status == '409') {
            this.matSnackBar.open('This connection already exists.', 'Close', { duration: 5000 });
          } else {
            this.matSnackBar.open('Error happened!', 'Close', { duration: 5000 });
          }
        },
      );
  }

  public deleteConnection(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http
      .delete(env.schoolUri, {
        body: {
          teacherId: this.selectedTeacherIdToAdd,
          subjectId: this.selectedSubjectIdsToAdd,
        },
        headers: headers,
      })
      .subscribe(
        (success) => {
          console.log('::SUCCESS::', success);
          this.matSnackBar.open('Successfully deleted connection', 'Close', { duration: 5000 });
        },
        (error) => {
          console.log(this.selectedSubjectIdsToAdd);
          console.log('::ERROR::', error);
          if (error.status == '404') {
            this.matSnackBar.open('This connection doesnt exist.', 'Close', { duration: 5000 });
          } else {
            this.matSnackBar.open('Error happened!', 'Close', { duration: 5000 });
          }
        },
      );
  }
}
