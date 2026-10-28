<template>
  <div 
    class="card" 
    draggable="true"
    @dragstart="onDragStart"
    @click="onClick"
  >
    <div class="card-title">{{ card.Title }}</div>
    <div v-if="card.Description" class="card-description">{{ card.Description }}</div>
    <div v-if="card.Tags && card.Tags.length > 0" class="card-tags">
      <span v-for="tag in card.Tags" :key="tag" class="tag">{{ tag }}</span>
    </div>
    <div v-if="card.Deadline" class="card-deadline">Дедлайн: {{ card.Deadline }}</div>
  </div>
</template>

<script setup lang="ts">
import type { Card } from '../types/board'

const props = defineProps<{
  card: Card
  columnName: string
}>()

const emit = defineEmits<{
  edit: [card: Card]
  dragstart: [card: Card, columnName: string]
}>()

function onDragStart(event: DragEvent) {
  emit('dragstart', props.card, props.columnName)
  if (event.dataTransfer) {
    event.dataTransfer.effectAllowed = 'move'
  }
}

function onClick() {
  emit('edit', props.card)
}
</script>

<style scoped>
.card {
  background: white;
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 10px;
  margin-bottom: 8px;
  cursor: pointer;
  box-shadow: 0 1px 2px rgba(0,0,0,0.1);
}

.card:hover {
  box-shadow: 0 2px 4px rgba(0,0,0,0.15);
}

.card-title {
  font-weight: bold;
  margin-bottom: 4px;
}

.card-description {
  font-size: 0.9em;
  color: #666;
  margin-bottom: 4px;
}

.card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-bottom: 4px;
}

.tag {
  background: #e0e0e0;
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 0.8em;
}

.card-deadline {
  font-size: 0.8em;
  color: #d33;
}
</style>
