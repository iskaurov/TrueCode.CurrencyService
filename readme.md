## 💪 Тестовое задание: TrueCode.CurrencyService

### 📦 Состав проекта

Микросервисная архитектура с 5 компонентами:

| Проект            | Назначение                              |
|-------------------|-----------------------------------------|
| `UserApi`         | Авторизация и регистрация пользователей |
| `FinanceApi`      | Работа с валютами, избранными курсами   |
| `GrpcService`     | Аналог FinanceApi по grpc протоколу     |
| `CurrencyUpdater` | Консольный воркер, обновляющий курсы    |
| `Gateway`         | API Gateway                             |
| `Migrator`        | Применение миграций к базе данных       |

---

## 🚀 Запуск через Docker

```bash
git clone git@github.com:ваш-юзер/TrueCode.CurrencyService.git
cd TrueCode.CurrencyService
docker compose up --build
```

После запуска будут доступны:

| Сервис      | URL                                   |
|-------------|---------------------------------------|
| Gateway     | http://localhost:5160                 |
| UserApi     | http://localhost:5161 (+`/swagger`)   |
| FinanceApi  | http://localhost:5162 (+`/swagger`)   |
| GrpcService | http://localhost:5163                 |
| Postgres    | `localhost:5432`                      |

---

## 🔮 Локальный запуск (Rider / VS)

> Используются другие порты:

| Сервис       | Порт |
|--------------|------|
| Gateway      | 5150 |
| UserApi      | 5151 |
| FinanceApi   | 5152 |
| GrpcService  | 5153 |

1. Укажи `.env` или `.env.dev`
2. Запускай проекты поочередно: UserApi → FinanceApi → Gateway
3. Swagger доступен по `/swagger`

---
## 🛠 Миграции
Миграции применяются автоматически при старте контейнера migrator, если в .env установлено:
```bash
APPLY_MIGRATIONS=true
```
Миграции можно применить вручную:
```bash
cd TrueCode.CurrencyService.Migrator
dotnet run
```
---
## 📊 Тесты

```bash
dotnet test
```

Покрыто:

- Регистрация/логин
- JWT авторизация
- Защита endpoint'ов

---

## ⚙️ Переменные `.env`
В этой репе .env храняться поскольку проект тестовый и описани есоставлено под соответствующие порты.

```dotenv
# ==== DB ====
DB_HOST=postgres
DB_PORT=5432
DB_NAME=currency_db
DB_USER=currency_user
DB_PASSWORD=12345

# ==== CONNECTION STRINGS ====
DEFAULT_CONNECTION=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}

APPLY_MIGRATIONS=true

# ==== JWT ====
JWT__Key=>vOCGh0q7{[18^}CyS_yMM7%4~2342342344234
JWT__Issuer=TrueCode
JWT__Audience=TrueCodeUsers

# ==== PORTS (для Docker Compose) ====
USER_API_PORT=5161
FINANCE_API_PORT=5162
GRPC_PORT=5163
GATEWAY_PORT=5160

# ==== API URLs (если нужно для консольных клиентов) ====
USER_API_URL=http://userapi:${USER_API_PORT}
FINANCE_API_URL=http://financeapi:${FINANCE_API_PORT}
GRPC_URL=http://grpc:${GATEWAY_PORT}
GATEWAY_URL=http://gateway:${GATEWAY_PORT}
```

---

## 🥴 Что сделать при проверке

- Swagger UI: протестировать ручки `/auth/register`, `/auth/login`, `/currency/favorites`
- Проверить авторизацию и работу токена
- Проверить gRPC (Kreya или прочее)

---

