import { env } from '../env/env';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Comment } from '../_models/comment';
import { Component, input, output, signal } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { CommentItem } from '../comment-item/comment-item';
import { CommonModule } from '@angular/common';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-comments',
  imports: [CommentItem, CommonModule, MATERIAL_IMPORTS, FormsModule],
  templateUrl: './comments.html',
  styleUrl: './comments.scss',
})
export class Comments {
  public comments = input<CommentToPost[]>([]);
  public commentCreated = output<CommentToPost>();
  public replyCreated = output<CommentToPost>();
  public postId = input.required<string>();
  public commentContent: string = '';
  public activeReplyCommentId = signal<string | null>(null);

  constructor(
    private httpClient: HttpClient,
    private router: Router,
  ) {}

  onReplyCreated(reply: CommentToPost): void {
    this.replyCreated.emit(reply);
  }

  onReplyClicked(commentId: string | null): void {
    this.activeReplyCommentId.set(commentId);
  }

  createComment(): void {
    const token = localStorage.getItem(env.jwtAccessToken);
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    const commentRequest: Comment = {
      content: this.commentContent,
      postId: this.postId(),
      parentCommentId: null,
    };

    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.httpClient
      .post<{ comment: CommentToPost }>(env.commentUri, commentRequest, { headers: headers })
      .subscribe(
        (success) => {
          console.log('::SUCCESS::', success);
          this.commentCreated.emit(success.comment);
          this.commentContent = '';
        },
        (error) => {
          console.log('::ERROR::', error);
        },
      );
  }

  getRootItems(): CommentToPost[] {
    return this.comments().filter((comment) => !comment.parentCommentId);
  }
}
