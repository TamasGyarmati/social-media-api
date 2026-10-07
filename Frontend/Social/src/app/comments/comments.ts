import { OnInit, signal } from '@angular/core';
import { env } from '../env/env';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Comment } from '../_models/comment';
import { Component, input } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { CommentItem } from '../comment-item/comment-item';
import { CommonModule } from '@angular/common';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-comments',
  imports: [CommentItem, CommonModule, MATERIAL_IMPORTS, FormsModule],
  templateUrl: './comments.html',
  styleUrl: './comments.scss',
})
export class Comments implements OnInit {
  public comments = input<CommentToPost[]>([]);
  public postId = input.required<string>();
  public commentContent: string = '';
  public localComments = signal<CommentToPost[]>([]);

  constructor(private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.localComments.set(this.comments());
  }

  createComment(): void {
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
          this.localComments.update((comments) => [success.comment, ...comments]);
          this.commentContent = '';
        },
        (error) => {
          console.log('::ERROR::', error);
        },
      );
  }

  getRootItems(): CommentToPost[] {
    return this.localComments().filter((comment) => !comment.parentCommentId);
  }
}
