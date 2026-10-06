# FarmMonitoring

Sistema distribuído de monitoramento ambiental para propriedades agrícolas. O **WeatherCollector** consulta periodicamente a API Open-Meteo e publica leituras no RabbitMQ; três consumidores independentes (**Storage**, **Analytics**, **Notifications**) armazenam, analisam e notificam; a **BackendApi** expõe os dados via REST.

A especificação completa está em [`docs/SPEC.md`](docs/SPEC.md).

| Serviço | Pasta | Tipo |
|---|---|---|
| WeatherCollector | `API/WeatherCollector` | Worker Service |
| Storage | `API/Storage` | Worker Service |
| Analytics | `API/Analytics` | Worker Service |
| Notifications | `API/Notifications` | Worker Service |
| BackendApi | `API/BackendApi` | ASP.NET Core Web API |

Infraestrutura: PostgreSQL 17 (schema criado por [`database/init.sql`](database/init.sql)) e RabbitMQ 4 com painel de gerenciamento.

## Como subir

Pré-requisito: Docker com Compose v2.

```bash
cp .env.example .env   # opcional; sem .env são usados os valores padrão
docker compose up --build
```

| Recurso | Endereço |
|---|---|
| BackendApi | http://localhost:8080 |
| PostgreSQL | `localhost:5432`, banco `farm`, usuário `farm` / senha `farm_password` |
| RabbitMQ (AMQP) | `localhost:5672` |

Para compilar localmente sem Docker: `dotnet build` na raiz.

## Painel do RabbitMQ

Acesse http://localhost:15672 com usuário `guest` e senha `guest` (ou os valores de `RABBITMQ_DEFAULT_USER` / `RABBITMQ_DEFAULT_PASS` do `.env`). Em **Queues and Streams** é possível acompanhar as filas `q.*`, `q.*.retry` e `q.*.dlq`.

## Escalar consumidores

Os consumidores suportam *competing consumers*, então dá para subir várias réplicas sem alterar código:

```bash
docker compose up --build --scale analytics=3
```

O mesmo vale para `storage` e `notifications`.

## Recriar o banco

O `init.sql` só é executado quando o volume do Postgres está vazio. Para recriar o banco do zero, remova o volume:

```bash
docker compose down -v      # remove os volumes do Postgres e do RabbitMQ
docker compose up --build
```

Para remover apenas o volume do Postgres: `docker compose down` seguido de `docker volume rm <projeto>_postgres-data` (veja o nome exato com `docker volume ls`).
