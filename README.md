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
| Testes unitários (35 testes) | ✅ |
| Testes de integração (4 testes) | ✅ |
| CI/CD com GitHub Actions | ✅ |
| Docker Compose para desenvolvimento local | ✅ |

## ✅ Status do MVP 2

> **MVP 2 concluído.** CI verde, build passa com 0 erros/warnings e todos os testes passam.

| Funcionalidade | Status |
|---|---|
| Histórico de execuções paginado (`GET /jobs/{id}/executions`) | ✅ |
| `HangfireJobId` persistido — cancelamento de job agendado real | ✅ |
| Contrato de API correto: `409 Conflict` para estado inválido em cancel/retry | ✅ |
| Migration: tabela `JobExecutions`, `MaxRetries` em recorrentes | ✅ |
| Cobertura de testes para `JobExecutionService` | ✅ |
| Cobertura de testes para `RecurringJobExecutionService` | ✅ |
| Backoff exponencial no mecanismo de retry | ✅ |

## ✅ Status do MVP 3

> **MVP 3 concluído.** O sistema é extensível, protegido por API Key/RBAC e possui controle de concorrência/rate limit por tipo de job.

| Funcionalidade | Status |
|---|---|
| Padrão plugin `IJobHandler<TPayload>` — handlers como classes independentes | ✅ |
| Handlers reais: envio de e-mail, geração de relatório, sync de dados, import CSV | ✅ |
| Validação de `job.Type` contra handlers registrados | ✅ |
| Retry policy explícita: Hangfire sem retry automático invisível | ✅ |
| Rate limiting / concorrência configurável por tipo de job | ✅ |
| Autenticação real na API com API Key | ✅ |
| RBAC simples (`Admin`, `Operator`, `Viewer`) | ✅ |
| Dashboard Hangfire protegido por API Key de `Admin` | ✅ |
| Testes unitários para cada handler | ✅ |

## 🔲 MVP 4 — Produção

> **"O sistema está pronto para deploy"** — observabilidade completa e cloud.

| Funcionalidade | Status |
|---|---|
| Métricas com Prometheus + dashboard Grafana | 🔲 |
| Alertas: job com muitas falhas, fila acumulando | 🔲 |
| Deploy em cloud (Railway, Fly.io ou Render com PostgreSQL gerenciado) | 🔲 |
| Variáveis de ambiente via secrets (sem credenciais em arquivos) | 🔲 |
| README com instruções de deploy e badge de status | 🔲 |

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

## Autenticação

Os endpoints de jobs e recurring jobs exigem API Key no header:

```http
X-API-Key: sua-chave
```

Configure as chaves por variáveis de ambiente ou secrets:

```powershell
$env:Authentication__ApiKey__Keys__0__Name = "local-admin"
$env:Authentication__ApiKey__Keys__0__Value = "sua-chave"
$env:Authentication__ApiKey__Keys__0__Roles__0 = "Admin"
```

Papéis suportados:

- `Admin`: acesso completo, incluindo Hangfire Dashboard.
- `Operator`: cria, cancela, retenta e consulta jobs.
- `Viewer`: consulta jobs e execuções.

Para acessar o Hangfire Dashboard no navegador, use uma chave com papel `Admin`:

```text
/hangfire?api_key=sua-chave
```

## Controle por tipo de job

A concorrência e o rate limit são configuráveis por tipo:

```json
{
  "Jobs": {
    "Types": {
      "send-email": {
        "MaxConcurrency": 5,
        "RateLimitPerMinute": 60
      },
      "generate-report": {
        "MaxConcurrency": 1,
        "RateLimitPerMinute": 10
      }
    }
  }
}
```

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
- GET /jobs/{id}/executions
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
