import { Component, inject } from '@angular/core';
import { MATERIAL_IMPORTS } from '../_shared/material';
import {
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
} from '@angular/material/dialog';

@Component({
  selector: 'app-dialog-animation',
  imports: [MatDialogActions, MatDialogContent, MatDialogClose, MATERIAL_IMPORTS],
  templateUrl: './dialog-animation.html',
  styleUrl: './dialog-animation.scss',
})
export class DialogAnimation {
  readonly dialogRef = inject(MatDialogRef<DialogAnimation>);
}
