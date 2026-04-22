# SportClub — Система управления спортивным клубом

Полнофункциональное веб-приложение для управления спортивным клубом с поддержкой членства, расписания тренировок и регистрации участников.

## 🎯 Возможности

- **Управление членами клуба** — добавление, удаление, просмотр профилей участников
- **Абонементы** — выдача абонементов разных типов (Standard, Premium, Student) с отслеживанием статуса
- **Расписание тренировок** — создание и управление тренировками с указанием тренера, времени и максимального числа участников
- **Система регистрации** — участники могут записываться и отменять запись на тренировки
- **Панель администратора** — полное управление клубом с статистикой и аналитикой
- **Кэширование** — Redis для оптимизации производительности

## 🏗️ Архитектура

```
SportClubSystem/
├── SportClubApi/              # ASP.NET Core 8 backend
│   ├── Controllers/           # API endpoints
│   ├── Models/                # EF Core entities
│   ├── Data/                  # Database context
│   ├── Services/              # Business logic (CacheService)
│   └── wwwroot/               # Статические файлы (HTML/CSS/JS)
│       ├── client/            # Клиентский портал
│       └── admin/             # Админ-панель
├── nginx/                     # Reverse proxy
├── docker-compose.yml         # Оркестрация контейнеров
└── Dockerfile                 # Многоэтапная сборка образа
```

## 🚀 Быстрый старт

### Требования
- Docker и Docker Compose
- Git

### Установка и запуск

```bash
# Клонируем репозиторий
git clone <repo-url>
cd SportClubSystem

# Запускаем контейнеры
docker-compose up -d --build

# API доступен по адресу: http://localhost/api
# Админ-панель: http://localhost/admin
# Клиентский портал: http://localhost/client
```

## 📋 Сервисы

| Сервис | Порт | Назначение |
|--------|------|-----------|
| **API** | 8080 | ASP.NET Core backend |
| **Nginx** | 80 | Reverse proxy, статические файлы |
| **PostgreSQL** | 5432 | База данных |
| **Redis** | 6379 | Кэширование |
| **Grafana** | 3000 | Мониторинг метрик |
| **Prometheus** | 9090 | Сбор метрик |

## 🔌 API Endpoints

### Участники
- `GET /api/members` — список всех участников
- `GET /api/members/{id}` — получить участника по ID
- `POST /api/members` — создать участника
- `PUT /api/members/{id}` — обновить участника
- `DELETE /api/members/{id}` — удалить участника

### Тренировки
- `GET /api/workouts` — список тренировок
- `GET /api/workouts/{id}/registrations` — кол-во записей на тренировку
- `POST /api/workouts` — создать тренировку
- `POST /api/workouts/{id}/register` — записать участника
- `DELETE /api/workouts/{id}/unregister/{memberId}` — отменить запись
- `DELETE /api/workouts/{id}` — удалить тренировку

### Абонементы
- `GET /api/memberships` — список абонементов
- `GET /api/memberships/member/{memberId}` — абонементы участника
- `POST /api/memberships` — выдать абонемент
- `DELETE /api/memberships/{id}` — удалить абонемент

## 🎨 Интерфейсы

### Админ-панель (`http://localhost/admin`)
- Управление членами клуба
- Добавление и удаление тренировок
- Выдача абонементов
- Статистика и аналитика

### Клиентский портал (`http://localhost/client`)
- Просмотр профиля и абонемента
- Просмотр расписания тренировок
- Запись и отмена записи на тренировки
- Управление личными записями

## 💾 Технологический стек

- **Backend:** ASP.NET Core 8.0
- **Database:** PostgreSQL 15
- **Cache:** Redis 7
- **Frontend:** Vanilla JavaScript, HTML5, CSS3
- **Proxy:** Nginx
- **Containerization:** Docker & Docker Compose
- **Monitoring:** Prometheus + Grafana

## 📦 Dockerfile

Используется многоэтапная сборка для оптимизации размера образа:
1. **build stage** — компиляция кода с SDK
2. **publish stage** — публикация приложения
3. **final stage** — запуск в минимальном runtime образе

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... компиляция ...

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# ... запуск приложения ...
```

## 🔐 Особенности

- ✅ **Кэширование с инвалидацией** — Redis кэширует данные пользователей, автоматически очищается при изменениях
- ✅ **Обработка JSON** — поддержка циклических ссылок в моделях через `ReferenceHandler.IgnoreCycles`
- ✅ **Статические файлы** — HTML/CSS/JS файлы копируются в контейнер и сервируются Nginx
- ✅ **CORS** — настроена кросс-доменная поддержка
- ✅ **Cache-busting** — клиент добавляет timestamp к запросам для обхода браузерного кэша

## 📝 Примеры использования

### Создание тренировки (POST)
```bash
curl -X POST http://localhost/api/workouts \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Йога для начинающих",
    "trainer": "Иван Иванов",
    "description": "Базовый курс йоги",
    "startsAt": "2026-04-25T10:00:00Z",
    "durationMinutes": 60,
    "maxParticipants": 20
  }'
```

### Запись на тренировку (POST)
```bash
curl -X POST http://localhost/api/workouts/1/register \
  -H "Content-Type: application/json" \
  -d '{"memberId": 1}'
```

### Получение абонементов участника (GET)
```bash
curl http://localhost/api/memberships/member/1
```

## 🛠️ Разработка

### Пересборка образа
```bash
docker-compose up -d --build
```

### Просмотр логов
```bash
docker-compose logs -f api
```

### Остановка сервисов
```bash
docker-compose down
```

### Удаление всех данных
```bash
docker-compose down -v
```

## 📄 Лицензия

MIT License

## 👨‍💻 Контрибьютинг

Приветствуются pull requests. Для больших изменений сначала откройте issue.

---

**Автор:** Разработано для лабораторной работы по контейнеризации приложений.
