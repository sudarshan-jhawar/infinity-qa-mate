# QAMate

Minimal ASP.NET Core 8 Web API using EF Core Database-First with SQLite and a simple layered architecture.

## Project Structure
- `Api/Controllers` -> Web API controllers (`DefectController`)
- `Data` -> Scaffolded `AppDbContext` and entity (`Defect`)
- `Repositories` -> Data access abstractions and implementations
- `Services` -> Business logic & mapping (DTO <-> Entity)
- `Models` -> DTO classes used by API layer
- `Program.cs` -> DI registrations & app bootstrap
- `appsettings.json` -> Connection string configuration

## Assumed Existing Database
SQLite file `app.db` containing table `defects`:
```
CREATE TABLE defects (
  Id INTEGER PRIMARY KEY,
  Name TEXT,
  Price REAL
);
```
Place `app.db` in the project root (same folder as `Program.cs`).

## CLI Commands
Run the following to create, add packages, scaffold, and start the API:
```
dotnet new webapi -n SimpleSqliteApi
cd SimpleSqliteApi
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef dbcontext scaffold "Data Source=app.db" Microsoft.EntityFrameworkCore.Sqlite -o Data -c AppDbContext --no-pluralize --force

dotnet run
```

## Example Endpoints
- GET /api/defect
- GET /api/defects/{id}
- POST /api/defects
- PUT /api/defects/{id}
- DELETE /api/defects/{id}

### Sample POST Body
```
{
  "name": "Layout Overlap",
  "price": 5.75
}
```
### Sample PUT Body
```
{
  "id": 1,
  "name": "Updated Layout Overlap",
  "price": 6.25
}
```

## Notes
- Mapping between entity and DTO is performed manually inside `DefectService`.
- Repository pattern isolates EF Core specifics (`DefectRepository`).
- Service layer adds business logic & mapping (`DefectService`).
- Swagger removed to keep dependencies minimal; you can add it back if desired.
