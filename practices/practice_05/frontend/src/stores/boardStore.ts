import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { Board, Card } from '../types/board'
import { boardApi } from '../services/boardApi'

export const useBoardStore = defineStore('board', () => {
  const board = ref<Board>({
    Name: 'My Board',
    Columns: [
      { Name: 'ToDo', Cards: [] },
      { Name: 'InProgress', Cards: [] },
      { Name: 'Done', Cards: [] }
    ]
  })
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function loadBoard() {
    loading.value = true
    error.value = null
    try {
      board.value = await boardApi.getBoard()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Неизвестная ошибка'
    } finally {
      loading.value = false
    }
  }

  async function saveBoard() {
    loading.value = true
    error.value = null
    try {
      await boardApi.saveBoard(board.value)
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Ошибка сохранения'
      throw e
    } finally {
      loading.value = false
    }
  }

  function addCard(columnName: string, card: Omit<Card, 'Id' | 'CreatedAt'>) {
    const column = board.value.Columns.find(c => c.Name === columnName)
    if (column) {
      const newCard: Card = {
        Id: crypto.randomUUID(),
        Title: card.Title,
        Description: card.Description,
        Tags: card.Tags || [],
        Deadline: card.Deadline,
        CreatedAt: new Date().toISOString()
      }
      column.Cards.push(newCard)
    }
  }

  function updateCard(updatedCard: Card) {
    for (const column of board.value.Columns) {
      const index = column.Cards.findIndex(c => c.Id === updatedCard.Id)
      if (index !== -1) {
        column.Cards[index] = updatedCard
        break
      }
    }
  }

  function moveCard(cardId: string, fromColumnName: string, toColumnName: string) {
    const fromColumn = board.value.Columns.find(c => c.Name === fromColumnName)
    const toColumn = board.value.Columns.find(c => c.Name === toColumnName)
    if (fromColumn && toColumn && fromColumnName !== toColumnName) {
      const cardIndex = fromColumn.Cards.findIndex(c => c.Id === cardId)
      if (cardIndex !== -1) {
        const [card] = fromColumn.Cards.splice(cardIndex, 1)
        toColumn.Cards.push(card)
      }
    }
  }

  return {
    board,
    loading,
    error,
    loadBoard,
    saveBoard,
    addCard,
    updateCard,
    moveCard
  }
})
