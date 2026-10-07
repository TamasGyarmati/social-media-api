import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { env } from '../_env/env';
import { Component, OnInit, signal } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { ActivatedRoute } from '@angular/router';
import { Subject } from '../_models/subject';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-update-subjects',
  imports: [MATERIAL_IMPORTS, FormsModule, FormsModule, CommonModule],
  templateUrl: './update-subjects.html',
  styleUrl: './update-subjects.scss',
})
export class UpdateSubjects implements OnInit {
  public subject = signal<Subject>(new Subject());
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
      const subjectId = param['id'];

      this.http.get<Subject>(`${env.subjectUri}/${subjectId}`).subscribe((resp) => {
        this.subject.set(resp);
      });

      this.isLoading.set(false);
    });
  }

  updateSubject(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });

    this.http
      .put(`${env.subjectUri}/${this.subject().id}`, this.subject(), { headers: headers })
      .subscribe(
        (success) => {
          this.router.navigate(['/list-subjects']);

          this.snackBar.open('Update was successful!', 'Close', { duration: 5000 });
          console.log('::SUCCESS::', success);
        },
        (error) => {
          this.snackBar.open('Error occured, please try again.', 'Close', { duration: 5000 });
          console.log('::ERROR::', error);
        },
      );
  }

  deleteSubject(): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtToken),
    });
    this.http.delete(`${env.subjectUri}/${this.subject().id}`, { headers: headers }).subscribe(
      (success) => {
        this.router.navigate(['/list-subjects']);

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
