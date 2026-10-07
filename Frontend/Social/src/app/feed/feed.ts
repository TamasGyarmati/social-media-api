import { CommentToPost } from '../_models/commentToPost';
import { env } from '../env/env';
import { getAllPost } from '../_models/getAllPost';
import { signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
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
  public isLoading = signal<boolean>(true);

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

        this.isLoading.set(false);
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }

  likePost(postId: string): void {
    const uri = `${env.postUri}/${postId}/like`;

    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.httpClient.post<any>(uri, null, { headers: headers }).subscribe(
      (success) => {
        console.log('::SUCCESS::', success);
        this.posts.update((posts) =>
          posts.map((post) =>
            post.id === postId ? { ...post, likes: success.likes, isLiked: success.isLiked } : post,
          ),
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
          const sortedComments = success.sort(
            (a, b) => new Date(b.createdAtUnc).getTime() - new Date(a.createdAtUnc).getTime(),
          );

          const comments = new Map(this.comments());
          comments.set(postId, sortedComments);
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
