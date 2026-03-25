# Cable Winding IS — Контекст проекта

## Структура
- `backend/api/` — ASP.NET Core Web API
- `backend/trace-connector/` — .NET Worker Service (симулятор → OPC UA)
- `frontend/` — React + TypeScript + Vite
- `docs/` — архитектура, UML-mapping, API
- `docks/diagrams/` — UML-диаграммы (State, use cases и др.)

## Ключевые маршруты API
- `GET /api/telemetry/ping` — проверка доступности
- `GET /api/telemetry/latest` — последние параметры для UI
- `POST /api/telemetry/params` — приём телеметрии от TraceConnector

## Запуск (PowerShell)
```powershell
# API
cd backend/api/CableWinding.Api; dotnet run

# TraceConnector (в отдельном терминале)
cd backend/trace-connector/CableWinding.TraceConnector; dotnet run

# Frontend (требуется Node.js)
cd frontend; npm install; npm run dev
```

## Docker
```powershell
cd cable-winding-is
docker-compose up -d
```
