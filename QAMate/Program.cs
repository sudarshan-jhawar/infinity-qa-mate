using Microsoft.EntityFrameworkCore;
using QAMate.Data;
using QAMate.Repositories;
using QAMate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core DbContext with SQLite using connection string from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Register repositories and services for DI.
builder.Services.AddScoped<IDefectRepository, DefectRepository>();
builder.Services.AddScoped<IDefectService, DefectService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
