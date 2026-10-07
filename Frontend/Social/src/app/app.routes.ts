import { Routes } from '@angular/router';
import { NotFound } from './not-found/not-found';
import { Login } from './login/login';
import { Register } from './register/register';
import { Feed } from './feed/feed';

export const routes: Routes = [
  { path: '', component: Feed },
  { path: 'feed', component: Feed },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'not-found', component: NotFound },
  { path: '**', redirectTo: 'not-found', pathMatch: 'full' },
];
