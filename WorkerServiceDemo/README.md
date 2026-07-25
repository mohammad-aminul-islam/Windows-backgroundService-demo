# WorkerServiceDemo

A Windows background service that backs up SQL Server and PostgreSQL databases on a cron schedule (via Hangfire), then sends a Telegram notification with the result.

## Configuration

Copy `WorkerServiceDemo/appsettings.example.json` to `WorkerServiceDemo/appsettings.json` and fill in your own values. Key sections:

- `BackupSettings.CronExpression` — standard 5-field cron (e.g. `0 2 * * *` = daily at 2 AM).
- `SqlServer` — server, credentials, database names, and backup directory for SQL Server backups.
- `Postgres` — server, credentials, database names, and backup directory for PostgreSQL backups (see below).
- `Telegram` — bot token and channel ID for completion notifications.

## Running PostgreSQL backups (pg_dump)

`PostgresBackupService` shells out to `pg_dump` for each database listed in `Postgres:DatabaseNames`, writing one `.sql` file per database to `Postgres:BackupDirectory`. There are two ways to run it, controlled by `Postgres:ContainerName`.

### Option A — local `pg_dump` (no Docker)

Use this when PostgreSQL client tools are installed directly on the machine running the service.

1. Install the PostgreSQL client tools (they include `pg_dump`):
   - Windows: install via the [EDB PostgreSQL installer](https://www.postgresql.org/download/windows/) (you can select just "Command Line Tools"), or `choco install postgresql`.
   - Confirm it's on PATH: `pg_dump --version`.
2. In `appsettings.json`, leave `Postgres:ContainerName` empty and set:
   ```json
   "Postgres": {
     "Server": "localhost",
     "DatabaseNames": [ "mydb" ],
     "UserName": "postgres",
     "Password": "password",
     "BackupDirectory": "D:\\Google drive backup\\Postgres",
     "PgDumpPath": "pg_dump",
     "ContainerName": ""
   }
   ```
   Set `PgDumpPath` to the full executable path instead (e.g. `C:\\Program Files\\PostgreSQL\\16\\bin\\pg_dump.exe`) if it isn't on PATH.
3. The service runs, per database:
   ```
   pg_dump -h <Server> -U <UserName> -f "<BackupDirectory>\<date>_<db>.sql" <db>
   ```
   with `PGPASSWORD` set from `Postgres:Password` as an environment variable for the process — the password is never placed on the command line.

**Note:** the installed `pg_dump` version should match (or be newer than) the PostgreSQL server version you're dumping from, or the dump may fail or omit newer features.

### Option B — `pg_dump` inside a Docker container

Use this when PostgreSQL itself runs in Docker — no local `pg_dump` install needed, and the version always matches the server.

1. Set `Postgres:ContainerName` to the running container's name (`docker ps` to find it):
   ```json
   "Postgres": {
     "Server": "localhost",
     "DatabaseNames": [ "mydb" ],
     "UserName": "postgres",
     "Password": "password",
     "BackupDirectory": "D:\\Google drive backup\\Postgres",
     "PgDumpPath": "pg_dump",
     "ContainerName": "my_postgres_container"
   }
   ```
   `PgDumpPath` here is the path to `pg_dump` *inside* the container (usually just `pg_dump`, since it's on PATH in the official Postgres image).
2. The service runs, per database:
   ```
   docker exec -e PGPASSWORD="<Password>" <ContainerName> <PgDumpPath> -U <UserName> <db>
   ```
   and streams the container's stdout directly into `<BackupDirectory>\<date>_<db>.sql` on the host — the dump never touches the container's filesystem.
3. Requirements: Docker must be installed and on PATH for the account running the service, and that account needs permission to run `docker exec` (i.e. be in the `docker-users` group on Windows, or run the service as an account with Docker access).

### Skipping PostgreSQL backups

Leave `Postgres:Server` and `Postgres:ContainerName` both empty to skip PostgreSQL backups entirely (only SQL Server will run).

## Running the service

```
dotnet run --project WorkerServiceDemo
```

For production, publish and install as a Windows Service:

```
dotnet publish WorkerServiceDemo -c Release -o ./publish
sc create "Database Backup Service" binPath="C:\path\to\publish\WorkerServiceDemo.exe"
sc start "Database Backup Service"
```

Hangfire uses in-memory storage for job scheduling, so no separate database is required to run the service itself; the recurring backup job is re-registered automatically every time the service starts.
