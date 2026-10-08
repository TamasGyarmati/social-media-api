import { Component } from '@angular/core';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { Location } from '@angular/common';
import { inject } from '@angular/core';

@Component({
  selector: 'app-not-found',
  imports: [MATERIAL_IMPORTS],
  templateUrl: './not-found.html',
  styleUrl: './not-found.scss',
})
export class NotFound {
  private location: Location = inject(Location);

  public goBack(): void {
    this.location.back();
  }
}
