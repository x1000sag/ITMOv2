# Журнал экспериментов Практики 2

- Выбранный слабый артефакт Практики 1: [Tests load](../../practice_01/tests_load.md).

| Техника | Файл эксперимента | Изменённый файл Практики 1 | Конкретное изменение | Проверка | Что отклонили |
|---|---|---|---|---|---|
| Few-shot | [`few_shot/experiment.md`](few_shot/experiment.md) | practices/practice_01/tests_load.md | Добавлены SLI/SLO (p50/p95/p99, T_req), сценарии (прогрев/спайк/soak/границы/отказы LLM), раздел «Метрики и артефакты», методология запуска; числа помечены «уточнить» | Проверка: наличие разделов и критериев pass/fail в Output [`few_shot/test_load.md`](few_shot/test_load.md) | ничего |
| R.C.T.F. | [`rctf/experiment.md`](rctf/experiment.md) | practices/practice_01/tests_load.md | Полная замена содержимого: структурированные SLO, «Метрики и артефакты», расширенные сценарии, «Методология» и терминология; пометки «N» вместо чисел | Проверка: готовый Markdown в [`rctf/tests_load.md`](rctf/tests_load.md), соответствие OUT-1/SEC-1/API-1/REL-1/QA-1/OBS-1 | ничего |
| Chain of Verification | [`chain_of_verification/experiment.md`](chain_of_verification/experiment.md) | practices/practice_01/tests_load.md | Минимальные правки по результатам CoV-вопросов: добавлены недостающие сценарии и критерии, терминология и раздел «Метрики и артефакты» | Проверка: таблица «Вопрос | Источник | Результат», исправленный фрагмент в [`chain_of_verification/tests_load.md`](chain_of_verification/tests_load.md) | ничего |
| Tree of Thoughts | [`tree_of_thoughts/experiment.md`](tree_of_thoughts/experiment.md) | practices/practice_01/tests_load.md | Сформирован ToT‑запрос; через ветвление выбраны минимальные правки: SLO, сценарии, «Метрики и артефакты», терминология; итог записать в [`tree_of_thoughts/tests_load.md`](tree_of_thoughts/tests_load.md) | Проверка: заполнена таблица альтернатив, зафиксирован выбор и проверка; наличие итогового файла | ничего |
| RAG | [`rag/experiment.md`](rag/experiment.md) | practices/practice_01/tests_load.md | Обновление с опорой на разрешённые источники: цели/охват, SLI/SLO с «N», профиль нагрузки, инструменты и отчётность, расширенная таблица сценариев | Проверка: соответствие цитатам из источников, результат в [`rag/tests_load.md`](rag/tests_load.md), ссылки валидны | ничего |
| ReAct | [`react/experiment.md`](react/experiment.md) | practices/practice_01/tests_load.md | Точечные правки по шагам ReAct: добавлены ramp/soak/границы API/отказы LLM; p50/p99 как «N»; политика 4xx; артефакты воспроизводимости | Проверка: протокол действий и результат в [`react/tests_load.md`](react/tests_load.md); соответствие Context Pack | ничего |

## Независимое ревью

| Замечание другой команды | Где исправили | Evidence |
|---|---|---|
| Двусмысленность |  |  |
| Непроверяемое требование |  |  |
| Пропущенный риск или источник |  |  |
