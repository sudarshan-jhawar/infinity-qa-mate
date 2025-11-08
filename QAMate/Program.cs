using Microsoft.EntityFrameworkCore;
using QAMate.Data;
using QAMate.Repositories;
using QAMate.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Swagger/OpenAPI generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (temporary: allow all)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Configure EF Core DbContext with SQLite using connection string from appsettings.json.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Register repositories and services for DI.
builder.Services.AddScoped<IDefectRepository, DefectRepository>();
builder.Services.AddScoped<IDefectService, DefectService>();

var app = builder.Build();

// Enable Swagger UI at /swagger (default RoutePrefix)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QAMate API v1");
});

app.UseHttpsRedirection();

// Apply CORS policy
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();
app.Run();
