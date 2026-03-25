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

## ✅ Status do MVP 1

> **MVP 1 está concluído.** O CI/CD na branch `main` está verde, build passa com 0 erros/warnings e todos os testes passam.

| Funcionalidade | Status |
|---|---|
| Criar e enfileirar job imediato (`POST /jobs`) | ✅ |
| Agendar job para data futura (`RunAt`) | ✅ |
| Cancelar job (`DELETE /jobs/{id}`) | ✅ |
| Retentar job com falha (`POST /jobs/{id}/retry`) | ✅ |
| Consultar status de um job (`GET /jobs/{id}`) | ✅ |
| Listar jobs com filtros e paginação (`GET /jobs`) | ✅ |
| Criar job recorrente com cron (`POST /recurring-jobs`) | ✅ |
| Listar jobs recorrentes (`GET /recurring-jobs`) | ✅ |
| Habilitar/desabilitar recorrente (`PATCH /recurring-jobs/{id}/enable\|disable`) | ✅ |
| Deletar job recorrente (`DELETE /recurring-jobs/{id}`) | ✅ |
| Worker em background executando jobs via Hangfire | ✅ |
| Histórico de execuções por tentativa (`JobExecution`) | ✅ |
| Persistência com PostgreSQL + EF Core (migrations) | ✅ |
| Logging estruturado (Serilog na API e no Worker) | ✅ |
| Health check (`/health`) | ✅ |
| Documentação da API (Swagger em `/swagger`) | ✅ |
| Dashboard Hangfire protegido (`/hangfire`) | ✅ |
| Testes unitários (5 testes) | ✅ |
| Teste de integração (1 teste) | ✅ |
| CI/CD com GitHub Actions | ✅ |
| Docker Compose para desenvolvimento local | ✅ |

### Próximos passos (MVP 2)

- [ ] Endpoints de histórico de execuções (`GET /jobs/{id}/executions`)
- [ ] Cobertura de testes para `JobExecutionService`
- [ ] Backoff exponencial no mecanismo de retry
- [ ] Padrão plugin `IJobHandler<TPayload>` para handlers extensíveis
- [ ] Métricas de observabilidade (Prometheus/Grafana)

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
