using Microsoft.EntityFrameworkCore;
using UrbanShiftRP;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration["DB_CONNECTION"];
builder.Services.AddDbContext<UrbanShiftRPDb>(opt => opt.UseNpgsql(conn));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
