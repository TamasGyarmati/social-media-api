import { Component } from '@angular/core';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-not-found',
  imports: [MATERIAL_IMPORTS, RouterLink],
  templateUrl: './not-found.html',
  styleUrl: './not-found.scss',
})
export class NotFound {}
