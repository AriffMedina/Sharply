using Microsoft.EntityFrameworkCore;
using Sharply.Application.Services;
using Sharply.Domain.Interfaces;
using Sharply.Infrastructure.Data;
using Sharply.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión a SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositorios (Adapters de salida)
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ISkillLogRepository, SkillLogRepository>();

// Servicios de aplicación (núcleo)
builder.Services.AddScoped<ISkillDecayService, SkillDecayService>();
builder.Services.AddScoped<IMissionService, MissionService>();

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sharply API",
        Version = "v1",
        Description = "API REST para el sistema de tracking de deterioro de habilidades Sharply"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sharply API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();