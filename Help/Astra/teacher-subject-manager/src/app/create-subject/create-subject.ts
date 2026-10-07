import { Location } from '@angular/common';
import { Subject } from '../_models/subject';
import { Component, OnInit, signal } from '@angular/core';
import { env } from '../_env/env';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-subject',
  imports: [MATERIAL_IMPORTS, FormsModule, CommonModule],
  templateUrl: './create-subject.html',
  styleUrl: './create-subject.scss',
})
export class CreateSubject implements OnInit {
  public subject: Subject;
  public isLoading = signal<boolean>(true);

  constructor(
    private http: HttpClient,
    private route: Router,
    private matSnackBar: MatSnackBar,
    private location: Location,
  ) {
    this.subject = new Subject();
  }

  ngOnInit(): void {
    this.isLoading.set(false);
  }

  createSubject(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http.post(env.subjectUri, this.subject, { headers: headers }).subscribe(
      (success) => {
        this.route.navigate(['/list-subjects']);
        console.log('::SUCCESS::', success);
        this.matSnackBar.open('Created the subject!', 'Close', { duration: 5000 });
      },
      (error) => {
        this.route.navigate(['/list-subjects']);
        console.log('::ERROR::', error);
        this.matSnackBar.open('Error happened!', 'Close', { duration: 5000 });
      },
    );
  }

  public goBack(): void {
    this.location.back();
  }
}
