
# DefectPilot 

## Overview
The Defect Management System is designed to streamline defect logging, tracking, and prioritization using AI-powered automation. It integrates **React frontend**, **.NET backend**, and **OpenAI GPT-5** for intelligent defect analysis, leveraging historical data stored in **Azure SQL Database**.

---

## Requirements
### Functional
- Log defects via a user-friendly web interface.
- Store defect details in a secure database.
- Detect duplicate defects automatically.
- Suggest defect **priority** and **severity** based on historical patterns.
- Refine defect descriptions for clarity and consistency.

### Non-Functional
- High availability and scalability.
- Secure API communication with OpenAI GPT-5.
- Role-based access control for defect management.
- Integration with existing CI/CD pipelines.

---

## Solution Architecture
The system consists of the following components:

- **Frontend**:  
  - Built using **React** for defect logging and visualization.
- **Backend**:  
  - Developed in **.NET** to handle business logic and API integration.
- **AI Integration (OpenAI GPT-5)**:  
  - Detects duplicate defects.
  - Suggests priority and severity.
  - Refines defect descriptions.
- **Database**:  
  - **Azure SQL Database** stores historical defect data for AI analysis.
- **Configuration**:  
  - Secure storage of **OpenAI API Key** for GPT-5 integration.

---

## Architecture Diagram
<img width="748" height="499" alt="image" src="https://github.com/user-attachments/assets/1a661349-4407-4433-9463-29a643bf9f90" />

---

## ER Diagram

<img width="696" height="511" alt="image" src="https://github.com/user-attachments/assets/39df0ee2-b521-4616-a6f0-974a2522126f" />


## Key Features
- **AI-driven defect analysis** for better prioritization.
- **Duplicate detection** to reduce redundancy.
- **Historical data utilization** for accurate severity prediction.
- **Seamless integration** with enterprise systems.


=======
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

