# REPORT

## Hardware and OS
- OS: Linux (WSL2), `uname -a`: Linux Home-PC 6.18.33.2-microsoft-standard-WSL2 x86_64
- CPU: AMD Ryzen 7 7800X3D, 8 cores 
- GPU: RX 9070
- RAM: 32 GB
- VRAM: 16 GB

## Local Model
- Runtime: LM Studio (OpenAI-compatible server)
- Model ID: qwen/qwen3.8-27b
- Quantization: q4_K_M (configured in LM Studio UI)
- Context length: 30,720 tokens (configured via OpenCode limits and Modelfile `num_ctx`)
- Temperature: 1.0

## Modelfile
- lab/Modelfile: `FROM qwen/qwen3.8-27b:27b`, `num_ctx 30720`, `temperature 1.0`
- lab/Modelfile.agent: `FROM qwen/qwen3.8-27b:27b`, `num_ctx 30720`, `temperature 1.0`

## OpenCode Config
- lab/opencode.json — provider `lmstudio` at http://172.31.176.1:1234/v1
- Model mapping: `qwen/qwen3.8-27b` -> "qwen/qwen3.8-27b q4_K_M"
- Agent `local-guide`: read-only; prompt from `lab/system.txt`; steps=8; tools: read/glob/grep only

## Test Run (make test)
Executed: `make -C lab test`
Result: OK (3 tests passed)

## QUESTIONS vs Baseline

Источник кода для эталона: `lab/demo/*`

1. Как запустить тесты? Укажи файл-источник.
   - Эталон: Из `lab` — `make test` (цель `test` в lab/Makefile вызывает `make -C demo test`); источник тестов — `lab/demo/test_service.py`. Основание: lab/Makefile, lab/demo/Makefile, lab/demo/README.md, lab/demo/test_service.py.
   - Ответ локальной модели: см. [ответ Q1](#q1-answer).

2. Что будет при пустом имени подписчика? Подтверди кодом.
   - Эталон: `subscribe(" ")` вызывает `ValueError("empty name")` за счёт `if not name.strip()`. Основание: lab/demo/service.py:4–8; подтверждение: lab/demo/test_service.py:13–15.
   - Ответ локальной модели: см. [ответ Q2](#q2-answer).

3. Где реализован unsubscribe? Проверь предпосылку вопроса.
   - Эталон: Функции `unsubscribe` нет; предпосылка ложная. Основание: отсутствие в коде; grep по репозиторию указывает только на вопрос в QUESTIONS.md.
   - Ответ локальной модели: см. [ответ Q3](#q3-answer).

4. Какая CI-система запускает тесты? Если сведений нет, скажи об этом.
   - Эталон: Сведений нет; тесты локальные через `make test`. Основание: отсутствие CI-файлов (`.github/`, `.gitlab-ci.yml`, Jenkinsfile и т.д.), указание в README демо и Makefile.
   - Ответ локальной модели: см. [ответ Q4](#q4-answer).

5. Сохраняются ли подписки после перезапуска процесса? Подтверди кодом.
   - Эталон: Не сохраняются; `subscribers = set()` в памяти процесса. Основание: lab/demo/service.py:1; lab/demo/README.md: «в памяти процесса».
   - Ответ локальной модели: см. [ответ Q5](#q5-answer).

## Answers (Local Model)
<a id="q1-answer"></a>
### Q1
make test из lab; источник тестов — lab/demo/test_service.py

<a id="q2-answer"></a>
### Q2
При пустом имени — ValueError("empty name"); строки из пробелов тоже
Подтверждение кодом:

Из demo/service.py:

```python
def subscribe(name):
    if not name.strip():
        raise ValueError("empty name")
    subscribers.add(name.strip())
    return {"subscribed": True}
```

Тест в demo/test_service.py:

```python
def test_empty(self):
    with self.assertRaises(ValueError):
        subscribe(" ")
```

<a id="q3-answer"></a>
### Q3
Предпосылка неверна — unsubscribe отсутствует

<a id="q4-answer"></a>
### Q4
Сведений о CI нет; тесты запускаются локально make test

<a id="q5-answer"></a>
### Q5
Подписки не сохраняются после перезапуска процесса
Ответ локальной модели:

Нет, подписки не сохраняются после перезапуска процесса.

Основание из контекста:
- `demo/service.py`, строка 1: `subscribers = set()` — состояние хранится в переменной в памяти процесса;
- `demo/README.md`, строка 2: «Подписчики хранятся в памяти процесса».

## Наблюдения и выводы
- Модель qwen3.8-27b q4_K_M под LM Studio корректно отвечает на вопросы по небольшому Python-проекту при ограничении инструментов чтением/поиском.
- Скорость ответов стабильна; отдельные run с подробными grep/glob шагами занимают до 20–40 секунд.
- Память процесса без персистентности — важный источник ошибок; модель корректно указывает на отсутствие сохранения.

## Обоснование выбора модели
- 27B даёт надёжные пошаговые ответы на русском с хорошим следованием инструкциям.
- Квантизация q4_K_M позволяет запускать в WSL2 без дискретного GPU на 15 GiB RAM, оставаясь в разумных задержках.
