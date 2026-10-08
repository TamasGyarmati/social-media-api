import { OnInit } from '@angular/core';
import { signal } from '@angular/core';
import { Token as TokenModel } from '../_models/token';
import { env } from '../env/env';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Validators } from '@angular/forms';
import { Login as LoginModel } from '../_models/login';
import { CommonModule } from '@angular/common';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [MATERIAL_IMPORTS, CommonModule, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  public email: FormControl;
  public password: FormControl;
  public rememberMe: boolean = false;
  public isLoading = signal<boolean>(true);
  public hide = signal<boolean>(true);

  constructor(
    private router: Router,
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private auth: AuthService,
  ) {
    this.password = new FormControl('', [Validators.required]);
    this.email = new FormControl('', [Validators.required, Validators.email]);
  }

  ngOnInit(): void {
    this.isLoading.set(false);
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

  get checkInput(): boolean {
    return this.email.valid && this.password.valid;
  }

  get formattedEmail(): string {
    return this.email.value.substring(0, this.email.value.indexOf('@'));
  }

  clickEvent(event: MouseEvent) {
    this.hide.set(!this.hide());
    event.stopPropagation();
  }

  sendLoginCredentials(): void {
    const loginModel: LoginModel = {
      email: this.email.value ?? '',
      password: this.password.value ?? '',
      rememberMe: this.rememberMe,
    };

    this.http.post<TokenModel>(`${env.authLoginUri}`, loginModel).subscribe(
      (success) => {
        localStorage.setItem('username', this.formattedEmail);
        localStorage.setItem('userid', success.userId);
        localStorage.setItem(env.jwtAccessToken, success.accessToken);
        localStorage.setItem(env.jwtAccessTokenExp, success.accessTokenExpireDate);
        localStorage.setItem(env.jwtRefreshToken, success.refreshToken);
        localStorage.setItem(env.jwtRefreshTokenExp, success.refreshTokenExpireDate);
        this.auth.checkIfLoggedIn();
        this.auth.activeUser.set(this.formattedEmail);
        this.snackBar
          .open('Login was successful!', 'Close', { duration: 5000 })
          .afterDismissed()
          .subscribe(() => {
            this.router.navigate(['/feed']);
          });
        console.log('::SUCCESS::', success);
        this.router.navigate(['/feed']);
      },
      (error) => {
        console.log('::ERROR::', error);
        this.snackBar.open(error.error.message, 'Close', { duration: 5000 });
      },
    );
  }
}
