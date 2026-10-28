<template>
  <div class="column" @dragover.prevent @drop="onDrop">
    <div class="column-header">
      <h3>{{ column.Name }}</h3>
      <button @click="onAddCard" class="add-btn">+ Добавить</button>
    </div>
    <div class="column-cards">
      <Card 
        v-for="card in column.Cards" 
        :key="card.Id" 
        :card="card" 
        :column-name="column.Name"
        @edit="onEditCard"
        @dragstart="onCardDragStart"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Column, Card } from '../types/board'
import CardComponent from './Card.vue'

const props = defineProps<{
  column: Column
}>()

const emit = defineEmits<{
  addCard: [columnName: string]
  editCard: [card: Card]
  cardDragStart: [card: Card, columnName: string]
  cardDrop: [cardId: string, fromColumn: string, toColumn: string]
}>()

const Card = CardComponent

function onAddCard() {
  emit('addCard', props.column.Name)
}

function onEditCard(card: Card) {
  emit('editCard', card)
}

function onCardDragStart(card: Card, columnName: string) {
  emit('cardDragStart', card, columnName)
}

function onDrop(event: DragEvent) {
  event.preventDefault()
  // The actual drop handling is done by parent via store
}
</script>

<style scoped>
.column {
  background: #f5f5f5;
  border-radius: 4px;
  padding: 10px;
  width: 300px;
  min-height: 400px;
}

.column-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.column-header h3 {
  margin: 0;
  font-size: 1.1em;
}

.add-btn {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 4px 8px;
  border-radius: 3px;
  cursor: pointer;
}

.add-btn:hover {
  background: #45a049;
}

.column-cards {
  min-height: 350px;
}
</style>
