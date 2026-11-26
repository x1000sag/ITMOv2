# Отчёт: тесты классификатора 7.2 (practice 7.2)

Дата прогона: 2026-11-26 (Europe/Moscow).

## GPTunnel E2E (Information Extractor)

**Дата:** 2026-11-26 (Europe/Moscow).

На cloud VM секрет **`LLM_API_KEY`** доступен в shell агента (длина 32, значение не логируется). Прогон **не** идёт через n8n UI, а имитирует ноду **Information Extractor**: тот же `systemPromptTemplate` и `inputSchema`, что в [`issue-classifier-sub.json`](issue-classifier-sub.json); endpoint Chat Completions `https://gptunnel.ru/v1/chat/completions`, **не** Responses API.

### Команда

```bash
LLM_MODEL=deepseek-v4.1-flash \
  node /cursor/stores/bc-65b848e3-d5ab-42b9-a9b2-ecbc5038f685/docs/n8n/scripts/test-classifier-llm.mjs
```

Скрипт: [`scripts/test-classifier-llm.mjs`](scripts/test-classifier-llm.mjs). Переменные: `LLM_API_KEY` (обязательно), `OPENAI_BASE_URL` (по умолчанию `https://gptunnel.ru/v1`), `LLM_MODEL` (по умолчанию `deepseek/deepseek-v4.1-flash` + fallback `deepseek-v4.1-flash`).

### Модель и credential

| Параметр | В экспорте n8n | На этой VM (ключ пользователя) |
| --- | --- | --- |
| ID модели | `deepseek/deepseek-v4.1-flash` | **`deepseek-v4.1-flash`** — 200 OK |
| `deepseek/deepseek-v4.1-flash` | — | **404** `Model not found` |

Если в n8n остаётся 404, в credential или в ноде **OpenAI Chat Model** выберите **`deepseek-v4.1-flash`**, либо добавьте namespaced id в разрешённые модели ключа в кабинете GPTunnel.

Сеть до `gptunnel.ru` нестабильна (иногда `fetch failed`); в скрипте есть короткие повторы. `GET /v1/models` в этом прогоне не ответил — список моделей проверяйте у себя.

### Кейсы ветки LLM (без heuristic short-circuit)

| ID | Вход | Ожидание | Результат |
| --- | --- | --- | --- |
| `unknown-vague` | title: `посмотрите пожалуйста` | `unknown`, conf &lt; 0.8 | **PASS** — `unknown`, **0.35** |
| `ambiguous-tooling` | «Иногда подвисает панель слева» + Chrome | `bug` или `unknown` | **PASS** — `bug`, **0.8** |
| `cyrillic-nick-503-api-only` | кириллический nick / 503 (информационно, только API) | `bug`, conf ≥ 0.85 | **PASS** — `bug`, **0.97** |

**Exit code:** 0.

Согласованность с промптом: для `посмотрите пожалуйста` LLM вернул тот же паттерн, что в примере system prompt (`unknown`, 0.35). Для явного HTTP-bug модель даёт высокую confidence без эвристического short-circuit.

## Автоматические тесты без API

### Команда

```bash
node /cursor/stores/bc-65b848e3-d5ab-42b9-a9b2-ecbc5038f685/docs/n8n/scripts/test-classifier-heuristics.mjs
```

Логика эвристик: `/cursor/stores/bc-65b848e3-d5ab-42b9-a9b2-ecbc5038f685/docs/n8n/scripts/classifier-heuristics.mjs`

### Фикстуры

| ID | Вход (title) | Ожидание | Результат |
| --- | --- | --- | --- |
| `cyrillic-nick-503` | `POST /v1/accounts с кириллицей в нике возвращает 503 вместо nick_invalid` | `bug`, conf ≥ 0.55, heuristic | **PASS** — `bug`, 0.99, `fromHeuristic: true` |
| `task-manual-test` | `Обновить manual-test.md` + тело про merge | `task`, conf ≥ 0.55 | **PASS** — `task`, 0.65 |
| `feature-filter` | `Фильтр карточек по метке` | `feature`, conf ≥ 0.55 | **PASS** — `feature`, 0.8 |
| `unknown-vague` | `посмотрите пожалуйста` | `unknown`, conf &lt; 0.55 | **PASS** — `unknown`, 0.3 |

### Валидация workflow JSON

Файл: `/cursor/stores/bc-65b848e3-d5ab-42b9-a9b2-ecbc5038f685/docs/n8n/issue-classifier-sub.json`

- Ноды: `Heuristic classify`, `If heuristic confident`, `Return heuristic`, `Extract issue type`, `Normalize` — **present**
- Связь Prepare → Heuristic → If → (Return heuristic \| Extract → Normalize) — **ok**

**Exit code:** 0

## Docker n8n smoke (без запуска LLM)

```bash
sudo docker run -d --name n8n-classifier-test -p 127.0.0.1:5678:5678 \
  -e N8N_SECURE_COOKIE=false -e N8N_DIAGNOSTICS_ENABLED=false n8nio/n8n:latest
curl -s http://127.0.0.1:5678/healthz   # → {"status":"ok"}
sudo docker cp .../issue-classifier-sub.json n8n-classifier-test:/tmp/issue-classifier-sub.json
sudo docker exec n8n-classifier-test n8n import:workflow --input=/tmp/issue-classifier-sub.json
```

| Шаг | Результат |
| --- | --- |
| Health `/healthz` | **PASS** |
| `import:workflow` | **PASS** — `Successfully imported 1 workflow.` |
| Execute sub на VM без UI / credential | **не запускался** (нужен UI или API key n8n + OpenAI у пользователя) |

Контейнер `n8n-classifier-test` на VM можно остановить: `sudo docker rm -f n8n-classifier-test`.

## Ручные шаги у пользователя (с API)

1. Импортировать `/cursor/stores/bc-65b848e3-d5ab-42b9-a9b2-ecbc5038f685/docs/n8n/issue-classifier-sub.json`.
2. Credential **OpenAI** на **OpenAI Chat Model**; модель как в экспорте или своя chat-compatible.
3. **Execute Workflow** на sub с `{ "title": "...", "body": "..." }` для case `unknown-vague` — должна пойти ветка LLM → Normalize.
4. В main подставить ID sub в **Classify issue**.
5. Открыть тестовый issue с кириллическим заголовком bug — ожидается лейбл `bug`, confidence ≥ 0.55, без needs-triage.

## Итог

- Корневую причину `unknown` ~0.3 для кириллических bug: **сломанные `\\b`-эвристики** и обязательный LLM.
- Исправление: **ранняя heuristic-ветка** + **Normalize** с `includes()`; практика 7.2 кейс **проходит без API**.
- **GPTunnel E2E:** ветка LLM проверена на живой модели; для данного ключа в API — **`deepseek-v4.1-flash`**, не `deepseek/deepseek-v4.1-flash`.

Подробности: [KanbanVibe7.2-classifier-notes.md](KanbanVibe7.2-classifier-notes.md)