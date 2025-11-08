# QAMate (ASP.NET Core + EF Core + SQLite)

Two .NET 8 projects:
- `QAMate` – Web API for managing software defects (Title, Description, Status, Severity, Priority, timestamps).
- `QAMate.DataGen` – Console utility to generate and insert realistic defects into the same SQLite database.

## Prerequisites
- .NET SDK 8.0+
- Optional: SQLite viewer (DB Browser for SQLite)
- Optional (AI generation): environment variable `OPENAI_API_KEY`

## Repository layout
- `QAMate/`
  - `Data/AppDbContext.cs` – EF Core `DbContext` and `Defect` entity mapped to table `defects`
  - `Api/Controllers/DefectController.cs` – CRUD endpoints under `/api`
  - `Services/*`, `Repositories/*`, `Models/DefectDto.cs`
  - `appsettings.json` – `ConnectionStrings:DefaultConnection = "Data Source=app.db"`
- `QAMate.DataGen/`
  - `Program.cs` – data generator (interactive and CLI)

## Quick start
1) Restore
- dotnet restore

2) Run API (Swagger UI enabled)
- dotnet run --project QAMate
- Browse: https://localhost:<port>/swagger
- First run creates `QAMate/app.db` and schema automatically.

3) Seed sample data (targets QAMate/app.db by default)
- Basic:
  - dotnet run --project QAMate.DataGen -- --count 50 --seed 123 --tag DEMO --cleanup
- Interactive prompts:
  - dotnet run --project QAMate.DataGen -- --interactive
- With OpenAI:
  - set OPENAI_API_KEY=<your key>
  - dotnet run --project QAMate.DataGen -- --count 50 --use-openai --openai-max 20 --tag AI --cleanup

The generator logs the absolute SQLite path, rows before/after insert, and a small sample of created defects.

## API overview
Base path: `/api`
- GET `/api/defect` – list all
- GET `/api/defects/{id}` – get by id
- POST `/api/defects` – create (`DefectDto`)
- PUT `/api/defects/{id}` – update (`DefectDto`)
- DELETE `/api/defects/{id}` – delete

`DefectDto` fields: Id, Title, Description, Status, Severity (1–5), Priority (1–5), CreatedAt, UpdatedAt, LastModifiedAt.

## Database location
- API uses `QAMate/app.db` from `QAMate/appsettings.json`.
- Generator normalizes any missing/relative `--db` to the solution’s `QAMate/app.db` (avoids writes in bin folders). It prints:
  - `[INFO] SQLite path: C:\...\QAMate\app.db exists=...`
- Be explicit if needed:
  - `--db "Data Source=./QAMate/app.db"`

## Troubleshooting
- DB created in bin: generator now fixes paths; check the logged SQLite path. Pass an explicit `--db` to override.
- No rows after generation: remove `--dry-run`. Check the “Rows before/after insert” logs.
- OpenAI warnings: the generator falls back to template text automatically.

## Development notes
- Startup enables Swagger and permissive CORS for ease of local testing.
- If you modify the `Defect` model/columns, ensure `defects` mapping in `AppDbContext` stays in sync.
