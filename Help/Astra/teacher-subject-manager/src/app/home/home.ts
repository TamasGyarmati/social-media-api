import { Component, signal } from '@angular/core';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { HomeModel } from '../_models/home';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [MATERIAL_IMPORTS, CommonModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  public models: Array<HomeModel>;
  public isLoading = signal<boolean>(true);

  constructor() {
    this.models = [
      {
        title: 'Features',
        content: 'Manage the main data of the school system with an easy-to-use interface.',
        chips: ['Authentication', 'Create', 'Update', 'Delete'],
      },
      {
        title: 'Technologies',
        content: 'Built with modern web technologies for both the frontend and backend.',
        chips: ['Angular', 'ASP.NET Core', 'TypeScript', 'C#'],
      },
      {
        title: 'UI & Styling',
        content:
          'A responsive and consistent user interface built with popular UI and styling tools.',
        chips: ['Angular Material', 'Bootstrap', 'SCSS'],
      },
      {
        title: 'Data Management',
        content: 'Create, view, edit and manage teachers, students and their related data.',
        chips: ['Teachers', 'Students', 'Subjects', 'CRUD'],
      },
      {
        title: 'Architecture',
        content:
          'The application uses a separate Angular frontend and ASP.NET Core backend communicating through a REST API.',
        chips: ['REST API', 'HTTP', 'Frontend', 'Backend'],
      },
      {
        title: 'Developer Experience',
        content:
          'Developed with tools and practices that make the project easier to maintain and extend.',
        chips: ['Git', 'VS Code', 'Prettier', 'ESLint'],
      },
      {
        title: 'Security',

        content:
          'Secure user authentication and protected application features ensure that sensitive data remains accessible only to authorized users.',

        chips: ['Authentication', 'Authorization', 'JWT', 'Security'],
      },
      {
        title: 'API Integration',

        content:
          'A clean REST API connects the frontend and backend, enabling reliable communication and efficient data exchange.',

        chips: ['REST', 'JSON', 'HTTP', 'API'],
      },
    ];

    this.isLoading.set(false);
  }
}
