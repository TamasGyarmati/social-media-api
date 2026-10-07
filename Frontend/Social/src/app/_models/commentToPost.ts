export interface CommentToPost {
  id: string;
  content: string;
  createdAtUnc: string;
  updatedAtUtc: string;
  deletedAtUtc: string;
  postId: string;
  parentCommentId: string | null;
  createdById: string;
  userName: string;
  likes: number;
  replyCount: number;
  isLiked: boolean;
}
