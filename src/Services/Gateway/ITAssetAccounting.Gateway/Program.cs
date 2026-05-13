using ITAssetAccounting.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

ServiceCollectionExtensions.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseCors();
app.MapReverseProxy();
app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "ok" }));
app.Run();

public partial class Program { }
