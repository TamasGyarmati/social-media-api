import { Injectable, signal } from '@angular/core';
import { env } from './_env/env';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  public isLoggedIn = signal(false);
  public userName = signal<string>('');

  constructor(private router: Router) {}

  public canActivate(): boolean {
    if (!this.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }
    return true;
  }

  public checkIfLoggedIn(): void {
    const token = localStorage.getItem(env.jwtToken);
    const expiration = localStorage.getItem(env.jwtTokenExp);
    this.userName.set(localStorage.getItem('username') ?? 'NaN');

    if (!token || !expiration) {
      this.isLoggedIn.set(false);
      return;
    }

    const expirationTime = new Date(expiration).getTime();

    this.isLoggedIn.set(expirationTime > Date.now());
  }
}
