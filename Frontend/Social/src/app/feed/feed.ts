import { env } from '../env/env';
import { getAllPost } from '../_models/getAllPost';
import { signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, FormsModule],
  templateUrl: './feed.html',
  styleUrl: './feed.scss',
})
export class Feed implements OnInit {
  public apiUri = env.apiUri;
  public posts = signal<getAllPost[]>([]);

  constructor(private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.httpClient.get<getAllPost[]>(env.postGetAllUri).subscribe(
      (success) => {
        console.log(success);
        this.posts.set(
          success.map((post) => ({
            ...post,
          })),
        );
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }
}
