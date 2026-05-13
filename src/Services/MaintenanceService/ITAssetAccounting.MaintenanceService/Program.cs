using ITAssetAccounting.Infrastructure.Extensions;
using ITAssetAccounting.MaintenanceService.Data;
using ITAssetAccounting.MaintenanceService.Hubs;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ServiceCollectionExtensions.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddDbContext<MaintenanceDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MaintenanceDb")));

builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddRedisCaching(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "IT Asset Maintenance Service", Version = "v1" }));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

Directory.CreateDirectory("data");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MaintenanceDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MaintenanceHub>("/hubs/maintenance");
app.MapGet("/health", () => Results.Ok(new { service = "maintenance", status = "ok" }));
app.Run();

public partial class Program { }
