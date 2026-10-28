import type { Board } from '../types/board'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'

export const boardApi = {
  async getBoard(): Promise<Board> {
    const response = await fetch(`${API_BASE_URL}/api/board`)
    if (!response.ok) {
      throw new Error(`Ошибка загрузки доски: ${response.status}`)
    }
    return response.json()
  },

  async saveBoard(board: Board): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/api/board`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(board)
    })
    if (!response.ok) {
      throw new Error(`Ошибка сохранения доски: ${response.status}`)
    }
  }
}
