import { RegisterError } from '../_models/registerError';
import { OnInit } from '@angular/core';
import { signal } from '@angular/core';
import { env } from '../env/env';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { Register as RegisterModel } from '../_models/register';
import { CommonModule } from '@angular/common';
import { MATERIAL_IMPORTS } from '../_shared/material';

@Component({
  selector: 'app-register',
  imports: [MATERIAL_IMPORTS, CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register implements OnInit {
  public email: FormControl;
  public password: FormControl;
  public registerModel: RegisterModel;
  public acceptTermsAndConditions: boolean;
  public isLoading = signal<boolean>(true);
  public hide = signal<boolean>(true);

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private router: Router,
  ) {
    this.acceptTermsAndConditions = false;
    this.email = new FormControl('', [Validators.required, Validators.email]);
    this.password = new FormControl('', [Validators.required]);
    this.registerModel = {
      firstName: '',
      lastName: '',
      userName: '',
      email: '',
      password: '',
    };
  }

  ngOnInit(): void {
    this.isLoading.set(false);
  }

  get canRegister(): boolean {
    return (
      this.acceptTermsAndConditions &&
      this.registerModel.firstName.trim() !== '' &&
      this.registerModel.lastName.trim() !== '' &&
      this.email.valid &&
      this.password.valid &&
      !this.email.hasError('email')
    );
  }

  get getEmailErrorMessage(): string {
    if (this.email.hasError('required')) {
      return 'You must enter a value!';
    }

    return this.email.hasError('email') ? 'Not a valid email' : '';
  }

  get getPasswordErrorMessage(): string {
    if (this.password.hasError('required')) {
      return 'You must enter a value!';
    }

    return this.password.hasError('password') ? 'Not a valid password' : '';
  }

  clickEvent(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }

  sendRegisterCredentials(): void {
    const registerRequestDto: RegisterModel = {
      email: this.email.value ?? '',
      password: this.password.value ?? '',
      firstName: this.registerModel.firstName,
      lastName: this.registerModel.lastName,
      userName: this.registerModel.userName,
    };

    this.http.post(`${env.authRegisterUri}`, registerRequestDto).subscribe(
      (success) => {
        this.router.navigate(['/login']);
        this.snackBar.open(
          'Register was successful! We have sent you a confirmation link to your email, you must accept the link to login.',
          'Close',
          { duration: 5000 },
        );
        console.log('::SUCCESS::', success);
      },
      (error) => {
        console.log('::ERROR::', error);
        const errors = error.error as RegisterError[];
        let errorString = '';
        errors.forEach((x) => (errorString += `${x.description}, `));
        this.snackBar.open(errorString, 'Close', { duration: 5000 });
      },
    );
  }
}
