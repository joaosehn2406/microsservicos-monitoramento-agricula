# Farm Monitoring

Scaffold inicial de um sistema distribuído para monitoramento agrícola. O monorepo contém cinco APIs independentes, sem regras de negócio e sem referências de projeto entre elas.

## Microsserviços

| Serviço | Responsabilidade | Porta HTTP | Banco dedicado |
| --- | --- | ---: | --- |
| Properties | Cadastro futuro de propriedades agrícolas | 8081 | `properties_db` em 5432 |
| WeatherCollector | Coleta futura do Open-Meteo e publicação de leituras | 8082 | `weather_collector_db` em 5433 |
| Storage | Consumo e armazenamento futuro de leituras | 8083 | `storage_db` em 5434 |
| Analytics | Análise futura de leituras e publicação de alertas | 8084 | `analytics_db` em 5435 |
| Notifications | Consumo e persistência futura de notificações | 8085 | `notifications_db` em 5436 |

RabbitMQ usa as portas `5672` (AMQP) e `15672` (painel de gerenciamento). O exchange preparado para eventos é `farm.events`.

## Tecnologias

.NET 10, ASP.NET Core Web API, Entity Framework Core, PostgreSQL, RabbitMQ, Docker Compose e Open-Meteo.

## Requisitos

- .NET SDK 10
- Docker e Docker Compose

## Executar localmente

```bash
dotnet restore --configfile NuGet.config
dotnet build FarmMonitoring.sln --no-restore
```

Para executar toda a infraestrutura, copie `.env.example` para `.env` e ajuste apenas valores locais se necessário:

```bash
docker compose up --build
```

## Arquitetura

Cada serviço possui seus próprios Controllers, Services, Repositories, DTOs, Mappers, Exceptions, Entities, DbContext, configurações, migrations, Dockerfile e banco PostgreSQL. Integrações futuras ocorrerão por HTTP ou RabbitMQ; nenhum serviço acessa o banco ou o código de outro.

Fluxo de mensageria preparado:

```text
WeatherCollector -- weather.reading --> farm.events --> q.storage
                                              \-----> q.analytics
Analytics        -- alert.* ---------> farm.events --> q.notifications
```

