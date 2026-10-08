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
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-feed',
  imports: [CommonModule, FormsModule, MATERIAL_IMPORTS, Comments, RouterLink],
  templateUrl: './feed.html',
  styleUrl: './feed.scss',
})
export class Feed implements OnInit {
  public apiUri = env.apiUri;
  public posts = signal<getAllPost[]>([]);
  public openedComments = signal<Set<string>>(new Set());
  public comments = signal<Map<string, CommentToPost[]>>(new Map());
  public isLoading = signal<boolean>(true);
  public currentUserName = localStorage.getItem('username');

  constructor(
    private httpClient: HttpClient,
    private matBar: MatSnackBar,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.httpClient.get<getAllPost[]>(env.postGetAllUri).subscribe(
      (success) => {
        const sortedPosts = success.sort(
          (a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime(),
        );
        this.posts.set(
          sortedPosts.map((post) => ({
            ...post,
          })),
        );

        this.isLoading.set(false);

        console.log('::LOADED::', success);
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }

  getDayCount(post: getAllPost): string {
    const now = new Date();
    const created = new Date(post.createdAtUtc);
    const difference = now.getTime() - created.getTime();
    const result = Math.floor(difference / (1000 * 60 * 60 * 24));

    if (result === 0) {
      return 'Posted today';
    } else {
      return `Posted ${result.toString()} days ago`;
    }
  }

  likePost(postId: string): void {
    const token = localStorage.getItem(env.jwtAccessToken);
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

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

        console.log('opened comments:', this.openedComments());
        console.log('comments:', this.comments());
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }

  deletePost(post: getAllPost): void {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.httpClient.delete(`${env.postUri}/${post.id}`, { headers: headers }).subscribe(
      (success) => {
        this.posts.update((posts) => posts.filter((x) => x.id !== post.id));
        this.matBar.open('Successfully deleted the post!', 'Close', { duration: 3000 });
        console.log('::SUCCESS::', success);
      },
      (error) => {
        if (error.status == '403') {
          this.matBar.open('Cannot delete post!', 'Close', { duration: 3000 });
        }
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

  addComment(postId: string, comment: CommentToPost): void {
    this.comments.update((comments) => {
      const newComments = new Map(comments);
      newComments.set(postId, [comment, ...(newComments.get(postId) ?? [])]);
      return newComments;
    });

    // ideiglenesen a memoriában lévő frontend commentCount-ot növeljük
    this.posts.update((posts) =>
      posts.map((post) =>
        post.id === postId ? { ...post, commentsCount: post.commentsCount + 1 } : post,
      ),
    );
  }
}
