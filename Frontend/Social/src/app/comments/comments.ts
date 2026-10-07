import { Component, input, OnInit } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { CommentItem } from '../comment-item/comment-item';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-comments',
  imports: [CommentItem, CommonModule],
  templateUrl: './comments.html',
  styleUrl: './comments.scss',
})
export class Comments implements OnInit {
  public comments = input<CommentToPost[]>([]);

  ngOnInit(): void {
    console.log('ALL COMMENTS:', this.comments());
    console.log('ROOT COMMENTS:', this.getRootItems());
  }

  getRootItems(): CommentToPost[] {
    return this.comments().filter((comment) => !comment.parentCommentId);
  }
}
