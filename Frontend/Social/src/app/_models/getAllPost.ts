export interface getAllPost {
  id: string;
  title: string;
  description: string;
  imageUrl: string;
  createdAtUtc: string;
  creatorById: string;
  likes: number;
  commentsCount: number;
  creatorAvatarUrl: string;
  creatorUserName: string;
}
