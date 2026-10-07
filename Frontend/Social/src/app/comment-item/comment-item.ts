import { OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { env } from '../env/env';
import { Component, input, signal } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-comment-item',
  imports: [MATERIAL_IMPORTS, CommonModule],
  templateUrl: './comment-item.html',
  styleUrl: './comment-item.scss',
})
export class CommentItem implements OnInit {
  public comment = input.required<CommentToPost>();
  public allComments = input<CommentToPost[]>([]);
  public likes = signal<number>(0);

  constructor(private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.likes.set(this.comment().likes);
  }

  getReplies(): CommentToPost[] {
    return this.allComments().filter((comment) => comment.parentCommentId === this.comment().id);
  }

  likeComment(commentId: string): void {
    const uri = `${env.commentUri}/${commentId}/like`;

    let headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.httpClient.post<any>(uri, null, { headers: headers }).subscribe(
      (success) => {
        console.log('::SUCCESS::', success);
        this.likes.set(success.likes);
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }
}
