using Application.Abstractions;
using Application.Dtos;
using Application.Implementations;
using AuthService.RegistersExtensions;
using AuthService.Settings;
using Domain.Entities;
using Infrastructure.CommonInjectDependence;
using Repository.Persistency.Generic;
using Repository.UnitOfWork;
using Repository.UnitOfWork.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddEnvironmentVariables();

// Add services to the container.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.ListenAnyIP(9002);
});

var serviceSettings = builder.Configuration.GetSection("ServiceSettings").Get<ServiceSettings>();
builder.Services.StartupBootstrap(builder.Configuration);
builder.Services.AddConsulSettings(serviceSettings);

builder.Services.AddControllers();
builder.Services.AddScoped(typeof(IBusinessBase<DespesaDto, Despesa>), typeof(DespesaBusinessImpl<DespesaDto>));
builder.Services.AddAutoMapper(typeof(DespesaProfile).Assembly);
builder.Services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
builder.Services.AddScoped(typeof(IRepositorio<>), typeof(GenericRepositorio<>));
builder.Services.AddOptions();
builder.Services.ConfigureMySqlServerContext(builder.Configuration);

var app = builder.Build();

app.UseConsul(serviceSettings); 
app.UseRouting();
app.MapControllers();
app.Run();
