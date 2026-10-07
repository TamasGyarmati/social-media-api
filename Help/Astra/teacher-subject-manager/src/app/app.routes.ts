import { Routes } from '@angular/router';
import { Home } from './home/home';
import { Login } from './login/login';
import { Register } from './register/register';
import { ListSubjects } from './list-subjects/list-subjects';
import { ListTeachers } from './list-teachers/list-teachers';
import { NotFound } from './not-found/not-found';
import { Logout } from './logout/logout';
import { ApiService } from './api.service';
import { UpdateSubjects } from './update-subjects/update-subjects';
import { UpdateTeachers } from './update-teachers/update-teachers';
import { CreateSubject } from './create-subject/create-subject';
import { CreateTeacher } from './create-teacher/create-teacher';
import { ConnectTeacherToSubject } from './connect-teacher-to-subject/connect-teacher-to-subject';

export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  { path: 'home', component: Home },
  { path: 'login', component: Login },
  { path: 'logout', component: Logout },
  { path: 'register', component: Register },
  { path: 'create-subject', component: CreateSubject, canActivate: [ApiService] },
  { path: 'create-teacher', component: CreateTeacher, canActivate: [ApiService] },
  { path: 'connect', component: ConnectTeacherToSubject, canActivate: [ApiService] },
  { path: 'list-subjects', component: ListSubjects, canActivate: [ApiService] },
  { path: 'list-teachers', component: ListTeachers, canActivate: [ApiService] },
  { path: 'update-subjects/:id', component: UpdateSubjects, canActivate: [ApiService] },
  { path: 'update-teachers/:id', component: UpdateTeachers, canActivate: [ApiService] },
  { path: '**', component: NotFound, canActivate: [ApiService] },
];
