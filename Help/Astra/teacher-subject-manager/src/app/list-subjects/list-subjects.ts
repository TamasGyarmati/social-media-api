import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { signal } from '@angular/core';
import { Subject } from '../_models/subject';
import { OnInit } from '@angular/core';
import { env } from '../_env/env';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-list-subjects',
  imports: [MATERIAL_IMPORTS, CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './list-subjects.html',
  styleUrl: './list-subjects.scss',
})
export class ListSubjects implements OnInit {
  public subjects = signal<Subject[]>([]);
  public filteredSubjects = signal<Subject[]>([]);
  public isLoading = signal<boolean>(true);
  public search = new FormControl('');

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      const search = params['search'] ?? '';
      this.search.setValue(search, { emitEvent: false });
      this.filterSubjects(search);
    });

    this.search.valueChanges.subscribe((value) => {
      this.filterSubjects(value ?? '');
    });

    this.http.get<Subject[]>(`${env.subjectUri}`).subscribe((resp) => {
      const subjects = resp.map((x) => {
        const s = new Subject();

        s.id = x.id;
        s.name = x.name;
        s.neptun = x.neptun;
        s.credit = x.credit;
        s.exam = x.exam;
        s.image = x.image;
        s.creatorName = x.creatorName;
        s.registeredStudents = x.registeredStudents;

        return s;
      });

      this.subjects.set(subjects);
      this.filterSubjects(this.search.value ?? '');

      this.isLoading.set(false);
    });
  }

  filterSubjects(search: string) {
    const query = search.toLowerCase().trim();
    this.filteredSubjects.set(
      this.subjects().filter(
        (subject) =>
          subject.name.toLowerCase().includes(query) ||
          subject.neptun.toLowerCase().includes(query),
      ),
    );
  }

  isSearchEmpty(): boolean {
    console.log(this.filteredSubjects().length);
    return this.filteredSubjects().length === 0;
  }

  clearSearch(): void {
    this.search.setValue('');
  }
}
