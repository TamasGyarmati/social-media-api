import { Component } from '@angular/core';
import { env } from '../_env/env';
import { Teacher } from '../_models/teacher';
import { OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-list-teachers',
  imports: [MATERIAL_IMPORTS, CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './list-teachers.html',
  styleUrl: './list-teachers.scss',
})
export class ListTeachers implements OnInit {
  public teachers = signal<Teacher[]>([]);
  public filteredTeachers = signal<Teacher[]>([]);
  public isLoading = signal<boolean>(true);
  public search = new FormControl('');

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.search.valueChanges.subscribe((value) => {
      this.filterTeachers(value ?? '');
    });

    this.http.get<Teacher[]>(`${env.teacherUri}`).subscribe((resp) => {
      const teachers = resp.map((x) => {
        let t = new Teacher();

        t.id = x.id;
        t.name = x.name;
        t.neptun = x.neptun;
        t.birthYear = x.birthYear;
        t.image = x.image;
        t.creatorName = x.creatorName;
        t.createSubjects(x.teachedSubjects);

        return t;
      });

      this.teachers.set(teachers);
      this.filteredTeachers.set(teachers);

      this.isLoading.set(false);
    });
  }

  filterTeachers(search: string) {
    const query = search.toLowerCase().trim();
    this.filteredTeachers.set(
      this.teachers().filter(
        (teacher) =>
          teacher.name.toLowerCase().includes(query) ||
          teacher.neptun.toLowerCase().includes(query),
      ),
    );
  }

  isSearchEmpty(): boolean {
    return this.filteredTeachers().length === 0;
  }

  clearSearch(): void {
    this.search.setValue('');
  }
}
