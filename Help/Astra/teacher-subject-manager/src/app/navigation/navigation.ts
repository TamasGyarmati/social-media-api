import { Component } from '@angular/core';
import { OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { ApiService } from '../api.service';
import { CommonModule } from '@angular/common';
import { signal } from '@angular/core';

@Component({
  selector: 'app-navigation',
  imports: [MATERIAL_IMPORTS, RouterLink, CommonModule],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
})
export class Navigation implements OnInit {
  constructor(public api: ApiService) {}

  ngOnInit(): void {
    this.api.checkIfLoggedIn();
  }
}
