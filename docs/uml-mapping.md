# Соответствие UML-диаграмм и кода

> См. также корневой `docs/uml-mapping.md` в репозитории.  
> Диаграммы: `../../docks/diagrams/` (State.png, use cases.png и др.)

Документ фиксирует соответствие между UML-диаграммами и реализацией в коде.

## Краткая сводка

- **Компоненты** → `backend/api`, `backend/trace-connector`, `frontend`
- **Use-Case** → страницы Monitoring, Процесс, Инциденты, Нормативы, Отчёты, Архив, Диагностика
- **State Machine** → Domain/StateMachine, process_state_transitions
- **Последовательности** → Controllers + Application services
