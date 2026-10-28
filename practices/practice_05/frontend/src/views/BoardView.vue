<template>
  <div class="board-view">
    <header class="board-header">
      <h1>{{ boardStore.board.Name }}</h1>
      <button @click="saveChanges" :disabled="boardStore.loading" class="save-btn">Сохранить</button>
    </header>
    
    <div v-if="boardStore.loading && !boardStore.board.Columns.some(c => c.Cards.length > 0)" class="loading">
      Загрузка...
    </div>
    
    <div v-if="boardStore.error" class="error">
      {{ boardStore.error }}
    </div>
    
    <div class="board-columns">
      <Column 
        v-for="column in boardStore.board.Columns" 
        :key="column.Name"
        :column="column"
        @add-card="openAddCardModal"
        @edit-card="openEditCardModal"
        @card-drag-start="onCardDragStart"
        @drop.prevent="onColumnDrop(column.Name)"
      />
    </div>
    
    <CardModal
      v-if="showModal"
      :card="selectedCard"
      :is-edit="isEditMode"
      @save="onModalSave"
      @cancel="closeModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useBoardStore } from '../stores/boardStore'
import Column from '../components/Column.vue'
import CardModal from '../components/CardModal.vue'
import type { Card } from '../types/board'

const boardStore = useBoardStore()

const showModal = ref(false)
const selectedCard = ref<Card | null>(null)
const isEditMode = ref(false)
const currentColumnForAdd = ref('')
const draggedCard = ref<{ card: Card, fromColumn: string } | null>(null)

onMounted(() => {
  boardStore.loadBoard()
})

function openAddCardModal(columnName: string) {
  currentColumnForAdd.value = columnName
  selectedCard.value = null
  isEditMode.value = false
  showModal.value = true
}

function openEditCardModal(card: Card) {
  selectedCard.value = { ...card }
  isEditMode.value = true
  showModal.value = true
}

function closeModal() {
  showModal.value = false
  selectedCard.value = null
}

function onModalSave(cardData: Partial<Card>) {
  if (isEditMode.value && cardData.Id) {
    // Update existing card
    const updatedCard: Card = {
      Id: cardData.Id,
      Title: cardData.Title!,
      Description: cardData.Description ?? null,
      Tags: cardData.Tags || [],
      Deadline: cardData.Deadline ?? null,
      CreatedAt: cardData.CreatedAt || new Date().toISOString()
    }
    boardStore.updateCard(updatedCard)
  } else {
    // Create new card
    boardStore.addCard(currentColumnForAdd.value, {
      Title: cardData.Title!,
      Description: cardData.Description ?? null,
      Tags: cardData.Tags || [],
      Deadline: cardData.Deadline ?? null
    } as any)
  }
  closeModal()
  // Auto-save after changes
  boardStore.saveBoard().catch(() => {})
}

function onCardDragStart(card: Card, columnName: string) {
  draggedCard.value = { card, fromColumn: columnName }
}

function onColumnDrop(toColumnName: string) {
  if (draggedCard.value && draggedCard.value.fromColumn !== toColumnName) {
    boardStore.moveCard(draggedCard.value.card.Id, draggedCard.value.fromColumn, toColumnName)
    boardStore.saveBoard().catch(() => {})
  }
  draggedCard.value = null
}

async function saveChanges() {
  try {
    await boardStore.saveBoard()
    alert('Доска сохранена')
  } catch (e) {
    // Error is already set in store
  }
}
</script>

<style scoped>
.board-view {
  padding: 20px;
}

.board-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.board-header h1 {
  margin: 0;
}

.save-btn {
  background: #2196F3;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  cursor: pointer;
}

.save-btn:disabled {
  background: #ccc;
}

.board-columns {
  display: flex;
  gap: 20px;
  overflow-x: auto;
}

.loading {
  text-align: center;
  padding: 20px;
}

.error {
  color: red;
  padding: 10px;
  background: #ffebee;
  border-radius: 3px;
  margin-bottom: 10px;
}
</style>
