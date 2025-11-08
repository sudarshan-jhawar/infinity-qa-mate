using Microsoft.EntityFrameworkCore;
using QATrainer.Models;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<InfinityQaContext>(options =>
    options.UseSqlite("Data Source=infinityQA.db"));

// Add controllers and Swagger/OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure DB/tables exist
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InfinityQaContext>();
    db.Database.EnsureCreated();
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // opens /swagger by default
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => "OK");

app.Run();