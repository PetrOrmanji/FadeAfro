# ✂️ FadeAfro

> Telegram Mini App для записи клиентов к мастерам барбершопа.
> Клиенты выбирают мастера, услуги, дату и время — мастера управляют расписанием и записями.

---

## 📋 Содержание

1. [Структура проекта](#1-структура-проекта)
2. [Требования](#2-требования)
3. [Запуск локально (без Docker)](#3-запуск-локально-без-docker)
4. [Запуск через Docker](#4-запуск-через-docker)

---

## 1. Структура проекта

```
FadeAfro/
├── FadeAfro/              # Backend — ASP.NET Core 10
├── FadeAfroFront/         # Frontend — React 19 + TypeScript + Vite
├── docker-compose.yml     # Оркестрация сервисов
└── .env                   # Переменные окружения
```

Подробная документация по каждой части:
- 📄 [Backend README](./FadeAfro/README.md)
- 📄 [Frontend README](./FadeAfroFront/README.md)

---

## 2. Требования

Для запуска через Docker:
- [Docker](https://docs.docker.com/get-docker/) + [Docker Compose](https://docs.docker.com/compose/)

Для локальной разработки:
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- PostgreSQL 17+

---

## 3. Запуск локально (без Docker)

### Требования

Убедись, что запущен PostgreSQL 17+. Быстрый старт через Docker (только БД):

```bash
docker run -d --name fadeafro-postgres \
  -e POSTGRES_DB=fadeafro \
  -e POSTGRES_USER=fadeafro \
  -e POSTGRES_PASSWORD=fadeafro \
  -p 5432:5432 postgres:17
```

### 1. Бэкенд

```bash
cd FadeAfro
```

Заполни `FadeAfro/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=fadeafro;Username=fadeafro;Password=fadeafro"
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "fadeafro",
    "Audience": "fadeafro",
    "ExpirationMinutes": 60
  },
  "Telegram": {
    "BotToken": "..."
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173"]
  }
}
```

> **JWT Key** — произвольная строка, минимум 32 байта:
> ```bash
> openssl rand -hex 32
> ```

Запуск:

```bash
dotnet run --project FadeAfro
```

Бэкенд: `http://localhost:5000`, Swagger — там же. Миграции применятся автоматически.

### 2. Фронтенд

```bash
cd FadeAfroFront
npm install
npm run dev
```

Фронтенд: `http://localhost:5173`. Запросы `/api/*` проксируются на бэкенд автоматически.

---

## 4. Запуск через Docker

Используется совместно с уже развёрнутой инфраструктурой (PostgreSQL, Nginx Proxy Manager) в сети `infra-network`.

### Конфигурация

Создай `.env` рядом с `docker-compose.yml`:

```env
POSTGRES_DB=fadeafro
POSTGRES_USER=
POSTGRES_PASSWORD=
JWT_SECRET_KEY=
TELEGRAM_BOT_TOKEN=
```

### Сервисы

| Сервис | Описание |
|--------|----------|
| `fade-afro-back` | ASP.NET Core API, запускается от непривилегированного пользователя `appuser` |
| `fade-afro-front` | React SPA + Nginx, стартует после того как бэкенд прошёл healthcheck |

Оба сервиса подключаются к внешней сети `infra-network` — бэкенд обращается к уже запущенному PostgreSQL, фронтенд доступен для Nginx Proxy Manager.

### Запуск

```bash
docker compose up -d --build
```

В Nginx Proxy Manager настрой Proxy Host:
- **Forward Hostname:** `fadeafro-frontend`
- **Forward Port:** `80`
- Включи SSL через Let's Encrypt

> Healthcheck бэкенда: `GET /health` — проверяет доступность PostgreSQL.
