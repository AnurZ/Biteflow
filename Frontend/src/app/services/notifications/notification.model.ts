export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  type: string;
  link?: string;
  createdAtUtc: string;
  readAtUtc?: string | null;
  isRead?: boolean;
}

export interface NotificationListResponse {
  totalCount: number;
  unreadCount: number;
  items: NotificationDto[];
}

export interface NotificationListRequest {
  pageNumber?: number;
  pageSize?: number;
  unreadOnly?: boolean;
  type?: string;
}
