using ITAssetAccounting.Infrastructure.Extensions;
using ITAssetAccounting.ReportService.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ServiceCollectionExtensions.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<EquipmentApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EquipmentService:BaseUrl"] ?? "http://localhost:5102");
});
builder.Services.AddScoped<ReportGenerator>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddRedisCaching(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "IT Asset Report Service", Version = "v1" }));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "reports", status = "ok" }));
app.Run();

public partial class Program { }
