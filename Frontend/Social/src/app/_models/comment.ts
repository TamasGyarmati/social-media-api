export interface Comment {
  content: string;
  postId: string;
  parentCommentId: string | null;
}
