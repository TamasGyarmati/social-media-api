import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { CommonModule, Location } from '@angular/common';
import { inject } from '@angular/core';
import { signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { env } from '../env/env';

@Component({
  selector: 'app-create-post',
  imports: [MATERIAL_IMPORTS, CommonModule, ReactiveFormsModule],
  templateUrl: './create-post.html',
  styleUrl: './create-post.scss',
})
export class CreatePost implements OnInit {
  private location: Location = inject(Location);
  private http: HttpClient = inject(HttpClient);
  private router: Router = inject(Router);
  private matBar: MatSnackBar = inject(MatSnackBar);

  public selectedFileName: string = '';

  public isLoading = signal<boolean>(true);

  public postForm = new FormGroup({
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    image: new FormControl<File | null>(null, {
      validators: [Validators.required],
    }),
  });

  ngOnInit(): void {
    this.isLoading.set(false);
  }

  public createPost(): void {
    if (this.postForm.invalid) {
      this.postForm.markAllAsTouched();
      return;
    }

    const formData = new FormData();

    formData.append('Title', this.postForm.controls.title.value);
    formData.append('Description', this.postForm.controls.description.value);

    const image = this.postForm.controls.image.value;

    if (image) {
      formData.append('Image', image);
    }

    let headers = new HttpHeaders({
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.http.post(env.postUri, formData, { headers }).subscribe({
      next: (successfully) => {
        this.router.navigate(['/feed']);
        this.matBar.open('Post created successfully', 'Close', { duration: 3000 });
        console.log('::SUCCESS::', successfully);
      },

      error: (error) => {
        console.error('::ERRORS::', error);
      },
    });
  }

  public goBack(): void {
    this.location.back();
  }

  public onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFileName = input.files[0].name;
      this.postForm.patchValue({
        image: input.files[0],
      });
    }
  }
}
