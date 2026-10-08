import { OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { env } from '../env/env';
import { Component, input, output, signal } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Comment } from '../_models/comment';

@Component({
  selector: 'app-comment-item',
  imports: [MATERIAL_IMPORTS, CommonModule, FormsModule],
  templateUrl: './comment-item.html',
  styleUrl: './comment-item.scss',
})
export class CommentItem implements OnInit {
  public comment = input.required<CommentToPost>();
  public allComments = input<CommentToPost[]>([]);
  public likes = signal<number>(0);
  public isLiked = signal<boolean>(false);
  public showReplyInput = signal<boolean>(false);
  public reply: string = '';
  public replyCreated = output<CommentToPost>();
  public activeReplyCommentId = input<string | null>(null);
  public replyClicked = output<string | null>();

  constructor(private httpClient: HttpClient) {}

  ngOnInit(): void {
    this.likes.set(this.comment().likes);
    this.isLiked.set(this.comment().isLiked);
  }

  createReply(comment: CommentToPost): void {
    const commentRequest: Comment = {
      content: this.reply,
      postId: comment.postId,
      parentCommentId: comment.id,
    };

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + localStorage.getItem(env.jwtAccessToken),
    });

    this.httpClient
      .post<{ comment: CommentToPost }>(env.commentUri, commentRequest, { headers: headers })
      .subscribe(
        (success) => {
          console.log('::SUCCESS::', success);
          this.replyCreated.emit(success.comment);
          this.reply = '';
          this.showReplyInput.set(false);
        },
        (error) => {
          console.log('::ERROR::', error);
        },
      );
  }

  toggleReplyInput(): void {
    if (this.activeReplyCommentId() === this.comment().id) {
      this.replyClicked.emit(null);
    } else {
      this.replyClicked.emit(this.comment().id);
    }
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
        this.isLiked.set(success.isLiked);
      },
      (error) => {
        console.log('::ERROR::', error);
      },
    );
  }

  onReplyCreated(reply: CommentToPost): void {
    this.replyCreated.emit(reply);
  }
}
