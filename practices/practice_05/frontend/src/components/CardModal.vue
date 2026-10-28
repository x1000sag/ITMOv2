<template>
  <div class="modal-overlay" @click="onCancel">
    <div class="modal-content" @click.stop>
      <h2>{{ isEdit ? 'Редактировать карточку' : 'Создать карточку' }}</h2>
      
      <div class="form-group">
        <label>Заголовок *</label>
        <input v-model="form.Title" type="text" />
      </div>
      
      <div class="form-group">
        <label>Описание</label>
        <textarea v-model="form.Description"></textarea>
      </div>
      
      <div class="form-group">
        <label>Теги (через запятую)</label>
        <input v-model="tagsInput" type="text" placeholder="тег1, тег2" />
      </div>
      
      <div class="form-group">
        <label>Дедлайн (YYYY-MM-DD)</label>
        <input v-model="form.Deadline" type="date" />
      </div>
      
      <div class="modal-actions">
        <button @click="onSave" class="save-btn">Сохранить</button>
        <button @click="onCancel" class="cancel-btn">Отмена</button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import type { Card } from '../types/board'

const props = defineProps<{
  card?: Card | null
  isEdit?: boolean
}>()

const emit = defineEmits<{
  save: [card: Partial<Card>]
  cancel: []
}>()

const form = ref<Partial<Card>>({
  Title: '',
  Description: '',
  Tags: [],
  Deadline: null
})

const tagsInput = ref('')

const isEdit = computed(() => props.isEdit || !!props.card?.Id)

watch(() => props.card, (newCard) => {
  if (newCard) {
    form.value = {
      Id: newCard.Id,
      Title: newCard.Title,
      Description: newCard.Description || '',
      Tags: [...newCard.Tags],
      Deadline: newCard.Deadline,
      CreatedAt: newCard.CreatedAt
    }
    tagsInput.value = newCard.Tags.join(', ')
  } else {
    form.value = {
      Title: '',
      Description: '',
      Tags: [],
      Deadline: null
    }
    tagsInput.value = ''
  }
}, { immediate: true })

function onSave() {
  if (!form.value.Title || form.value.Title.trim() === '') {
    alert('Заголовок обязателен')
    return
  }
  
  // Parse tags
  const tags = tagsInput.value
    .split(',')
    .map(t => t.trim())
    .filter(t => t !== '')
  
  // Check unique tags
  const uniqueTags = [...new Set(tags)]
  if (uniqueTags.length !== tags.length) {
    alert('Теги должны быть уникальными')
    return
  }
  
  emit('save', {
    ...form.value,
    Title: form.value.Title.trim(),
    Description: form.value.Description?.trim() || null,
    Tags: uniqueTags,
    Deadline: form.value.Deadline || null
  })
}

function onCancel() {
  emit('cancel')
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  padding: 20px;
  border-radius: 4px;
  width: 400px;
  max-width: 90%;
}

.modal-content h2 {
  margin-top: 0;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input,
.form-group textarea {
  width: 100%;
  padding: 8px;
  box-sizing: border-box;
  border: 1px solid #ddd;
  border-radius: 3px;
}

.form-group textarea {
  resize: vertical;
  min-height: 60px;
}

.modal-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
}

.save-btn {
  background: #4CAF50;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  cursor: pointer;
}

.cancel-btn {
  background: #f44336;
  color: white;
  border: none;
  padding: 8px 16px;
  border-radius: 3px;
  cursor: pointer;
}
</style>
