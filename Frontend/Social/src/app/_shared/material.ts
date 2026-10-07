import { MatOption, MatSelect } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatAnchor, MatButton } from '@angular/material/button';
import { MatCheckbox } from '@angular/material/checkbox';
import { MatDivider } from '@angular/material/divider';
import { MatError, MatFormField, MatHint, MatLabel, MatSuffix } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInput } from '@angular/material/input';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatMenuModule } from '@angular/material/menu';
import {
  MatCard,
  MatCardContent,
  MatCardFooter,
  MatCardHeader,
  MatCardSubtitle,
  MatCardTitle,
} from '@angular/material/card';

export const MATERIAL_IMPORTS = [
  MatFormField,
  MatDivider,
  MatCard,
  MatCardHeader,
  MatCardTitle,
  MatCardSubtitle,
  MatCardContent,
  MatMenuModule,
  MatInput,
  MatProgressSpinner,
  MatLabel,
  MatCheckbox,
  MatAnchor,
  MatButton,
  MatIcon,
  MatHint,
  MatSuffix,
  MatError,
  MatCardFooter,
  MatChipsModule,
  MatSelect,
  MatOption,
];
