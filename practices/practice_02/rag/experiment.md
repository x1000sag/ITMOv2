# RAG

Файл ведёт OpenCode по вашим запросам. Агент записывает фактические результаты экспериментов и вносит изменения в связанные файлы. Свою оценку сообщайте ему в чате; вручную заполнять шаблон не нужно.

- Вопрос к источникам:
Как улучшить [файл](../../practice_01/tests_load.md): уточнить цели и метрики нагрузочного тестирования, предложить минимальный набор сценариев, методику профиля нагрузки (ramp-up/steady/ramp-down), определить SLI/SLO (латентность p50/p95/p99, error rate, throughput/RPS, доступность), зафиксировать отчётность и инструменты.

## Разрешённые источники

| Файл или документ | Зачем нужен | Какой фрагмент используем |
|---|---|---|
| Google SRE Book – Monitoring Distributed Systems (https://sre.google/sre-book/monitoring-distributed-systems/) | Определения SLI/SLO, ключевые метрики (латентность, ошибки, доступность) | Разделы про метрики и мониторинг, SLI/SLO |
| Google SRE Book – Service Level Objectives (https://sre.google/sre-book/service-level-objectives/) | Методика постановки SLO | Практические рекомендации по выбору SLI/SLO |
| NGINX Blog – Understanding Latency Percentiles (https://www.nginx.com/blog/latency-percentiles/) | Почему важны p95/p99, интерпретация хвоста | Разделы про интерпретацию перцентилей |
| Martin Fowler – Microservice Performance Metrics (https://martinfowler.com/articles/microservice-trade-offs.html#PerformanceMetrics) | Контекст throughput/latency и влияние на UX | Разделы про throughput, latency |
| Locust Docs – Distributed & Metrics (https://docs.locust.io/en/stable/running-locust-distributed.html) | Инструмент и метрики нагрузочного теста | Разделы про распределённый запуск, метрики (RPS, response time, failures) |
| k6 Docs – Metrics & Thresholds (https://grafana.com/docs/k6/latest/using-k6/metrics/thresholds/) | Пороговые значения метрик (thresholds) как SLO | Описание thresholds, p95/p99, http_req_failed |

## Запрос

Ты обновляешь документ [файл](../../practice_01/tests_load.md). На основе только «Разрешённых источников» выше:

1) Цели и охват:
- Сформулируй цель нагрузочного тестирования для учебного сервиса: подтверждение соответствия SLO и раннее выявление деградаций.
- Определи минимальные сценарии: базовый happy-path (GET/POST), сценарий с авторизацией (если есть), пагинация/фильтры, негативный (ожидаемые 4xx, контроль неожиданных 5xx), стресс/спайк.

2) Метрики и SLI:
- Определи SLI: латентность p50/p95/p99, error rate, throughput/RPS, доступность.
- Дай ориентиры SLO для учебного контекста, не выдумывая числа для конкретного проекта: используйте обозначения «N» (например, p95 < N мс при N RPS, http_req_failed < N%).
- Объясни, почему перцентили важнее средних значений (подтверди источниками).

3) Профиль нагрузки:
- Предложи профиль: ramp-up → steady-state → ramp-down, с примерными длительностями (например, 2–5–2 минуты) и шагами RPS; укажи, как выбирать целевой RPS из ожидаемого трафика и бюджетов задержек.

4) Инструменты и отчётность:
- Предложи инструменты: Locust или k6, укажи плюсы для учебного проекта.
- Опиши, какие отчёты сохранять: сырые результаты, графики latency percentiles, error distribution, time-series throughput.
- Предложи thresholds/пороговые проверки (k6 thresholds или проверки в Locust) в терминах SLO.

5) Практические советы:
- Как интерпретировать p95/p99 и «хвост», что делать при всплесках ошибок.
- Рекомендации по распределённому запуску (если требуется), фиксации окружения и версий для воспроизводимости.

6) заполнить таблицу 
  ```
    | Сценарий | Нагрузка и длительность | Допустимый предел | Что измеряем | Evidence |
    |---|---|---|---|---|
  ```

Формат результата: краткий, структурированный текст, который надо вставить в [файл вывода](./tests_load.md); приводить утверждения с цитатами и ссылками на «Разрешённые источники». Не добавляй фактов вне указанных источников. Используй только RAG-подход (retrieval из разрешённых источников). Обнови `Что изменили в исходном артефакте` ниже. Ссылки на источники писать ниже.

## Ответ со ссылками на источники

Результат записан в файл: [tests_load.md](./tests_load.md)

Ссылки на источники:
- Google SRE Book – Monitoring Distributed Systems: https://sre.google/sre-book/monitoring-distributed-systems/
- Google SRE Book – Service Level Objectives: https://sre.google/sre-book/service-level-objectives/
- NGINX Blog – Understanding Latency Percentiles: https://www.nginx.com/blog/latency-percentiles/
- Martin Fowler – Microservice Performance Metrics: https://martinfowler.com/articles/microservice-trade-offs.html#PerformanceMetrics
- Locust Docs – Distributed & Metrics: https://docs.locust.io/en/stable/running-locust-distributed.html
- k6 Docs – Metrics & Thresholds: https://grafana.com/docs/k6/latest/using-k6/metrics/thresholds/

## Что изменили в исходном артефакте

- Файл и раздел: practices/practice_01/tests_load.md — весь документ (переписывается целиком в итоговой версии).
- Изменение: уточнены цели и SLI/SLO (латентность p50/p95/p99, error rate, throughput, доступность) с обозначениями «N»; добавлены профиль нагрузки (ramp‑up/steady/ramp‑down) и методика выбора RPS; предложены инструменты (k6/Locust) и отчётность; расширена таблица сценариев (happy‑path, авторизация/фильтры, негативные 4xx, спайк/стресс, soak) с критериями pass/fail.
- Как проверили ссылки: каждое утверждение соотнесено с разделами указанных источников; ссылки открыты и доступны, заголовки/разделы соответствуют описанным темам.
- Что отклонили как неподтверждённое: конкретные численные пороги SLO; рекомендации вне разрешённых источников; использование средних значений вместо перцентилей; любые неисточаемые практики инструментов.
