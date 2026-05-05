# FadeAfro — Frontend

> React-фронтенд для **FadeAfro** — Telegram Mini App для записи клиентов к мастерам барбершопа.

---

## Содержание

1. [Общее описание](#1-общее-описание)
2. [Требования](#2-требования)
3. [Технологии](#3-технологии)
4. [Архитектура](#4-архитектура)
5. [Страницы](#5-страницы)
6. [Запуск](#6-запуск)

---

## 1. Общее описание

SPA-приложение, встроенное в Telegram как Mini App. Адаптировано под нативный Telegram-дизайн: цвета, кнопка «Назад», алерты через Telegram SDK.

Возможности:
- аутентификация через Telegram initData (без логина и пароля)
- просмотр мастеров и их доступности
- запись к мастеру с выбором услуг, даты и времени
- управление своими записями и их отмена
- уведомления
- панель мастера: расписание, недоступность, услуги, записи
- панель владельца: управление пользователями и мастерами

---

## 2. Требования

- [Node.js 22+](https://nodejs.org/)
- Запущенный бэкенд FadeAfro

---

## 3. Технологии

| Компонент    | Технология        |
|--------------|-------------------|
| Фреймворк    | React 19          |
| Язык         | TypeScript        |
| Сборщик      | Vite 8            |
| HTTP-клиент  | Axios             |
| Роутинг      | React Router 7    |
| Telegram SDK | @tma.js/sdk-react |

---

## 4. Архитектура

```
FadeAfroFront/src/
├── api/                        # Axios-клиент и функции запросов к бэкенду
│   ├── client.ts               # Базовый клиент с JWT-интерцептором и обработкой 429
│   ├── errors.ts               # RateLimitError
│   ├── auth.ts
│   ├── appointments.ts
│   ├── availability.ts
│   ├── masters.ts
│   ├── notifications.ts
│   ├── schedule.ts
│   ├── services.ts
│   └── user.ts
├── pages/                      # Страницы приложения
├── components/                 # Переиспользуемые компоненты
│   ├── LoadingScreen/
│   ├── MasterCard/
│   └── UserInfoCard/
├── context/
│   └── AuthContext.tsx         # JWT, роли, статус аутентификации
├── hooks/
│   └── useBackButton.ts        # Кнопка «Назад» Telegram
├── router/
│   └── AppRouter.tsx           # HashRouter + защита маршрутов по ролям
├── utils/
│   └── duration.ts             # Работа с длительностью услуг
└── mock/
    └── telegramMock.ts         # Мок Telegram SDK для локальной разработки
```

### Прокси

В dev-режиме Vite проксирует все запросы `/api/*` на бэкенд (`http://localhost:5058`). В prod-режиме (Docker) эту роль выполняет Nginx.

### Аутентификация

При старте приложения `AuthContext` получает `initData` из Telegram SDK и отправляет на бэкенд. В ответ приходит JWT-токен, который сохраняется в `localStorage`. Axios-интерцептор автоматически добавляет его в каждый запрос:

```
Authorization: Bearer <token>
```

Роли (`Client`, `Master`, `Owner`) извлекаются из JWT payload и используются для защиты маршрутов.

### Rate limiting

При получении `429` Axios-интерцептор бросает `RateLimitError`. Обработчики на страницах показывают алерт через `window.Telegram.WebApp.showAlert()`.

### Моки для разработки

В dev-режиме (`import.meta.env.DEV`) реальный Telegram SDK заменяется моком. Пользователь для тестирования задаётся через `MOCK_USER_ID` в `mock/telegramMock.ts`:

| ID | Имя | Роль |
|----|-----|------|
| `777777777` | Petr Masterov | Master |
| `100000001` | Oleg Ownerow | Owner + Master |
| `100000002` | Artem Masterov | Master |
| `100000003` | Vasya Clientov | Client |

---

## 5. Страницы

### Клиент

| Маршрут | Страница | Описание |
|---------|----------|----------|
| `/client` | ClientPage | Список мастеров |
| `/client/settings` | SettingsPage | Настройки профиля |
| `/client/appointments` | MyAppointmentsPage | Мои записи |
| `/client/notifications` | NotificationsPage | Уведомления |
| `/client/master/:id/services` | SelectServicePage | Выбор услуг |
| `/client/master/:id/date` | SelectDatePage | Выбор даты |
| `/client/master/:id/time` | SelectTimePage | Выбор времени |
| `/client/master/:id/confirm` | ConfirmPage | Подтверждение записи |
| `/client/booking-success` | BookingSuccessPage | Успешная запись |

### Мастер

| Маршрут | Страница | Описание |
|---------|----------|----------|
| `/master` | MasterPage | Главная мастера |
| `/master/settings` | MasterSettingsPage | Настройки профиля |
| `/master/schedule` | MasterSchedulePage | Расписание работы |
| `/master/unavailability` | MasterUnavailabilityPage | Недоступные даты |
| `/master/appointments` | MasterAppointmentsPage | Записи клиентов |
| `/master/notifications` | NotificationsPage | Уведомления |

### Владелец

| Маршрут | Страница | Описание |
|---------|----------|----------|
| `/owner` | OwnerPage | Главная владельца |
| `/owner/settings` | SettingsPage | Настройки профиля |
| `/owner/users` | OwnerUsersPage | Управление пользователями |

### Прочее

| Маршрут | Страница | Описание |
|---------|----------|----------|
| `/error` | ErrorPage | Страница ошибки |

---

## 6. Запуск

```bash
cd FadeAfroFront
npm install
npm run dev
```

Приложение: `http://localhost:5173`

### Сборка

```bash
npm run build
```

Собранные файлы окажутся в папке `dist/`.
