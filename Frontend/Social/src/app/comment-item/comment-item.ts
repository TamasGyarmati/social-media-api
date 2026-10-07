import { Component, input } from '@angular/core';
import { CommentToPost } from '../_models/commentToPost';
import { MATERIAL_IMPORTS } from '../_shared/material';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-comment-item',
  imports: [MATERIAL_IMPORTS, CommonModule],
  templateUrl: './comment-item.html',
  styleUrl: './comment-item.scss',
})
export class CommentItem {
  public comment = input.required<CommentToPost>();

  public allComments = input<CommentToPost[]>([]);

  getReplies(): CommentToPost[] {
    return this.allComments().filter((comment) => comment.parentCommentId === this.comment().id);
  }
}
