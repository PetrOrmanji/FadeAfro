import apiClient from './client'

export interface NotificationDto {
  id: string
  text: string
  isRead: boolean
}

export const getMyNotifications = async (): Promise<NotificationDto[]> => {
  const res = await apiClient.get<{ notifications: NotificationDto[] }>('/notifications/get/me/all')
  return res.data.notifications
}

export const markAllNotificationsAsRead = async (): Promise<void> => {
  await apiClient.put('/notifications/read/me/all')
}

export const markNotificationAsRead = async (id: string): Promise<void> => {
  await apiClient.put(`/notifications/read/me/${id}`)
}
