import { Injectable, signal } from '@angular/core';
import { env } from '../env/env';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public isLoggedIn = signal(false);
  public activeUser = signal<string>('');

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

    if (!token || !expiration) {
      this.isLoggedIn.set(false);
      this.activeUser.set('');
      return;
    }

    const expirationTime = new Date(expiration).getTime();
    const loggedIn = expirationTime > Date.now();

    this.isLoggedIn.set(loggedIn);

    if (loggedIn) {
      this.activeUser.set(localStorage.getItem('username') ?? '');
    } else {
      this.activeUser.set('');
    }
  }

  public logout(): void {
    localStorage.removeItem(env.jwtAccessToken);
    localStorage.removeItem(env.jwtAccessTokenExp);
    localStorage.removeItem(env.jwtRefreshToken);
    localStorage.removeItem(env.jwtRefreshTokenExp);

    this.activeUser.set('');
    this.isLoggedIn.set(false);

    this.router.navigate(['/feed']);
  }
}
