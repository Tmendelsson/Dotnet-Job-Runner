# Dotnet Job Runner

Plataforma backend para agendar, executar, monitorar e reprocessar tarefas assíncronas.

## Stack

- .NET 8
- ASP.NET Core Web API
- Hangfire
- PostgreSQL
- Entity Framework Core
- Serilog
- Swagger

## Estrutura

- src/DotnetJobRunner.Api
- src/DotnetJobRunner.Application
- src/DotnetJobRunner.Domain
- src/DotnetJobRunner.Infrastructure
- src/DotnetJobRunner.Worker
- tests/DotnetJobRunner.UnitTests
- tests/DotnetJobRunner.IntegrationTests

## Rodando localmente

1. Suba o banco:

   docker compose up -d

2. Aplique migrações no banco:

  $ef = "$env:USERPROFILE\\.dotnet\\tools\\dotnet-ef"
  & $ef database update --project src\\DotnetJobRunner.Infrastructure\\DotnetJobRunner.Infrastructure.csproj

3. Rode a API:

   cd src/DotnetJobRunner.Api
   dotnet run

4. Acesse recursos:

- Swagger: /swagger
- Hangfire Dashboard: /hangfire
- Health: /health

Use a porta que aparecer no console ao executar `dotnet run`.

## Migrações EF Core

Criar nova migration:

$ef = "$env:USERPROFILE\\.dotnet\\tools\\dotnet-ef"
& $ef migrations add NomeDaMigration --project src\\DotnetJobRunner.Infrastructure\\DotnetJobRunner.Infrastructure.csproj --output-dir Persistence\\Migrations

Aplicar migrations:

& $ef database update --project src\\DotnetJobRunner.Infrastructure\\DotnetJobRunner.Infrastructure.csproj

## Endpoints de jobs

- POST /jobs
- GET /jobs?page=1&pageSize=20&status=Queued&type=send-email&priority=high
- GET /jobs/{id}
- DELETE /jobs/{id}
- POST /jobs/{id}/retry

## Endpoints de recurring jobs

- POST /recurring-jobs
- GET /recurring-jobs
- PATCH /recurring-jobs/{id}/enable
- PATCH /recurring-jobs/{id}/disable
- DELETE /recurring-jobs/{id}

## Exemplo de payload (POST /jobs)

{
  "type": "send-email",
  "payload": {
    "to": "cliente@email.com",
    "subject": "Bem-vindo"
  },
  "priority": "normal",
  "maxRetries": 3
}

## Exemplo de payload (POST /recurring-jobs)

{
  "name": "sync-customers-nightly",
  "type": "sync-customers",
  "cronExpression": "0 2 * * *",
  "payload": {
    "source": "erp"
  },
  "priority": "normal"
}
