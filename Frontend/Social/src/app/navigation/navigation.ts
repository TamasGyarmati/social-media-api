import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatAnchor } from '@angular/material/button';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterLink, MatAnchor],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
})
export class Navigation {
  constructor(public auth: AuthService) {}

  public logout(): void {
    this.auth.logout();
  }
}
