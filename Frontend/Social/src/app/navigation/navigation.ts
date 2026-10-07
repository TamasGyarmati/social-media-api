import { signal } from '@angular/core';
import { Component, OnInit } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatAnchor } from '@angular/material/button';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterLink, MatAnchor, FormsModule],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
})
export class Navigation implements OnInit {
  public activeUser = signal<string>('');

  constructor(public auth: AuthService) {}

  public logout(): void {
    this.auth.logout();
  }

  ngOnInit(): void {
    if (this.auth.isLoggedIn()) {
      this.activeUser.set(localStorage.getItem('username') ?? '');
      console.log(this.activeUser);
    }
  }
}
