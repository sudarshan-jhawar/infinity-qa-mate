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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});


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


// Enable CORS
app.UseCors("AllowAll");


app.MapControllers();

app.MapGet("/", () => "OK");

app.Run();