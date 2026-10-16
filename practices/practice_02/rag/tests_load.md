# Нагрузочные проверки

## Цели и охват

- Цель: подтвердить соответствие SLO и раннее выявление деградаций сервиса на основе измеримых SLI [Google SRE - Monitoring Distributed Systems, Service Level Objectives].
- Сценарии (минимальный набор):
  - Базовый happy-path (типовые GET/POST).
  - С авторизацией (если применимо) и пагинацией/фильтрами.
- Негативный: ожидаемые 4xx, контроль неожиданных 5xx.
  - Пик/спайк и стресс.

## Метрики и SLI

- SLI:
  - Латентность: p50, p95, p99 времени ответа по ключевым эндпойнтам.
  - Error rate: доля неуспешных ответов (5xx; отдельно отслеживать ожидаемые 4xx).
  - Throughput: RPS/завершённых запросов/сек.
  - Доступность: доля успешно обслуженных запросов за интервал наблюдения.
- SLO (для учебного контекста: обозначать "N"):
  - p95 < N мс при N RPS; p99 < N мс; p50 < N мс.
  - http_req_failed (или эквивалент) < N%.
  - Доступность ≥ 100% − N% за период.
- Почему перцентили важнее средних: средние значения маскируют "хвост" распределения, тогда как p95/p99 показывают опыт большинства и хвостовые задержки, влияющие на UX [NGINX Blog - Understanding Latency Percentiles].

## Профиль нагрузки

- Методика: ramp-up -> steady-state -> ramp-down.
  - Пример: 2 мин прогрев (ramp-up), 5 мин плато (steady), 2 мин спад (ramp-down).
  - Шаги RPS: увеличивать ступенчато до целевого N RPS, фиксируя p50/p95/p99 и error rate на каждой ступени.
- Выбор целевого RPS: из ожидаемого трафика (средние и пиковые значения) и бюджета задержек, согласованных в SLO [Google SRE - Service Level Objectives; Martin Fowler - Microservice Performance Metrics].

## Инструменты и отчётность

- Инструменты:
- k6: пороговые проверки (thresholds) как выражение SLO; встроенные метрики p95/p99, http_req_failed [k6 Docs - Metrics & Thresholds].
  - Locust: гибкая модель пользователей, распределённый запуск при необходимости; метрики RPS/латентности/ошибок [Locust Docs - Distributed & Metrics].
- Отчётность и артефакты:
  - Сырые результаты прогона.
- Графики latency percentiles (p50/p95/p99), error distribution, time-series throughput.
  - Ссылки/скриншоты APM/трейсинга, если используются.
- Пороговые проверки:
- k6 thresholds: p95 < N, p99 < N, http_req_failed < N% (соответствие SLO) [k6 Docs - Thresholds].
  - Locust: мониторинг метрик RPS/латентности/ошибок и оценка соответствия SLO по отчётам; при необходимости распределённый запуск [Locust Docs - Distributed & Metrics].

## Практические советы

- Интерпретация p95/p99 и "хвоста": p95 отражает опыт 95% запросов, p99 - хвост; деградации в хвосте критичны для UX, даже при "хорошем" среднем [NGINX Blog - Latency Percentiles].
- При всплесках ошибок: фиксировать временные интервалы и коррелировать с throughput/внешними зависимостями; нарушение порогов - fail прогона [Google SRE - Monitoring].
- Распределённый запуск: при высокой цели RPS - масштабировать генераторы (Locust distributed) [Locust Docs].
- Воспроизводимость: фиксировать версии инструмента, скриптов и параметров запуска; сохранять конфигурации вместе с отчётами [Google SRE - Monitoring].

## Таблица сценариев

| Сценарий | Нагрузка и длительность | Допустимый предел | Что измеряем | Evidence |
|---|---|---|---|---|
| Прогрев | 1 → N RPS, 2 мин | p95 < N мс; http_req_failed < N% | p50/p95/p99; error rate | Отчёт нагрузки |
| Базовый happy‑path | N RPS, 5 мин | p95 < N мс; p99 < N мс; 5xx = 0% | Латентность, 5xx; доступность | Отчёт + графики |
| С авторизацией/фильтрами | N RPS, 5 мин | как для базового | Латентность; ошибки | Отчёт |
| Негативный (4xx ожидаемые) | N RPS, 3 мин | ожидаемые 4xx согласно контракту; 5xx = 0% | Доля 4xx; неожид. 5xx | Отчёт |
| Пик/спайк | до N RPS, 2 мин | p95 < N мс; p99 < N мс; 5xx = 0% | Устойчивость под пиком | Графики APM |
| Стресс (ступени) | 1 → N RPS, шаг N/мин, 10 мин | без скачков ошибок; перцентили в пределах SLO | Перцентили по ступеням; 5xx | Отчёт |
| Soak | N RPS, 30-60 мин | отсутствие тренда деградации; 5xx = 0% | Тренд латентности; ресурсы | APM/отчёт |

## Источники

- Google SRE Book - Monitoring Distributed Systems: https://sre.google/sre-book/monitoring-distributed-systems/
- Google SRE Book - Service Level Objectives: https://sre.google/sre-book/service-level-objectives/
- NGINX Blog - Understanding Latency Percentiles: https://www.nginx.com/blog/latency-percentiles/
- Martin Fowler - Microservice Performance Metrics: https://martinfowler.com/articles/microservice-trade-offs.html#PerformanceMetrics
- Locust Docs - Distributed & Metrics: https://docs.locust.io/en/stable/running-locust-distributed.html
- k6 Docs - Metrics & Thresholds: https://grafana.com/docs/k6/latest/using-k6/metrics/thresholds/
