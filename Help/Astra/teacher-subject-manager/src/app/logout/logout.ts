import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { env } from '../_env/env';
import { ApiService } from '../api.service';

@Component({
  selector: 'app-logout',
  imports: [],
  templateUrl: './logout.html',
  styleUrl: './logout.scss',
})
export class Logout implements OnInit {
  constructor(
    private router: Router,
    private api: ApiService,
  ) {}

  ngOnInit(): void {
    localStorage.removeItem(env.jwtToken);
    localStorage.removeItem(env.jwtTokenExp);
    localStorage.clear();

    this.api.checkIfLoggedIn();

    this.router.navigate(['/home']);
  }
}
