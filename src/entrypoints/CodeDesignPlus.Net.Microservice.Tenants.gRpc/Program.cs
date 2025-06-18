using CodeDesignPlus.Net.Logger.Extensions;
using CodeDesignPlus.Net.Microservice.Commons.EntryPoints.gRpc.Extensions;
using CodeDesignPlus.Net.Microservice.Commons.EntryPoints.gRpc.Interceptors;
using CodeDesignPlus.Net.Microservice.Commons.FluentValidation;
using CodeDesignPlus.Net.Microservice.Commons.HealthChecks;
using CodeDesignPlus.Net.Microservice.Commons.MediatR;
using CodeDesignPlus.Net.Microservice.Tenants.gRpc.Core.Mapster;
using CodeDesignPlus.Net.Microservice.Tenants.gRpc.Services;
using CodeDesignPlus.Net.Mongo.Extensions;
using CodeDesignPlus.Net.Observability.Extensions;
using CodeDesignPlus.Net.RabbitMQ.Extensions;
using CodeDesignPlus.Net.Redis.Cache.Extensions;
using CodeDesignPlus.Net.Redis.Extensions;
using CodeDesignPlus.Net.Security.Extensions;
using CodeDesignPlus.Net.Vault.Extensions;

var builder = WebApplication.CreateBuilder(args);

Serilog.Debugging.SelfLog.Enable(Console.Error);

MapsterConfig.Configure();

builder.Host.UseSerilog();

builder.Configuration.AddVault();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ErrorInterceptor>();
});
builder.Services.AddGrpcInterceptors();
builder.Services.AddGrpcReflection();

builder.Services.AddVault(builder.Configuration);
builder.Services.AddMapster();
builder.Services.AddMediatR<CodeDesignPlus.Net.Microservice.Tenants.Application.Startup>();
builder.Services.AddFluentValidation();

builder.Services.AddMongo<CodeDesignPlus.Net.Microservice.Tenants.Infrastructure.Startup>(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);
builder.Services.AddRabbitMQ<CodeDesignPlus.Net.Microservice.Tenants.Domain.Startup>(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddObservability(builder.Configuration, builder.Environment);
builder.Services.AddLogger(builder.Configuration);
builder.Services.AddCache(builder.Configuration);
builder.Services.AddHealthChecksServices();

var app = builder.Build();

app.UseHealthChecks();

if (!app.Environment.IsProduction())
    app.MapGrpcReflectionService();

app.UseAuth();

app.MapGrpcService<TenantService>().RequireAuthorization();


app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}