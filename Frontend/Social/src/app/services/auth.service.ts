import { Injectable, signal } from '@angular/core';
import { env } from '../env/env';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public isLoggedIn = signal(false);
  public userName = signal<string>('');

  constructor(private router: Router) {
    this.checkIfLoggedIn();
  }

  public canActivate(): boolean {
    if (!this.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }
    return true;
  }

  public checkIfLoggedIn(): void {
    const token = localStorage.getItem(env.jwtAccessToken);
    const expiration = localStorage.getItem(env.jwtAccessTokenExp);
    this.userName.set(localStorage.getItem('username') ?? 'NaN');

    if (!token || !expiration) {
      this.isLoggedIn.set(false);
      return;
    }

    const expirationTime = new Date(expiration).getTime();

    this.isLoggedIn.set(expirationTime > Date.now());
  }

  public logout(): void {
    localStorage.removeItem(env.jwtAccessToken);
    localStorage.removeItem(env.jwtAccessTokenExp);
    localStorage.removeItem(env.jwtRefreshToken);
    localStorage.removeItem(env.jwtRefreshTokenExp);
    localStorage.clear();

    this.checkIfLoggedIn();

    this.router.navigate(['/feed']);
  }
}
