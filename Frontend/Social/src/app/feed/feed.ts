import { CommentToPost } from '../_models/commentToPost';
import { env } from '../env/env';
import { getAllPost } from '../_models/getAllPost';
import { signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { Comments } from '../comments/comments';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, FormsModule, MATERIAL_IMPORTS, Comments],
  templateUrl: './feed.html',
  styleUrl: './feed.scss',
})
export class Feed implements OnInit {
  public apiUri = env.apiUri;
  public posts = signal<getAllPost[]>([]);
  public openedComments = signal<Set<string>>(new Set());
  public comments = signal<Map<string, CommentToPost[]>>(new Map());

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

  getComments(postId: string): void {
    const opened = new Set(this.openedComments());

    if (opened.has(postId)) {
      opened.delete(postId);
      this.openedComments.set(opened);
      return;
    }

    if (!this.comments().has(postId)) {
      this.httpClient.get<CommentToPost[]>(`${env.commentGetByPostIdUri}/${postId}`).subscribe(
        (success) => {
          const comments = new Map(this.comments());
          comments.set(postId, success);

          this.comments.set(comments);

          opened.add(postId);
          this.openedComments.set(opened);

          console.log('::SUCCESS::', success);
        },
        (error) => {
          console.log('::ERROR::', error);
        },
      );
      return;
    }
    opened.add(postId);
    this.openedComments.set(opened);
  }
}
