using ITAssetAccounting.EquipmentService.Data;
using ITAssetAccounting.EquipmentService.Hubs;
using ITAssetAccounting.EquipmentService.Repositories;
using ITAssetAccounting.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ServiceCollectionExtensions.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddDbContext<EquipmentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EquipmentDb")));

builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<EquipmentUnitOfWork>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddRedisCaching(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "IT Asset Equipment Service", Version = "v1" }));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

Directory.CreateDirectory("App_Data");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EquipmentDbContext>();
    await db.Database.EnsureCreatedAsync();
    await EquipmentSeedData.SeedAsync(db);
}

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<EquipmentHub>("/hubs/equipment");
app.MapGet("/health", () => Results.Ok(new { service = "equipment", status = "ok" }));
app.Run();

public partial class Program { }
