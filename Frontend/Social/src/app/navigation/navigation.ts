import { signal } from '@angular/core';
import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatAnchor } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { MATERIAL_IMPORTS } from '../_shared/material';

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterLink, MatAnchor, FormsModule, MATERIAL_IMPORTS],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
})
export class Navigation {
  constructor(public auth: AuthService) {}

  isMobileMenuOpen = signal(false);

  public logout(): void {
    this.auth.logout();
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen.update((open) => !open);
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen.set(false);
  }
}
