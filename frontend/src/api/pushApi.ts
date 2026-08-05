import axiosInstance from './axiosInstance'

export const pushApi = {
  getVapidPublicKey: async (): Promise<string> => {
    const response = await axiosInstance.get<{ publicKey: string }>('/push/vapid-public-key')
    return response.data.publicKey
  },
  subscribe: async (subscription: { endpoint: string; p256dh: string; auth: string }): Promise<void> => {
    await axiosInstance.post('/push/subscribe', subscription)
  },
  unsubscribe: async (endpoint: string): Promise<void> => {
    await axiosInstance.post('/push/unsubscribe', { endpoint })
  },
}
