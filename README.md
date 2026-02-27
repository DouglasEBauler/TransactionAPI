# TransactionAPI Real-Time System

## How to Execute
```bash
# 1. Docker only Kafka
docker-compose up -d

# 2. API
cd backend/src/TransactionAPI/Transaction.Api && dotnet run

# 3. Worker
cd backend/src/TransactionAPI/Transaction.Worker && dotnet run

# 4. Frontend
cd frontend && ng serve
```

## Tech Stack
- .NET 10, Kafka, MongoDB(with cloud Mongo Atlas), SignalR, Angular

## Notes
- Code developed during live coding session (2h30)
- Focus on functionality and basic architecture